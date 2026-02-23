# Contributing to Beacon

Thanks for contributing to Beacon. This project uses a `main` / `develop` branching model and Semantic Versioning for package releases.

## Branching model

- `main`: stable history, release-ready commits.
- `develop`: integration branch for upcoming work.
- Feature branches: create from `develop` and open PRs targeting `develop` unless a hotfix is required.

## Versioning rules (SemVer)

Beacon package versions follow `MAJOR.MINOR.PATCH`:

- `MAJOR`: breaking API or behavior changes.
- `MINOR`: backwards-compatible feature additions.
- `PATCH`: backwards-compatible bug fixes.

Release tags use a `v` prefix (for example, `v0.2.0`).

## Running tests locally

### Unity Editor (recommended)

1. Open a Unity project that references this package.
2. Open **Window → General → Test Runner**.
3. Run **EditMode** tests for `com.kbanakakis.beacon.tests`.

### Unity CLI example

You can also run tests in batch mode:

```bash
/Applications/Unity/Hub/Editor/2022.3.20f1/Unity \
  -batchmode \
  -projectPath <path-to-project-using-beacon> \
  -runTests \
  -testPlatform editmode \
  -logFile - \
  -quit
```

## Cutting a release

Use **GitHub Actions → Release** workflow:

1. Open the **Release** workflow.
2. Run it manually with `version` input (for example, `0.2.0`).
3. The workflow will:
   - bump `Packages/com.kbanakakis.beacon/package.json`
   - create/update `CHANGELOG.md` section for the version
   - commit release metadata
   - create and push tag `vX.Y.Z`
4. Tag push triggers GitHub Release creation with notes from the matching changelog section.

You can also push a pre-created `v*` tag directly to generate a GitHub Release from existing changelog content.
