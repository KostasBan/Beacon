using System.Threading;
using System.Threading.Tasks;

namespace KBanakakis.Beacon.Repositories
{
    public interface IConfigStore
    {
        bool TryLoad(out StoredConfig stored);

        Task StoreAsync(StoredConfig stored, CancellationToken ct);
    }

    public readonly struct StoredConfig
    {
        public StoredConfig(byte[] bytes, string? storedVersion)
        {
            Bytes = bytes;
            StoredVersion = storedVersion;
        }

        public byte[] Bytes { get; }

        public string? StoredVersion { get; }
    }
}
