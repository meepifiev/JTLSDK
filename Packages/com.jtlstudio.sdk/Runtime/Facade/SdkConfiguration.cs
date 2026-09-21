using System.Collections.Generic;
using JTLStudio.SDK.Providers;
using UnityEngine;
using UnityEngine.Serialization;

namespace JTLStudio.SDK
{
    public class SdkConfiguration : ScriptableObject
    {
        [SerializeField] private string _displayName = "";
        [SerializeField] private PlatformId _platform = PlatformId.Editor;
        [SerializeField] private string _defineSymbol = "";
        [SerializeField] private bool _pauseOnFocusLoss = true;
        [SerializeField] private List<Language> _languages = new List<Language> { Language.English };
        [SerializeField] private PlayerSettingsPreset _playerSettings = new PlayerSettingsPreset();
        [SerializeReference] private IPlatformProvider _platformProvider;
        [SerializeReference] private IAdsProvider _ads;
        [SerializeReference] private IDataProvider _data;
        [SerializeReference] private IPaymentsProvider _payments;
        [SerializeReference] private ILanguageProvider _languageProvider;
        [SerializeReference] private IPlayerProvider _player;
        [SerializeReference] private ILeaderboardsProvider _leaderboards;
        [SerializeReference] private IFlagsProvider _flags;
        [SerializeReference] private ITimeProvider _timeProvider;
        [SerializeReference, FormerlySerializedAs("_gameplay")] private IGameEventsProvider _gameEvents;
        [SerializeReference] private IReviewProvider _review;
        [SerializeReference, FormerlySerializedAs("_shortcut")] private IGameLabelProvider _gameLabel;

        public string DisplayName
        {
            get => _displayName;
            internal set => _displayName = value;
        }

        public PlatformId Platform
        {
            get => _platform;
            internal set => _platform = value;
        }

        public string DefineSymbol
        {
            get => _defineSymbol;
            internal set => _defineSymbol = value;
        }

        public bool PauseOnFocusLoss
        {
            get => _pauseOnFocusLoss;
            internal set => _pauseOnFocusLoss = value;
        }

        public List<Language> Languages => _languages;
        public PlayerSettingsPreset PlayerSettings => _playerSettings;

        public IPlatformProvider PlatformProvider
        {
            get => _platformProvider;
            internal set => _platformProvider = value;
        }

        public IAdsProvider Ads
        {
            get => _ads;
            internal set => _ads = value;
        }

        public IDataProvider Data
        {
            get => _data;
            internal set => _data = value;
        }

        public IPaymentsProvider Payments
        {
            get => _payments;
            internal set => _payments = value;
        }

        public ILanguageProvider LanguageProvider
        {
            get => _languageProvider;
            internal set => _languageProvider = value;
        }

        public IPlayerProvider Player
        {
            get => _player;
            internal set => _player = value;
        }

        public ILeaderboardsProvider Leaderboards
        {
            get => _leaderboards;
            internal set => _leaderboards = value;
        }

        public IFlagsProvider Flags
        {
            get => _flags;
            internal set => _flags = value;
        }

        public ITimeProvider TimeProvider
        {
            get => _timeProvider;
            internal set => _timeProvider = value;
        }

        public IGameEventsProvider GameEvents
        {
            get => _gameEvents;
            internal set => _gameEvents = value;
        }

        public IReviewProvider Review
        {
            get => _review;
            internal set => _review = value;
        }

        public IGameLabelProvider GameLabel
        {
            get => _gameLabel;
            internal set => _gameLabel = value;
        }
    }
}
