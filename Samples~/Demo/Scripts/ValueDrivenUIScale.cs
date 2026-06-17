using KostasBan.Beacon.Repositories;
using KostasBan.Beacon.Unity;
using UnityEngine;

namespace KostasBan.Beacon.Samples.Demo
{
    public sealed class ValueDrivenUIScale : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private string valueKey = "ui_scale";
        [SerializeField] private float defaultValue = 1f;
        [SerializeField, Min(1)] private int installerLookupMaxFrames = 60;

        private readonly MonoBehaviourClientBinder _binder = new MonoBehaviourClientBinder();
        private BeaconClient _client;

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
                    ApplyValue();
                },
                _ => ApplyValue(),
                message => Debug.LogWarning($"[BeaconDemo] {message}"));

            ApplyValue();
        }

        private void OnDisable()
        {
            _binder.Stop();
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
