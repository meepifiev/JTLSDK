using NUnit.Framework;

namespace JTLStudio.SDK.Tests
{
    public class LanguageTests
    {
        private readonly LanguageCodes _codes = new LanguageCodes();

        [TestCase("ru", Language.Russian)]
        [TestCase("en-US", Language.English)]
        [TestCase("zh-CN", Language.ChineseSimplified)]
        [TestCase("zh-Hans", Language.ChineseSimplified)]
        [TestCase("iw", Language.Hebrew)]
        [TestCase("PT_br", Language.Portuguese)]
        public void ParsesPlatformCodes(string code, Language expected)
        {
            Assert.That(_codes.TryParse(code, out Language language), Is.True);
            Assert.That(language, Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase("xx")]
        [TestCase(null)]
        public void UnknownCodesFail(string code)
        {
            Assert.That(_codes.TryParse(code, out _), Is.False);
        }

        [Test]
        public void CodesRoundTrip()
        {
            foreach (Language language in System.Enum.GetValues(typeof(Language)))
            {
                string code = _codes.ToCode(language);

                Assert.That(_codes.TryParse(code, out Language parsed), Is.True);
                Assert.That(parsed, Is.EqualTo(language));
            }
        }
    }
}
