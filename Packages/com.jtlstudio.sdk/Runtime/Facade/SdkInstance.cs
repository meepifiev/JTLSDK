using System;
using System.Collections.Generic;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;
using JTLStudio.SDK.Services;
#if UNITY_EDITOR
using JTLStudio.SDK.Prototype;
#endif
using UnityEngine;

namespace JTLStudio.SDK
{
    public class SdkInstance
    {
        private const string RuntimeObjectName = "JTLSDK";

        private readonly JTLSDKSettings _settings;
        private readonly SdkLogger _logger;
        private readonly List<ModuleBase> _modules = new List<ModuleBase>();
        private readonly List<Action> _readyCallbacks = new List<Action>();
        private readonly PlatformService _platform;
        private readonly PauseService _pause;
        private readonly TimeService _time;
        private readonly AudioService _audio;
        private readonly DeviceService _device;
        private readonly GameplayService _gameplay;
        private readonly AdsService _ads;
        private readonly DataService _data;
        private readonly PaymentsService _payments;
        private readonly LanguageService _language;
        private readonly LeaderboardsService _leaderboards;
        private readonly PlayerService _player;
        private readonly FlagsService _flags;
        private readonly ReviewService _review;
        private readonly ShortcutService _shortcut;

        private readonly WebBridge _bridge;
        private SdkRuntimeBehaviour _behaviour;
        private float _initializationSeconds;
        private bool _initialized;
        private bool _disposed;

        public SdkInstance(JTLSDKSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = new SdkLogger(settings.LogLevel);

            SdkConfiguration configuration = settings.ActiveConfiguration;

            IPlatformProvider platformProvider = ResolveProvider(configuration?.PlatformProvider, new FallbackPlatformProvider());
            IAdsProvider adsProvider = ResolveProvider(configuration?.Ads, new UnsupportedAdsProvider());
            IDataProvider dataProvider = ResolveProvider(configuration?.Data, new UnsupportedDataProvider());
            IPaymentsProvider paymentsProvider = ResolveProvider(configuration?.Payments, new UnsupportedPaymentsProvider());
            ILanguageProvider languageProvider = ResolveProvider(configuration?.LanguageProvider, new FallbackLanguageProvider());
            IPlayerProvider playerProvider = ResolveProvider(configuration?.Player, new UnsupportedPlayerProvider());
            ILeaderboardsProvider leaderboardsProvider = ResolveProvider(configuration?.Leaderboards, new UnsupportedLeaderboardsProvider());
            IFlagsProvider flagsProvider = ResolveProvider(configuration?.Flags, new UnsupportedFlagsProvider());
            ITimeProvider timeProvider = ResolveProvider(configuration?.TimeProvider, new FallbackTimeProvider());
            IGameplayProvider gameplayProvider = ResolveProvider(configuration?.Gameplay, new FallbackGameplayProvider());
            IReviewProvider reviewProvider = ResolveProvider(configuration?.Review, new UnsupportedReviewProvider());
            IShortcutProvider shortcutProvider = ResolveProvider(configuration?.Shortcut, new UnsupportedShortcutProvider());

            bool pauseOnFocusLoss = configuration == null || configuration.PauseOnFocusLoss;
            PlatformId platformId = configuration == null ? PlatformId.Editor : configuration.Platform;

            _bridge = new WebBridge(_logger, platformId);
            AttachBridge(platformProvider, adsProvider, dataProvider, paymentsProvider, languageProvider, playerProvider, leaderboardsProvider, flagsProvider, timeProvider, gameplayProvider, reviewProvider, shortcutProvider);

#if UNITY_EDITOR
            if (settings.UsePrototypesInEditor)
            {
                PrototypeFactory prototypes = new PrototypeFactory(platformId, settings);
                platformProvider = prototypes.Platform(platformProvider);
                adsProvider = prototypes.Ads(adsProvider);
                dataProvider = prototypes.Data(dataProvider);
                paymentsProvider = prototypes.Payments(paymentsProvider);
                languageProvider = prototypes.Language(languageProvider);
                playerProvider = prototypes.Player(playerProvider);
                leaderboardsProvider = prototypes.Leaderboards(leaderboardsProvider);
                flagsProvider = prototypes.Flags(flagsProvider);
                timeProvider = prototypes.Time(timeProvider);
                gameplayProvider = prototypes.Gameplay(gameplayProvider);
                reviewProvider = prototypes.Review(reviewProvider);
                shortcutProvider = prototypes.Shortcut(shortcutProvider);
            }
#endif

            _pause = new PauseService(platformProvider, pauseOnFocusLoss, _logger);
            _time = new TimeService(timeProvider, _pause, _logger);
            _audio = new AudioService(platformProvider, _pause, _logger);
            _device = new DeviceService(platformProvider, _pause, _logger);
            _gameplay = new GameplayService(gameplayProvider, _pause, _logger);
            _ads = new AdsService(adsProvider, _pause, _gameplay, _logger);
            _data = new DataService(dataProvider, settings.AutosaveDelaySeconds, _logger);
            _player = new PlayerService(playerProvider, _data, _logger);
            _payments = new PaymentsService(paymentsProvider, _data, _pause, settings.Products, platformId, _logger);
            _language = new LanguageService(languageProvider, settings, configuration, _logger);
            _leaderboards = new LeaderboardsService(leaderboardsProvider, _player, settings.Leaderboards, platformId, _logger);
            _flags = new FlagsService(flagsProvider, settings.Flags, _logger);
            _review = new ReviewService(reviewProvider, _logger);
            _shortcut = new ShortcutService(shortcutProvider, _logger);
            _platform = new PlatformService(platformProvider, _ads, _payments, _leaderboards, _player, _flags, _time, _review, _shortcut, _logger);

            _modules.Add(_platform);
            _modules.Add(_pause);
            _modules.Add(_time);
            _modules.Add(_audio);
            _modules.Add(_device);
            _modules.Add(_gameplay);
            _modules.Add(_ads);
            _modules.Add(_data);
            _modules.Add(_player);
            _modules.Add(_payments);
            _modules.Add(_language);
            _modules.Add(_leaderboards);
            _modules.Add(_flags);
            _modules.Add(_review);
            _modules.Add(_shortcut);

            foreach (ModuleBase module in _modules)
            {
                module.StateChanged += OnModuleStateChanged;
            }
        }

