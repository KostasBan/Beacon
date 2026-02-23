using System;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Repositories;
using TMPro;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class BeaconDebugOverlay : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private TMP_Text overlayText;
        [SerializeField] private Button refreshButton;
        [SerializeField, Min(1)] private int installerLookupMaxFrames = 60;

        private DefaultContextProvider _contextProvider;
        private BeaconClient _client;
        private Coroutine _bindRoutine;
        private string _lastRefreshResult = "Not refreshed yet";
        private bool _warnedMissingInstaller;

        private void OnEnable()
        {
            StartBindFlow();

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(OnRefreshClicked);
                refreshButton.onClick.AddListener(OnRefreshClicked);
            }

            Render();
        }

        private void OnDisable()
        {
            StopBindRoutine();

            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(OnRefreshClicked);

            if (installer != null)
                installer.ClientReady -= OnClientReady;

            UnbindClient();
        }

        private void OnClientReady(BeaconClient client)
        {
            _contextProvider = installer != null ? installer.ContextProvider : null;
            BindClient(client);
            Render();
        }

        private void OnSnapshotChanged(RepositorySnapshot _)
        {
            Render();
        }

        private async void OnRefreshClicked()
        {
            if (_client == null)
            {
                _lastRefreshResult = "Refresh failed: Beacon client not available.";
                Render();
                return;
            }

            try
            {
                var result = await _client.RefreshAsync(CancellationToken.None);
                _lastRefreshResult = result.Error == null
                    ? $"Refresh succeeded (changed={result.Changed})"
                    : $"Refresh failed: {result.Error}";
            }
            catch (Exception ex)
            {
                _lastRefreshResult = $"Refresh exception: {ex.Message}";
            }

            Render();
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
                    Render();
                    yield break;
                }

                yield return null;
            }

            if (!_warnedMissingInstaller)
            {
                _warnedMissingInstaller = true;
                Debug.LogWarning($"[BeaconDemo] {nameof(BeaconDebugOverlay)} could not find {nameof(BeaconInstaller)} after {installerLookupMaxFrames} frames.");
            }

            _bindRoutine = null;
            Render();
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
            _contextProvider = installer.ContextProvider;

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

        private void Render()
        {
            if (overlayText == null)
                return;

            if (_client == null)
            {
                overlayText.text = "Beacon client unavailable. Ensure BeaconInstaller is active.";
                return;
            }

            if (_contextProvider == null)
            {
                overlayText.text = "Beacon context unavailable. Ensure BeaconInstaller is active.";
                return;
            }

            var snapshot = _client.GetSnapshot();
            var context = _contextProvider.GetContext();

            var sb = new StringBuilder();
            sb.AppendLine("Beacon Debug Overlay");
            sb.AppendLine($"configVersion: {snapshot.ConfigVersion ?? "(none)"}");
            sb.AppendLine($"schemaVersion: {snapshot.SchemaVersion}");
            sb.AppendLine($"provenance: {snapshot.Provenance}");
            sb.AppendLine($"platform: {context.Platform}");
            sb.AppendLine($"installId: {context.InstallId}");
            sb.AppendLine($"last refresh result: {_lastRefreshResult}");

            overlayText.text = sb.ToString();
        }
    }
}
