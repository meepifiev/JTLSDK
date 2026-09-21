using System;
using UnityEngine.Scripting.APIUpdating;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    [MovedFrom(false, sourceClassName: "UnsupportedShortcutProvider")]
    public class UnsupportedGameLabelProvider : IGameLabelProvider
    {
        public bool CanShow => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public void ShowDialog(Action<bool> onResult)
        {
            onResult(false);
        }
    }
}