        public bool IsReady { get; private set; }

        public IAds Ads => _ads;
        public IData Data => _data;
        public IPayments Payments => _payments;
        public ILanguage Language => _language;
        public IPause Pause => _pause;
        public ITime Time => _time;
        public IAudio Audio => _audio;
        public IGameplay Gameplay => _gameplay;
        public ILeaderboards Leaderboards => _leaderboards;
        public IPlayer Player => _player;
        public IFlags Flags => _flags;
        public IPlatform Platform => _platform;
        public IDevice Device => _device;
        public IReview Review => _review;
        public IShortcut Shortcut => _shortcut;

        internal IReadOnlyList<ModuleBase> Modules => _modules;

        public void WhenReady(Action onReady)
        {
            if (onReady == null)
            {
                throw new ArgumentNullException(nameof(onReady));
            }

            if (IsReady)
            {
                onReady();
                return;
            }

            _readyCallbacks.Add(onReady);
        }

        internal void Initialize()
        {
            if (_initialized)
            {
                throw new InvalidOperationException(nameof(Initialize));
            }

            _initialized = true;
            CreateRuntimeObject();
            _bridge.Connect();

            foreach (ModuleBase module in _modules)
            {
                try
                {
                    module.Initialize();
                }
                catch (Exception exception)
                {
                    _logger.Error(module.ModuleName + " failed to initialize.");
                    _logger.Exception(exception);
                    module.MarkFailed();
                }
            }

            EvaluateReadiness();
        }

        internal void Tick(float unscaledDeltaTime)
        {
            if (IsReady == false)
            {
                _initializationSeconds += unscaledDeltaTime;

                if (_initializationSeconds >= _settings.InitializationTimeoutSeconds)
                {
                    TimeOut();
                }
            }

            _data.Tick(unscaledDeltaTime);
        }

        internal void HandleApplicationFocus(bool hasFocus)
        {
            _pause.HandleApplicationFocus(hasFocus);

            if (hasFocus == false)
            {
                _data.FlushIfDirty();
            }
        }

        internal void HandleApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                _data.FlushIfDirty();
            }
        }

        internal void HandleApplicationQuit()
        {
            _data.FlushIfDirty();
        }

        internal void HandleBehaviourDestroyed()
        {
            _behaviour = null;
            JTLSDK.Destroy();
        }

        internal void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (ModuleBase module in _modules)
            {
                module.StateChanged -= OnModuleStateChanged;
                module.Dispose();
            }

            _readyCallbacks.Clear();
            _bridge.Disconnect();

            if (_behaviour != null)
            {
                GameObject runtimeObject = _behaviour.gameObject;
                _behaviour.Bind(null);
                _behaviour = null;

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(runtimeObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(runtimeObject);
                }
            }
        }

        private void AttachBridge(params object[] providers)
        {
            foreach (object provider in providers)
            {
                if (provider is IBridgeConsumer consumer)
                {
                    consumer.Attach(_bridge);
                }
            }
        }

        private T ResolveProvider<T>(T configured, T fallback) where T : class
        {
            return configured ?? fallback;
        }

        private void CreateRuntimeObject()
        {
            GameObject runtimeObject = new GameObject(RuntimeObjectName);

            if (Application.isPlaying)
            {
                runtimeObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
                UnityEngine.Object.DontDestroyOnLoad(runtimeObject);
            }
            else
            {
                runtimeObject.hideFlags = HideFlags.HideAndDontSave;
            }

            _behaviour = runtimeObject.AddComponent<SdkRuntimeBehaviour>();
            _behaviour.Bind(this);
        }

        private void TimeOut()
        {
            foreach (ModuleBase module in _modules)
            {
                if (module.State == ModuleState.Pending)
                {
                    _logger.Warning(module.ModuleName + " did not initialize within " + _settings.InitializationTimeoutSeconds + " seconds.");
                    module.MarkFailed();
                }
            }
        }

        private void EvaluateReadiness()
        {
            if (IsReady)
            {
                return;
            }

            foreach (ModuleBase module in _modules)
            {
                if (module.State == ModuleState.Pending)
                {
                    return;
                }
            }

            IsReady = true;
            _logger.Info("Ready after " + _initializationSeconds.ToString("0.00") + " seconds.");

            List<Action> callbacks = new List<Action>(_readyCallbacks);
            _readyCallbacks.Clear();

            foreach (Action callback in callbacks)
            {
                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    _logger.Exception(exception);
                }
            }
        }

        private void OnModuleStateChanged(ModuleBase module)
        {
            _logger.Info(module.ModuleName + " is " + module.State + ".");
            EvaluateReadiness();
        }
    }
}
