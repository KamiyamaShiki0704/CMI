# Custom Music Injector - Elden Ring / Nightreign fork

This is an unofficial compatibility fork of Custom Music Injector (CMI).

- Original project: [Pear0533/CMI](https://github.com/Pear0533/CMI)
- Fork maintained by: [KamiyamaShiki0704](https://github.com/KamiyamaShiki0704)

The original repository did not include a license file at the time this fork was prepared. Please preserve the original author attribution when sharing modified builds. This fork does not include game files, audio files, or other copyrighted assets.

## What This Fork Adds

- Elden Ring and Elden Ring Nightreign runtime configuration.
- ME3 native-loader startup without a PowerShell or batch-file launcher.
- External .NET host process, so WinForms/WMP/.NET do not run inside the game process.
- Native shared-memory event-flag bridge for direct `EventFlagId` checks.
- Optional hidden UI release builds for normal gameplay use.
- Compatibility fields for older CMI `Pointer1` / `Pointer2` / `Startbit` entries.
- Example ME3 profiles/snippets and JSON configuration files.

## Project Layout

- `CMI/` - managed CMI WinForms/audio code.
- `ManagedHost/` - small external host executable that starts CMI.
- `NativeLoader/` - native ME3 DLL loader and event-flag bridge.
- `examples/` - sample ME3 snippets/profiles.
- `CMI/CMI.config.eldenring.json` - Elden Ring config template.
- `CMI/CMI.config.nightreign.json` - Nightreign config template.
- `CMI/sound.example.json` - minimal CMI sound configuration example.
- `CMI/sound.eldenring.example.json` - legacy original-CMI style sound configuration example.

## Runtime Files

A packaged install normally contains:

- `CMI.dll`
- `CMI.Host.exe`
- one native loader:
  - `cmi_eldenring_loader.dll`
  - `cmi_nightreign_loader.dll`
- `CMI.config.json`
- `sound.json`
- `sound/`

The release build hides the CMI debug/control window. The audio controller still runs in `CMI.Host.exe`.

## Installation With ME3

Copy the packaged files into your ME3 mod `dll` folder.

For Elden Ring:

```toml
[[natives]]
path = "dll/cmi_eldenring_loader.dll"
```

Copy `CMI.config.eldenring.json` as `CMI.config.json` next to `CMI.dll`.

For Nightreign:

```toml
[[natives]]
path = "dll/cmi_nightreign_loader.dll"
```

Copy `CMI.config.nightreign.json` as `CMI.config.json` next to `CMI.dll`.

Then place your music files under `sound/` and edit `sound.json`.

## `sound.json` Fields

Each top-level entry is one sound event. Later entries of the same `Type` override earlier active entries.

Important fields:

- `SoundPath`: path to an audio file under the configured `sound` folder.
- `AlwaysActive`: if `true`, this event is active without checking a flag.
- `EventFlagId`: direct event flag trigger. Use `null` when not using this field.
- `Pointer1`, `Pointer2`, `Startbit`: legacy CMI pointer trigger fields.
- `Type`: audio channel type. BGM usually uses `4`.
- `FadeInSeconds`: fade-in duration.
- `FadeOutSeconds`: fade-out duration.
- `FadeIntoNextTrack`: if `false`, deactivation stops/fades the current track.
- `Loop`: whether the track loops.

Activation precedence:

1. `AlwaysActive = true`
2. non-null `EventFlagId`
3. legacy `Pointer1` / `Pointer2` / `Startbit`

For direct event flags:

```json
{
  "BossPhase1": {
    "SoundPath": "boss_phase1.mp3",
    "Pointer1": "0x0",
    "Pointer2": "0x0",
    "Startbit": 0,
    "AlwaysActive": false,
    "EventFlagId": 10003599,
    "Type": 4,
    "FadeInSeconds": 1.0,
    "FadeOutSeconds": 2.0,
    "FadeIntoNextTrack": false,
    "Loop": true
  }
}
```

For legacy pointer entries:

```json
{
  "LegacyEntry": {
    "SoundPath": "legacy.mp3",
    "Pointer1": "0x28",
    "Pointer2": "0x152186",
    "Startbit": 5,
    "AlwaysActive": false,
    "EventFlagId": null,
    "Type": 4,
    "FadeInSeconds": 0.0,
    "FadeOutSeconds": 2.0,
    "FadeIntoNextTrack": true,
    "Loop": true
  }
}
```

Do not use `EventFlagId: 0` as a placeholder. Use `null` or remove the field.

## Configuration

CMI loads configuration in this order:

1. `CMI_CONFIG_FILE` environment variable, if set.
2. `CMI.config.json` next to `CMI.dll`.
3. `CMINightreign.config.json` as a backwards-compatible fallback.

Common settings:

```json
{
  "ProcessName": "eldenring",
  "DisplayName": "ELDEN RING",
  "SoundFolder": "sound",
  "SoundJson": "sound.json",
  "EventFlagIdReader": "NativeBridge",
  "Memory": {
    "EnableLegacySignatureScanning": true
  }
}
```

Use `ProcessName: "nightreign"` and `DisplayName: "ELDEN RING NIGHTREIGN"` for Nightreign.

## Building

Requirements:

- Visual Studio 2019 or 2022.
- .NET Framework 4.8 targeting pack.
- C++ x64 toolchain with v142 platform toolset.
- Windows Media Player COM components.

Build order:

1. Build managed CMI:

```powershell
MSBuild.exe CMI\CMI.sln /p:Configuration=Release /p:Platform="Any CPU"
```

2. Build external host:

```powershell
MSBuild.exe ManagedHost\CMI.Nightreign.Host.csproj /p:Configuration=Release /p:Platform="Any CPU"
```

3. Build native loader:

```powershell
MSBuild.exe NativeLoader\CmiNightreignLoader.vcxproj /p:Configuration=Release /p:Platform=x64
```

Build output is written under `dist/dll/`.

## Notes

- The native loader starts `CMI.Host.exe` and then runs a small in-process event-flag bridge.
- The managed UI/audio code runs outside the game process.
- `EventFlagId` reads are designed for Elden Ring and Nightreign through the native bridge.
- Legacy signature scanning is mainly for older CMI pointer compatibility.
