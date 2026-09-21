using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests
{
    public class FacadeTests
    {
        private TestSettings _settings;

        [TearDown]
        public void TearDown()
        {
            _settings?.Dispose();
            _settings = null;
        }

        [Test]
        public void ModuleAccessBeforeCreateThrows()
        {
            Assert.That(JTLSDK.IsCreated, Is.False);
            Assert.Throws<InvalidOperationException>(() => { IAds ads = JTLSDK.Ads; });
        }

        [Test]
        public void CreateTwiceThrows()
        {
            _settings = new TestSettings();
            _settings.Create();

            Assert.Throws<InvalidOperationException>(() => JTLSDK.Create(_settings.Settings));
        }

        [Test]
        public void ReadyCallbacksRunInRegistrationOrderAfterProvidersComplete()
        {
            FakePlatformProvider platform = new FakePlatformProvider();
            _settings = new TestSettings().WithPlatform(platform);
            _settings.Create();

            List<int> order = new List<int>();
            JTLSDK.WhenReady(() => order.Add(1));
            JTLSDK.WhenReady(() => order.Add(2));
            JTLSDK.WhenReady(() => order.Add(3));

            Assert.That(JTLSDK.IsReady, Is.False);
            Assert.That(order, Is.Empty);

            platform.Complete(ProviderState.Ready);

            Assert.That(JTLSDK.IsReady, Is.True);
            Assert.That(order, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void WhenReadyAfterReadyRunsImmediately()
        {
            _settings = new TestSettings();
            _settings.Create();

            bool called = false;
            JTLSDK.WhenReady(() => called = true);

            Assert.That(called, Is.True);
        }

        [Test]
        public void TimeoutMarksPendingModulesFailedAndReleasesReadyCallbacks()
        {
            FakePlatformProvider platform = new FakePlatformProvider();
            _settings = new TestSettings().WithPlatform(platform).WithTimeout(5f);
            _settings.Create();

            bool called = false;
            JTLSDK.WhenReady(() => called = true);
            JTLSDK.Current.Tick(4f);

            Assert.That(JTLSDK.IsReady, Is.False);

            JTLSDK.Current.Tick(1.5f);

            Assert.That(JTLSDK.IsReady, Is.True);
            Assert.That(called, Is.True);
            Assert.That(JTLSDK.Platform.State, Is.EqualTo(ModuleState.Failed));
        }

        [Test]
        public void LateProviderAnswerAfterTimeoutMakesModuleReady()
        {
            FakePlatformProvider platform = new FakePlatformProvider();
            _settings = new TestSettings().WithPlatform(platform).WithTimeout(1f);
            _settings.Create();
            JTLSDK.Current.Tick(2f);

            platform.Complete(ProviderState.Ready);

            Assert.That(JTLSDK.Platform.State, Is.EqualTo(ModuleState.Ready));
        }

        [Test]
        public void ThrowingReadyCallbackDoesNotStopOthers()
        {
            FakePlatformProvider platform = new FakePlatformProvider();
            _settings = new TestSettings().WithPlatform(platform);
            _settings.Create();

            bool secondCalled = false;
            JTLSDK.WhenReady(() => throw new InvalidOperationException("boom"));
            JTLSDK.WhenReady(() => secondCalled = true);

            platform.Complete(ProviderState.Ready);

            Assert.That(secondCalled, Is.True);
        }

        [Test]
        public void UnsupportedModulesReportNotSupported()
        {
            _settings = new TestSettings();
            _settings.Create();

            Assert.That(JTLSDK.Payments.IsSupported, Is.False);
            Assert.That(JTLSDK.Platform.Supports(Capability.Purchases), Is.False);
            Assert.That(JTLSDK.Platform.Supports(Capability.Rewarded), Is.False);

            PurchaseResult result = PurchaseResult.Failed;
            JTLSDK.Payments.Purchase("remove_ads", value => result = value);

            Assert.That(result, Is.EqualTo(PurchaseResult.NotSupported));
        }

        [Test]
        public void DestroyAllowsCreateAgain()
        {
            _settings = new TestSettings();
            _settings.Create();
            JTLSDK.Destroy();

            Assert.That(JTLSDK.IsCreated, Is.False);

            _settings.Create();

            Assert.That(JTLSDK.IsCreated, Is.True);
        }
    }
}
