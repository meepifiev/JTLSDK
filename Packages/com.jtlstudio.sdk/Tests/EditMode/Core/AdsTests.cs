using System;
using JTLStudio.SDK.Providers;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests
{
    public class AdsTests
    {
        private TestSettings _settings;
        private FakePlatformProvider _platform;
        private FakeAdsProvider _ads;

        [SetUp]
        public void SetUp()
        {
            _platform = new FakePlatformProvider();
            _ads = new FakeAdsProvider();
            _settings = new TestSettings().WithPlatform(_platform).WithAds(_ads);
            _settings.Create();
        }

        [TearDown]
        public void TearDown()
        {
            _settings.Dispose();
        }

        [Test]
        public void RewardedBeforeReadyReturnsNotReadyWithoutCallingProvider()
        {
            AdResult result = AdResult.Failed;
            JTLSDK.Ads.ShowRewarded("double_money", value => result = value);

            Assert.That(JTLSDK.Ads.IsReady, Is.True);
            Assert.That(JTLSDK.IsReady, Is.False);
        }

        [Test]
        public void RewardedPausesAndSuspendsGameplayUntilResult()
        {
            _platform.Complete(ProviderState.Ready);
            JTLSDK.Gameplay.Start();

            AdResult result = AdResult.Failed;
            JTLSDK.Ads.ShowRewarded("double_money", value => result = value);

            Assert.That(JTLSDK.Ads.IsShowing, Is.True);
            Assert.That(JTLSDK.Pause.IsPaused, Is.True);
            Assert.That(JTLSDK.Gameplay.IsPlaying, Is.False);
            Assert.That(_ads.LastRewardId, Is.EqualTo("double_money"));

            _ads.CompleteShow(AdResult.Rewarded);

            Assert.That(result, Is.EqualTo(AdResult.Rewarded));
            Assert.That(JTLSDK.Ads.IsShowing, Is.False);
            Assert.That(JTLSDK.Pause.IsPaused, Is.False);
            Assert.That(JTLSDK.Gameplay.IsPlaying, Is.True);
        }

        [Test]
        public void SecondShowWhileShowingIsRejected()
        {
            _platform.Complete(ProviderState.Ready);

            JTLSDK.Ads.ShowRewarded("first", value => { });
            AdResult second = AdResult.Failed;
            JTLSDK.Ads.ShowRewarded("second", value => second = value);

            Assert.That(second, Is.EqualTo(AdResult.NotShown));
            Assert.That(_ads.ShowCalls, Is.EqualTo(1));
        }

        [Test]
        public void ThrowingResultCallbackStillReleasesPause()
        {
            _platform.Complete(ProviderState.Ready);

            JTLSDK.Ads.ShowInterstitial(value => throw new InvalidOperationException("boom"));
            _ads.CompleteShow(AdResult.Shown);

            Assert.That(JTLSDK.Pause.IsPaused, Is.False);
            Assert.That(JTLSDK.Ads.IsShowing, Is.False);
        }

        [Test]
        public void UnsupportedFormatReturnsNotSupported()
        {
            _ads.SupportsBanner = false;
            _ads.SupportsInterstitial = false;
            _platform.Complete(ProviderState.Ready);

            AdResult result = AdResult.Failed;
            JTLSDK.Ads.ShowInterstitial(value => result = value);

            Assert.That(result, Is.EqualTo(AdResult.NotSupported));
            Assert.That(JTLSDK.Platform.Supports(Capability.Interstitial), Is.False);
            Assert.That(JTLSDK.Platform.Supports(Capability.Rewarded), Is.True);
        }

        [Test]
        public void OpenedAndClosedEventsFireOnce()
        {
            _platform.Complete(ProviderState.Ready);
            int opened = 0;
            int closed = 0;
            JTLSDK.Ads.Opened += () => opened++;
            JTLSDK.Ads.Closed += () => closed++;

            JTLSDK.Ads.ShowInterstitial();
            _ads.CompleteShow(AdResult.Shown);
            _ads.CompleteShow(AdResult.Shown);

            Assert.That(opened, Is.EqualTo(1));
            Assert.That(closed, Is.EqualTo(1));
        }
    }
}
