using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class GameLabelService : ModuleBase, IGameLabel
    {
        private readonly IGameLabelProvider _provider;

        public GameLabelService(IGameLabelProvider provider, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public bool CanShow => State == ModuleState.Ready && _provider.CanShow;

        internal override string ModuleName => "GameLabel";

        public void ShowDialog(Action<bool> onResult)
        {
            if (onResult == null)
            {
                throw new ArgumentNullException(nameof(onResult));
            }

            if (CanShow == false)
            {
                onResult(false);
                return;
            }

            _provider.ShowDialog(onResult);
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }
    }
}
