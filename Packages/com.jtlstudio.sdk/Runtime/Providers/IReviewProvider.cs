using System;

namespace JTLStudio.SDK.Providers
{
    public interface IReviewProvider : IProvider
    {
        bool CanRequest { get; }

        void Request(Action<bool> onResult);
    }
}
