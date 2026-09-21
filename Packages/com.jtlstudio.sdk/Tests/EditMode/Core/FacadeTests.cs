using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class FacadeTests
    {
        private TestSettingsBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _builder = new TestSettingsBuilder();
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void AccessingModuleBeforeCreateThrows()
        {
            Assert.Throws<InvalidOperationException>(() => JTLSDK.Ads.ShowInterstitial());
        }

        [Test]
        public void CreateTwiceThrows()
        {
            JTLSDK.Create(_builder.Build());

            Assert.Throws<InvalidOperationException>(() => JTLSDK.Create(_builder.Build()));
        }

        [Test]
        public void ReadyWhenEveryProviderAnswered()
        {
            JTLSDK.Create(_builder.Build());

            Assert.IsTrue(JTLSDK.IsReady);
            Assert.AreEqual(ModuleState.Ready, JTLSDK.Platform.State);
            Assert.AreEqual(ModuleState.Unsupported, JTLSDK.Ads.State);
            Assert.AreEqual(PlatformId.YandexGames, JTLSDK.Platform.Current);
        }

        [Test]
        public void WhenReadyRunsCallbacksInRegistrationOrder()
        {
            _builder.Platform.CompleteImmediately = false;
            JTLSDK.Create(_builder.Build());
            List<int> order = new List<int>();

            JTLSDK.WhenReady(() => order.Add(1));
            JTLSDK.WhenReady(() => order.Add(2));
            JTLSDK.WhenReady(() => order.Add(3));

            Assert.IsFalse(JTLSDK.IsReady);
            Assert.IsEmpty(order);

            _builder.Platform.Complete(ProviderState.Ready);

            Assert.IsTrue(JTLSDK.IsReady);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, order);
        }

        [Test]
        public void WhenReadyAfterReadyRunsImmediately()
        {
            JTLSDK.Create(_builder.Build());
            bool called = false;

            JTLSDK.WhenReady(() => called = true);

            Assert.IsTrue(called);
        }

        [Test]
        public void TimeoutMarksPendingModulesFailedAndFiresReady()
        {
            _builder.Platform.CompleteImmediately = false;
            _builder.InitializationTimeoutSeconds = 1f;
            JTLSDK.Create(_builder.Build());
            bool called = false;
            JTLSDK.WhenReady(() => called = true);

            JTLSDK.Current.Tick(0.5f);
            Assert.IsFalse(JTLSDK.IsReady);

            JTLSDK.Current.Tick(0.6f);

            Assert.IsTrue(JTLSDK.IsReady);
            Assert.IsTrue(called);
            Assert.AreEqual(ModuleState.Failed, JTLSDK.Platform.State);
        }

        [Test]
        public void LateAnswerAfterTimeoutTurnsModuleReady()
        {
            _builder.Platform.CompleteImmediately = false;
            _builder.InitializationTimeoutSeconds = 1f;
            JTLSDK.Create(_builder.Build());
            JTLSDK.Current.Tick(2f);

            _builder.Platform.Complete(ProviderState.Ready);

            Assert.AreEqual(ModuleState.Ready, JTLSDK.Platform.State);
        }

        [Test]
        public void DestroyAllowsCreateAgain()
        {
            JTLSDK.Create(_builder.Build());
            JTLSDK.Destroy();

            Assert.IsFalse(JTLSDK.IsCreated);
            JTLSDK.Create(_builder.Build());
            Assert.IsTrue(JTLSDK.IsCreated);
        }
    }
}
