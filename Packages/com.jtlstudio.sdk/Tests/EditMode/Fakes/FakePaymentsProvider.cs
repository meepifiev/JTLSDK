using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Tests.Fakes
{
    public class FakePaymentsProvider : IPaymentsProvider
    {
        private readonly List<PlatformProduct> _products = new List<PlatformProduct>();
        private readonly List<PlatformPurchase> _purchases = new List<PlatformPurchase>();
        private string _pendingProductId;
        private Action<PurchaseResult, PlatformPurchase> _pendingResult;
        private int _tokenCounter;

        public IReadOnlyList<PlatformProduct> Products => _products;
        public IReadOnlyList<PlatformPurchase> Purchases => _purchases;
        public List<string> ConsumedTokens { get; } = new List<string>();
        public IReadOnlyList<ProductDefinition> ConfiguredProducts { get; private set; }
        public bool HasPendingPurchase => _pendingResult != null;

        public void AddProduct(string id, decimal price)
        {
            _products.Add(new PlatformProduct(id, new ProductPrice(price, "YAN", price + " YAN")));
        }

        public PlatformPurchase AddOwnedPurchase(string productId)
        {
            PlatformPurchase purchase = new PlatformPurchase(productId, NextToken());
            _purchases.Add(purchase);
            return purchase;
        }

        public void Configure(IReadOnlyList<ProductDefinition> products, PlatformId platform)
        {
            ConfiguredProducts = products;
        }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void Purchase(string platformProductId, Action<PurchaseResult, PlatformPurchase> onResult)
        {
            _pendingProductId = platformProductId;
            _pendingResult = onResult;
        }

        public void CompletePurchase(PurchaseResult result)
        {
            Action<PurchaseResult, PlatformPurchase> callback = _pendingResult;
            string productId = _pendingProductId;
            _pendingResult = null;
            _pendingProductId = null;

            if (result != PurchaseResult.Purchased)
            {
                callback?.Invoke(result, default);
                return;
            }

            PlatformPurchase purchase = new PlatformPurchase(productId, NextToken());
            _purchases.Add(purchase);
            callback?.Invoke(PurchaseResult.Purchased, purchase);
        }

        public void Consume(PlatformPurchase purchase, Action<bool> onConsumed)
        {
            ConsumedTokens.Add(purchase.Token);
            _purchases.RemoveAll(owned => owned.Token == purchase.Token);
            onConsumed(true);
        }

        public void RefreshPurchases(Action onRefreshed)
        {
            onRefreshed();
        }

        private string NextToken()
        {
            _tokenCounter++;
            return "token-" + _tokenCounter;
        }
    }
}
