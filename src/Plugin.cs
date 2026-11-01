using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace WineLoadFix
{
    /// <summary>
    /// Loading a save unpacks it into a fresh folder under the system's temporary directory, loads
    /// the world from there, then deletes the folder. Under CrossOver (Wine) on macOS that delete can
    /// fail after the world has loaded, and the game, which doesn't expect it to, treats the whole
    /// load as failed. This plugin makes just that one delete, in <c>LoadHelper.LoadWorldTask</c>,
    /// log a failure and carry on, leaving the folder behind.
    /// </summary>
    [BepInPlugin(Guid, "Wine Save Load Fix", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.github.subitan.stationeers-wine-load-fix";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            // LoadWorldTask is async, so its code, and the delete, are in its state machine's MoveNext.
            // LoadHelper is found by name, so the plugin builds without the game's DLLs.
            var stateMachine = AccessTools.TypeByName("Assets.Scripts.Serialization.LoadHelper")
                ?.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(t => t.Name.Contains("LoadWorldTask"));
            var moveNext = stateMachine?.GetMethod(
                "MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (moveNext == null)
            {
                Log.LogWarning("LoadHelper.LoadWorldTask wasn't found (the game may have changed): nothing patched.");
                return;
            }
            new Harmony(Guid).Patch(moveNext, transpiler: new HarmonyMethod(typeof(Plugin), nameof(Transpiler)));
            Log.LogInfo(Patched == 0
                ? "LoadHelper.LoadWorldTask deletes nothing any more (the game may have changed): nothing patched."
                : "A failure to delete a loaded save's temporary folder is now logged and ignored.");
        }

        private static int Patched;

        /// <summary>Replaces each call to <c>Directory.Delete(string, bool)</c> with <see cref="DeleteQuietly"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
        {
            var delete = AccessTools.Method(typeof(Directory), nameof(Directory.Delete), new[] { typeof(string), typeof(bool) });
            var quietly = AccessTools.Method(typeof(Plugin), nameof(DeleteQuietly));
            foreach (var instruction in code)
            {
                if (instruction.Calls(delete))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = quietly;
                    Patched++;
                }
                yield return instruction;
            }
        }

        /// <summary>Deletes a loaded save's temporary folder, logging rather than throwing if that fails.</summary>
        public static void DeleteQuietly(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (Exception e)
            {
                Log.LogWarning($"Couldn't delete the loaded save's temporary folder {path} ({e.Message}); " +
                    "the load carries on, and the folder is left behind.");
            }
        }
    }
}
