using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakeLeaderboardsProvider : ILeaderboardsProvider
    {
        public List<KeyValuePair<string, long>> Scores { get; } = new List<KeyValuePair<string, long>>();
        public bool SupportsLoad { get; set; } = true;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void SetScore(string platformLeaderboardId, long score, Action<bool> onResult)
        {
            Scores.Add(new KeyValuePair<string, long>(platformLeaderboardId, score));
            onResult(true);
        }

        public void GetPlayerEntry(string platformLeaderboardId, Action<LeaderboardEntry?> onResult)
        {
            onResult(new LeaderboardEntry(1, 10, "Tester", "", true));
        }

        public void Load(string platformLeaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult)
        {
            List<LeaderboardEntry> entries = new List<LeaderboardEntry> { new LeaderboardEntry(1, 10, "Tester", "", true) };
            onResult(new LeaderboardPage(entries, entries[0]));
        }
    }
}
