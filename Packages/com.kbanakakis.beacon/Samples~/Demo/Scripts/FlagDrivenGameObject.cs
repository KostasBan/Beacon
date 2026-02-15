using UnityEngine;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class FlagDrivenGameObject : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private string flagKey = "new_home_ui";
        [SerializeField] private GameObject target;

        private BeaconClient _client;

        private void Awake()
        {
            if (installer == null)
            {
                installer = BeaconInstaller.Instance;
            }

            _client = installer != null ? installer.Client : null;
        }

        private void OnEnable()
        {
            if (_client != null)
            {
                _client.SnapshotChanged += OnSnapshotChanged;
            }

            ApplyFlag();
        }

        private void OnDisable()
        {
            if (_client != null)
            {
                _client.SnapshotChanged -= OnSnapshotChanged;
            }
        }

        private void OnSnapshotChanged(RepositorySnapshot _)
        {
            ApplyFlag();
        }

        private void Start()
        {
            ApplyFlag();
        }

        private void ApplyFlag()
        {
            if (target == null)
            {
                return;
            }

            if (_client == null || string.IsNullOrWhiteSpace(flagKey))
            {
                target.SetActive(false);
                return;
            }

            target.SetActive(_client.IsEnabled(flagKey));
        }
    }
}
