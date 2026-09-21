using System;
using JTLStudio.SDK.Providers;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class GameEventsTests
    {
        private TestSettingsBuilder _builder;
        private FakeGameEventsProvider _gameplay;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _gameplay = new FakeGameEventsProvider();
            _builder = new TestSettingsBuilder { GameEvents = _gameplay };
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

            JTLSDK.GameEvents.GameReady();
            JTLSDK.GameEvents.GameReady();
            Assert.IsEmpty(_gameplay.Calls);

            _gameplay.Complete(ProviderState.Ready);

            CollectionAssert.AreEqual(new[] { "ready" }, _gameplay.Calls);
            Assert.IsTrue(JTLSDK.GameEvents.IsGameReady);
        }

        [Test]
        public void RestartStopsAndStartsActiveGameplay()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.GameEvents.GameplayStarted();
            JTLSDK.GameEvents.GameplayRestarted();

            CollectionAssert.AreEqual(new[] { "start", "stop", "start" }, _gameplay.Calls);
            Assert.IsTrue(JTLSDK.GameEvents.IsGameplayActive);
        }

        [Test]
        public void RestartStartsInactiveGameplay()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.GameEvents.GameplayRestarted();

            CollectionAssert.AreEqual(new[] { "start" }, _gameplay.Calls);
        }

        [Test]
        public void StartAndStopAreIdempotent()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.GameEvents.GameplayStarted();
            JTLSDK.GameEvents.GameplayStarted();
            JTLSDK.GameEvents.GameplayStopped();
            JTLSDK.GameEvents.GameplayStopped();

            CollectionAssert.AreEqual(new[] { "start", "stop" }, _gameplay.Calls);
        }

        [Test]
        public void PauseSuspendsGameplayAndRestoresIt()
        {
            JTLSDK.Create(_builder.Build());
            JTLSDK.GameEvents.GameplayStarted();

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
            Assert.IsFalse(JTLSDK.GameEvents.IsGameplayActive);
        }
    }
}
