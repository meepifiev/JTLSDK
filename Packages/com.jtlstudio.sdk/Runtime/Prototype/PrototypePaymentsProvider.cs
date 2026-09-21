#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypePaymentsProvider : IPaymentsProvider
    {
        private readonly PrototypeSimulationSettings _settings;
        private readonly PrototypePurchaseStore _store = new PrototypePurchaseStore();
        private readonly List<PlatformProduct> _products = new List<PlatformProduct>();
        private readonly List<PlatformPurchase> _purchases = new List<PlatformPurchase>();
        private readonly Dictionary<string, ProductDefinition> _definitions = new Dictionary<string, ProductDefinition>();
        private PlatformId _platform;

        public PrototypePaymentsProvider(PlatformId platform, PrototypeSimulationSettings settings)
        {
            _platform = platform;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IReadOnlyList<PlatformProduct> Products => _products;
        public IReadOnlyList<PlatformPurchase> Purchases => _purchases;

        public void Configure(IReadOnlyList<ProductDefinition> products, PlatformId platform)
        {
            _platform = platform;
            _products.Clear();
            _definitions.Clear();

            foreach (ProductDefinition definition in products)
            {
                string platformId = definition.PlatformIdFor(platform);
                decimal price = (decimal)definition.TestPrice;
                string formatted = price.ToString("0.##", CultureInfo.InvariantCulture) + " " + definition.TestCurrency;
                _products.Add(new PlatformProduct(platformId, new ProductPrice(price, definition.TestCurrency, formatted)));
                _definitions[platformId] = definition;
            }
        }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            _store.Load();
            RefreshPurchaseList();
            onInitialized(ProviderState.Ready);
        }

        public void Purchase(string platformProductId, Action<PurchaseResult, PlatformPurchase> onResult)
        {
            string priceText = "";

            foreach (PlatformProduct product in _products)
            {
                if (product.Id == platformProductId)
                {
                    priceText = product.Price.Formatted;
                }
            }

            if (_settings.AskPurchaseResult && PrototypeBridge.HasPurchasePresenter)
            {
                PrototypeBridge.RequestPurchase(new PrototypePurchaseRequest(platformProductId, priceText, _platform, (result, crashBeforeGrant) => OnPurchaseDecided(platformProductId, result, crashBeforeGrant, onResult)));
                return;
            }

            OnPurchaseDecided(platformProductId, _settings.PurchaseResult, false, onResult);
        }

        public void Consume(PlatformPurchase purchase, Action<bool> onConsumed)
        {
            foreach (PrototypePurchaseRecord record in _store.Records)
            {
                if (record.Token == purchase.Token)
                {
                    record.Consumed = true;
                }
            }

            _store.Save();
            RefreshPurchaseList();
            onConsumed(true);
        }

        public void RefreshPurchases(Action onRefreshed)
        {
            _store.Load();
            RefreshPurchaseList();
            onRefreshed();
        }

        private void OnPurchaseDecided(string platformProductId, PurchaseResult result, bool crashBeforeGrant, Action<PurchaseResult, PlatformPurchase> onResult)
        {
            if (result != PurchaseResult.Purchased && crashBeforeGrant == false)
            {
                onResult(result, default);
                return;
            }

            PrototypePurchaseRecord record = new PrototypePurchaseRecord(platformProductId, Guid.NewGuid().ToString("N"));
            _store.Records.Add(record);
            _store.Save();
            RefreshPurchaseList();

            if (crashBeforeGrant)
            {
                onResult(PurchaseResult.Failed, default);
                return;
            }

            onResult(PurchaseResult.Purchased, new PlatformPurchase(record.ProductId, record.Token));
        }

        private void RefreshPurchaseList()
        {
            _purchases.Clear();

            foreach (PrototypePurchaseRecord record in _store.Records)
            {
                bool consumable = _definitions.TryGetValue(record.ProductId, out ProductDefinition definition) && definition.Type == ProductType.Consumable;

                if (consumable && record.Consumed)
                {
                    continue;
                }

                _purchases.Add(new PlatformPurchase(record.ProductId, record.Token));
            }
        }
    }
}
#endif
