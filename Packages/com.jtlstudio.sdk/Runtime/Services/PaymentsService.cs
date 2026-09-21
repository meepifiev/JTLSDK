using System;
using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Services
{
    public class PaymentsService : ModuleBase, IPayments
    {
        private readonly IPaymentsProvider _provider;
        private readonly DataService _data;
        private readonly PauseService _pause;
        private readonly IReadOnlyList<ProductDefinition> _catalog;
        private readonly PlatformId _platform;
        private bool _purchaseInProgress;

        public PaymentsService(
            IPaymentsProvider provider,
            DataService data,
            PauseService pause,
            IReadOnlyList<ProductDefinition> catalog,
            PlatformId platform,
            SdkLogger logger) : base(logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _platform = platform;
        }

        public event Action<string> Granted;

        internal override string ModuleName => "Payments";

        public bool IsPurchased(string productId)
        {
            ProductDefinition definition = FindDefinition(productId);

            if (definition == null || State != ModuleState.Ready)
            {
                return false;
            }

            string platformProductId = definition.PlatformIdFor(_platform);

            foreach (PlatformPurchase purchase in _provider.Purchases)
            {
                if (purchase.ProductId == platformProductId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPrice(string productId, out ProductPrice price)
        {
            price = default;
            ProductDefinition definition = FindDefinition(productId);

            if (definition == null || State != ModuleState.Ready)
            {
                return false;
            }

            string platformProductId = definition.PlatformIdFor(_platform);

            foreach (PlatformProduct product in _provider.Products)
            {
                if (product.Id == platformProductId)
                {
                    price = product.Price;
                    return true;
                }
            }

            return false;
        }

        public void Purchase(string productId, Action<PurchaseResult> onResult)
        {
            if (onResult == null)
            {
                throw new ArgumentNullException(nameof(onResult));
            }

            if (IsReady == false)
            {
                onResult(PurchaseResult.NotReady);
                return;
            }

            if (IsSupported == false || State != ModuleState.Ready)
            {
                onResult(PurchaseResult.NotSupported);
                return;
            }

            ProductDefinition definition = FindDefinition(productId);

            if (definition == null)
            {
                Logger.Error("Product '" + productId + "' is not declared in the catalog.");
                onResult(PurchaseResult.Failed);
                return;
            }

            if (_purchaseInProgress)
            {
                Logger.Warning("A purchase is already in progress.");
                onResult(PurchaseResult.Failed);
                return;
            }

            _purchaseInProgress = true;
            IDisposable pauseHold = _pause.Hold(PauseSources.Purchase);

            try
            {
                _provider.Purchase(definition.PlatformIdFor(_platform), (result, purchase) => OnPurchased(definition, pauseHold, result, purchase, onResult));
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
                OnPurchased(definition, pauseHold, PurchaseResult.Failed, default, onResult);
            }
        }

        internal override void Initialize()
        {
            _provider.Configure(_catalog, _platform);
            _provider.Initialize(OnProviderInitialized);
        }

        private void OnProviderInitialized(ProviderState state)
        {
            CompleteInitialization(state);

            if (state == ProviderState.Ready)
            {
                _data.WhenReady(RestorePending);
            }
        }

        private void RestorePending()
        {
            foreach (PlatformPurchase purchase in new List<PlatformPurchase>(_provider.Purchases))
            {
                ProductDefinition definition = FindDefinitionByPlatformId(purchase.ProductId);

                if (definition != null && definition.Type == ProductType.Consumable)
                {
                    Grant(definition, purchase, null);
                }
            }
        }

        private void OnPurchased(ProductDefinition definition, IDisposable pauseHold, PurchaseResult result, PlatformPurchase purchase, Action<PurchaseResult> onResult)
        {
            _purchaseInProgress = false;
            pauseHold.Dispose();

            if (result != PurchaseResult.Purchased)
            {
                onResult(result);
                return;
            }

            Grant(definition, purchase, () => onResult(PurchaseResult.Purchased));
        }

        private void Grant(ProductDefinition definition, PlatformPurchase purchase, Action onDone)
        {
            try
            {
                Granted?.Invoke(definition.Id);
            }
            catch (Exception exception)
            {
                Logger.Error("Granting '" + definition.Id + "' threw. The purchase stays pending and is granted again on the next launch.");
                Logger.Exception(exception);
                onDone?.Invoke();
                return;
            }

            _data.Flush(success => OnGrantFlushed(definition, purchase, success, onDone));
        }

        private void OnGrantFlushed(ProductDefinition definition, PlatformPurchase purchase, bool success, Action onDone)
        {
            if (definition.Type != ProductType.Consumable)
            {
                onDone?.Invoke();
                return;
            }

            if (success == false)
            {
                Logger.Warning("Save data was not written after granting '" + definition.Id + "'. The purchase is not consumed.");
                onDone?.Invoke();
                return;
            }

            _provider.Consume(purchase, consumed => OnConsumed(definition, consumed, onDone));
        }

        private void OnConsumed(ProductDefinition definition, bool consumed, Action onDone)
        {
            if (consumed == false)
            {
                Logger.Warning("The platform did not consume '" + definition.Id + "'.");
            }

            onDone?.Invoke();
        }

        private ProductDefinition FindDefinition(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                throw new ArgumentException(nameof(productId));
            }

            foreach (ProductDefinition definition in _catalog)
            {
                if (definition.Id == productId)
                {
                    return definition;
                }
            }

            return null;
        }

        private ProductDefinition FindDefinitionByPlatformId(string platformProductId)
        {
            foreach (ProductDefinition definition in _catalog)
            {
                if (definition.PlatformIdFor(_platform) == platformProductId)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
