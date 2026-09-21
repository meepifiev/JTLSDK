using System.Collections.Generic;

namespace JTLStudio.SDK
{
    public readonly struct LeaderboardPage
    {
        public LeaderboardPage(IReadOnlyList<LeaderboardEntry> entries, LeaderboardEntry? currentPlayer)
        {
            Entries = entries;
            CurrentPlayer = currentPlayer;
        }

        public IReadOnlyList<LeaderboardEntry> Entries { get; }
        public LeaderboardEntry? CurrentPlayer { get; }
    }
}
