using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class LeaderboardsTests
    {
        private TestSettingsBuilder _builder;
        private FakeLeaderboardsProvider _leaderboards;
        private FakePlayerProvider _player;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _leaderboards = new FakeLeaderboardsProvider();
            _player = new FakePlayerProvider();
            _builder = new TestSettingsBuilder { Leaderboards = _leaderboards, Player = _player };
            _builder.LeaderboardDefinitions.Add(new LeaderboardDefinition("levels", new PlatformIdentifier(PlatformId.YandexGames, "levels_board")));
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void ScoreIsDeferredUntilAuthorized()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.Leaderboards.SetScore("levels", 27);
            Assert.IsEmpty(_leaderboards.Scores);

            JTLSDK.Player.Authorize(_ => { });

            Assert.AreEqual(1, _leaderboards.Scores.Count);
            Assert.AreEqual("levels_board", _leaderboards.Scores[0].Key);
            Assert.AreEqual(27, _leaderboards.Scores[0].Value);
        }

        [Test]
        public void LatestDeferredScoreWins()
        {
            JTLSDK.Create(_builder.Build());

            JTLSDK.Leaderboards.SetScore("levels", 10);
            JTLSDK.Leaderboards.SetScore("levels", 30);
            JTLSDK.Player.Authorize(_ => { });

            Assert.AreEqual(1, _leaderboards.Scores.Count);
            Assert.AreEqual(30, _leaderboards.Scores[0].Value);
        }

        [Test]
        public void ScoreIsSentDirectlyWithoutPlayerModule()
        {
            _builder.Player = null;
            JTLSDK.Create(_builder.Build());

            JTLSDK.Leaderboards.SetScore("levels", 5);

            Assert.AreEqual(1, _leaderboards.Scores.Count);
        }

        [Test]
        public void LoadReturnsEntriesWhenSupported()
        {
            JTLSDK.Create(_builder.Build());
            LeaderboardPage? page = null;

            JTLSDK.Leaderboards.Load("levels", 10, 5, value => page = value);

            Assert.IsTrue(JTLSDK.Leaderboards.CanLoad);
            Assert.AreEqual(1, page.Value.Entries.Count);
            Assert.IsTrue(page.Value.CurrentPlayer.HasValue);
        }

        [Test]
        public void LoadReturnsEmptyPageWhenUnsupported()
        {
            _leaderboards.SupportsLoad = false;
            JTLSDK.Create(_builder.Build());
            LeaderboardPage? page = null;

            JTLSDK.Leaderboards.Load("levels", 10, 5, value => page = value);

            Assert.IsFalse(JTLSDK.Leaderboards.CanLoad);
            Assert.IsEmpty(page.Value.Entries);
        }
    }
}
