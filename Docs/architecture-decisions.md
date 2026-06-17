# Beacon Architecture Decisions

## Root-Level UPM Package

Beacon uses a root-level Unity Package Manager layout. This keeps the repository installable directly from a Git URL and avoids shipping a full Unity project to users.

## Snapshot-Based Reads

Runtime consumers read from immutable `RepositorySnapshot` instances through `BeaconClient`. Config refresh is explicit and publishes a new snapshot only after validation succeeds.

## Small Interfaces Over Frameworks

Beacon uses small interfaces for sources, stores, validators, repositories, context, and flag evaluation. This keeps extension points clear without introducing DI, backend, analytics, or UI framework dependencies.

## Conservative Failure Behavior

Fetch failures, invalid JSON, invalid UTF-8, malformed flag rules, and missing payloads should not replace a valid active snapshot. Risky flag evaluation should fail closed.

## Package Owns Surface, Project Owns Data

Beacon owns the runtime config surface. Consuming projects own where config is hosted, how credentials are provided, how environments are selected, and how feature decisions affect gameplay.

## Optional Debug Integrations

Beacon can expose debug information through editor tooling and future optional Lens providers, but Beacon must not depend on Lens at runtime.
