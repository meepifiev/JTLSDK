using System;
using System.Collections.Generic;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YouTubePlayables)]
    public class YouTubePlayablesLeaderboardsProvider : BridgeProviderBase, ILeaderboardsProvider
    {
        public bool SupportsLoad => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(IsBridgeReady ? ProviderState.Ready : ProviderState.Failed);
        }

        public void SetScore(string platformLeaderboardId, long score, Action<bool> onResult)
        {
            Call("leaderboards", "setScore", new BridgePayload().Set("id", platformLeaderboardId).Set("score", score), response => onResult(response.IsSuccess));
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
