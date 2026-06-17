# Beacon

[![Package Validation](https://github.com/KostasBan/Beacon/actions/workflows/package-validation.yml/badge.svg)](https://github.com/KostasBan/Beacon/actions/workflows/package-validation.yml)
[![Unity EditMode Tests](https://github.com/KostasBan/Beacon/actions/workflows/unity-tests.yml/badge.svg)](https://github.com/KostasBan/Beacon/actions/workflows/unity-tests.yml)

Beacon is a Unity remote configuration and feature flag package focused on safe runtime configuration.

It gives Unity projects a small runtime layer for loading validated config, reading typed values with explicit defaults, evaluating feature flags, and falling back to last-known-good data when refresh fails.

> Preview assets: screenshots and workflow GIFs should live in `Docs/images/` and `Docs/media/` once the demo scene is visually captured.

## Status

Version `0.1.0` is the package foundation release. Beacon is useful as a runtime architecture sample and early package, but it is not production-ready until schema validation, remote-source tests, sample validation, and CI results are proven in a real Unity project.

## Install

Beacon is a root-level UPM package. Add it to a Unity `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.kostasban.beacon": "https://github.com/KostasBan/Beacon.git"
  }
}
```

Beacon targets Unity `6000.3`.

## Why Beacon Exists

Shipped games often need runtime configuration changes: tuning values, UI copy, kill switches, staged rollouts, and platform-specific behavior. Those changes need safe defaults and predictable failure behavior. A missing file, malformed JSON payload, or failed network request should not break gameplay.

Beacon is built around:

- explicit defaults at call sites
- immutable config snapshots
- typed config access
- deterministic feature flag evaluation
- validation before config becomes active
- last-known-good fallback
- QA/debug visibility through samples and editor tooling

## Quick Start

```csharp
using System.Threading;
using KostasBan.Beacon;
using KostasBan.Beacon.Context;
using KostasBan.Beacon.Evaluation;
using KostasBan.Beacon.Repositories;
using UnityEngine;

public sealed class BeaconBootstrap : MonoBehaviour
{
    [SerializeField] private TextAsset configAsset;

    public BeaconClient Client { get; private set; }

    private async void Awake()
    {
        var source = new EmbeddedTextAssetConfigSource(configAsset);
        var store = new DiskConfigStore();
        var validator = new BasicConfigValidator();
        var repository = new DefaultConfigRepository(source, store, validator);

        Client = new BeaconClient(
            repository,
            new DefaultContextProvider(),
            new JsonFlagEvaluator());

        var result = await Client.RefreshAsync(CancellationToken.None);
        if (result.Error != null)
        {
            Debug.LogWarning($"Beacon refresh failed: {result.Error}");
        }
    }
}
```

Read typed values with explicit defaults:

```csharp
var title = Client.GetString("ui_home_title", "Default Title");
var scale = Client.GetFloat("ui_scale", 1.0f);
var maxLives = Client.GetInt("max_lives", 3);
var showDebug = Client.GetBool("show_debug", false);
```

Evaluate a feature flag:

```csharp
if (Client.IsEnabled("new_home_ui", defaultValue: false))
{
    // Enable the new UI path.
}
```

## Config Shape

```json
{
  "meta": {
    "schemaVersion": 1,
    "configVersion": "demo-1"
  },
  "safety": {
    "killAllExperiments": false,
    "allowlistFlagsWhenKilled": []
  },
  "flags": {
    "new_home_ui": {
      "enabled": true,
      "rolloutPercent": 50,
      "salt": "home-ui-v1",
      "targets": {
        "platforms": ["Standalone", "Android"],
        "minAppVersion": "1.2.0"
      }
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

## Architecture

Beacon follows a small composition-first flow:

```text
IConfigSource -> IConfigValidator -> IConfigStore -> IConfigRepository -> BeaconClient
```

The package owns the config surface. Consuming projects own where config comes from, when refresh happens, how environments are selected, and how feature decisions are used.

Current public surface:

- `BeaconClient`
- `RepositorySnapshot`
- `IConfigRepository`, `IConfigSource`, `IConfigStore`, `IConfigValidator`
- `RefreshResult`, `ConfigSourceResult`, `StoredConfig`, `ValidationResult`
- `EmbeddedTextAssetConfigSource`, `HttpConfigSource`
- `DiskConfigStore`
- `BasicConfigValidator`
- `BeaconContext`, `IContextProvider`, `DefaultContextProvider`
- `IFlagEvaluator`, `JsonFlagEvaluator`
- `MonoBehaviourClientBinder` for lifecycle-safe sample/demo binding

Implementation helpers such as JSON parsing, bucketing, version comparison, and test repositories are internal.

## Implemented

- Root-level UPM package structure.
- Local/default config source from `TextAsset`.
- HTTP config source with ETag support and retry/backoff semantics.
- Last-known-good disk cache.
- Snapshot-based repository.
- Typed value access for bool, int, float, and string.
- Feature flag evaluation.
- Deterministic percentage rollout.
- Platform targeting and minimum app version targeting.
- Global kill switch with allowlist.
- Editor debug window under `Tools/Beacon/Debug Window`.
- Basic demo sample.
- Runtime/EditMode tests for repository, values, and evaluator behavior.
- Package validation and Unity EditMode CI workflows.

## Partial Or Planned

- Strict schema validation.
- Explicit development/staging/production environment model.
- More complete disk-store failure coverage.
- Provider cookbook sample.
- Lens integration as an optional debug provider.
- Captured preview images/GIFs.
- Production hardening from real project integration.

## Samples

Import `Samples~/Demo` through Unity Package Manager. The demo shows local JSON loading, refresh events, typed values, a flag-driven GameObject, and a runtime debug overlay.

A future provider cookbook sample should show local defaults, an HTTP source adapter, fallback behavior, feature flag usage, and optional Lens visibility without making Beacon depend on Lens.

## Safety Notes

- Treat remote config as data, not trusted gameplay code.
- Keep safe defaults at every call site.
- Fail closed for risky feature flags.
- Use kill switches for reversible runtime changes only.
- Do not expose secrets, credentials, private player data, or production-only controls through config/debug surfaces.
- Avoid `IsEnabled` polling in hot `Update()` paths; cache decisions at system boundaries.
- Refresh on explicit lifecycle moments or controlled intervals.
- Keep Beacon main-thread-oriented unless a future design explicitly changes the threading model.

## Roadmap

1. Validate `0.1.0` in a clean Unity project.
2. Capture demo screenshots/GIFs.
3. Add strict schema validation.
4. Add explicit environment support.
5. Expand disk and HTTP source tests.
6. Add Provider Cookbook sample.
7. Add optional Lens provider integration.
8. Prepare a combined sample game that uses Beacon with the related Unity packages.

## Documentation

- [API Overview](Docs/api-overview.md)
- [Architecture Decisions](Docs/architecture-decisions.md)
- [Future Integrations](Docs/future-integrations.md)
- [Agent Usage](Docs/agent-usage.md)
- [Contributing](CONTRIBUTING.md)
- [Security](SECURITY.md)
