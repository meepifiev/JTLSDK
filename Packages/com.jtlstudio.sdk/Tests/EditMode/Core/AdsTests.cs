using System;
using JTLStudio.SDK.Providers;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class AdsTests
    {
        private TestSettingsBuilder _builder;
        private FakeAdsProvider _ads;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _ads = new FakeAdsProvider();
            _builder = new TestSettingsBuilder { Ads = _ads };
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void ShowBeforeReadyReturnsNotReady()
        {
            _ads.CompleteImmediately = false;
            JTLSDK.Create(_builder.Build());
            AdResult? result = null;

            JTLSDK.Ads.ShowRewarded("reward", value => result = value);

            Assert.AreEqual(AdResult.NotReady, result);
            Assert.AreEqual(0, _ads.ShowCount);
        }

        [Test]
        public void UnsupportedFormatReturnsNotSupported()
        {
            _ads.SupportsRewarded = false;
            JTLSDK.Create(_builder.Build());
            AdResult? result = null;

            JTLSDK.Ads.ShowRewarded("reward", value => result = value);

            Assert.AreEqual(AdResult.NotSupported, result);
        }

        [Test]
        public void SecondShowWhileFirstPendingReturnsNotShown()
        {
            JTLSDK.Create(_builder.Build());
            AdResult? second = null;

            JTLSDK.Ads.ShowRewarded("first", _ => { });
            JTLSDK.Ads.ShowRewarded("second", value => second = value);

            Assert.AreEqual(AdResult.NotShown, second);
            Assert.AreEqual(1, _ads.ShowCount);
            Assert.AreEqual("first", _ads.LastRewardId);
        }

        [Test]
        public void PauseAndGameplayAreHeldDuringShow()
        {
            JTLSDK.Create(_builder.Build());
            JTLSDK.GameEvents.GameplayStarted();
            AdResult? result = null;

            JTLSDK.Ads.ShowRewarded("reward", value => result = value);

            Assert.IsTrue(JTLSDK.Ads.IsShowing);
            Assert.IsTrue(JTLSDK.Pause.IsPaused);
            Assert.IsFalse(JTLSDK.GameEvents.IsGameplayActive);

            _ads.CompleteShow(AdResult.Rewarded);

            Assert.AreEqual(AdResult.Rewarded, result);
            Assert.IsFalse(JTLSDK.Ads.IsShowing);
            Assert.IsFalse(JTLSDK.Pause.IsPaused);
            Assert.IsTrue(JTLSDK.GameEvents.IsGameplayActive);
        }

        [Test]
        public void CallbackExceptionDoesNotKeepPause()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.Ads.ShowInterstitial(_ => throw new InvalidOperationException("game bug"));
            _ads.CompleteShow(AdResult.Shown);

            Assert.IsFalse(JTLSDK.Pause.IsPaused);
            Assert.IsFalse(JTLSDK.Ads.IsShowing);
        }

        [Test]
        public void ProviderCompletingTwiceInvokesCallbackOnce()
        {
            JTLSDK.Create(_builder.Build());
            int calls = 0;

            JTLSDK.Ads.ShowInterstitial(_ => calls++);
            _ads.CompleteShow(AdResult.Shown);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void OpenedAndClosedEventsFireAroundShow()
        {
            JTLSDK.Create(_builder.Build());
            int opened = 0;
            int closed = 0;
            JTLSDK.Ads.Opened += () => opened++;
            JTLSDK.Ads.Closed += () => closed++;

            JTLSDK.Ads.ShowInterstitial();
            Assert.AreEqual(1, opened);
            Assert.AreEqual(0, closed);

            _ads.CompleteShow(AdResult.Shown);
            Assert.AreEqual(1, closed);
        }
    }
}
