#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KostasBan.Beacon.Context;
using KostasBan.Beacon.Repositories;
using UnityEditor;
using UnityEngine;

namespace KostasBan.Beacon.Editor
{
    public sealed class BeaconDebugWindow : EditorWindow
    {
        private enum ValueType
        {
            Bool,
            Int,
            Float,
            String
        }

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        private MonoBehaviour? _clientSource;
        private BeaconClient? _resolvedClient;
        private BeaconContext? _resolvedContext;

        private Task<RefreshResult>? _refreshTask;
        private string _lastRefreshStatus = "Not run.";

        private string _jsonInput = "";
        private string _jsonValidationResult = "Paste JSON and click Validate.";

        private string _flagKey = "";
        private bool _flagDefault;
        private string _flagResult = "-";

        private string _valueKey = "";
        private ValueType _valueType;
        private bool _defaultBool;
        private int _defaultInt;
        private float _defaultFloat;
        private string _defaultString = "";
        private string _valueResult = "-";

        [MenuItem("Tools/Beacon/Debug Window")]
        public static void Open()
        {
            GetWindow<BeaconDebugWindow>("Beacon Debug");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                _resolvedClient = null;
                _resolvedContext = null;
                _refreshTask = null;
                _lastRefreshStatus = "Cleared (not in play mode).";
            }

            Repaint();
        }

        private void Update()
        {
            if (_refreshTask == null || !_refreshTask.IsCompleted)
            {
                return;
            }

            if (_refreshTask.IsFaulted)
            {
                _lastRefreshStatus = _refreshTask.Exception?.GetBaseException().Message ?? "Refresh failed.";
            }
            else if (_refreshTask.IsCanceled)
            {
                _lastRefreshStatus = "Refresh canceled.";
            }
            else
            {
                var result = _refreshTask.Result;
                _lastRefreshStatus = $"Changed: {result.Changed}, Error: {result.Error ?? "<none>"}";
            }

            _refreshTask = null;
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Beacon Debug Window", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawClientSourcePanel();
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect runtime client.", MessageType.Info);
                return;
            }

