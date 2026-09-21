namespace JTLStudio.SDK
{
    public readonly struct LeaderboardEntry
    {
        public LeaderboardEntry(int rank, long score, string playerName, string avatarUrl, bool isCurrentPlayer)
        {
            Rank = rank;
            Score = score;
            PlayerName = playerName;
            AvatarUrl = avatarUrl;
            IsCurrentPlayer = isCurrentPlayer;
        }

        public int Rank { get; }
        public long Score { get; }
        public string PlayerName { get; }
        public string AvatarUrl { get; }
        public bool IsCurrentPlayer { get; }
    }
}
