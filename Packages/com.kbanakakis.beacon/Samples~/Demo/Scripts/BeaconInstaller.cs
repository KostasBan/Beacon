using System;
using System.Threading;
using UnityEngine;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Evaluation;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class BeaconInstaller : MonoBehaviour
    {
        [SerializeField] private TextAsset configAsset;

        public DefaultContextProvider ContextProvider { get; private set; }
        public BeaconClient Client { get; private set; }

        public static BeaconInstaller Instance { get; private set; }

        /// <summary>
        /// Fired once the BeaconClient has been created.
        /// Useful for demo components that may enable before the installer Awake runs.
        /// </summary>
        public event Action<BeaconClient> ClientReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            try
            {
                CreateClient();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BeaconDemo] Failed to create BeaconClient: {ex}");
            }
        }

        private void CreateClient()
        {
            if (configAsset == null)
                throw new InvalidOperationException("BeaconInstaller requires a config TextAsset.");

            var source = new EmbeddedTextAssetConfigSource(configAsset);
            var store = new DiskConfigStore();
            var validator = new BasicConfigValidator();
            var repo = new DefaultConfigRepository(source, store, validator);

            ContextProvider = new DefaultContextProvider();
            var evaluator = new JsonFlagEvaluator();

            Client = new BeaconClient(repo, ContextProvider, evaluator);

            // Notify listeners first so they can subscribe to SnapshotChanged.
            ClientReady?.Invoke(Client);

            // Kick initial refresh so snapshot has RawBytes and flags can evaluate immediately.
            _ = Client.RefreshAsync(CancellationToken.None);

            Debug.Log($"[BeaconDemo] BeaconClient created. ConfigAsset={configAsset.name}");
        }
    }
}