            ResolveClientAndContext();
            DrawRuntimePanels();
        }

        private void DrawClientSourcePanel()
        {
            EditorGUILayout.LabelField("Client Source", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _clientSource = (MonoBehaviour?)EditorGUILayout.ObjectField("MonoBehaviour", _clientSource, typeof(MonoBehaviour), true);
            if (GUILayout.Button("Auto-find", GUILayout.Width(90)))
            {
                AutoFindClientSource();
            }

            EditorGUILayout.EndHorizontal();

            var sourceLabel = _clientSource == null
                ? "None selected."
                : $"Selected: {_clientSource.GetType().Name} on {_clientSource.gameObject.name}";
            EditorGUILayout.LabelField(sourceLabel, EditorStyles.miniLabel);
        }

        private void DrawRuntimePanels()
        {
            if (_resolvedClient == null)
            {
                EditorGUILayout.HelpBox(
                    "No BeaconClient found. Click Auto-find to search active scene objects, or drag a MonoBehaviour exposing a BeaconClient (Client field/property).",
                    MessageType.Warning);
                return;
            }

            DrawRefreshPanel();
            EditorGUILayout.Space();

            DrawSnapshotPanel(_resolvedClient.GetSnapshot());
            EditorGUILayout.Space();

            DrawContextPanel();
            EditorGUILayout.Space();

            DrawJsonValidationPanel();
            EditorGUILayout.Space();

            DrawFlagPanel();
            EditorGUILayout.Space();

            DrawValuesPanel();
        }

        private void DrawRefreshPanel()
        {
            EditorGUILayout.LabelField("Refresh", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_refreshTask != null))
            {
                if (GUILayout.Button("Refresh"))
                {
                    _lastRefreshStatus = "Refreshing...";
                    _refreshTask = _resolvedClient!.RefreshAsync(CancellationToken.None);
                }
            }

            EditorGUILayout.LabelField("Last Result", _lastRefreshStatus);
        }

        private static void DrawSnapshotPanel(RepositorySnapshot snapshot)
        {
            EditorGUILayout.LabelField("RepositorySnapshot", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("configVersion", snapshot.ConfigVersion ?? "<null>");
            EditorGUILayout.LabelField("schemaVersion", snapshot.SchemaVersion.ToString());
            EditorGUILayout.LabelField("provenance", snapshot.Provenance.ToString());
            EditorGUILayout.LabelField("retrievedAtUtc", snapshot.RetrievedAtUtc.ToString("O"));
            EditorGUILayout.LabelField("rawBytes length", snapshot.RawBytes?.Length.ToString() ?? "<null>");
        }

        private void DrawContextPanel()
        {
            EditorGUILayout.LabelField("BeaconContext", EditorStyles.boldLabel);
            if (_resolvedContext.HasValue)
            {
                var context = _resolvedContext.Value;
                EditorGUILayout.LabelField("platform", context.Platform);
                EditorGUILayout.LabelField("installId", context.InstallId);
                EditorGUILayout.LabelField("appVersion", context.AppVersion);
            }
            else
            {
                EditorGUILayout.HelpBox("Context provider was not found on selected source object.", MessageType.None);
            }
        }

        private void DrawJsonValidationPanel()
        {
            EditorGUILayout.LabelField("JSON Validation", EditorStyles.boldLabel);
            _jsonInput = EditorGUILayout.TextArea(_jsonInput, GUILayout.MinHeight(90));

            if (GUILayout.Button("Validate JSON"))
            {
                _jsonValidationResult = ValidateJson(_jsonInput);
            }

            EditorGUILayout.HelpBox(_jsonValidationResult, MessageType.None);
        }

        private static string ValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "No JSON input.";
            }

            var validator = new BasicConfigValidator();
            var bytes = StrictUtf8.GetBytes(json);
            var result = validator.Validate(bytes);
            if (!result.IsValid)
            {
                return $"Invalid JSON/config. Error: {result.Error ?? "Unknown error."}";
            }

            return $"Valid. schemaVersion={result.SchemaVersion}, configVersion={result.ConfigVersion ?? "<null>"}";
        }

        private void DrawFlagPanel()
        {
            EditorGUILayout.LabelField("Flag Evaluation", EditorStyles.boldLabel);
            _flagKey = EditorGUILayout.TextField("flagKey", _flagKey);
            _flagDefault = EditorGUILayout.Toggle("defaultValue", _flagDefault);

            if (GUILayout.Button("Evaluate IsEnabled"))
            {
                _flagResult = string.IsNullOrWhiteSpace(_flagKey)
                    ? "Provide a flagKey."
                    : _resolvedClient!.IsEnabled(_flagKey, _flagDefault).ToString();
            }

            EditorGUILayout.LabelField("Result", _flagResult);
        }

        private void DrawValuesPanel()
        {
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            _valueKey = EditorGUILayout.TextField("key", _valueKey);
            _valueType = (ValueType)EditorGUILayout.EnumPopup("type", _valueType);

            switch (_valueType)
            {
                case ValueType.Bool:
                    _defaultBool = EditorGUILayout.Toggle("default", _defaultBool);
                    break;
                case ValueType.Int:
                    _defaultInt = EditorGUILayout.IntField("default", _defaultInt);
                    break;
                case ValueType.Float:
                    _defaultFloat = EditorGUILayout.FloatField("default", _defaultFloat);
                    break;
                case ValueType.String:
                    _defaultString = EditorGUILayout.TextField("default", _defaultString);
                    break;
            }

            if (GUILayout.Button("Get Value"))
            {
                _valueResult = EvaluateValue();
            }

            EditorGUILayout.LabelField("Result", _valueResult);
        }

        private string EvaluateValue()
        {
            if (string.IsNullOrWhiteSpace(_valueKey))
            {
                return "Provide a key.";
            }

            return _valueType switch
            {
                ValueType.Bool => _resolvedClient!.GetBool(_valueKey, _defaultBool).ToString(),
                ValueType.Int => _resolvedClient!.GetInt(_valueKey, _defaultInt).ToString(),
                ValueType.Float => _resolvedClient!.GetFloat(_valueKey, _defaultFloat).ToString(),
                ValueType.String => _resolvedClient!.GetString(_valueKey, _defaultString) ?? "<null>",
                _ => "Unsupported type"
            };
        }

        private void AutoFindClientSource()
        {
            if (!Application.isPlaying)
            {
                _lastRefreshStatus = "Auto-find is available in Play Mode.";
                return;
            }

            var allBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            _clientSource = allBehaviours.FirstOrDefault(TryResolveClientFromSource);
            ResolveClientAndContext();
        }

        private void ResolveClientAndContext()
        {
            _resolvedClient = null;
            _resolvedContext = null;

            if (!Application.isPlaying || _clientSource == null)
            {
                return;
            }

            if (!TryResolveClientAndContextFromSource(_clientSource, out _resolvedClient, out _resolvedContext))
            {
                _resolvedClient = null;
                _resolvedContext = null;
            }
        }

        private static bool TryResolveClientFromSource(MonoBehaviour source)
        {
            return TryResolveClientAndContextFromSource(source, out _, out _);
        }

        private static bool TryResolveClientAndContextFromSource(MonoBehaviour source, out BeaconClient? client, out BeaconContext? context)
        {
            client = null;
            context = null;

            var sourceType = source.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var members = sourceType.GetMembers(flags);
            foreach (var member in members)
            {
                object? memberValue;
                try
                {
                    switch (member)
                    {
                        case PropertyInfo property when property.CanRead && property.GetIndexParameters().Length == 0:
                            memberValue = property.GetValue(source);
                            break;
                        case FieldInfo field:
                            memberValue = field.GetValue(source);
                            break;
                        default:
                            continue;
                    }
                }
                catch
                {
                    continue;
                }

                if (memberValue is BeaconClient beaconClient)
                {
                    client = beaconClient;
                }

                if (memberValue is IContextProvider contextProvider)
                {
                    context = contextProvider.GetContext();
                }

                if (client != null && context.HasValue)
                {
                    return true;
                }
            }

            return client != null;
        }
    }
}
