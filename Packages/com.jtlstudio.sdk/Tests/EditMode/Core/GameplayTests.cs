using System;
using JTLStudio.SDK.Providers;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class GameplayTests
    {
        private TestSettingsBuilder _builder;
        private FakeGameplayProvider _gameplay;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _gameplay = new FakeGameplayProvider();
            _builder = new TestSettingsBuilder { Gameplay = _gameplay };
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void GameReadyIsSentOnceAndDeferredUntilReady()
        {
            _gameplay.CompleteImmediately = false;
            JTLSDK.Create(_builder.Build());

            JTLSDK.Gameplay.GameReady();
            JTLSDK.Gameplay.GameReady();
            Assert.IsEmpty(_gameplay.Calls);

            _gameplay.Complete(ProviderState.Ready);

            CollectionAssert.AreEqual(new[] { "ready" }, _gameplay.Calls);
            Assert.IsTrue(JTLSDK.Gameplay.IsGameReady);
        }

        [Test]
        public void StartAndStopAreIdempotent()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.Gameplay.Start();
            JTLSDK.Gameplay.Start();
            JTLSDK.Gameplay.Stop();
            JTLSDK.Gameplay.Stop();

            CollectionAssert.AreEqual(new[] { "start", "stop" }, _gameplay.Calls);
        }

        [Test]
        public void PauseSuspendsGameplayAndRestoresIt()
        {
            JTLSDK.Create(_builder.Build());
            JTLSDK.Gameplay.Start();

            IDisposable hold = JTLSDK.Pause.Hold("Menu");
            hold.Dispose();

            CollectionAssert.AreEqual(new[] { "start", "stop", "start" }, _gameplay.Calls);
        }

        [Test]
        public void PauseDoesNotStartGameplayThatWasNotPlaying()
        {
            JTLSDK.Create(_builder.Build());

            IDisposable hold = JTLSDK.Pause.Hold("Menu");
            hold.Dispose();

            Assert.IsEmpty(_gameplay.Calls);
            Assert.IsFalse(JTLSDK.Gameplay.IsPlaying);
        }
    }
}
