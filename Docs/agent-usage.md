# Beacon Agent Usage

Use Beacon when a Unity project needs safe runtime configuration, feature flags, local defaults, last-known-good fallback, and QA/debug visibility.

When changing Beacon:

- preserve the root-level UPM package layout
- update README and docs with public API changes
- add tests for behavior changes
- keep implementation helpers internal where practical
- keep runtime dependencies minimal
- avoid dashboards, analytics, A/B testing, or backend code unless explicitly planned
- keep Lens integration optional

Validation before handoff:

- package metadata is correct
- required docs exist
- namespaces and asmdefs use `KostasBan.Beacon`
- no package content remains under `Packages/`
- static package validation passes
- Unity tests are run when a Unity project/editor is available
