using System;
using System.Threading;
using System.Threading.Tasks;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Evaluation;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon
{
    /// <summary>
    /// Entry point for accessing Beacon configuration data.
    /// </summary>
    public sealed class BeaconClient
    {
        private readonly IConfigRepository _repository;
        private readonly IContextProvider _contextProvider;
        private readonly IFlagEvaluator _flagEvaluator;

        public BeaconClient(IConfigRepository repository, IContextProvider contextProvider, IFlagEvaluator flagEvaluator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
            _flagEvaluator = flagEvaluator ?? throw new ArgumentNullException(nameof(flagEvaluator));
        }

        public RepositorySnapshot GetSnapshot()
        {
            return _repository.GetSnapshot();
        }

        public bool IsEnabled(string flagKey, bool defaultValue = false)
        {
            var snapshot = _repository.GetSnapshot();
            if (snapshot.RawBytes == null)
            {
                return defaultValue;
            }

            return _flagEvaluator.IsEnabled(snapshot.RawBytes, _contextProvider.GetContext(), flagKey, defaultValue);
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
