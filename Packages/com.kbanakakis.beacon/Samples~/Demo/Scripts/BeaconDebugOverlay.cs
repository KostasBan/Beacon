using System;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon.Samples.Demo
{
    public sealed class BeaconDebugOverlay : MonoBehaviour
    {
        [SerializeField] private BeaconInstaller installer;
        [SerializeField] private Component overlayText;
        [SerializeField] private Button refreshButton;

        private DefaultContextProvider _contextProvider;
        private BeaconClient _client;
        private string _lastRefreshResult = "Not refreshed yet";

        private void Awake()
        {
            if (installer == null)
            {
                installer = BeaconInstaller.Instance;
            }

            _contextProvider = new DefaultContextProvider();
            _client = installer != null ? installer.Client : null;

            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(OnRefreshClicked);
            }
        }

        private void OnEnable()
        {
            if (_client != null)
            {
                _client.SnapshotChanged += OnSnapshotChanged;
            }

            Render();
        }

        private void OnDisable()
        {
            if (_client != null)
            {
                _client.SnapshotChanged -= OnSnapshotChanged;
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(OnRefreshClicked);
            }
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

        private void Render()
        {
            if (_client == null)
            {
                SetText("Beacon client unavailable. Assign BeaconInstaller in the inspector.");
                return;
            }

            var snapshot = _client.GetSnapshot();
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

            if (overlayText is Text uiText)
            {
                uiText.text = value;
                return;
            }

            var tmpType = Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro");
            if (tmpType == null || !tmpType.IsInstanceOfType(overlayText))
            {
                return;
            }

            var property = tmpType.GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
            property?.SetValue(overlayText, value);
        }
    }
}
