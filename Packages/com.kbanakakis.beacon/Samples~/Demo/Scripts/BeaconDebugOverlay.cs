using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Repositories;
using KBanakakis.Beacon.Unity;
using TMPro;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class BeaconDebugOverlay : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private TMP_Text overlayText;
        [SerializeField] private Button refreshButton;
        [SerializeField, Min(1)] private int installerLookupMaxFrames = 60;

        private readonly MonoBehaviourClientBinder _binder = new MonoBehaviourClientBinder();
        private DefaultContextProvider _contextProvider;
        private BeaconClient _client;
        private string _lastRefreshResult = "Not refreshed yet";

        private void OnEnable()
        {
            _binder.Start(
                this,
                () => installer ?? BeaconInstaller.Instance,
                installerLookupMaxFrames,
                inst => inst.Client,
                (inst, callback) => inst.ClientReady += callback,
                (inst, callback) => inst.ClientReady -= callback,
                inst =>
                {
                    installer = inst;
                    _contextProvider = inst.ContextProvider;
                },
                client =>
                {
                    _client = client;
                    Render();
                },
                _ => Render(),
                message => Debug.LogWarning($"[BeaconDemo] {message}"));

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(OnRefreshClicked);
                refreshButton.onClick.AddListener(OnRefreshClicked);
            }

            Render();
        }

        private void OnDisable()
        {
            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(OnRefreshClicked);

            _binder.Stop();
            _client = null;
            _contextProvider = null;
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
