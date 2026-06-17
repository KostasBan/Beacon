using KostasBan.Beacon.Repositories;
using KostasBan.Beacon.Unity;
using UnityEngine;

namespace KostasBan.Beacon.Samples.Demo
{
    public sealed class FlagDrivenGameObject : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private string flagKey = "new_home_ui";
        [SerializeField] private GameObject target;
        [SerializeField, Min(1)] private int installerLookupMaxFrames = 60;

        private readonly MonoBehaviourClientBinder _binder = new MonoBehaviourClientBinder();
        private BeaconClient _client;
        private bool _warnedAboutSelfTarget;

        private void OnEnable()
        {
            _binder.Start(
                this,
                () => installer ?? BeaconInstaller.Instance,
                installerLookupMaxFrames,
                inst => inst.Client,
                (inst, callback) => inst.ClientReady += callback,
                (inst, callback) => inst.ClientReady -= callback,
                inst => installer = inst,
                client =>
                {
                    _client = client;
                    ApplyFlag();
                },
                _ => ApplyFlag(),
                message => Debug.LogWarning($"[BeaconDemo] {message}"));

            ApplyFlag();
        }

        private void OnDisable()
        {
            _binder.Stop();
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
