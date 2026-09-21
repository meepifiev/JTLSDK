using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Tests
{
    public class TestSettings
    {
        public JTLSDKSettings Settings { get; }
        public SdkConfiguration Configuration { get; }

        public TestSettings()
        {
            Settings = ScriptableObject.CreateInstance<JTLSDKSettings>();
            Configuration = ScriptableObject.CreateInstance<SdkConfiguration>();
            Configuration.Platform = PlatformId.YandexGames;
            Configuration.DisplayName = "Test";
            Settings.ActiveConfiguration = Configuration;
            Settings.LogLevel = LogLevel.None;
        }

        public TestSettings WithPlatform(IPlatformProvider provider)
        {
            Configuration.PlatformProvider = provider;
            return this;
        }

        public TestSettings WithAds(IAdsProvider provider)
        {
            Configuration.Ads = provider;
            return this;
        }

        public TestSettings WithData(IDataProvider provider)
        {
            Configuration.Data = provider;
            return this;
        }

        public TestSettings WithTimeout(float seconds)
        {
            Settings.InitializationTimeoutSeconds = seconds;
            return this;
        }

        public TestSettings WithAutosaveDelay(float seconds)
        {
            Settings.AutosaveDelaySeconds = seconds;
            return this;
        }

        public void Create()
        {
            JTLSDK.Create(Settings);
        }

        public void Dispose()
        {
            JTLSDK.Destroy();
            Object.DestroyImmediate(Configuration);
            Object.DestroyImmediate(Settings);
        }
    }
}
