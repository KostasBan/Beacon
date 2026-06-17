using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KostasBan.Beacon.Repositories
{
    public sealed class DiskConfigStore : IConfigStore
    {
        private const string ConfigFileName = "lkg.json";
        private const string MetadataFileName = "lkg.state";
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
                    storedVersion = File.ReadAllText(metadataPath);
                    if (string.IsNullOrEmpty(storedVersion))
                    {
                        storedVersion = null;
                    }
                }
                catch (IOException)
                {
                    storedVersion = null;
                }
            }

            stored = new StoredConfig(bytes, storedVersion);
            return true;
        }

        public Task StoreAsync(StoredConfig stored, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (stored.Bytes == null || stored.Bytes.Length == 0)
            {
                throw new ArgumentException("Stored config bytes must be non-empty.", nameof(stored));
            }

            Directory.CreateDirectory(_baseDirectory);

            var configPath = GetConfigPath();
            WriteAtomic(configPath, stored.Bytes);

            var metadataPath = GetMetadataPath();
            var metadataPayload = stored.StoredVersion ?? string.Empty;
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
            try
            {
                File.WriteAllBytes(tempPath, payload);
                if (File.Exists(destinationPath))
                {
                    try
                    {
                        File.Replace(tempPath, destinationPath, null);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Delete(destinationPath);
                        File.Move(tempPath, destinationPath);
                    }
                }
                else
                {
                    File.Move(tempPath, destinationPath);
                }
            }
            finally
            {
                TryDeleteTemp(tempPath);
            }
        }

        private static void WriteAtomic(string destinationPath, string payload)
        {
            var tempPath = destinationPath + ".tmp";
            try
            {
                File.WriteAllText(tempPath, payload);
                if (File.Exists(destinationPath))
                {
                    try
                    {
                        File.Replace(tempPath, destinationPath, null);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Delete(destinationPath);
                        File.Move(tempPath, destinationPath);
                    }
                }
                else
                {
                    File.Move(tempPath, destinationPath);
                }
            }
            finally
            {
                TryDeleteTemp(tempPath);
            }
        }

        private static void TryDeleteTemp(string tempPath)
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
