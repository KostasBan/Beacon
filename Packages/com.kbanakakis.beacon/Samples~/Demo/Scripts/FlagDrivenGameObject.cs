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

        private void Start()
        {
            if (installer == null)
                installer = BeaconInstaller.Instance;

            if (installer != null)
            {
                installer.ClientReady += OnClientReady;

                // Bind immediately if already created.
                if (installer.Client != null)
                    OnClientReady(installer.Client);
            }

            ApplyFlag();
        }

        private void OnDestroy()
        {
            if (installer != null)
                installer.ClientReady -= OnClientReady;

            if (_client != null)
                _client.SnapshotChanged -= OnSnapshotChanged;
        }

        private void OnClientReady(BeaconClient client)
        {
            if (_client == client)
            {
                ApplyFlag();
                return;
            }

            if (_client != null)
                _client.SnapshotChanged -= OnSnapshotChanged;

            _client = client;

            if (_client != null)
                _client.SnapshotChanged += OnSnapshotChanged;

            ApplyFlag();
        }

        private void OnSnapshotChanged(RepositorySnapshot _)
        {
            ApplyFlag();
        }

        private void ApplyFlag()
        {
            if (target == null)
                return;

            if (_client == null || string.IsNullOrWhiteSpace(flagKey))
            {
                target.SetActive(false);
                return;
            }

            target.SetActive(_client.IsEnabled(flagKey));
        }
    }
}
