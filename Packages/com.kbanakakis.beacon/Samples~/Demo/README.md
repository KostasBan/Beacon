# Beacon – Demo Sample

This sample demonstrates how to integrate **Beacon** into a Unity scene to:

- Load configuration from an embedded JSON file
- Evaluate feature flags at runtime
- React to configuration refresh events
- Inspect snapshot and context via a debug overlay

The demo is intentionally minimal and focuses on architecture rather than visuals.

---

## What This Sample Demonstrates

✔ Repository-driven configuration loading
✔ Snapshot-based runtime access
✔ Deterministic rollout evaluation
✔ Event-driven UI updates (`SnapshotChanged`)
✔ Safe refresh lifecycle handling
✔ Context-aware flag evaluation
✔ Typed remote config values (`values`)

This scene acts as a reference implementation for integrating Beacon into a real Unity project.

---

## Scene Overview

`BeaconDemo.unity` contains:

### Core Objects

| Object | Purpose |
|--------|---------|
| `BeaconInstaller` | Creates and wires the `BeaconClient` |
| `Canvas` | UI root |
| `OverlayText` | Displays snapshot and context information |
| `RefreshButton` | Triggers `BeaconClient.RefreshAsync()` |
| `NewHomePanel` | A panel controlled by the `new_home_ui` flag |

---

## JSON Configuration Used

`Config/demo-config.json`

```json
{
  "meta": {
    "schemaVersion": 1,
    "configVersion": "demo-1"
  },
  "flags": {
    "new_home_ui": {
      "enabled": true
    }
  },
  "values": {
    "ui_home_title": "Hello Beacon",
    "ui_scale": 1.1,
    "max_lives": 5,
    "show_debug": true
  }
}
```

---

## Extended Flag Schema Example

A flag may optionally include:

```json
{
  "enabled": true,
  "rolloutPercent": 50,
  "salt": "experimentA",
  "targets": {
    "platforms": ["Standalone", "Android"],
    "minAppVersion": "1.2.0"
  }
}
```

Beacon supports:

- Global kill switch via `safety.killAllExperiments`
- Allowlist override when killed
- Deterministic bucketing (FNV-1a hash)
- Platform targeting
- Minimum app version targeting
- Percentage rollouts

---

## Setup Instructions

### 1️⃣ Import the Sample

Open:

Package Manager → Beacon → Samples → Demo → Import

The sample will be imported into:

```
Assets/Samples/Beacon/<version>/Beacon Demo
```

---

### 2️⃣ Scene Wiring

Open `BeaconDemo.unity`.

---

### BeaconInstaller

Create an empty GameObject:

- Add `BeaconInstaller`
- Assign:
  - `configAsset` → `Config/demo-config.json`

The installer:

- Creates repository
- Creates context provider
- Creates evaluator
- Creates `BeaconClient`
- Triggers initial `RefreshAsync()`
- Raises `ClientReady` event

---

### BeaconDebugOverlay

Attach to `Canvas`.

Assign:

- `installer` → `BeaconInstaller`
- `overlayText` → `TMP_Text`
- `refreshButton` → `RefreshButton`

This component:

- Subscribes to `SnapshotChanged`
- Displays snapshot + context data
- Allows manual refresh

---

### FlagDrivenGameObject

Attach to any GameObject.

Assign:

- `installer` → `BeaconInstaller`
- `flagKey` → `new_home_ui`
- `target` → `NewHomePanel`

This component:

- Subscribes to snapshot changes
- Evaluates the flag
- Toggles the target GameObject

> **Important:** Attach `FlagDrivenGameObject` to an always-active object (for example `DemoRoot`) and set `target` to the object you want to toggle. Avoid placing this component on the same object it toggles.


### ValueDrivenText

Attach to any `TMP_Text` GameObject.

Assign:

- `installer` → `BeaconInstaller`
- `targetText` → any `TMP_Text` in the Canvas
- `valueKey` → `ui_home_title`
- `defaultValue` → fallback text

