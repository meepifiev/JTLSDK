using System;
using System.Collections.Generic;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedLeaderboardsProvider : ILeaderboardsProvider
    {
        public bool SupportsLoad => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public void SetScore(string platformLeaderboardId, long score, Action<bool> onResult)
        {
            onResult(false);
        }

        public void GetPlayerEntry(string platformLeaderboardId, Action<LeaderboardEntry?> onResult)
        {
            onResult(null);
        }

        public void Load(string platformLeaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult)
        {
            onResult(new LeaderboardPage(new List<LeaderboardEntry>(), null));
        }
    }
}
