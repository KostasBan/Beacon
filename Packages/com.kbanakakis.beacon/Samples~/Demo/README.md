# Beacon Demo Sample

This sample provides a minimal Unity scene with a debug overlay and a flag-driven UI toggle.

## Contents

- `BeaconDemo.unity` scene with:
  - Canvas
  - Overlay text area (`OverlayText`)
  - Refresh button (`RefreshButton`)
  - Toggle target panel (`NewHomePanel`)
- `Config/demo-config.json` sample Beacon config (`new_home_ui` is enabled).
- `Scripts/BeaconInstaller.cs` to build a demo `BeaconClient` from an embedded `TextAsset` config.
- `Scripts/BeaconDebugOverlay.cs` to display snapshot/context information and execute refresh.
- `Scripts/FlagDrivenGameObject.cs` to set a target object active/inactive from a flag key.

## How to run

1. Import the sample from Package Manager (`Samples > Demo > Import`).
2. Open `BeaconDemo.unity`.
3. Create an empty object named `BeaconInstaller` and add `BeaconInstaller` component.
4. Assign `Config/demo-config.json` to `BeaconInstaller.configAsset`.
5. Add `BeaconDebugOverlay` to any object (for example `Canvas`) and wire:
   - `installer` -> `BeaconInstaller`
   - `overlayText` -> `OverlayText` (`UnityEngine.UI.Text` or TMP text)
   - `refreshButton` -> `RefreshButton`
6. Add `FlagDrivenGameObject` to any object and wire:
   - `installer` -> `BeaconInstaller`
   - `flagKey` -> `new_home_ui`
   - `target` -> `NewHomePanel`
7. Enter Play Mode.

## What to click

- Click **Refresh** to call `BeaconClient.RefreshAsync`.
- Watch the overlay values update with:
  - `configVersion`
  - `schemaVersion`
  - `provenance`
  - `platform`
  - `installId`
  - last refresh result
- `NewHomePanel` visibility is driven by the `new_home_ui` flag.
