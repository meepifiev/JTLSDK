using System;
using System.Collections.Generic;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class PaymentsTests
    {
        private const string RemoveAds = "remove_ads";
        private const string Coins = "coins_1000";

        private TestSettingsBuilder _builder;
        private FakePaymentsProvider _payments;
        private FakeDataProvider _data;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _payments = new FakePaymentsProvider();
            _payments.AddProduct(RemoveAds, 49m);
            _payments.AddProduct(Coins, 15m);
            _data = new FakeDataProvider();
            _builder = new TestSettingsBuilder { Payments = _payments, Data = _data };
            _builder.Products.Add(new ProductDefinition(RemoveAds, ProductType.NonConsumable));
            _builder.Products.Add(new ProductDefinition(Coins, ProductType.Consumable));
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void ProviderReceivesCatalogBeforeInitialization()
        {
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(2, _payments.ConfiguredProducts.Count);
        }

        [Test]
        public void PriceComesFromPlatformCatalog()
        {
            JTLSDK.Create(_builder.Build());

            Assert.IsTrue(JTLSDK.Payments.TryGetPrice(RemoveAds, out ProductPrice price));
            Assert.AreEqual(49m, price.Value);
            Assert.AreEqual("YAN", price.CurrencyCode);
            Assert.IsFalse(JTLSDK.Payments.TryGetPrice("unknown", out _));
        }

        [Test]
        public void ConsumableIsGrantedThenFlushedThenConsumed()
        {
            JTLSDK.Create(_builder.Build());
            List<string> order = new List<string>();
            JTLSDK.Payments.Granted += id => order.Add("granted:" + id + ":saves=" + _data.SaveCount);
            PurchaseResult? result = null;

            JTLSDK.Payments.Purchase(Coins, value => result = value);
            Assert.IsTrue(JTLSDK.Pause.IsPaused);

            _payments.CompletePurchase(PurchaseResult.Purchased);

            Assert.AreEqual(PurchaseResult.Purchased, result);
            Assert.IsFalse(JTLSDK.Pause.IsPaused);
            CollectionAssert.AreEqual(new[] { "granted:" + Coins + ":saves=0" }, order);
            Assert.AreEqual(1, _data.SaveCount);
            Assert.AreEqual(1, _payments.ConsumedTokens.Count);
        }

        [Test]
        public void NonConsumableIsNotConsumedAndBecomesPurchased()
        {
            JTLSDK.Create(_builder.Build());
            Assert.IsFalse(JTLSDK.Payments.IsPurchased(RemoveAds));

            JTLSDK.Payments.Purchase(RemoveAds, _ => { });
            _payments.CompletePurchase(PurchaseResult.Purchased);

            Assert.IsTrue(JTLSDK.Payments.IsPurchased(RemoveAds));
            Assert.AreEqual(0, _payments.ConsumedTokens.Count);
        }

        [Test]
        public void GrantExceptionLeavesPurchasePending()
        {
            JTLSDK.Create(_builder.Build());
            JTLSDK.Payments.Granted += _ => throw new InvalidOperationException("game bug");

            JTLSDK.Payments.Purchase(Coins, _ => { });
            _payments.CompletePurchase(PurchaseResult.Purchased);

            Assert.AreEqual(0, _payments.ConsumedTokens.Count);
            Assert.AreEqual(1, _payments.Purchases.Count);
            Assert.IsFalse(JTLSDK.Pause.IsPaused);
        }

        [Test]
        public void FailedFlushLeavesPurchasePending()
        {
            _data.SaveSucceeds = false;
            JTLSDK.Create(_builder.Build());

            JTLSDK.Payments.Purchase(Coins, _ => { });
            _payments.CompletePurchase(PurchaseResult.Purchased);

            Assert.AreEqual(0, _payments.ConsumedTokens.Count);
        }

        [Test]
        public void PendingConsumableIsGrantedWhenHandlerIsAdded()
        {
            _payments.AddOwnedPurchase(Coins);
            _payments.AddOwnedPurchase(RemoveAds);
            List<string> granted = new List<string>();

            JTLSDK.Create(_builder.Build());
            Assert.AreEqual(0, _payments.ConsumedTokens.Count);

            JTLSDK.Payments.Granted += granted.Add;

            CollectionAssert.AreEqual(new[] { Coins }, granted);
            Assert.AreEqual(1, _payments.ConsumedTokens.Count);
            Assert.IsTrue(JTLSDK.Payments.IsPurchased(RemoveAds));
        }

        [Test]
        public void PendingConsumableIsGrantedOnce()
        {
            _payments.AddOwnedPurchase(Coins);
            List<string> granted = new List<string>();

            JTLSDK.Create(_builder.Build());
            JTLSDK.Payments.Granted += granted.Add;
            JTLSDK.Payments.Granted += _ => { };

            CollectionAssert.AreEqual(new[] { Coins }, granted);
            Assert.AreEqual(1, _payments.ConsumedTokens.Count);
        }

        [Test]
        public void PurchaseWithoutHandlerIsNotConsumed()
        {
            JTLSDK.Create(_builder.Build());
            PurchaseResult? result = null;

            JTLSDK.Payments.Purchase(Coins, value => result = value);
            _payments.CompletePurchase(PurchaseResult.Purchased);

            Assert.AreEqual(PurchaseResult.Purchased, result);
            Assert.AreEqual(0, _payments.ConsumedTokens.Count);
        }

        [Test]
        public void CancelledPurchaseReturnsCancelled()
        {
            JTLSDK.Create(_builder.Build());
            PurchaseResult? result = null;
            int granted = 0;
            JTLSDK.Payments.Granted += _ => granted++;

            JTLSDK.Payments.Purchase(Coins, value => result = value);
            _payments.CompletePurchase(PurchaseResult.Cancelled);

            Assert.AreEqual(PurchaseResult.Cancelled, result);
            Assert.AreEqual(0, granted);
            Assert.IsFalse(JTLSDK.Pause.IsPaused);
        }

        [Test]
        public void UnknownProductFails()
        {
            JTLSDK.Create(_builder.Build());
            PurchaseResult? result = null;

            JTLSDK.Payments.Purchase("missing", value => result = value);

            Assert.AreEqual(PurchaseResult.Failed, result);
            Assert.IsFalse(_payments.HasPendingPurchase);
        }

        [Test]
        public void SecondPurchaseWhileFirstPendingFails()
        {
            JTLSDK.Create(_builder.Build());
            PurchaseResult? second = null;

            JTLSDK.Payments.Purchase(Coins, _ => { });
            JTLSDK.Payments.Purchase(RemoveAds, value => second = value);

            Assert.AreEqual(PurchaseResult.Failed, second);
        }

        [Test]
        public void UnsupportedProviderReturnsNotSupported()
        {
            _builder.Payments = null;
            JTLSDK.Create(_builder.Build());
            PurchaseResult? result = null;

            JTLSDK.Payments.Purchase(Coins, value => result = value);

            Assert.AreEqual(PurchaseResult.NotSupported, result);
            Assert.IsFalse(JTLSDK.Platform.Supports(Capability.Purchases));
        }
    }
}
