using TMPro;
using UnityEngine;
using KBanakakis.Beacon.Repositories;
using KBanakakis.Beacon.Unity;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class ValueDrivenText : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private string valueKey = "ui_home_title";
        [SerializeField] private string defaultValue = "Welcome";
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
