# CMI.Nightreign

Nightreign-targeted copy of Custom Music Injector.

## Current State

- Builds packaged output as `CMI.dll`.
- Can be launched either through the legacy `run_cmi_nightreign.bat` smoke-test path or through ME3 with `cmi_nightreign_loader.dll`.
- Looks for the `nightreign` process.
- Reads `CMINightreign.config.json` next to the DLL.
- Reads `sound.json` and audio files from the configured mod folder.
- Fails gracefully when the old Elden Ring memory signatures are not found.
- Supports `AlwaysActive` entries for checking the external audio playback chain.
- Keeps legacy `Pointer1`/`Pointer2`/`Startbit` entries for compatibility with the original CMI format.

## Known Limitation

`EventFlagId` entries are read through the native bridge when CMI is launched by
ME3 in supported game processes.

Use `sound.example.json` as the editable shape for Nightreign entries, then copy
tested entries into `sound.json`.

## ME3 Usage

Build the managed DLL, managed host, and native loader, copy the contents of
`dist/dll` into the game's `Game/dll` folder, and add this native entry to a
Nightreign `.me3` profile:

```toml
[[natives]]
path = "dll/cmi_nightreign_loader.dll"
```

The native loader does not host .NET inside `nightreign.exe`. It only launches
`CMI.Host.exe` as an external process and returns immediately, so no
PowerShell or batch file is required and WinForms/WMP stay out of the game
process.
