# Beacon Future Integrations

## Lens

A future optional Lens integration should expose a read-only Beacon section with snapshot version, provenance, refresh status, environment, context, selected typed values, and evaluated flags.

The integration should live in a sample or optional assembly so Beacon can be installed without Lens.

## Sample Game

Beacon should be usable in a combined sample game alongside the related Unity packages. The sample should show local defaults, remote refresh, fallback behavior, feature-gated UI, and runtime debug visibility.

## Remote Hosting

Beacon should stay backend-agnostic. CDN, static hosting, custom backend, or platform config services should integrate through `IConfigSource`.

## Environment Selection

Development, staging, and production environment selection should be explicit, documented, and owned by project bootstrap code.
