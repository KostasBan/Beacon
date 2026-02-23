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
        private bool _warnedAboutSelfTarget;

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

            ApplyFlag();
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
            ApplyFlag();
        }

        private void OnSnapshotChanged(RepositorySnapshot _)
        {
            ApplyFlag();
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

        private void ApplyFlag()
        {
            if (target == null)
                return;

            var shouldBeActive = _client != null
                && !string.IsNullOrWhiteSpace(flagKey)
                && _client.IsEnabled(flagKey);

            if (ReferenceEquals(target, gameObject) && !shouldBeActive)
            {
                if (!_warnedAboutSelfTarget)
                {
                    _warnedAboutSelfTarget = true;
                    Debug.LogWarning("[BeaconDemo] FlagDrivenGameObject target is the same GameObject as this component. " +
                                     "Attach this script to an always-active GameObject (for example DemoRoot) to avoid lifecycle issues.");
                }

                return;
            }

            if (target.activeSelf != shouldBeActive)
                target.SetActive(shouldBeActive);
        }
    }
}
