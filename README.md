# Wine Save Load Fix (Stationeers)

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

It is a [StationeersLaunchPad](https://github.com/StationeersLaunchPad/StationeersLaunchPad)
plugin mod: with BepInEx and StationeersLaunchPad installed, copy the `WineLoadFix` folder of
a release (or of `dist/`, below) into StationeersLaunchPad's local mods directory
(`Documents/My Games/Stationeers/mods`), so it holds `WineLoadFix.dll` and `About/`, and
enable it in the mod list. Without StationeersLaunchPad, copy just `WineLoadFix.dll` into
`BepInEx/plugins/`.

On start, BepInEx's log (`BepInEx/LogOutput.log`) says "A failure to delete a loaded save's
temporary folder is now logged and ignored." If the game has changed so the patch no longer
applies, it says "nothing patched" instead and does nothing.

## Build

    dotnet build -c Release

It needs only the .NET SDK and NuGet packages, not the game. CI builds it on every push and pull
request, and keeps the packaged mod as the run's artifact.

The DLL is `bin/Release/net48/WineLoadFix.dll`. With `-t:Dist`, the mod's layout is also
put in `dist/WineLoadFix`.

`About/Preview.png` and `About/thumb.png` are rendered at 1232×706 from `art/preview.svg`, an
original drawing, with any browser.

## License

MIT; see `LICENSE`.
