using System;

namespace JTLStudio.SDK
{
    public interface ILeaderboards : IModule
    {
        bool CanLoad { get; }

        void SetScore(string leaderboardId, long score);
        void GetPlayerEntry(string leaderboardId, Action<LeaderboardEntry?> onResult);
        void Load(string leaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult);
    }
}
