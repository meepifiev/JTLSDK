using System;

namespace JTLStudio.SDK
{
    public interface IPayments : IModule
    {
        event Action<string> Granted;

        bool IsPurchased(string productId);
        bool TryGetPrice(string productId, out ProductPrice price);
        void Purchase(string productId, Action<PurchaseResult> onResult);
    }
}
