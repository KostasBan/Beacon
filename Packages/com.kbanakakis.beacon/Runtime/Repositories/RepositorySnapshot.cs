using System;

namespace KBanakakis.Beacon.Repositories
{
    /// <summary>
    /// Immutable snapshot of configuration data.
    /// </summary>
    public readonly struct RepositorySnapshot
    {
        public enum ConfigProvenance
        {
            None,
            Default,
            LastKnownGood,
            Remote
        }

        public RepositorySnapshot(
            string? configVersion,
            int schemaVersion,
            DateTimeOffset retrievedAtUtc,
            ConfigProvenance provenance,
            byte[]? rawBytes)
        {
            ConfigVersion = configVersion;
            SchemaVersion = schemaVersion;
            RetrievedAtUtc = retrievedAtUtc;
            Provenance = provenance;
            RawBytes = rawBytes;
        }

        public string? ConfigVersion { get; }

        public int SchemaVersion { get; }

        public DateTimeOffset RetrievedAtUtc { get; }

        public ConfigProvenance Provenance { get; }

        public byte[]? RawBytes { get; }
    }
}
