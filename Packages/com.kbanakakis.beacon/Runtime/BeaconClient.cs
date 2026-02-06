using System;
using System.Threading;
using System.Threading.Tasks;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon
{
    /// <summary>
    /// Entry point for accessing Beacon configuration data.
    /// </summary>
    public sealed class BeaconClient
    {
        private readonly IConfigRepository _repository;

        public BeaconClient(IConfigRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public RepositorySnapshot GetSnapshot()
        {
            return _repository.GetSnapshot();
        }

        public event Action<RepositorySnapshot> SnapshotChanged
        {
            add => _repository.SnapshotChanged += value;
            remove => _repository.SnapshotChanged -= value;
        }

        public Task<RefreshResult> RefreshAsync(CancellationToken ct)
        {
            return _repository.RefreshAsync(ct);
        }
    }
}
