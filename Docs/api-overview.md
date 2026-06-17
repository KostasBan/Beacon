# Beacon API Overview

This page summarizes the public runtime API that consuming Unity projects are expected to use.

## Client

`BeaconClient` is the primary entry point. It exposes the active `RepositorySnapshot`, typed value access, feature flag evaluation, refresh, and `SnapshotChanged` notifications.

`BeaconPackageInfo` exposes the package name and version for diagnostics and validation.

## Repository Pipeline

Beacon composes config loading from small interfaces:

- `IConfigSource` fetches raw bytes.
- `IConfigValidator` validates bytes before activation.
- `IConfigStore` persists accepted config.
- `IConfigRepository` owns the active immutable snapshot.

The default repository keeps the current snapshot unchanged on fetch or validation failure.

## Sources And Stores

Use `EmbeddedTextAssetConfigSource` for local defaults or demos.

Use `HttpConfigSource` for remote JSON payloads. It supports ETag-based conditional requests, optional extra headers, optional dynamic headers, timeouts, retry count, and exponential backoff.

Use `DiskConfigStore` to persist last-known-good config under `Application.persistentDataPath/beacon`.

## Typed Values

Use `BeaconClient.GetBool`, `GetInt`, `GetFloat`, and `GetString` with explicit defaults. Beacon reads values from the top-level `values` object.

## Feature Flags

Use `BeaconClient.IsEnabled(flagKey, defaultValue)` for feature gates. The JSON evaluator supports enabled state, deterministic rollout percentage, optional salt, platform targeting, minimum app version targeting, and a global kill switch allowlist.

## Context

`DefaultContextProvider` supplies install ID, platform, and `Application.version`. Consuming projects can implement `IContextProvider` when they need custom app version, platform, user cohort, or environment data.

## Editor Tooling

Open `Tools/Beacon/Debug Window` in Play Mode to inspect a runtime client, refresh config, validate JSON, evaluate a flag, and read typed values.
