using System;
using System.Collections.Generic;
using JTLStudio.SDK.Prototype;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class PrototypeRequestPresenter : IDisposable
    {
        private readonly GameViewHost _host;
        private readonly SimulationSession _session;
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
                title = "Rewarded · " + request.RewardId + " · " + request.Platform;
                options.Add(AdOption("Watched, grant reward", AdResult.Rewarded, true, request));
                options.Add(AdOption("Closed early, no reward", AdResult.Closed, false, request));
                options.Add(AdOption("Ad unavailable", AdResult.NotShown, false, request));
                options.Add(AdOption("Show failed", AdResult.Failed, false, request));
            }
            else
            {
                title = "Interstitial · " + request.Platform;
                options.Add(AdOption("Shown and closed", AdResult.Shown, true, request));
                options.Add(AdOption("Ad unavailable", AdResult.NotShown, false, request));
                options.Add(AdOption("Show failed", AdResult.Failed, false, request));
            }

            ShowOverlay(title, "Game is paused. Choose the result.", options, (option, remember) => OnAdChosen(request, option, remember));
        }

        private void ShowPurchaseOverlay(PrototypePurchaseRequest request)
        {
            string title = "Purchase · " + request.ProductId + (string.IsNullOrEmpty(request.PriceText) ? "" : " · " + request.PriceText);
            List<PrototypeRequestOption> options = new List<PrototypeRequestOption>
            {
                new PrototypeRequestOption("Pay", "Granted + PurchaseResult.Purchased", true, () => request.Complete(PurchaseResult.Purchased)),
                new PrototypeRequestOption("Pay, game crashed before grant", "Granted on next launch", false, request.CompleteAsCrashBeforeGrant),
                new PrototypeRequestOption("Cancel", "PurchaseResult.Cancelled", false, () => request.Complete(PurchaseResult.Cancelled)),
                new PrototypeRequestOption("Payment failed", "PurchaseResult.Failed", false, () => request.Complete(PurchaseResult.Failed))
            };

            ShowOverlay(title, "Game is paused. Choose the result.", options, (option, remember) => OnPurchaseChosen(request, option, remember));
        }

        private PrototypeRequestOption AdOption(string label, AdResult result, bool primary, PrototypeAdRequest request)
        {
            return new PrototypeRequestOption(label, "AdResult." + result, primary, () => request.Complete(result));
        }

        private void OnAdChosen(PrototypeAdRequest request, PrototypeRequestOption option, bool remember)
        {
            HideOverlay();

            if (remember)
            {
                AdResult result = (AdResult)Enum.Parse(typeof(AdResult), option.Result.Substring("AdResult.".Length));

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

        private Action<PrototypePurchaseRequest> RememberPurchase(PrototypeRequestOption option)
        {
            switch (option.Result)
            {
                case "Granted on next launch":
                    return request => request.CompleteAsCrashBeforeGrant();

                case "PurchaseResult.Cancelled":
                    return request => request.Complete(PurchaseResult.Cancelled);

                case "PurchaseResult.Failed":
                    return request => request.Complete(PurchaseResult.Failed);

                default:
                    return request => request.Complete(PurchaseResult.Purchased);
            }
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
            _overlay = new PrototypeRequestOverlay(title, caption, options, onChosen);
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
