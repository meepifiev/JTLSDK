using System;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class LanguageResolutionTests
    {
        private TestSettingsBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _builder = new TestSettingsBuilder();
            _builder.SupportedLanguages.Clear();
            _builder.SupportedLanguages.Add(Language.English);
            _builder.SupportedLanguages.Add(Language.Russian);
            _builder.SupportedLanguages.Add(Language.Turkish);
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void SupportedPlatformLanguageIsUsed()
        {
            _builder.LanguageProvider = new FakeLanguageProvider("tr-TR");
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(Language.Turkish, JTLSDK.Language.Current);
        }

        [Test]
        public void ReplacementIsUsedForUnsupportedLanguage()
        {
            _builder.LanguageProvider = new FakeLanguageProvider("be");
            _builder.Replacements.Add(new LanguageReplacement(Language.Belarusian, Language.Russian));
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(Language.Russian, JTLSDK.Language.Current);
        }

        [Test]
        public void DefaultIsUsedWhenNothingMatches()
        {
            _builder.LanguageProvider = new FakeLanguageProvider("ja");
            _builder.DefaultLanguage = Language.Russian;
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(Language.Russian, JTLSDK.Language.Current);
        }

        [Test]
        public void SetChangesCurrentAndRaisesChangedOnce()
        {
            JTLSDK.Create(_builder.Build());
            int changes = 0;
            JTLSDK.Language.Changed += _ => changes++;

            JTLSDK.Language.Set(Language.Russian);
            JTLSDK.Language.Set(Language.Russian);

            Assert.AreEqual(Language.Russian, JTLSDK.Language.Current);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void SetUnsupportedLanguageThrows()
        {
            JTLSDK.Create(_builder.Build());

            Assert.Throws<ArgumentOutOfRangeException>(() => JTLSDK.Language.Set(Language.Japanese));
        }

        [Test]
        public void SupportedIsIntersectionWithConfiguration()
        {
            _builder.SupportedLanguages.Add(Language.German);
            JTLSDK.Create(_builder.Build());

            CollectionAssert.AreEquivalent(new[] { Language.English, Language.Russian, Language.Turkish, Language.German }, JTLSDK.Language.Supported);
        }
    }
}
