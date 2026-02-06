# Beacon

Beacon is a lightweight Unity package that provides a client entry point and repository interfaces for configuration data.

## Requirements

- Unity 2022 LTS or newer

## Installation

Add the package to your project by referencing the repository in your `manifest.json` or by copying the package folder into your `Packages` directory.

## Usage

Create a repository implementation and provide it to `BeaconClient`:

```csharp
using KBanakakis.Beacon;
using KBanakakis.Beacon.Repositories;

IConfigRepository repository = new YourConfigRepository();
var client = new BeaconClient(repository);
var snapshot = client.GetSnapshot();
```
