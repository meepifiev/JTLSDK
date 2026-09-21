using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class LeaderboardsService : ModuleBase, ILeaderboards
    {
        private readonly ILeaderboardsProvider _provider;
        private readonly PlayerService _player;
        private readonly IReadOnlyList<LeaderboardDefinition> _definitions;
        private readonly PlatformId _platform;
        private readonly Dictionary<string, long> _deferredScores = new Dictionary<string, long>();

        public LeaderboardsService(
            ILeaderboardsProvider provider,
            PlayerService player,
            IReadOnlyList<LeaderboardDefinition> definitions,
            PlatformId platform,
            SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            _platform = platform;
            _player.Authorized += OnPlayerAuthorized;
        }

        public bool CanLoad => State == ModuleState.Ready && _provider.SupportsLoad;

        internal override string ModuleName => "Leaderboards";

        public void SetScore(string leaderboardId, long score)
        {
            string platformLeaderboardId = ResolvePlatformId(leaderboardId);

            if (State != ModuleState.Ready)
            {
                Logger.Warning("Leaderboards.SetScore was ignored because the module is " + State + ".");
                return;
            }

            if (_player.IsSupported && _player.IsAuthorized == false)
            {
                _deferredScores[platformLeaderboardId] = score;
                Logger.Info("Score for '" + leaderboardId + "' is deferred until the player authorizes.");
                return;
            }

            _provider.SetScore(platformLeaderboardId, score, success => OnScoreSet(leaderboardId, success));
        }

        public void GetPlayerEntry(string leaderboardId, Action<LeaderboardEntry?> onResult)
        {
            if (onResult == null)
            {
                throw new ArgumentNullException(nameof(onResult));
            }

            string platformLeaderboardId = ResolvePlatformId(leaderboardId);

            if (State != ModuleState.Ready)
            {
                onResult(null);
                return;
            }

            _provider.GetPlayerEntry(platformLeaderboardId, onResult);
        }

        public void Load(string leaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult)
        {
            if (onResult == null)
            {
                throw new ArgumentNullException(nameof(onResult));
            }

            string platformLeaderboardId = ResolvePlatformId(leaderboardId);

            if (CanLoad == false)
            {
                onResult(new LeaderboardPage(new List<LeaderboardEntry>(), null));
                return;
            }

            _provider.Load(platformLeaderboardId, topCount, aroundCount, onResult);
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }

        internal override void Dispose()
        {
            _player.Authorized -= OnPlayerAuthorized;
        }

        private string ResolvePlatformId(string leaderboardId)
        {
            if (string.IsNullOrEmpty(leaderboardId))
            {
                throw new ArgumentException(nameof(leaderboardId));
            }

            foreach (LeaderboardDefinition definition in _definitions)
            {
                if (definition.Id == leaderboardId)
                {
                    return definition.PlatformIdFor(_platform);
                }
            }

            return leaderboardId;
        }

        private void OnScoreSet(string leaderboardId, bool success)
        {
            if (success == false)
            {
                Logger.Warning("The platform rejected the score for '" + leaderboardId + "'.");
            }
        }

        private void OnPlayerAuthorized()
        {
            if (State != ModuleState.Ready)
            {
                return;
            }

            foreach (KeyValuePair<string, long> deferred in new List<KeyValuePair<string, long>>(_deferredScores))
            {
                _provider.SetScore(deferred.Key, deferred.Value, success => OnScoreSet(deferred.Key, success));
            }

            _deferredScores.Clear();
        }
    }
}
