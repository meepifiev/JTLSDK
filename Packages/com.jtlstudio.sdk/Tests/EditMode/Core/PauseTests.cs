using System;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace JTLStudio.SDK.Tests.Core
{
    public class PauseTests
    {
        private TestSettingsBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _builder = new TestSettingsBuilder();
            JTLSDK.Create(_builder.Build());
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void HoldPausesAndDisposeResumes()
        {
            int changes = 0;
            JTLSDK.Pause.Changed += _ => changes++;

            IDisposable hold = JTLSDK.Pause.Hold("Menu");

            Assert.IsTrue(JTLSDK.Pause.IsPaused);
            Assert.AreEqual(1, changes);

            hold.Dispose();

            Assert.IsFalse(JTLSDK.Pause.IsPaused);
            Assert.AreEqual(2, changes);
        }

        [Test]
        public void SameSourceTwiceChangesOnce()
        {
            int changes = 0;
            JTLSDK.Pause.Changed += _ => changes++;

            JTLSDK.Pause.Set("Menu", true);
            JTLSDK.Pause.Set("Menu", true);

            Assert.AreEqual(1, changes);
            Assert.AreEqual(1, JTLSDK.Pause.Sources.Count);
        }

        [Test]
        public void ReleasingUnknownSourceDoesNothing()
        {
            int changes = 0;
            JTLSDK.Pause.Changed += _ => changes++;

            JTLSDK.Pause.Set("Unknown", false);

            Assert.AreEqual(0, changes);
            Assert.IsFalse(JTLSDK.Pause.IsPaused);
        }

        [Test]
        public void ResumesOnlyWhenEverySourceReleased()
        {
            JTLSDK.Pause.Set("Menu", true);
            JTLSDK.Pause.Set("Settings", true);

            JTLSDK.Pause.Set("Menu", false);
            Assert.IsTrue(JTLSDK.Pause.IsPaused);

            JTLSDK.Pause.Set("Settings", false);
            Assert.IsFalse(JTLSDK.Pause.IsPaused);
        }

        [Test]
        public void TimeScaleFollowsPauseAndKeepsGameValue()
        {
            JTLSDK.Time.Scale = 0.3f;
            Assert.AreEqual(0.3f, Time.timeScale, 0.0001f);

            IDisposable hold = JTLSDK.Pause.Hold("Menu");
            Assert.AreEqual(0f, Time.timeScale, 0.0001f);
            Assert.AreEqual(0.3f, JTLSDK.Time.Scale, 0.0001f);

            JTLSDK.Time.Scale = 1f;
            Assert.AreEqual(0f, Time.timeScale, 0.0001f);

            hold.Dispose();
            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
        }

        [Test]
        public void AudioIsSilentWhilePaused()
        {
            JTLSDK.Audio.Volume = 0.8f;
            Assert.AreEqual(0.8f, AudioListener.volume, 0.0001f);

            IDisposable hold = JTLSDK.Pause.Hold("Menu");
            Assert.AreEqual(0f, AudioListener.volume, 0.0001f);
            Assert.IsTrue(AudioListener.pause);
            Assert.AreEqual(0.8f, JTLSDK.Audio.Volume, 0.0001f);

            hold.Dispose();
            Assert.AreEqual(0.8f, AudioListener.volume, 0.0001f);
            Assert.IsFalse(AudioListener.pause);
        }

        [Test]
        public void PlatformPauseRequestUsesPlatformSource()
        {
            _builder.Platform.RequestPause(true);

            Assert.IsTrue(JTLSDK.Pause.IsPaused);
            CollectionAssert.Contains(JTLSDK.Pause.Sources, PauseSources.Platform);

            _builder.Platform.RequestPause(false);

            Assert.IsFalse(JTLSDK.Pause.IsPaused);
        }

        [Test]
        public void ContinuePromptReleasesPlatformPause()
        {
            _builder.Platform.RequestPause(true);
            bool continued = false;

            JTLSDK.Pause.ShowContinuePrompt(() => continued = true);

            Assert.IsTrue(continued);
            Assert.IsFalse(JTLSDK.Pause.IsPaused);
            Assert.AreEqual(1, _builder.Platform.ContinuePromptCount);
        }

        [Test]
        public void GameplaySuspendsDuringPauseAndResumes()
        {
            JTLSDK.Gameplay.Start();
            Assert.IsTrue(JTLSDK.Gameplay.IsPlaying);

            IDisposable hold = JTLSDK.Pause.Hold("Menu");
            Assert.IsFalse(JTLSDK.Gameplay.IsPlaying);

            hold.Dispose();
            Assert.IsTrue(JTLSDK.Gameplay.IsPlaying);
        }
    }
}
