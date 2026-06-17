# Agent Guide For Beacon

Beacon is a Unity remote configuration and feature flag package. Keep the package root-level, dependency-light, and production-minded.

## Architecture Rules

- Keep runtime code under `Runtime/`.
- Keep editor-only tooling under `Editor/`.
- Keep tests under `Tests/Runtime/`.
- Keep samples under `Samples~/`.
- Keep public APIs intentional and small.
- Prefer composition through interfaces and callbacks.
- Keep Unity-facing code thin when possible.
- Put config/domain logic in plain C# types, not directly inside `MonoBehaviour`.
- Avoid service locators, reflection discovery, global command buses, and DI dependencies.
- Keep Beacon independent from Lens; optional integration belongs in a sample or separate assembly.

## Runtime Safety

- Defaults should be explicit at call sites.
- Invalid config should not replace the active snapshot.
- Failed refresh should preserve last-known-good data.
- Risky flags should fail closed.
- Do not expose secrets or private player data.
- Avoid file/network IO in per-frame paths.
- Cache expensive state in owning systems and expose snapshots.

## Validation Checklist

- Package imports as a root-level UPM package.
- `package.json` version and docs match.
- Runtime tests pass.
- Sample scene compiles.
- README examples match the public API.
- CI package validation passes.
- Public API changes are documented in `Docs/api-overview.md`.
