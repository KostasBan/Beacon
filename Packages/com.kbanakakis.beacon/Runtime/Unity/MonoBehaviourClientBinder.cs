#nullable enable
using System;
using System.Collections;
using KBanakakis.Beacon.Repositories;
using UnityEngine;

namespace KBanakakis.Beacon.Unity
{
    /// <summary>
    /// Unity-friendly binder that:
    /// - resolves an installer (inspector ref or singleton) even if enabled before installer Awake
    /// - binds to ClientReady and SnapshotChanged
    /// - cleans up safely on Stop/OnDisable
    /// </summary>
    public sealed class MonoBehaviourClientBinder
    {
        private MonoBehaviour? _owner;
        private Coroutine? _routine;
        private bool _warned;
        private int _maxFrames;

        private BeaconClient? _client;

        private Func<object?>? _installerProvider;
        private Func<object, BeaconClient?>? _clientProvider;
        private Action<object, Action<BeaconClient>>? _subscribeClientReady;
        private Action<object, Action<BeaconClient>>? _unsubscribeClientReady;
        private Action<object>? _onInstallerReady;
        private Action<BeaconClient?>? _onClientReady;
        private Action<RepositorySnapshot>? _onSnapshotChanged;
        private Action<string>? _onWarning;

        private object? _installer;

        public void Start<TInstaller>(
            MonoBehaviour owner,
            Func<TInstaller?> installerProvider,
            int maxFrames,
            Func<TInstaller, BeaconClient?> clientProvider,
            Action<TInstaller, Action<BeaconClient>> subscribeClientReady,
            Action<TInstaller, Action<BeaconClient>> unsubscribeClientReady,
            Action<TInstaller>? onInstallerReady,
            Action<BeaconClient?> onClientReady,
            Action<RepositorySnapshot> onSnapshotChanged,
            Action<string> onWarning)
            where TInstaller : class
        {
            Stop();

            _owner = owner;
            _maxFrames = Mathf.Max(1, maxFrames);
            _warned = false;

            _installerProvider = () => installerProvider();
            _clientProvider = inst => clientProvider((TInstaller)inst);
            _subscribeClientReady = (inst, cb) => subscribeClientReady((TInstaller)inst, cb);
            _unsubscribeClientReady = (inst, cb) => unsubscribeClientReady((TInstaller)inst, cb);
            _onInstallerReady = inst => onInstallerReady?.Invoke((TInstaller)inst);
            _onClientReady = onClientReady;
            _onSnapshotChanged = onSnapshotChanged;
            _onWarning = onWarning;

            TryResolveInstaller();
            if (HookInstallerAndClientIfReady())
                return;

            _routine = _owner.StartCoroutine(BindWhenInstallerReady());
        }

        public void Stop()
        {
            if (_routine != null && _owner != null)
            {
                _owner.StopCoroutine(_routine);
            }

            _routine = null;

            UnbindClient();
            UnhookInstaller();

            _owner = null;

            _installerProvider = null;
            _clientProvider = null;
            _subscribeClientReady = null;
            _unsubscribeClientReady = null;
            _onInstallerReady = null;
            _onClientReady = null;
            _onSnapshotChanged = null;
            _onWarning = null;

            _warned = false;
            _maxFrames = 0;
        }

        private IEnumerator BindWhenInstallerReady()
        {
            for (var frame = 0; frame < _maxFrames; frame++)
            {
                TryResolveInstaller();
                if (HookInstallerAndClientIfReady())
                {
                    _routine = null;
                    yield break;
                }

                yield return null;
            }

            if (!_warned)
            {
                _warned = true;
                _onWarning?.Invoke($"[Beacon] {nameof(MonoBehaviourClientBinder)} could not resolve installer after {_maxFrames} frames.");
            }

            _routine = null;
        }

        private void TryResolveInstaller()
        {
            if (_installer != null || _installerProvider == null)
                return;

            _installer = _installerProvider();
        }

        private bool HookInstallerAndClientIfReady()
        {
            if (_installer == null)
                return false;

            // Subscribe to ClientReady exactly once.
            _unsubscribeClientReady?.Invoke(_installer, OnClientReadyInternal);
            _subscribeClientReady?.Invoke(_installer, OnClientReadyInternal);

            _onInstallerReady?.Invoke(_installer);

            var existingClient = _clientProvider?.Invoke(_installer);
            if (existingClient != null)
            {
                BindClient(existingClient);
            }

            return true;
        }

        private void UnhookInstaller()
        {
            if (_installer == null)
                return;

            _unsubscribeClientReady?.Invoke(_installer, OnClientReadyInternal);
            _installer = null;
        }

        private void OnClientReadyInternal(BeaconClient client)
        {
            BindClient(client);
        }

        private void BindClient(BeaconClient client)
        {
            if (_client == client)
            {
                _onClientReady?.Invoke(_client);
                return;
            }

            UnbindClient();
            _client = client;

            _client.SnapshotChanged += OnSnapshotChangedInternal;

            _onClientReady?.Invoke(_client);
        }

        private void UnbindClient()
        {
            if (_client == null)
                return;

            _client.SnapshotChanged -= OnSnapshotChangedInternal;
            _client = null;
        }

        private void OnSnapshotChangedInternal(RepositorySnapshot snapshot)
        {
            _onSnapshotChanged?.Invoke(snapshot);
        }
    }
}