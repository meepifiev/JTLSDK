using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YouTubePlayables)]
    public class YouTubePlayablesPlatformProvider : BridgeProviderBase, IPlatformProvider
    {
        private readonly LanguageCodes _codes = new LanguageCodes();
        private DeviceType _deviceType = DeviceType.Desktop;
        private string _languageCode = "";
        private bool _muted;

        public event Action<bool> PauseRequested;
        public event Action<bool> PlatformMuteChanged;

        public PlatformId Platform => PlatformId.YouTubePlayables;
        public string AppId => "";
        public DeviceType DeviceType => _deviceType;
        public bool SupportsPlatformMute => true;
        public bool IsPlatformMuted => _muted;

        public override void Attach(WebBridge bridge)
        {
            base.Attach(bridge);
            bridge.EventReceived += OnBridgeEvent;
        }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("platform", "initialize", onInitialized, response =>
            {
                _languageCode = response.GetString("language");
                _muted = response.GetBool("audioEnabled", true) == false;
                _deviceType = response.GetString("deviceType") == "mobile" ? DeviceType.Mobile : response.GetString("deviceType") == "tablet" ? DeviceType.Tablet : DeviceType.Desktop;
            });
        }

        public void ShowContinuePrompt(Action onContinue)
        {
            string languageCode = _codes.TryParse(_languageCode, out Language language) ? _codes.ToCode(language) : "en";
            Call("platform", "continuePrompt", new BridgePayload().Set("language", languageCode), _ => onContinue?.Invoke());
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
                    _muted = response.GetBool("muted");
                    PlatformMuteChanged?.Invoke(_muted);
                    break;
            }
        }
    }
}
