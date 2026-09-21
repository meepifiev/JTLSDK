using System;
using System.Collections.Generic;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesLeaderboardsProvider : BridgeProviderBase, ILeaderboardsProvider
    {
        public bool SupportsLoad => true;

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
            Call("leaderboards", "playerEntry", new BridgePayload().Set("id", platformLeaderboardId), response =>
            {
                if (response.IsSuccess == false || response.GetBool("found") == false)
                {
                    onResult(null);
                    return;
                }

                onResult(ToEntry(response.Values));
            });
        }

        public void Load(string platformLeaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult)
        {
            BridgePayload payload = new BridgePayload().Set("id", platformLeaderboardId).Set("top", topCount).Set("around", aroundCount);

            Call("leaderboards", "load", payload, response =>
            {
                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
                LeaderboardEntry? current = null;

                foreach (object item in response.GetList("entries"))
                {
                    LeaderboardEntry entry = ToEntry(AsObject(item));
                    entries.Add(entry);

                    if (entry.IsCurrentPlayer)
                    {
                        current = entry;
                    }
                }

                onResult(new LeaderboardPage(entries, current));
            });
        }

        private LeaderboardEntry ToEntry(Dictionary<string, object> values)
        {
            return new LeaderboardEntry((int)GetLong(values, "rank"), GetLong(values, "score"), GetString(values, "name"), GetString(values, "avatar"), GetBool(values, "isCurrentPlayer"));
        }
    }
}
