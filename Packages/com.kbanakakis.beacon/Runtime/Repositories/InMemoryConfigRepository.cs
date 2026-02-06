using System;
using System.Threading;
using System.Threading.Tasks;

namespace KBanakakis.Beacon.Repositories
{
    /// <summary>
    /// Minimal in-memory implementation of <see cref="IConfigRepository"/>.
    /// </summary>
    public sealed class InMemoryConfigRepository : IConfigRepository
    {
        private RepositorySnapshot _snapshot;

        public InMemoryConfigRepository(RepositorySnapshot snapshot)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        }

        public event Action<RepositorySnapshot>? SnapshotChanged;

        public RepositorySnapshot GetSnapshot()
        {
            return _snapshot;
        }

        public Task<RefreshResult> RefreshAsync(CancellationToken ct)
        {
            return Task.FromResult(new RefreshResult(false, null));
        }

        public void SetSnapshotForTests(RepositorySnapshot snapshot)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            SnapshotChanged?.Invoke(_snapshot);
        }
    }
}
