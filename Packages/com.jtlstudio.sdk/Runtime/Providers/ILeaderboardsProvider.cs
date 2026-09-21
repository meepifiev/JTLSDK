using System;

namespace JTLStudio.SDK.Providers
{
    public interface ILeaderboardsProvider : IProvider
    {
        bool SupportsLoad { get; }

        void SetScore(string platformLeaderboardId, long score, Action<bool> onResult);
        void GetPlayerEntry(string platformLeaderboardId, Action<LeaderboardEntry?> onResult);
        void Load(string platformLeaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult);
    }
}
