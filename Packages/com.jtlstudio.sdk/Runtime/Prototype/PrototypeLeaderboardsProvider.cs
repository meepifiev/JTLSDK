#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeLeaderboardsProvider : ILeaderboardsProvider
    {
        private const string StorageKeyPrefix = "JTLSDK.Prototype.Leaderboard.";
        private const int FakeEntriesAbove = 3;
        private const int FakeEntriesBelow = 3;
        private const long FakeScoreStep = 10;

        private readonly PrototypeSimulationSettings _settings;

        public PrototypeLeaderboardsProvider(bool supportsLoad, PrototypeSimulationSettings settings)
        {
            SupportsLoad = supportsLoad;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool SupportsLoad { get; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void SetScore(string platformLeaderboardId, long score, Action<bool> onResult)
        {
            PlayerPrefs.SetString(StorageKeyPrefix + platformLeaderboardId, score.ToString());
            PlayerPrefs.Save();
            onResult(true);
        }

        public void GetPlayerEntry(string platformLeaderboardId, Action<LeaderboardEntry?> onResult)
        {
            if (TryGetScore(platformLeaderboardId, out long score) == false)
            {
                onResult(null);
                return;
            }

            onResult(new LeaderboardEntry(FakeEntriesAbove + 1, score, _settings.PlayerName, "", true));
        }

        public void Load(string platformLeaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult)
        {
            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
            long playerScore = TryGetScore(platformLeaderboardId, out long score) ? score : 0;

            for (int index = 0; index < FakeEntriesAbove; index++)
            {
                entries.Add(new LeaderboardEntry(index + 1, playerScore + (FakeEntriesAbove - index) * FakeScoreStep, "Player " + (index + 1), "", false));
            }

            LeaderboardEntry current = new LeaderboardEntry(FakeEntriesAbove + 1, playerScore, _settings.PlayerName, "", true);
            entries.Add(current);

            for (int index = 0; index < FakeEntriesBelow; index++)
            {
                int rank = FakeEntriesAbove + 2 + index;
                entries.Add(new LeaderboardEntry(rank, Math.Max(0, playerScore - (index + 1) * FakeScoreStep), "Player " + rank, "", false));
            }

            onResult(new LeaderboardPage(entries, current));
        }

        private bool TryGetScore(string platformLeaderboardId, out long score)
        {
            string stored = PlayerPrefs.GetString(StorageKeyPrefix + platformLeaderboardId, "");
            return long.TryParse(stored, out score);
        }
    }
}
#endif
