namespace KBanakakis.Beacon.Context
{
    public readonly struct BeaconContext
    {
        public BeaconContext(string installId, string platform, string appVersion)
        {
            InstallId = installId;
            Platform = platform;
            AppVersion = appVersion;
        }

        public string InstallId { get; }

        public string Platform { get; }

        public string AppVersion { get; }
    }
}
