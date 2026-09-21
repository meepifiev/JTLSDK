using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class AnalyticsTests
    {
        private TestSettingsBuilder _builder;
        private FakeAnalyticsProvider _analytics;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _analytics = new FakeAnalyticsProvider();
            _builder = new TestSettingsBuilder { Analytics = _analytics };
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void EventWithoutParametersIsReportedByName()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.Analytics.Report("level_start");

            CollectionAssert.AreEqual(new[] { "level_start" }, _analytics.Events);
        }

        [Test]
        public void ParametersAreSerializedToJson()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.Analytics.Report("level_complete", new Dictionary<string, object> { { "level", 3 }, { "result", "win" } });

            CollectionAssert.AreEqual(new[] { "level_complete {\"level\":3,\"result\":\"win\"}" }, _analytics.Events);
        }

        [Test]
        public void UnsupportedProviderIgnoresEvents()
        {
            _analytics.InitialState = ProviderState.Unsupported;
            JTLSDK.Create(_builder.Build());

            JTLSDK.Analytics.Report("level_start");

            Assert.IsFalse(JTLSDK.Analytics.IsSupported);
            Assert.IsEmpty(_analytics.Events);
        }

        [Test]
        public void MissingProviderFallsBackToUnsupported()
        {
            _builder.Analytics = null;
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(ModuleState.Unsupported, JTLSDK.Analytics.State);
        }

        [Test]
        public void EmptyEventNameThrows()
        {
            JTLSDK.Create(_builder.Build());

            Assert.Throws<ArgumentException>(() => JTLSDK.Analytics.Report(" "));
        }
    }
}
