using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesLeaderboardsProvider : ILeaderboardsProvider
    {
        public bool SupportsLoad => true;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
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