This component:

- Subscribes to snapshot changes
- Reads a string remote value using `BeaconClient.GetString(...)`
- Updates text whenever config changes

---

## Running the Demo

1. Enter Play Mode
2. Click **Refresh**
3. Observe the overlay updating
4. Edit `Config/demo-config.json` (for example, change `values.ui_home_title`) and click **Refresh** again

Overlay displays:

- `configVersion`
- `schemaVersion`
- `provenance`
- `platform`
- `installId`
- `last refresh result`

`NewHomePanel` visibility updates based on the `new_home_ui` flag.
Any `ValueDrivenText` target updates from `values.ui_home_title`.

---

## Architecture Overview

### Snapshot Model

Beacon uses an immutable `RepositorySnapshot`.

All runtime reads are based on the snapshot.
No consumer directly touches JSON.

This guarantees:

- Thread-safe reads
- Predictable behavior
- Clear separation of concerns

---

### Repository Abstraction

The demo uses:

- `EmbeddedTextAssetConfigSource`
- `DiskConfigStore`
- `BasicConfigValidator`
- `DefaultConfigRepository`

In production you can swap:

- Remote HTTP config source
- CDN-backed configuration
- Backend-driven experimentation system

Without changing any consuming gameplay code.

---

### Event-Driven Updates

Consumers subscribe to:

```csharp
BeaconClient.SnapshotChanged
```

This avoids polling and ensures UI updates only when configuration changes.

---

### Deterministic Rollouts

Rollouts use FNV-1a hashing:

```
InstallId + FlagKey + Salt
```

This guarantees stable bucket assignment across sessions.

Users remain in the same rollout bucket across app restarts.

---

## Example: Kill Switch

```json
{
  "safety": {
    "killAllExperiments": true,
    "allowlistFlagsWhenKilled": ["critical_ui_flag"]
  }
}
```

This disables all flags except those in the allowlist.

---

## What This Demo Is Not

This sample does not include:

- Backend API
- A/B test analytics
- Tracking/metrics integration
- Editor tooling
- Cloud sync

It focuses purely on runtime architecture and flag evaluation.

---

## Extending the Demo

You can extend the demo by:

- Adding additional flags
- Introducing rollout percentages
- Testing kill switch behavior
- Adding platform targeting
- Simulating app version targeting
- Replacing the config source with a mock remote fetch

---

## Production Integration Tips

If integrating Beacon into a production Unity project:

1. Use a persistent bootstrap scene
2. Ensure installer initializes before dependent systems
3. Wrap flag checks behind domain-specific services
4. Avoid calling `IsEnabled` inside `Update()` loops
5. Centralize refresh scheduling (e.g., on app resume)
6. Persist and fallback to Last Known Good config

---

## Design Philosophy

Beacon is designed around:

- Explicit dependencies
- Immutable runtime state
- Repository pattern
- Safe failure modes
- Deterministic evaluation
- No reflection-heavy JSON parsing
- No Unity lifecycle magic

The goal is predictability and testability in large Unity projects.

---

## Why This Sample Exists

This demo is meant to:

- Showcase architecture decisions
- Provide integration guidance
- Act as a reference implementation
- Serve as a minimal reproducible setup

It is not intended to be a production-ready UI sample.

---

End of file.

## Lifecycle Safety Notes

Demo driver components (`BeaconDebugOverlay`, `FlagDrivenGameObject`, `ValueDrivenText`, `ValueDrivenUIScale`) are lifecycle-safe:

- They subscribe in `OnEnable`.
- They unsubscribe in `OnDisable`.
- They rebind immediately if `BeaconInstaller` already has a client.

This means they safely support GameObject/component enable-disable toggling, scene reloads, and repeated refresh clicks without duplicate subscriptions.
These drivers now use a shared runtime `MonoBehaviourClientBinder` helper that retries `BeaconInstaller.Instance` discovery for a short window when enabled before installer startup, so they can still bind without Script Execution Order dependencies.
