using System;
using System.Text;
using System.Text.Json;

namespace KBanakakis.Beacon.Repositories
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

            var schemaVersion = 0;
            string? configVersion = null;

            try
            {
                using var document = JsonDocument.Parse(payload);
                if (document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("meta", out var metaElement)
                    && metaElement.ValueKind == JsonValueKind.Object)
                {
                    if (metaElement.TryGetProperty("schemaVersion", out var schemaElement))
                    {
                        if (schemaElement.ValueKind == JsonValueKind.Number
                            && schemaElement.TryGetInt32(out var parsedSchema))
                        {
                            schemaVersion = parsedSchema;
                        }
                        else if (schemaElement.ValueKind == JsonValueKind.String
                            && int.TryParse(schemaElement.GetString(), out var parsedSchemaText))
                        {
                            schemaVersion = parsedSchemaText;
                        }
                    }

                    if (metaElement.TryGetProperty("configVersion", out var configElement))
                    {
                        if (configElement.ValueKind == JsonValueKind.String)
                        {
                            configVersion = configElement.GetString();
                        }
                        else
                        {
                            configVersion = configElement.ToString();
                        }
                    }
                }
            }
            catch (JsonException)
            {
                return new ValidationResult(true, 0, null, null);
            }

            return new ValidationResult(true, schemaVersion, configVersion, null);
        }
    }
}
