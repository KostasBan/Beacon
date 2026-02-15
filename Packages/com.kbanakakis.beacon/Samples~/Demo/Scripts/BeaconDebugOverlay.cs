using System;
using System.Reflection;
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

        private DefaultContextProvider _contextProvider;
        private BeaconClient _client;
        private string _lastRefreshResult = "Not refreshed yet";

        private void Awake()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(OnRefreshClicked);
            }
        }
        
        private void OnEnable()
        {
            if (installer == null) installer = BeaconInstaller.Instance;
            if (installer != null) installer.ClientReady += OnClientReady;

            // if already created, bind immediately
            if (installer != null && installer.Client != null) OnClientReady(installer.Client);

            Render();
        }
        
        private void EnsureClient()
        {
            if (installer == null) installer = BeaconInstaller.Instance;

            if (_client == null && installer != null)
                _client = installer.Client;

            if (_contextProvider == null && installer != null)
                _contextProvider = installer.ContextProvider;
        }

        private void OnDisable()
        {
            if (installer != null) installer.ClientReady -= OnClientReady;
            if (_client != null) _client.SnapshotChanged -= OnSnapshotChanged;

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(OnRefreshClicked);
            }
        }
        
        private void OnClientReady(BeaconClient client)
        {
            if (_client != null) _client.SnapshotChanged -= OnSnapshotChanged;
            _client = client;
            _client.SnapshotChanged += OnSnapshotChanged;
            _contextProvider = installer != null ? installer.ContextProvider : null;
            Render();
        }

        private void OnSnapshotChanged(RepositorySnapshot _)
        {
            Render();
        }

        private async void OnRefreshClicked()
        {
            EnsureClient();
            
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
            EnsureClient();
            
            if (_client == null)
            {
                SetText("Beacon client unavailable. Assign BeaconInstaller in the inspector.");
                return;
            }
            var snapshot = _client.GetSnapshot();
    
            if (_contextProvider == null)
            {
                SetText("Beacon context unavailable. Ensure BeaconInstaller is active.");
                return;
            }
            var context = _contextProvider.GetContext();
            
            var text = new StringBuilder();
            text.AppendLine("Beacon Debug Overlay");
            text.AppendLine($"configVersion: {snapshot.ConfigVersion ?? "(none)"}");
            text.AppendLine($"schemaVersion: {snapshot.SchemaVersion}");
            text.AppendLine($"provenance: {snapshot.Provenance}");
            text.AppendLine($"platform: {context.Platform}");
            text.AppendLine($"installId: {context.InstallId}");
            text.AppendLine($"last refresh result: {_lastRefreshResult}");
            SetText(text.ToString());
        }

        private void SetText(string value)
        {
            if (overlayText == null)
            {
                return;
            }
            overlayText.text = value;
        }
    }
}
