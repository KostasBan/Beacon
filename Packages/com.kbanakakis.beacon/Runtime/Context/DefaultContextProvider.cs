using System;
using UnityEngine;

namespace KBanakakis.Beacon.Context
{
    public sealed class DefaultContextProvider : IContextProvider
    {
        private const string InstallIdKey = "KBanakakis.Beacon.InstallId";

        public BeaconContext GetContext()
        {
            return new BeaconContext(GetOrCreateInstallId(), GetPlatform(), Application.version);
        }

        private static string GetOrCreateInstallId()
        {
            var installId = PlayerPrefs.GetString(InstallIdKey, string.Empty);
            if (!string.IsNullOrEmpty(installId))
            {
                return installId;
            }

            installId = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(InstallIdKey, installId);
            PlayerPrefs.Save();
            return installId;
        }

        private static string GetPlatform()
        {
            return Application.platform switch
            {
                RuntimePlatform.Android => "Android",
                RuntimePlatform.IPhonePlayer => "iOS",
                RuntimePlatform.WindowsEditor => "Standalone",
                RuntimePlatform.OSXEditor => "Standalone",
                RuntimePlatform.LinuxEditor => "Standalone",
                RuntimePlatform.WindowsPlayer => "Standalone",
                RuntimePlatform.OSXPlayer => "Standalone",
                RuntimePlatform.LinuxPlayer => "Standalone",
                _ => Application.platform.ToString()
            };
        }
    }
}
