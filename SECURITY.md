# Security Policy

Beacon is a runtime configuration and feature flag package for Unity development, QA, staging, and carefully controlled production usage.

Beacon is not a secrets manager, permissions system, analytics backend, or experimentation platform.

## Supported Usage

- Load small config payloads from trusted project-controlled sources.
- Validate payloads before publishing snapshots.
- Keep explicit defaults at call sites.
- Use last-known-good fallback when refresh fails.
- Gate risky runtime behavior behind conservative feature flags.
- Keep environment selection and credentials in the consuming project.

## Do Not Expose

- Auth tokens, API keys, private certificates, or credentials.
- Private player data or payment data.
- Destructive or irreversible actions.
- Production-only controls without project-side build gates.
- Sensitive internal identifiers in copied reports or public issues.

## Remote Config Safety

Beacon redirection, targeting, and rollout data should be treated as untrusted input. Invalid config should fail closed and leave the active snapshot unchanged.

## Reporting Issues

Use the GitHub issue templates for public issues. Do not paste secrets, private player data, production credentials, or proprietary config payloads into issues, logs, screenshots, or sample files.
