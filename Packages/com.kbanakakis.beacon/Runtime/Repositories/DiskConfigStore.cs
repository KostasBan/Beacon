using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KBanakakis.Beacon.Repositories
{
    public sealed class DiskConfigStore : IConfigStore
    {
        private const string ConfigFileName = "lkg.json";
        private const string MetadataFileName = "lkg.meta.json";
        private readonly string _baseDirectory;

        public DiskConfigStore()
            : this(Path.Combine(Application.persistentDataPath, "beacon"))
        {
        }

        public DiskConfigStore(string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new ArgumentException("Base directory must be provided.", nameof(baseDirectory));
            }

            _baseDirectory = baseDirectory;
        }

        public bool TryLoad(out StoredConfig stored)
        {
            stored = default;
            var configPath = GetConfigPath();
            if (!File.Exists(configPath))
            {
                return false;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(configPath);
            }
            catch (IOException)
            {
                return false;
            }

            string? storedVersion = null;
            var metadataPath = GetMetadataPath();
            if (File.Exists(metadataPath))
            {
                try
                {
                    var metadataBytes = File.ReadAllBytes(metadataPath);
                    using var document = JsonDocument.Parse(metadataBytes);
                    if (document.RootElement.ValueKind == JsonValueKind.Object
                        && document.RootElement.TryGetProperty("storedVersion", out var versionElement)
                        && versionElement.ValueKind == JsonValueKind.String)
                    {
                        storedVersion = versionElement.GetString();
                    }
                }
                catch (IOException)
                {
                    storedVersion = null;
                }
                catch (JsonException)
                {
                    storedVersion = null;
                }
            }

            stored = new StoredConfig(bytes, storedVersion);
            return true;
        }

        public Task StoreAsync(StoredConfig stored, CancellationToken ct)
        {
            if (stored.Bytes == null)
            {
                throw new ArgumentNullException(nameof(stored));
            }

            Directory.CreateDirectory(_baseDirectory);

            var configPath = GetConfigPath();
            WriteAtomic(configPath, stored.Bytes);

            var metadataPath = GetMetadataPath();
            var metadataPayload = JsonSerializer.SerializeToUtf8Bytes(new StoredMetadata(stored.StoredVersion));
            WriteAtomic(metadataPath, metadataPayload);

            return Task.CompletedTask;
        }

        private string GetConfigPath()
        {
            return Path.Combine(_baseDirectory, ConfigFileName);
        }

        private string GetMetadataPath()
        {
            return Path.Combine(_baseDirectory, MetadataFileName);
        }

        private static void WriteAtomic(string destinationPath, byte[] payload)
        {
            var tempPath = destinationPath + ".tmp";
            File.WriteAllBytes(tempPath, payload);
            if (File.Exists(destinationPath))
            {
                File.Replace(tempPath, destinationPath, null);
            }
            else
            {
                File.Move(tempPath, destinationPath);
            }
        }

        private readonly struct StoredMetadata
        {
            public StoredMetadata(string? storedVersion)
            {
                StoredVersion = storedVersion;
            }

            public string? StoredVersion { get; }
        }
    }
}
