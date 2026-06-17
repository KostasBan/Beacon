using System;
using System.Threading;
using System.Threading.Tasks;

namespace KostasBan.Beacon.Repositories
{
    /// <summary>
    /// Abstraction for loading configuration data.
    /// </summary>
    public interface IConfigRepository
    {
        RepositorySnapshot GetSnapshot();

        event Action<RepositorySnapshot> SnapshotChanged;

        Task<RefreshResult> RefreshAsync(CancellationToken ct);
    }

    public readonly struct RefreshResult
    {
        public RefreshResult(bool changed, string? error)
        {
            Changed = changed;
            Error = error;
        }

        public bool Changed { get; }

        public string? Error { get; }
    }
}
