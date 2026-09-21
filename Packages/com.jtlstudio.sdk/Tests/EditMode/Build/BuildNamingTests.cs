using JTLStudio.SDK.Editor.Build;
using JTLStudio.SDK.Editor.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace JTLStudio.SDK.Tests.Build
{
    public class BuildNamingTests
    {
        private readonly SdkBuildService _builds = new SdkBuildService();
        private JTLSDKEditorSettings _settings;
        private string _pattern;
        private SdkConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _settings = JTLSDKEditorSettings.instance;
            _pattern = _settings.BuildNamePattern;
            _configuration = ScriptableObject.CreateInstance<SdkConfiguration>();
            _configuration.DisplayName = "Yandex Games";
        }

        [TearDown]
        public void TearDown()
        {
            _settings.BuildNamePattern = _pattern;
            Object.DestroyImmediate(_configuration);
        }

        [Test]
        public void ResolvesConfigurationAndBuildNumber()
        {
            _settings.BuildNamePattern = "{configuration}_b{build}";

            Assert.AreEqual("YandexGames_b7", _builds.ResolveName(_settings, _configuration, 7));
        }

        [Test]
        public void MissingConfigurationResolvesAsEditor()
        {
            _settings.BuildNamePattern = "{configuration}_b{build}";

            Assert.AreEqual("Editor_b1", _builds.ResolveName(_settings, null, 1));
        }

        [Test]
        public void InvalidFileNameCharactersAreReplaced()
        {
            _settings.BuildNamePattern = "a/b:{build}";

            StringAssert.DoesNotContain("/", _builds.ResolveName(_settings, _configuration, 2));
        }

        [Test]
        public void EmptyPatternFallsBackToDefault()
        {
            _settings.BuildNamePattern = " ";

            Assert.AreEqual("{product}_{configuration}_b{build}", _settings.BuildNamePattern);
        }
    }
}
