# Beacon Demo

This sample demonstrates how to integrate Beacon into a Unity scene.

It shows:

- loading configuration from an embedded JSON file
- creating a `BeaconClient`
- refreshing configuration
- reading typed values
- evaluating feature flags
- reacting to `SnapshotChanged`
- inspecting snapshot and context state in a debug overlay

## Scene

Open `BeaconDemo.unity` after importing the sample from Unity Package Manager.

Main objects:

| Object | Purpose |
| --- | --- |
| `BeaconInstaller` | Creates the config source, store, validator, repository, context provider, evaluator, and client. |
| `RefreshButton` | Calls `BeaconClient.RefreshAsync`. |
| `OverlayText` | Displays snapshot, context, and refresh state. |
| `NewHomePanel` | Example GameObject controlled by the `new_home_ui` flag. |

## Config

The sample config lives at:

```text
Config/demo-config.json
```

It contains `meta`, `flags`, and `values` sections.

## Usage Pattern

`BeaconInstaller` wires:

- `EmbeddedTextAssetConfigSource`
- `DiskConfigStore`
- `BasicConfigValidator`
- `DefaultConfigRepository`
- `DefaultContextProvider`
- `JsonFlagEvaluator`
- `BeaconClient`

Scene components bind to the installer and update when the client becomes available or when the snapshot changes.

## Lifecycle Notes

Demo driver components subscribe in `OnEnable` and unsubscribe in `OnDisable`.

They use `MonoBehaviourClientBinder` to resolve the installer immediately or after a short startup window. This avoids Script Execution Order requirements and prevents duplicate subscriptions when GameObjects are enabled, disabled, or reloaded.

## What This Sample Is Not

This sample does not include a backend, analytics, A/B testing, production environment selection, or Lens integration. It is intentionally focused on the runtime package architecture.

## Production Guidance

- Keep defaults explicit in consuming code.
- Refresh on controlled lifecycle events or intervals.
- Avoid feature flag polling in hot `Update()` paths.
- Keep remote payloads small and validated.
- Use last-known-good fallback before relying on remote config in production.
