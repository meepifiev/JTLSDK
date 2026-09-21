using System;
using UnityEngine.Scripting.APIUpdating;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [MovedFrom(false, sourceClassName: "YandexGamesShortcutProvider")]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesGameLabelProvider : BridgeProviderBase, IGameLabelProvider
    {
        public bool CanShow { get; private set; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("shortcut", "canRequest", onInitialized, response => CanShow = response.GetBool("value"));
        }

        public void ShowDialog(Action<bool> onResult)
        {
            Call("shortcut", "request", null, response =>
            {
                CanShow = false;
                onResult(response.IsSuccess && response.GetBool("created"));
            });
        }
    }
}
