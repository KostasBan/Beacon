using KBanakakis.Beacon.Repositories;
using UnityEngine;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class ValueDrivenUIScale : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private string valueKey = "ui_scale";
        [SerializeField] private float defaultValue = 1f;

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
            if (targetRect == null)
                return;

            if (_client == null || string.IsNullOrWhiteSpace(valueKey))
            {
                targetRect.localScale = new Vector3(defaultValue, defaultValue, 1f);
                return;
            }

            var value = _client.GetFloat(valueKey, defaultValue);
            targetRect.localScale = new Vector3(value, value, 1f);
        }
    }
}
