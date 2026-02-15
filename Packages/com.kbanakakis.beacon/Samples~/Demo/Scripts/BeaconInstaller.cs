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

        public static BeaconInstaller Instance { get; private set; }

        public BeaconClient Client { get; private set; }
        
        public event Action<BeaconClient> ClientReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CreateClient();
        }

        private void CreateClient()
        {
            if (configAsset == null)
            {
                throw new InvalidOperationException("BeaconInstaller requires a config TextAsset.");
            }

            var source = new EmbeddedTextAssetConfigSource(configAsset);
            var store = new DiskConfigStore();
            var validator = new BasicConfigValidator();
            var repo = new DefaultConfigRepository(source, store, validator);
            ContextProvider = new DefaultContextProvider();
            var evaluator = new JsonFlagEvaluator();
            Client = new BeaconClient(repo, ContextProvider, evaluator);
            ClientReady?.Invoke(Client);
            // Kick initial refresh so snapshot has RawBytes and flags can evaluate immediately.
            _ = Client.RefreshAsync(CancellationToken.None);
        }
    }
}
