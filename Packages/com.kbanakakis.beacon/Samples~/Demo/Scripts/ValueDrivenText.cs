using System.Collections;
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
        [SerializeField, Min(1)] private int installerLookupMaxFrames = 60;

        private BeaconClient _client;
        private Coroutine _bindRoutine;
        private bool _warnedMissingInstaller;

        private void OnEnable()
        {
            StartBindFlow();
            ApplyValue();
        }

        private void OnDisable()
        {
            StopBindRoutine();

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

        private void StartBindFlow()
        {
            StopBindRoutine();
            EnsureInstaller();

            if (HookInstallerAndBindIfReady())
                return;

            _bindRoutine = StartCoroutine(BindWhenInstallerReady());
        }

        private IEnumerator BindWhenInstallerReady()
        {
            for (var frame = 0; frame < installerLookupMaxFrames && installer == null; frame++)
            {
                EnsureInstaller();
                if (HookInstallerAndBindIfReady())
                {
                    _bindRoutine = null;
                    yield break;
                }

                yield return null;
            }

            if (!_warnedMissingInstaller)
            {
                _warnedMissingInstaller = true;
                Debug.LogWarning($"[BeaconDemo] {nameof(ValueDrivenText)} could not find {nameof(BeaconInstaller)} after {installerLookupMaxFrames} frames.");
            }

            _bindRoutine = null;
        }

        private void StopBindRoutine()
        {
            if (_bindRoutine == null)
                return;

            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        private void EnsureInstaller()
        {
            if (installer == null)
                installer = BeaconInstaller.Instance;
        }

        private bool HookInstallerAndBindIfReady()
        {
            if (installer == null)
                return false;

            installer.ClientReady -= OnClientReady;
            installer.ClientReady += OnClientReady;

            if (installer.Client != null)
                BindClient(installer.Client);

            return true;
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
