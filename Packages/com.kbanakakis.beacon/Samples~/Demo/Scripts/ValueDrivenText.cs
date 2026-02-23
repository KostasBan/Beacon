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

        private void OnEnable()
        {
            EnsureInstaller();

            if (installer != null)
            {
                installer.ClientReady -= OnClientReady;
                installer.ClientReady += OnClientReady;

                if (installer.Client != null)
                    BindClient(installer.Client);
            }

            ApplyValue();
        }

        private void OnDisable()
        {
            if (installer != null)
                installer.ClientReady -= OnClientReady;

            UnbindClient();
        }

        private void OnClientReady(BeaconClient client)
        {
            BindClient(client);
            ApplyValue();
        }

        private void OnSnapshotChanged(RepositorySnapshot _)
        {
            ApplyValue();
        }

        private void EnsureInstaller()
        {
            if (installer == null)
                installer = BeaconInstaller.Instance;
        }

        private void BindClient(BeaconClient client)
        {
            if (_client == client)
                return;

            UnbindClient();
            _client = client;

            if (_client != null)
                _client.SnapshotChanged += OnSnapshotChanged;
        }

        private void UnbindClient()
        {
            if (_client == null)
                return;

            _client.SnapshotChanged -= OnSnapshotChanged;
            _client = null;
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
