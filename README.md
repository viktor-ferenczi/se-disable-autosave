# Disable Autosave

Space Engineers (version 1) client plugin for [Pulsar](https://github.com/SpaceGT/Pulsar).

It turns the **Autosave** option off in all newly created world saves, so new worlds never
start out with the game saving on its own every five minutes.

## What it covers

| Way a new world save is created | How it is handled |
| --- | --- |
| New world from classic content, custom or procedural world | First checkpoint written for the new save folder |
| Scenario, campaign or mission started as a new world | Same as above |
| `Save As` in the in-game menu | Same as above, it always targets a new save folder |
| `Save As` in the Load World screen | Checkpoint of the copied save is amended before it is written back |
| Autosave checkbox in New World / Edit Settings | Unchecked and disabled on the screen, and forced off in the settings |

Existing world saves are not touched. Their autosave setting only changes if their settings
are edited through the Edit Settings screen while the plugin is enabled.

## How it works

Three Harmony patches, all under `ClientPlugin/Patches`:

- `MySessionPatch` — postfix on `MySession.GetCheckpoint`. When the session's `CurrentPath`
  does not exist yet, the checkpoint being assembled belongs to a brand new world save, so
  `AutoSaveInMinutes` is zeroed both in the checkpoint and in the live session settings.
- `MyGuiScreenWorldSettingsPatch` — postfixes on `GetSettingsFromControls` and
  `SetSettingsToControls` of the world settings screen.
- `WorldCopyPatch` — the `Save As` command of the Load World screen copies the save folder
  outside of any session, so the checkpoint it reloads from the copy is amended before it is
  written back. The copy runs in a private nested class, which is patched by name.

## Building

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
(plus the [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481)
on Windows) and a Space Engineers install.

```bash
dotnet build
```

The `DeployPlugin` target copies the build output into Pulsar's `Local` plugin folder after
every successful build.
