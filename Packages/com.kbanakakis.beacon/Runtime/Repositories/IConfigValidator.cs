namespace KBanakakis.Beacon.Repositories
{
    public interface IConfigValidator
    {
        ValidationResult Validate(byte[] bytes);
    }

    public readonly struct ValidationResult
    {
        public ValidationResult(bool isValid, int schemaVersion, string? configVersion, string? error)
        {
            IsValid = isValid;
            SchemaVersion = schemaVersion;
            ConfigVersion = configVersion;
            Error = error;
        }

        public bool IsValid { get; }

        public int SchemaVersion { get; }

        public string? ConfigVersion { get; }

        public string? Error { get; }
    }
}
