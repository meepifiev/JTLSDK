using NUnit.Framework;
using UnityEditor;

namespace JTLStudio.SDK.Tests.Build
{
    public class BridgeConstraintTests
    {
        private const string YandexBridge = "Packages/com.jtlstudio.sdk/Runtime/Platforms/YandexGames/Plugins/WebGL/jtlsdk.yandex.jspre";
        private const string YouTubeBridge = "Packages/com.jtlstudio.sdk/Runtime/Platforms/YouTubePlayables/Plugins/WebGL/jtlsdk.youtube.jspre";

        [TestCase(YandexBridge, "JTLSDK_YANDEX_GAMES")]
        [TestCase(YouTubeBridge, "JTLSDK_YOUTUBE_PLAYABLES")]
        public void PlatformBridgeIsLimitedToItsDefine(string path, string define)
        {
            PluginImporter importer = AssetImporter.GetAtPath(path) as PluginImporter;

            Assert.IsNotNull(importer, path);
            CollectionAssert.AreEqual(new[] { define }, importer.DefineConstraints, path);
        }
    }
}
