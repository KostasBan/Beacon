using System;
using System.Text;
using System.Text.RegularExpressions;

namespace KostasBan.Beacon.Repositories
{
    public sealed class BasicConfigValidator : IConfigValidator
    {
        public ValidationResult Validate(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return new ValidationResult(false, 0, null, "Config payload was empty.");
            }

            string payload;
            try
            {
                payload = new UTF8Encoding(false, true).GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                return new ValidationResult(false, 0, null, "Config payload was not valid UTF-8.");
            }

            if (!payload.Contains("{") || !payload.Contains("}"))
            {
                return new ValidationResult(false, 0, null, "Config payload did not look like JSON.");
            }

            if (!IsWellFormedJsonObject(payload))
            {
                return new ValidationResult(false, 0, null, "Malformed JSON.");
            }

            var schemaVersion = ExtractSchemaVersion(payload);
            var configVersion = ExtractConfigVersion(payload);

            return new ValidationResult(true, schemaVersion, configVersion, null);
        }

        private static bool IsWellFormedJsonObject(string payload)
        {
            var index = 0;
            while (index < payload.Length && char.IsWhiteSpace(payload[index]))
            {
                index++;
            }

            if (index >= payload.Length || payload[index] != '{')
            {
                return false;
            }

            var stack = new char[payload.Length];
            var stackCount = 0;
            var inString = false;
            var escape = false;

            for (var i = 0; i < payload.Length; i++)
            {
                var current = payload[i];
                if (inString)
                {
                    if (escape)
                    {
                        escape = false;
                        continue;
                    }

                    if (current == '\\')
                    {
                        escape = true;
                        continue;
                    }

                    if (current == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (current == '"')
                {
                    inString = true;
                    continue;
                }

                if (current == '{' || current == '[')
                {
                    stack[stackCount++] = current;
                    continue;
                }

                if (current == '}' || current == ']')
                {
                    if (stackCount == 0)
                    {
                        return false;
                    }

                    var open = stack[--stackCount];
                    if ((current == '}' && open != '{') || (current == ']' && open != '['))
                    {
                        return false;
                    }

                    continue;
                }
            }

            if (inString || stackCount != 0)
            {
                return false;
            }

            return true;
        }

        private static int ExtractSchemaVersion(string payload)
        {
            var match = Regex.Match(payload, "\"schemaVersion\"\\s*:\\s*(-?\\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var schemaVersion))
            {
                return schemaVersion;
            }

            return 0;
        }

        private static string? ExtractConfigVersion(string payload)
        {
            var match = Regex.Match(payload, "\"configVersion\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
