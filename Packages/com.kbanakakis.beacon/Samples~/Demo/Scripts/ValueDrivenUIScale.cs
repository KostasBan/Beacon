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
            if (targetRect == null)
                return;

            if (_client == null || string.IsNullOrWhiteSpace(valueKey))
            {
                targetRect.localScale= new Vector3(defaultValue, defaultValue, 1);
                return;
            }

            float value = _client.GetFloat(valueKey, defaultValue);
            targetRect.localScale = new Vector3(value, value, 1) ;
        }
    }
}