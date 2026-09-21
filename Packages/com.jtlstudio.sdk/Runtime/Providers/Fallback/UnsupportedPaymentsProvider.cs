using System;
using System.Collections.Generic;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedPaymentsProvider : IPaymentsProvider
    {
        private readonly List<PlatformProduct> _products = new List<PlatformProduct>();
        private readonly List<PlatformPurchase> _purchases = new List<PlatformPurchase>();

        public IReadOnlyList<PlatformProduct> Products => _products;
        public IReadOnlyList<PlatformPurchase> Purchases => _purchases;

        public void Configure(IReadOnlyList<ProductDefinition> products, PlatformId platform)
        {
        }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public void Purchase(string platformProductId, Action<PurchaseResult, PlatformPurchase> onResult)
        {
            onResult(PurchaseResult.NotSupported, default);
        }

        public void Consume(PlatformPurchase purchase, Action<bool> onConsumed)
        {
            onConsumed(false);
        }

        public void RefreshPurchases(Action onRefreshed)
        {
            onRefreshed();
        }
    }
}
