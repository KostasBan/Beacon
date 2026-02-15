using System;
using System.Collections.Generic;
using System.Text;
using KBanakakis.Beacon.Context;

namespace KBanakakis.Beacon.Evaluation
{
    public sealed class JsonFlagEvaluator : IFlagEvaluator
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public bool IsEnabled(byte[] configBytes, BeaconContext ctx, string flagKey, bool defaultValue)
        {
            if (configBytes == null || string.IsNullOrEmpty(flagKey))
            {
                return defaultValue;
            }

            string json;
            try
            {
                json = StrictUtf8.GetString(configBytes);
            }
            catch
            {
                return defaultValue;
            }

            if (!JsonLite.IsValidObjectJson(json))
            {
                return defaultValue;
            }

            if (!TryEvaluateKillState(json, flagKey, out var isKilled))
            {
                return defaultValue;
            }

            if (isKilled)
            {
                return false;
            }

            if (!JsonLite.TryExtractObject(json, "flags", out var flagsJson, out var flagsPresent))
            {
                return defaultValue;
            }

            if (!flagsPresent || flagsJson == null)
            {
                return defaultValue;
            }

            if (!JsonLite.TryExtractObject(flagsJson, flagKey, out var flagJson, out var flagPresent))
            {
                return defaultValue;
            }

            if (!flagPresent || flagJson == null)
            {
                return defaultValue;
            }

            return EvaluateFlag(flagJson, ctx, flagKey);
        }

        private static bool EvaluateFlag(string flagJson, BeaconContext ctx, string flagKey)
        {
            if (!JsonLite.TryExtractBool(flagJson, "enabled", out var enabled, out var enabledPresent) ||
                !enabledPresent ||
                !enabled)
            {
                return false;
            }

            if (!PassesTargets(flagJson, ctx))
            {
                return false;
            }

            if (!JsonLite.TryExtractInt(flagJson, "rolloutPercent", out var rolloutPercent, out var rolloutPresent))
            {
                return false;
            }

            if (!rolloutPresent)
            {
                return true;
            }

            rolloutPercent = Math.Clamp(rolloutPercent, 0, 100);

            if (!JsonLite.TryExtractString(flagJson, "salt", out var salt, out var saltPresent))
            {
                return false;
            }

            if (!saltPresent)
            {
                salt = string.Empty;
            }

            var input = $"{ctx.InstallId}:{flagKey}:{salt}";
            return StableBucketer.GetBucket0To99(input) < rolloutPercent;
        }

        private static bool PassesTargets(string flagJson, BeaconContext ctx)
        {
            if (!JsonLite.TryExtractObject(flagJson, "targets", out var targetsJson, out var targetsPresent))
            {
                return false;
            }

            if (!targetsPresent)
            {
                return true;
            }

            if (targetsJson == null)
            {
                return false;
            }

            if (!JsonLite.TryExtractStringArray(targetsJson, "platforms", out var platforms, out var platformsPresent))
            {
                return false;
            }

            if (platformsPresent && (platforms == null || !platforms.Contains(ctx.Platform)))
            {
                return false;
            }

            if (!JsonLite.TryExtractString(targetsJson, "minAppVersion", out var minVersion, out var minVersionPresent))
            {
                return false;
            }

            if (minVersionPresent && minVersion != null && VersionComparer.Compare(ctx.AppVersion, minVersion) < 0)
            {
                return false;
            }

            return true;
        }

        private static bool TryEvaluateKillState(string json, string flagKey, out bool isKilled)
        {
            isKilled = false;
            if (!JsonLite.TryExtractObject(json, "safety", out var safetyJson, out var safetyPresent))
            {
                return false;
            }

            if (!safetyPresent)
            {
                return true;
            }

            if (safetyJson == null)
            {
                return false;
            }

            if (!JsonLite.TryExtractBool(safetyJson, "killAllExperiments", out var killed, out var killedPresent))
            {
                return false;
            }

            if (!killedPresent || !killed)
            {
                return true;
            }

            var allowlist = new HashSet<string>(StringComparer.Ordinal);
            if (JsonLite.TryExtractStringArray(safetyJson, "allowlistFlagsWhenKilled", out var parsedAllowlist, out var allowlistPresent) && allowlistPresent && parsedAllowlist != null)
            {
                allowlist = parsedAllowlist;
            }

            isKilled = !allowlist.Contains(flagKey);
            return true;
        }
    }
}
