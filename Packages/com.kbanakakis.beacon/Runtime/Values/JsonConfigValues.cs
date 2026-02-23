using KBanakakis.Beacon.Evaluation;

namespace KBanakakis.Beacon.Values
{
    public sealed class JsonConfigValues : IConfigValues
    {
        private readonly string? _valuesJson;

        public JsonConfigValues(string json)
        {
            if (!JsonLite.IsValidObjectJson(json))
            {
                return;
            }

            if (!JsonLite.TryExtractObject(json, "values", out var valuesJson, out var present))
            {
                return;
            }

            if (!present)
            {
                _valuesJson = "{}";
                return;
            }

            _valuesJson = valuesJson;
        }

        public bool TryGetBool(string key, out bool value)
        {
            value = false;
            return TryExtract(key, JsonLite.TryExtractBool, out value);
        }

        public bool TryGetInt(string key, out int value)
        {
            value = 0;
            return TryExtract(key, JsonLite.TryExtractInt, out value);
        }

        public bool TryGetFloat(string key, out float value)
        {
            value = 0f;
            return TryExtract(key, JsonLite.TryExtractFloat, out value);
        }

        public bool TryGetString(string key, out string value)
        {
            value = string.Empty;
            if (_valuesJson == null)
            {
                return false;
            }

            if (!JsonLite.TryExtractString(_valuesJson, key, out var parsed, out var present) || !present || parsed == null)
            {
                return false;
            }

            value = parsed;
            return true;
        }

        private bool TryExtract<T>(string key, TryExtractToken<T> extractor, out T value)
        {
            value = default!;
            return _valuesJson != null && extractor(_valuesJson, key, out value, out var present) && present;
        }

        private delegate bool TryExtractToken<T>(string json, string propertyName, out T value, out bool present);
    }
}
