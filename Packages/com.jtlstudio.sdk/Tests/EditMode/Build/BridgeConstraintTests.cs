using JTLStudio.SDK.Editor.Build;
using NUnit.Framework;
using UnityEditor;

namespace JTLStudio.SDK.Tests.Build
{
    public class BridgeConstraintTests
    {
        private readonly PlatformBridgeFilter _filter = new PlatformBridgeFilter();

        [TestCase(PlatformBridgeFilter.YandexBridge)]
        [TestCase(PlatformBridgeFilter.YouTubeBridge)]
        public void BridgeHasNoDefineConstraints(string path)
        {
            PluginImporter importer = AssetImporter.GetAtPath(path) as PluginImporter;

            Assert.IsNotNull(importer, path);
            CollectionAssert.IsEmpty(importer.DefineConstraints, path);
        }

        [Test]
        public void OnlyTheActivePlatformBridgeIsIncluded()
        {
            Assert.IsTrue(_filter.IsIncluded(PlatformId.YandexGames, PlatformId.YandexGames));
            Assert.IsFalse(_filter.IsIncluded(PlatformId.YouTubePlayables, PlatformId.YandexGames));
            Assert.IsTrue(_filter.IsIncluded(PlatformId.YouTubePlayables, PlatformId.YouTubePlayables));
            Assert.IsFalse(_filter.IsIncluded(PlatformId.YandexGames, PlatformId.Editor));
        }
    }
}
