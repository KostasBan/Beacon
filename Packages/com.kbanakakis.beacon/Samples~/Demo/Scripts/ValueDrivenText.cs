using TMPro;
using UnityEngine;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class ValueDrivenText : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private string valueKey = "ui_home_title";
        [SerializeField] private string defaultValue = "Welcome";

        private BeaconClient _client;

        private void Start()
        {
            if (installer == null)
                installer = BeaconInstaller.Instance;

            if (installer != null)
            {
                installer.ClientReady += OnClientReady;

                if (installer.Client != null)
                    OnClientReady(installer.Client);
            }

            ApplyValue();
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
                ApplyValue();
                return;
            }

            if (_client != null)
                _client.SnapshotChanged -= OnSnapshotChanged;

            _client = client;

            if (_client != null)
                _client.SnapshotChanged += OnSnapshotChanged;

            ApplyValue();
        }

        private void OnSnapshotChanged(RepositorySnapshot _)
        {
            ApplyValue();
        }

        private void ApplyValue()
        {
            if (targetText == null)
                return;

            if (_client == null || string.IsNullOrWhiteSpace(valueKey))
            {
                targetText.text = defaultValue;
                return;
            }

            targetText.text = _client.GetString(valueKey, defaultValue);
        }
    }
}
