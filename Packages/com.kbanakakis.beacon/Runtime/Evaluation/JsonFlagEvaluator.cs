using System;
using System.Collections.Generic;
using System.Text.Json;
using KBanakakis.Beacon.Context;

namespace KBanakakis.Beacon.Evaluation
{
    public sealed class JsonFlagEvaluator : IFlagEvaluator
    {
        public bool IsEnabled(byte[] configBytes, BeaconContext ctx, string flagKey, bool defaultValue)
        {
            if (configBytes == null || string.IsNullOrEmpty(flagKey))
            {
                return defaultValue;
            }

            try
            {
                using var doc = JsonDocument.Parse(configBytes);
                var root = doc.RootElement;

                if (IsKilled(root, flagKey))
                {
                    return false;
                }

                if (!TryGetFlag(root, flagKey, out var flag))
                {
                    return defaultValue;
                }

                if (!GetBool(flag, "enabled", false))
                {
                    return false;
                }

                if (!PassesPlatform(flag, ctx.Platform) || !PassesMinVersion(flag, ctx.AppVersion))
                {
                    return false;
                }

                if (TryGetInt(flag, "rolloutPercent", out var rolloutPercent))
                {
                    rolloutPercent = Math.Clamp(rolloutPercent, 0, 100);
                    var salt = GetString(flag, "salt") ?? string.Empty;
                    var input = $"{ctx.InstallId}:{flagKey}:{salt}";
                    return StableBucketer.GetBucket0To99(input) < rolloutPercent;
                }

                return true;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static bool IsKilled(JsonElement root, string flagKey)
        {
            if (!TryGetObject(root, "safety", out var safety))
            {
                return false;
            }

            if (!GetBool(safety, "killAllExperiments", false))
            {
                return false;
            }

            if (!TryGetStringArray(safety, "allowlistFlagsWhenKilled", out var allowlist))
            {
                return true;
            }

            return !allowlist.Contains(flagKey);
        }

        private static bool TryGetFlag(JsonElement root, string flagKey, out JsonElement flag)
        {
            flag = default;
            if (!TryGetObject(root, "flags", out var flags))
            {
                return false;
            }

            if (flags.ValueKind != JsonValueKind.Object || !flags.TryGetProperty(flagKey, out flag))
            {
                return false;
            }

            return flag.ValueKind == JsonValueKind.Object;
        }

        private static bool PassesPlatform(JsonElement flag, string platform)
        {
            if (!TryGetObject(flag, "targets", out var targets))
            {
                return true;
            }

            if (!TryGetStringArray(targets, "platforms", out var platforms))
            {
                return true;
            }

            return platforms.Contains(platform);
        }

        private static bool PassesMinVersion(JsonElement flag, string appVersion)
        {
            if (!TryGetObject(flag, "targets", out var targets))
            {
                return true;
            }

            var minVersion = GetString(targets, "minAppVersion");
            if (string.IsNullOrEmpty(minVersion))
            {
                return true;
            }

            return VersionComparer.Compare(appVersion, minVersion) >= 0;
        }

        private static bool TryGetObject(JsonElement element, string name, out JsonElement obj)
        {
            obj = default;
            return element.ValueKind == JsonValueKind.Object &&
                   element.TryGetProperty(name, out obj) &&
                   obj.ValueKind == JsonValueKind.Object;
        }

        private static bool GetBool(JsonElement element, string name, bool defaultValue)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
            {
                return defaultValue;
            }

            return value.ValueKind == JsonValueKind.True ||
                   (value.ValueKind != JsonValueKind.False && defaultValue);
        }

        private static bool TryGetInt(JsonElement element, string name, out int value)
        {
            value = 0;
            return element.ValueKind == JsonValueKind.Object &&
                   element.TryGetProperty(name, out var jsonValue) &&
                   jsonValue.TryGetInt32(out value);
        }

        private static string GetString(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        private static bool TryGetStringArray(JsonElement element, string name, out HashSet<string> result)
        {
            result = null;
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            result = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var str = item.GetString();
                    if (!string.IsNullOrEmpty(str))
                    {
                        result.Add(str);
                    }
                }
            }

            return true;
        }
    }
}
