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
                title = "Ads.ShowRewarded(\"" + request.RewardId + "\")";
                options.Add(AdOption(AdResult.Rewarded, "watched to the end", true, request));
                options.Add(AdOption(AdResult.Closed, "closed early", false, request));
                options.Add(AdOption(AdResult.NotShown, "no ad available", false, request));
                options.Add(AdOption(AdResult.Failed, "platform error", false, request));
            }
            else
            {
                title = "Ads.ShowInterstitial()";
                options.Add(AdOption(AdResult.Shown, "shown and closed", true, request));
                options.Add(AdOption(AdResult.NotShown, "no ad available", false, request));
                options.Add(AdOption(AdResult.Failed, "platform error", false, request));
            }

            ShowOverlay(title, options, (option, remember) => OnAdChosen(request, option, remember));
        }

        private void ShowPurchaseOverlay(PrototypePurchaseRequest request)
        {
            string title = "Payments.Purchase(\"" + request.ProductId + "\")" + (string.IsNullOrEmpty(request.PriceText) ? "" : " · " + request.PriceText);
            List<PrototypeRequestOption> options = new List<PrototypeRequestOption>
            {
                new PrototypeRequestOption("PurchaseResult.Purchased", "Granted, then consumed after the save", true, () => request.Complete(PurchaseResult.Purchased)),
                new PrototypeRequestOption("PurchaseResult.Failed", "paid, Granted on next launch", false, request.CompleteAsCrashBeforeGrant),
                new PrototypeRequestOption("PurchaseResult.Cancelled", "", false, () => request.Complete(PurchaseResult.Cancelled)),
                new PrototypeRequestOption("PurchaseResult.Failed", "payment error", false, () => request.Complete(PurchaseResult.Failed))
            };

            ShowOverlay(title, options, (option, remember) => OnPurchaseChosen(request, option, remember));
        }

        private PrototypeRequestOption AdOption(AdResult result, string note, bool primary, PrototypeAdRequest request)
        {
            return new PrototypeRequestOption("AdResult." + result, note, primary, () => request.Complete(result));
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
            return (AdResult)Enum.Parse(typeof(AdResult), callback.Substring("AdResult.".Length));
        }

        private Action<PrototypePurchaseRequest> RememberPurchase(PrototypeRequestOption option)
        {
            if (option.Note.Contains("next launch"))
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

        private void ShowOverlay(string title, IReadOnlyList<PrototypeRequestOption> options, Action<PrototypeRequestOption, bool> onChosen)
        {
            HideOverlay();
            _overlay = new PrototypeRequestOverlay(title, options, onChosen);
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
