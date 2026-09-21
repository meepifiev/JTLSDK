using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class FlagsTests
    {
        private TestSettingsBuilder _builder;
        private FakeFlagsProvider _flags;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _flags = new FakeFlagsProvider();
            _builder = new TestSettingsBuilder { Flags = _flags };
            _builder.FlagDefinitions.Add(new FlagDefinition("tutorial_enabled", FlagType.Bool, "true"));
            _builder.FlagDefinitions.Add(new FlagDefinition("ads_interval", FlagType.Int, "60"));
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void PlatformValueOverridesDefault()
        {
            _flags.Values["ads_interval"] = "90";
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(90, JTLSDK.Flags.GetInt("ads_interval"));
        }

        [Test]
        public void DefaultIsUsedWhenPlatformHasNoValue()
        {
            JTLSDK.Create(_builder.Build());

            Assert.IsTrue(JTLSDK.Flags.GetBool("tutorial_enabled"));
            Assert.AreEqual(60, JTLSDK.Flags.GetInt("ads_interval"));
            Assert.IsTrue(JTLSDK.Flags.HasKey("ads_interval"));
        }

        [Test]
        public void UnknownKeyReturnsArgumentDefault()
        {
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(1.5f, JTLSDK.Flags.GetFloat("missing", 1.5f), 0.0001f);
            Assert.AreEqual("x", JTLSDK.Flags.GetString("missing", "x"));
            Assert.IsFalse(JTLSDK.Flags.HasKey("missing"));
        }

        [Test]
        public void DefaultsWorkWithoutFlagsProvider()
        {
            _builder.Flags = null;
            JTLSDK.Create(_builder.Build());

            Assert.IsFalse(JTLSDK.Platform.Supports(Capability.Flags));
            Assert.AreEqual(60, JTLSDK.Flags.GetInt("ads_interval"));
        }
    }
}
