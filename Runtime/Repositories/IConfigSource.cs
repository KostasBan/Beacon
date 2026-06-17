using System.Threading;
using System.Threading.Tasks;

namespace KostasBan.Beacon.Repositories
{
    public interface IConfigSource
    {
        Task<ConfigSourceResult> FetchAsync(CancellationToken ct);
    }

    public readonly struct ConfigSourceResult
    {
        public ConfigSourceResult(bool success, byte[]? bytes, string? sourceVersion, string? error)
        {
            Success = success;
            Bytes = bytes;
            SourceVersion = sourceVersion;
            Error = error;
        }

        public bool Success { get; }

        public byte[]? Bytes { get; }

        public string? SourceVersion { get; }

        public string? Error { get; }
    }
}
