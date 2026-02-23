#nullable enable
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Evaluation;
using KBanakakis.Beacon.Repositories;
using KBanakakis.Beacon.Values;

namespace KBanakakis.Beacon
{
    /// <summary>
    /// Entry point for accessing Beacon configuration data.
    /// </summary>
    public sealed class BeaconClient
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

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

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (!TryGetConfigValues(out var configValues))
            {
                return defaultValue;
            }

            return configValues.TryGetBool(key, out var value) ? value : defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (!TryGetConfigValues(out var configValues))
            {
                return defaultValue;
            }

            return configValues.TryGetInt(key, out var value) ? value : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (!TryGetConfigValues(out var configValues))
            {
                return defaultValue;
            }

            return configValues.TryGetFloat(key, out var value) ? value : defaultValue;
        }

        public string? GetString(string key, string? defaultValue = null)
        {
            if (!TryGetConfigValues(out var configValues))
            {
                return defaultValue;
            }

            return configValues.TryGetString(key, out var value) ? value : defaultValue;
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

        private bool TryGetConfigValues(out JsonConfigValues configValues)
        {
            // Satisfy compiler: always assign.
            configValues = default!;

            var snapshot = _repository.GetSnapshot();
            if (snapshot.RawBytes == null)
            {
                return false;
            }

            try
            {
                var json = StrictUtf8.GetString(snapshot.RawBytes);
                configValues = new JsonConfigValues(json);
                return true;
            }
            catch (DecoderFallbackException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}