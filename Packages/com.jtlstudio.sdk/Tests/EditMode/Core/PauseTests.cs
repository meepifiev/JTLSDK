using System;
using JTLStudio.SDK.Providers;
using NUnit.Framework;
using UnityEngine;

namespace JTLStudio.SDK.Tests
{
    public class PauseTests
    {
        private TestSettings _settings;
        private FakePlatformProvider _platform;

        [SetUp]
        public void SetUp()
        {
            _platform = new FakePlatformProvider();
            _settings = new TestSettings().WithPlatform(_platform);
            _settings.Create();
            _platform.Complete(ProviderState.Ready);
        }

        [TearDown]
        public void TearDown()
        {
            _settings.Dispose();
            Time.timeScale = 1f;
        }

        [Test]
        public void SetAndReleaseSourceTogglesPauseOnce()
        {
            int changes = 0;
            JTLSDK.Pause.Changed += paused => changes++;

            JTLSDK.Pause.Set("Menu", true);
            JTLSDK.Pause.Set("Menu", true);

            Assert.That(JTLSDK.Pause.IsPaused, Is.True);
            Assert.That(changes, Is.EqualTo(1));

            JTLSDK.Pause.Set("Menu", false);
            JTLSDK.Pause.Set("Unknown", false);

            Assert.That(JTLSDK.Pause.IsPaused, Is.False);
            Assert.That(changes, Is.EqualTo(2));
        }

        [Test]
        public void PauseStaysWhileAnySourceRemains()
        {
            JTLSDK.Pause.Set("Menu", true);
            _platform.RequestPause(true);
            _platform.RequestPause(true);

            JTLSDK.Pause.Set("Menu", false);

            Assert.That(JTLSDK.Pause.IsPaused, Is.True);
            Assert.That(JTLSDK.Pause.Sources, Is.EquivalentTo(new[] { PauseSources.Platform }));

            _platform.RequestPause(false);

            Assert.That(JTLSDK.Pause.IsPaused, Is.False);
        }

        [Test]
        public void HoldReleasesOnDispose()
        {
            using (JTLSDK.Pause.Hold("Menu"))
            {
                Assert.That(JTLSDK.Pause.IsPaused, Is.True);
            }

            Assert.That(JTLSDK.Pause.IsPaused, Is.False);
        }

        [Test]
        public void TimeScaleFollowsPauseAndKeepsGameValue()
        {
            JTLSDK.Time.Scale = 0.3f;

            Assert.That(Time.timeScale, Is.EqualTo(0.3f).Within(0.0001f));

            using (JTLSDK.Pause.Hold("Menu"))
            {
                Assert.That(Time.timeScale, Is.EqualTo(0f));
                Assert.That(JTLSDK.Time.Scale, Is.EqualTo(0.3f).Within(0.0001f));

                JTLSDK.Time.Scale = 1f;

                Assert.That(Time.timeScale, Is.EqualTo(0f));
            }

            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [Test]
        public void AudioIsSilencedWhilePausedOrPlatformMuted()
        {
            _platform.SupportsPlatformMute = true;
            JTLSDK.Audio.Volume = 0.8f;

            Assert.That(AudioListener.volume, Is.EqualTo(0.8f).Within(0.0001f));

            _platform.SetPlatformMuted(true);

            Assert.That(AudioListener.volume, Is.EqualTo(0f));
            Assert.That(JTLSDK.Audio.IsPlatformMuted, Is.True);

            _platform.SetPlatformMuted(false);

            Assert.That(AudioListener.volume, Is.EqualTo(0.8f).Within(0.0001f));

            using (JTLSDK.Pause.Hold("Menu"))
            {
                Assert.That(AudioListener.volume, Is.EqualTo(0f));
                Assert.That(AudioListener.pause, Is.True);
            }

            Assert.That(AudioListener.pause, Is.False);
        }

        [Test]
        public void GameplayIsSuspendedDuringPauseAndRestored()
        {
            JTLSDK.Gameplay.Start();

            Assert.That(JTLSDK.Gameplay.IsPlaying, Is.True);

            using (JTLSDK.Pause.Hold("Menu"))
            {
                Assert.That(JTLSDK.Gameplay.IsPlaying, Is.False);
            }

            Assert.That(JTLSDK.Gameplay.IsPlaying, Is.True);
        }

        [Test]
        public void InvalidSourceThrows()
        {
            Assert.Throws<ArgumentException>(() => JTLSDK.Pause.Set("", true));
        }
    }
}
