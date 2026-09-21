using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesShortcutProvider : BridgeProviderBase, IShortcutProvider
    {
        public bool CanRequest { get; private set; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("shortcut", "canRequest", onInitialized, response => CanRequest = response.GetBool("value"));
        }

        public void Request(Action<bool> onResult)
        {
            Call("shortcut", "request", null, response =>
            {
                CanRequest = false;
                onResult(response.IsSuccess && response.GetBool("created"));
            });
        }
    }
}
