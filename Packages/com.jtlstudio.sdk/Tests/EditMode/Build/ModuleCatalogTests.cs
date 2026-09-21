using JTLStudio.SDK.Editor.Configuration;
using JTLStudio.SDK.Editor.Updates;
using JTLStudio.SDK.Providers;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Build
{
    public class ModuleCatalogTests
    {
        private const string Catalog = "{\"modules\":[{\"id\":\"example\",\"name\":\"Example\",\"package\":\"com.jtlstudio.sdk.example\",\"repository\":\"meepifiev/JTLSDK\",\"path\":\"Modules/com.jtlstudio.sdk.example\",\"requires\":\"0.1.0\",\"platforms\":[\"YandexGames\"]},{\"name\":\"No package\"}]}";

        private readonly ModuleCatalog _catalog = new ModuleCatalog();

        [Test]
        public void ModulesWithPackageAreParsed()
        {
            ModuleCatalogResult result = _catalog.Parse(Catalog);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Modules.Count);
            Assert.AreEqual("com.jtlstudio.sdk.example", result.Modules[0].Package);
            CollectionAssert.AreEqual(new[] { "YandexGames" }, result.Modules[0].Platforms);
        }

        [Test]
        public void GitUrlIncludesPathAndTag()
        {
            ModuleDefinition module = _catalog.Parse(Catalog).Modules[0];

            Assert.AreEqual("https://github.com/meepifiev/JTLSDK.git?path=Modules/com.jtlstudio.sdk.example#v1.0.0", _catalog.GitUrl(module, "v1.0.0"));
            Assert.AreEqual("https://github.com/meepifiev/JTLSDK.git?path=Modules/com.jtlstudio.sdk.example", _catalog.GitUrl(module, ""));
        }

        [Test]
        public void CatalogWithoutModulesListFails()
        {
            Assert.IsFalse(_catalog.Parse("{}").IsSuccess);
        }

        [Test]
        public void PlatformAttributeLimitsProviders()
        {
            ProviderPlatformRules rules = new ProviderPlatformRules();

            Assert.IsTrue(rules.IsAvailable(typeof(YandexGames.YandexGamesAdsProvider), PlatformId.YandexGames));
            Assert.IsFalse(rules.IsAvailable(typeof(YandexGames.YandexGamesAdsProvider), PlatformId.YouTubePlayables));
            Assert.IsTrue(rules.IsAvailable(typeof(UnsupportedAdsProvider), PlatformId.YouTubePlayables));
        }
    }
}
