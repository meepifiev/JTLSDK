#if UNITY_EDITOR
using System;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypePurchaseRequest
    {
        private readonly Action<PurchaseResult, bool> _onResult;
        private bool _completed;

        public PrototypePurchaseRequest(string productId, string priceText, PlatformId platform, Action<PurchaseResult, bool> onResult)
        {
            ProductId = productId ?? "";
            PriceText = priceText ?? "";
            Platform = platform;
            _onResult = onResult ?? throw new ArgumentNullException(nameof(onResult));
        }

        public string ProductId { get; }
        public string PriceText { get; }
        public PlatformId Platform { get; }
        public bool IsCompleted => _completed;

        public void Complete(PurchaseResult result)
        {
            Complete(result, false);
        }

        public void CompleteAsCrashBeforeGrant()
        {
            Complete(PurchaseResult.Failed, true);
        }

        private void Complete(PurchaseResult result, bool crashBeforeGrant)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _onResult(result, crashBeforeGrant);
        }
    }
}
#endif
