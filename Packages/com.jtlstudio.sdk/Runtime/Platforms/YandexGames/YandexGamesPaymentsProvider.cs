using System;
using System.Collections.Generic;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesPaymentsProvider : BridgeProviderBase, IPaymentsProvider
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
            InitializeWith("payments", "initialize", onInitialized, response =>
            {
                _products.Clear();

                foreach (object item in response.GetList("products"))
                {
                    Dictionary<string, object> product = AsObject(item);
                    decimal price = (decimal)GetDouble(product, "price");
                    _products.Add(new PlatformProduct(GetString(product, "id"), new ProductPrice(price, GetString(product, "currency"), GetString(product, "formatted"))));
                }

                ReplacePurchases(response);
            });
        }

        public void Purchase(string platformProductId, Action<PurchaseResult, PlatformPurchase> onResult)
        {
            Call("payments", "purchase", new BridgePayload().Set("id", platformProductId), response =>
            {
                if (response.IsSuccess == false)
                {
                    onResult(response.Code == BridgeResultCode.Cancelled ? PurchaseResult.Cancelled : PurchaseResult.Failed, default);
                    return;
                }

                PlatformPurchase purchase = new PlatformPurchase(response.GetString("productId"), response.GetString("token"));
                _purchases.Add(purchase);
                onResult(PurchaseResult.Purchased, purchase);
            });
        }

        public void Consume(PlatformPurchase purchase, Action<bool> onConsumed)
        {
            Call("payments", "consume", new BridgePayload().Set("token", purchase.Token), response =>
            {
                if (response.IsSuccess)
                {
                    _purchases.RemoveAll(owned => owned.Token == purchase.Token);
                }

                onConsumed(response.IsSuccess);
            });
        }

        public void RefreshPurchases(Action onRefreshed)
        {
            Call("payments", "refresh", null, response =>
            {
                if (response.IsSuccess)
                {
                    ReplacePurchases(response);
                }

                onRefreshed();
            });
        }

        private void ReplacePurchases(BridgeResponse response)
        {
            _purchases.Clear();

            foreach (object item in response.GetList("purchases"))
            {
                Dictionary<string, object> purchase = AsObject(item);
                _purchases.Add(new PlatformPurchase(GetString(purchase, "productId"), GetString(purchase, "token")));
            }
        }
    }
}
