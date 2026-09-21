using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class ReviewService : ModuleBase, IReview
    {
        private readonly IReviewProvider _provider;

        public ReviewService(IReviewProvider provider, SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public bool CanRequest => State == ModuleState.Ready && _provider.CanRequest;

        internal override string ModuleName => "Review";

        public void Request(Action<bool> onResult)
        {
            if (onResult == null)
            {
                throw new ArgumentNullException(nameof(onResult));
            }

            if (CanRequest == false)
            {
                onResult(false);
                return;
            }

            _provider.Request(onResult);
        }

        internal override void Initialize()
        {
            _provider.Initialize(CompleteInitialization);
        }
    }
}
