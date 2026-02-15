using System;
using UnityEngine;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Evaluation;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class BeaconInstaller : MonoBehaviour
    {
        [SerializeField] private TextAsset configAsset;

        public static BeaconInstaller Instance { get; private set; }

        public BeaconClient Client { get; private set; }

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
            var contextProvider = new DefaultContextProvider();
            var evaluator = new JsonFlagEvaluator();
            Client = new BeaconClient(repo, contextProvider, evaluator);
        }
    }
}
