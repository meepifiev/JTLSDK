using JTLStudio.SDK.Editor.Configuration;
using UnityEditor;

namespace JTLStudio.SDK.Editor.Build
{
    [InitializeOnLoad]
    public class PlatformBridgeFilter
    {
        public const string YandexBridge = "Packages/com.jtlstudio.sdk/Runtime/Platforms/YandexGames/Plugins/WebGL/jtlsdk.yandex.jspre";
        public const string YouTubeBridge = "Packages/com.jtlstudio.sdk/Runtime/Platforms/YouTubePlayables/Plugins/WebGL/jtlsdk.youtube.jspre";

        private readonly SettingsAssetService _settings = new SettingsAssetService();

        static PlatformBridgeFilter()
        {
            EditorApplication.delayCall += new PlatformBridgeFilter().Register;
        }

        public void Register()
        {
            Register(YandexBridge, PlatformId.YandexGames);
            Register(YouTubeBridge, PlatformId.YouTubePlayables);
        }

        public bool IsIncluded(PlatformId bridgePlatform, PlatformId activePlatform)
        {
            return bridgePlatform == activePlatform;
        }

        private void Register(string path, PlatformId platform)
        {
            if (AssetImporter.GetAtPath(path) is PluginImporter importer)
            {
                importer.SetIncludeInBuildDelegate(pluginPath => IsIncluded(platform, ActivePlatform()));
            }
        }

        private PlatformId ActivePlatform()
        {
            JTLSDKSettings settings = _settings.Find();
            return settings == null || settings.ActiveConfiguration == null ? PlatformId.Editor : settings.ActiveConfiguration.Platform;
        }
    }
}
