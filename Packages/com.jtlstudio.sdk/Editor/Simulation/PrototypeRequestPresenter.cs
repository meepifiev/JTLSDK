using System;
using System.Collections.Generic;
using JTLStudio.SDK.Prototype;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class PrototypeRequestPresenter : IDisposable
    {
        private readonly GameViewHost _host;
        private readonly SimulationSession _session;
        private readonly SimulationTexts _texts = new SimulationTexts();
        private readonly Queue<Action> _pending = new Queue<Action>();
        private AdResult? _rememberedInterstitial;
        private AdResult? _rememberedRewarded;
        private Action<PrototypePurchaseRequest> _rememberedPurchase;
        private PrototypeRequestOverlay _overlay;

        public PrototypeRequestPresenter(GameViewHost host, SimulationSession session)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            PrototypeBridge.AdRequested += OnAdRequested;
            PrototypeBridge.PurchaseRequested += OnPurchaseRequested;
        }

        public void Dispose()
        {
            PrototypeBridge.AdRequested -= OnAdRequested;
            PrototypeBridge.PurchaseRequested -= OnPurchaseRequested;
            HideOverlay();
            _pending.Clear();
        }

        private void OnAdRequested(PrototypeAdRequest request)
        {
            AdResult? remembered = request.IsRewarded ? _rememberedRewarded : _rememberedInterstitial;

            if (remembered.HasValue)
            {
                request.Complete(remembered.Value);
                return;
            }

            if (_host.HasGameView == false)
            {
                request.Complete(request.IsRewarded ? _session.Settings.RewardedResult : _session.Settings.InterstitialResult);
                return;
            }

            Enqueue(() => ShowAdOverlay(request));
        }

        private void OnPurchaseRequested(PrototypePurchaseRequest request)
        {
            if (_rememberedPurchase != null)
            {
                _rememberedPurchase(request);
                return;
            }

            if (_host.HasGameView == false)
            {
                request.Complete(_session.Settings.PurchaseResult);
                return;
            }

            Enqueue(() => ShowPurchaseOverlay(request));
        }

        private void ShowAdOverlay(PrototypeAdRequest request)
        {
            List<PrototypeRequestOption> options = new List<PrototypeRequestOption>();
            string title;

            if (request.IsRewarded)
            {
                title = "JTLSDK.Ads.ShowRewarded(\"" + request.RewardId + "\")";
                options.Add(AdOption("rewardedWatched", AdResult.Rewarded, "rewardedWatchedHint", true, request));
                options.Add(AdOption("rewardedClosed", AdResult.Closed, "rewardedClosedHint", false, request));
                options.Add(AdOption("adNotShown", AdResult.NotShown, "adNotShownHint", false, request));
                options.Add(AdOption("adFailed", AdResult.Failed, "adFailedHint", false, request));
            }
            else
            {
                title = "JTLSDK.Ads.ShowInterstitial()";
                options.Add(AdOption("interstitialShown", AdResult.Shown, "interstitialShownHint", true, request));
                options.Add(AdOption("adNotShown", AdResult.NotShown, "interstitialNotShownHint", false, request));
                options.Add(AdOption("adFailed", AdResult.Failed, "interstitialFailedHint", false, request));
            }

            ShowOverlay(title, _texts.Get("chooseAd"), options, (option, remember) => OnAdChosen(request, option, remember));
        }

        private void ShowPurchaseOverlay(PrototypePurchaseRequest request)
        {
            string title = "JTLSDK.Payments.Purchase(\"" + request.ProductId + "\")" + (string.IsNullOrEmpty(request.PriceText) ? "" : " · " + request.PriceText);
            string granted = "Granted(\"" + request.ProductId + "\")";
            List<PrototypeRequestOption> options = new List<PrototypeRequestOption>
            {
                new PrototypeRequestOption(_texts.Get("purchasePaid"), granted + " → onResult(PurchaseResult.Purchased)", _texts.Get("purchasePaidHint"), true, () => request.Complete(PurchaseResult.Purchased)),
                new PrototypeRequestOption(_texts.Get("purchaseCrash"), "onResult(PurchaseResult.Failed) → " + granted + " on next launch", _texts.Get("purchaseCrashHint"), false, request.CompleteAsCrashBeforeGrant),
                new PrototypeRequestOption(_texts.Get("purchaseCancelled"), "onResult(PurchaseResult.Cancelled)", _texts.Get("purchaseCancelledHint"), false, () => request.Complete(PurchaseResult.Cancelled)),
                new PrototypeRequestOption(_texts.Get("purchaseFailed"), "onResult(PurchaseResult.Failed)", _texts.Get("purchaseFailedHint"), false, () => request.Complete(PurchaseResult.Failed))
            };

            ShowOverlay(title, _texts.Get("choosePurchase"), options, (option, remember) => OnPurchaseChosen(request, option, remember));
        }

        private PrototypeRequestOption AdOption(string labelKey, AdResult result, string hintKey, bool primary, PrototypeAdRequest request)
        {
            return new PrototypeRequestOption(_texts.Get(labelKey), "onResult(AdResult." + result + ")", _texts.Get(hintKey), primary, () => request.Complete(result));
        }

        private void OnAdChosen(PrototypeAdRequest request, PrototypeRequestOption option, bool remember)
        {
            HideOverlay();

            if (remember)
            {
                AdResult result = ParseAdResult(option.Callback);

                if (request.IsRewarded)
                {
                    _rememberedRewarded = result;
                }
                else
                {
                    _rememberedInterstitial = result;
                }
            }

            option.OnChosen();
            ShowNext();
        }

        private void OnPurchaseChosen(PrototypePurchaseRequest request, PrototypeRequestOption option, bool remember)
        {
            HideOverlay();

            if (remember)
            {
                _rememberedPurchase = RememberPurchase(option);
            }

            option.OnChosen();
            ShowNext();
        }

        private AdResult ParseAdResult(string callback)
        {
            const string prefix = "onResult(AdResult.";
            int start = callback.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length;
            int end = callback.IndexOf(')', start);
            return (AdResult)Enum.Parse(typeof(AdResult), callback.Substring(start, end - start));
        }

        private Action<PrototypePurchaseRequest> RememberPurchase(PrototypeRequestOption option)
        {
            if (option.Callback.Contains("on next launch"))
            {
                return request => request.CompleteAsCrashBeforeGrant();
            }

            if (option.Callback.Contains("Cancelled"))
            {
                return request => request.Complete(PurchaseResult.Cancelled);
            }

            if (option.Callback.Contains("Failed"))
            {
                return request => request.Complete(PurchaseResult.Failed);
            }

            return request => request.Complete(PurchaseResult.Purchased);
        }

        private void Enqueue(Action show)
        {
            _pending.Enqueue(show);

            if (_overlay == null)
            {
                ShowNext();
            }
        }

        private void ShowNext()
        {
            if (_overlay != null || _pending.Count == 0)
            {
                return;
            }

            Action show = _pending.Dequeue();
            show();
        }

        private void ShowOverlay(string title, string caption, IReadOnlyList<PrototypeRequestOption> options, Action<PrototypeRequestOption, bool> onChosen)
        {
            HideOverlay();
            _overlay = new PrototypeRequestOverlay(title, caption, _texts.Get("remember"), options, onChosen);
            _host.Attach(_overlay);
        }

        private void HideOverlay()
        {
            if (_overlay == null)
            {
                return;
            }

            _host.Detach(_overlay);
            _overlay = null;
        }
    }
}
