# Save-load temp folder fix (Stationeers)

A BepInEx plugin. To load a save, Stationeers unpacks it into a fresh folder under the
temporary directory, loads the world from there, and then deletes the folder. Under CrossOver
(Wine) on macOS that delete can fail after the world has loaded, and the game then treats the
whole load as failed:

    Exception: Unknown error (0x11e40c0) : '\?\C:\users\crossover\AppData\Local\Temp\<guid>'
      at System.IO.Directory.Delete ...
      at Assets.Scripts.Serialization.LoadHelper.LoadWorldTask ...

The plugin patches only that one delete, in `LoadHelper.LoadWorldTask`, so a failure is logged
as a warning and the load carries on. The folder is left in the temporary directory. Nothing
else is touched, and where the delete works (Windows) nothing changes.

## Install

Copy `SaveLoadTempFix.dll` into `BepInEx/plugins/`. On start, BepInEx's log
(`BepInEx/LogOutput.log`) says "A failure to delete a loaded save's temporary folder is now
logged and ignored." If the game has changed so the patch no longer applies, it says
"nothing patched" instead and does nothing.

## Build

    STATIONEERS_DIR=<the game's install directory> dotnet build -c Release

The DLL is `bin/Release/net48/SaveLoadTempFix.dll`.
