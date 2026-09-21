#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace JTLStudio.SDK.Prototype
{
    [Serializable]
    public class PrototypeSimulationSettings
    {
        public const string FilePath = "UserSettings/JTLSDKSimulation.json";

        [SerializeField] private DeviceType _deviceType = DeviceType.Desktop;
        [SerializeField] private float _initializationDelaySeconds;
        [SerializeField] private bool _simulateInitializationFailure;
        [SerializeField] private bool _askAdResult = true;
        [SerializeField] private AdResult _interstitialResult = AdResult.Shown;
        [SerializeField] private AdResult _rewardedResult = AdResult.Rewarded;
        [SerializeField] private float _adDurationSeconds = 1f;
        [SerializeField] private bool _askPurchaseResult = true;
        [SerializeField] private PurchaseResult _purchaseResult = PurchaseResult.Purchased;
        [SerializeField] private bool _authorized;
        [SerializeField] private string _playerName = "Test player";
        [SerializeField] private string _playerId = "editor-player";
        [SerializeField] private bool _simulateLoadFailure;
        [SerializeField] private bool _emptySaveOnStart;
        [SerializeField] private bool _overlayInGameView = true;
        [SerializeField] private Language _startLanguage = Language.English;

        public DeviceType DeviceType
        {
            get => _deviceType;
            set => _deviceType = value;
        }

        public float InitializationDelaySeconds
        {
            get => _initializationDelaySeconds;
            set => _initializationDelaySeconds = Mathf.Max(0f, value);
        }

        public bool SimulateInitializationFailure
        {
            get => _simulateInitializationFailure;
            set => _simulateInitializationFailure = value;
        }

        public bool AskAdResult
        {
            get => _askAdResult;
            set => _askAdResult = value;
        }

        public AdResult InterstitialResult
        {
            get => _interstitialResult;
            set => _interstitialResult = value;
        }

        public AdResult RewardedResult
        {
            get => _rewardedResult;
            set => _rewardedResult = value;
        }

        public float AdDurationSeconds
        {
            get => _adDurationSeconds;
            set => _adDurationSeconds = Mathf.Max(0f, value);
        }

        public bool AskPurchaseResult
        {
            get => _askPurchaseResult;
            set => _askPurchaseResult = value;
        }

        public PurchaseResult PurchaseResult
        {
            get => _purchaseResult;
            set => _purchaseResult = value;
        }

        public bool Authorized
        {
            get => _authorized;
            set => _authorized = value;
        }

        public string PlayerName
        {
            get => _playerName;
            set => _playerName = value ?? "";
        }

        public string PlayerId
        {
            get => _playerId;
            set => _playerId = value ?? "";
        }

        public bool SimulateLoadFailure
        {
            get => _simulateLoadFailure;
            set => _simulateLoadFailure = value;
        }

        public bool EmptySaveOnStart
        {
            get => _emptySaveOnStart;
            set => _emptySaveOnStart = value;
        }

        public bool OverlayInGameView
        {
            get => _overlayInGameView;
            set => _overlayInGameView = value;
        }

        public Language StartLanguage
        {
            get => _startLanguage;
            set => _startLanguage = value;
        }

        public bool Load()
        {
            if (File.Exists(FilePath) == false)
            {
                return false;
            }

            try
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(FilePath), this);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(FilePath);

            if (string.IsNullOrEmpty(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(FilePath, JsonUtility.ToJson(this, true));
        }
    }
}
#endif
