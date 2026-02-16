namespace KBanakakis.Beacon.Values
{
    public interface IConfigValues
    {
        bool TryGetBool(string key, out bool value);
        bool TryGetInt(string key, out int value);
        bool TryGetFloat(string key, out float value);
        bool TryGetString(string key, out string value);
    }
}
