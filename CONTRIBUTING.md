# Contributing To Beacon

Beacon is a small Unity remote configuration and feature flag package. Contributions should keep it focused, dependency-light, safe by default, and easy to install through Unity Package Manager.

## Local Validation

Use Unity `6000.3` or newer.

Recommended flow:

1. Create or open a clean Unity project outside this package root.
2. Add Beacon through Package Manager using a local path or Git URL.
3. Add `com.kostasban.beacon` to the project's `testables` list.
4. Run EditMode tests from Unity Test Runner.
5. Import the `Beacon Demo` sample and confirm it compiles.

Keep local validation projects ignored, for example under `DevProject/`.

## Package Rules

- Runtime source belongs in `Runtime/`.
- Editor-only tooling belongs in `Editor/`.
- Runtime tests belong in `Tests/Runtime/`.
- Samples belong in `Samples~/`.
- Keep public APIs small and intentional.
- Do not add external runtime dependencies without a package-level decision.
- Keep Beacon independent from Lens; Lens integration should be optional.
- Document main-thread assumptions and expensive refresh behavior.
- Preserve source compatibility once the package is public.

## Versioning

Beacon uses semantic versioning. Tag releases with a `v` prefix, for example `v0.1.0`.

- Patch: bug fixes and docs.
- Minor: additive public API, sample, or tooling improvements.
- Major: breaking public API or behavior changes.

## Safety

Remote config can affect live runtime behavior. Keep defaults explicit, validate payloads before publishing snapshots, and fail closed for risky flags.

Do not include secrets, player-private data, production credentials, or irreversible debug actions in public issues, sample config, screenshots, copied reports, or docs.
