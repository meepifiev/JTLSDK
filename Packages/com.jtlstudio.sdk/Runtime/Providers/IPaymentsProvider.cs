using System;
using System.Collections.Generic;

namespace JTLStudio.SDK.Providers
{
    public interface IPaymentsProvider : IProvider
    {
        IReadOnlyList<PlatformProduct> Products { get; }
        IReadOnlyList<PlatformPurchase> Purchases { get; }

        void Configure(IReadOnlyList<ProductDefinition> products, PlatformId platform);
        void Purchase(string platformProductId, Action<PurchaseResult, PlatformPurchase> onResult);
        void Consume(PlatformPurchase purchase, Action<bool> onConsumed);
        void RefreshPurchases(Action onRefreshed);
    }
}
