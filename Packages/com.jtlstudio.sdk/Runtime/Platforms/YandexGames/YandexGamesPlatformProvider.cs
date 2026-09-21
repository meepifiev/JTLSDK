using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;
using UnityEngine;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesPlatformProvider : BridgeProviderBase, IPlatformProvider
    {
        [SerializeField] private string _appId = "";

        private readonly LanguageCodes _codes = new LanguageCodes();
        private string _resolvedAppId;
        private DeviceType _deviceType = DeviceType.Desktop;
        private string _languageCode = "";

        public event Action<bool> PauseRequested;
        public event Action<bool> PlatformMuteChanged;

        public PlatformId Platform => PlatformId.YandexGames;
        public string AppId => string.IsNullOrEmpty(_resolvedAppId) ? _appId : _resolvedAppId;
        public DeviceType DeviceType => _deviceType;
        public bool SupportsPlatformMute => false;
        public bool IsPlatformMuted => false;

        public override void Attach(WebBridge bridge)
        {
            base.Attach(bridge);
            bridge.EventReceived += OnBridgeEvent;
        }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("platform", "initialize", onInitialized, response =>
            {
                _resolvedAppId = response.GetString("appId");
                _languageCode = response.GetString("language");
                _deviceType = ParseDeviceType(response.GetString("deviceType"));
            });
        }

        public void ShowContinuePrompt(Action onContinue)
        {
            string languageCode = _codes.TryParse(_languageCode, out Language language) ? _codes.ToCode(language) : "en";
            Call("platform", "continuePrompt", new BridgePayload().Set("language", languageCode), _ => onContinue?.Invoke());
        }

        private DeviceType ParseDeviceType(string value)
        {
            switch (value)
            {
                case "mobile":
                    return DeviceType.Mobile;

                case "tablet":
                    return DeviceType.Tablet;

                case "tv":
                    return DeviceType.TV;

                default:
                    return DeviceType.Desktop;
            }
        }

        private void OnBridgeEvent(BridgeEventCode code, BridgeResponse response)
        {
            switch (code)
            {
                case BridgeEventCode.Pause:
                    PauseRequested?.Invoke(true);
                    break;

                case BridgeEventCode.Resume:
                    PauseRequested?.Invoke(false);
                    break;

                case BridgeEventCode.MuteChanged:
                    PlatformMuteChanged?.Invoke(response.GetBool("muted"));
                    break;
            }
        }
    }
}
