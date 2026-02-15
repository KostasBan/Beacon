using System;
using System.Threading;
using System.Threading.Tasks;

namespace KBanakakis.Beacon.Repositories
{
    public sealed class DefaultConfigRepository : IConfigRepository
    {
        private readonly IConfigSource _source;
        private readonly IConfigStore _store;
        private readonly IConfigValidator _validator;
        private RepositorySnapshot _snapshot;

        public DefaultConfigRepository(IConfigSource source, IConfigStore store, IConfigValidator validator)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));

            if (_store.TryLoad(out var stored))
            {
                _snapshot = new RepositorySnapshot(
                    stored.StoredVersion,
                    0,
                    DateTimeOffset.UtcNow,
                    RepositorySnapshot.ConfigProvenance.LastKnownGood,
                    stored.Bytes);
            }
            else
            {
                _snapshot = new RepositorySnapshot(
                    null,
                    0,
                    DateTimeOffset.UtcNow,
                    RepositorySnapshot.ConfigProvenance.None,
                    null);
            }
        }

        public event Action<RepositorySnapshot>? SnapshotChanged;

        public RepositorySnapshot GetSnapshot()
        {
            return _snapshot;
        }

        public async Task<RefreshResult> RefreshAsync(CancellationToken ct)
        {
            var fetchResult = await _source.FetchAsync(ct).ConfigureAwait(false);
            if (!fetchResult.Success || fetchResult.Bytes == null)
            {
                return new RefreshResult(false, fetchResult.Error ?? "Failed to fetch config.");
            }

            var validation = _validator.Validate(fetchResult.Bytes);
            if (!validation.IsValid)
            {
                return new RefreshResult(false, validation.Error ?? "Config validation failed.");
            }

            await _store.StoreAsync(new StoredConfig(fetchResult.Bytes, validation.ConfigVersion), ct)
                .ConfigureAwait(false);

            _snapshot = new RepositorySnapshot(
                validation.ConfigVersion,
                validation.SchemaVersion,
                DateTimeOffset.UtcNow,
                RepositorySnapshot.ConfigProvenance.Remote,
                fetchResult.Bytes);
            SnapshotChanged?.Invoke(_snapshot);

            return new RefreshResult(true, null);
        }
    }
}
