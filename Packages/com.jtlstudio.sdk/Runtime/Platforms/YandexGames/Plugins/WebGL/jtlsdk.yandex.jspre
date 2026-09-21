// JTL SDK bridge (yandex). Generated from Bridge~/src, do not edit.
"use strict";
(() => {
  var __defProp = Object.defineProperty;
  var __getOwnPropSymbols = Object.getOwnPropertySymbols;
  var __hasOwnProp = Object.prototype.hasOwnProperty;
  var __propIsEnum = Object.prototype.propertyIsEnumerable;
  var __defNormalProp = (obj, key, value) => key in obj ? __defProp(obj, key, { enumerable: true, configurable: true, writable: true, value }) : obj[key] = value;
  var __spreadValues = (a, b) => {
    for (var prop in b || (b = {}))
      if (__hasOwnProp.call(b, prop))
        __defNormalProp(a, prop, b[prop]);
    if (__getOwnPropSymbols)
      for (var prop of __getOwnPropSymbols(b)) {
        if (__propIsEnum.call(b, prop))
          __defNormalProp(a, prop, b[prop]);
      }
    return a;
  };

  // src/core/invoker.ts
  var CallbackInvoker = class {
    constructor() {
      this.pointer = 0;
    }
    get isRegistered() {
      return this.pointer !== 0;
    }
    register(pointer) {
      this.pointer = pointer;
    }
    invoke(requestId, code, payload) {
      const size = lengthBytesUTF8(payload) + 1;
      const buffer = _malloc(size);
      stringToUTF8(payload, buffer, size);
      try {
        const entry = this.resolve();
        entry(requestId, code, buffer);
      } finally {
        _free(buffer);
      }
    }
    resolve() {
      if (typeof getWasmTableEntry === "function") {
        const entry = getWasmTableEntry(this.pointer);
        if (entry !== void 0) {
          return entry;
        }
      }
      if (typeof wasmTable !== "undefined" && wasmTable !== void 0) {
        const entry = wasmTable.get(this.pointer);
        if (entry !== void 0) {
          return entry;
        }
      }
      const dynamic = Module["dynCall_viii"];
      if (typeof dynamic === "function") {
        const pointer2 = this.pointer;
        return (requestId, code, payloadPointer) => dynamic(pointer2, requestId, code, payloadPointer);
      }
      const pointer = this.pointer;
      return (requestId, code, payloadPointer) => dynCall("viii", pointer, [requestId, code, payloadPointer]);
    }
  };

  // src/core/types.ts
  var ResultCode = {
    Ok: 0,
    Unavailable: 1,
    InvalidParams: 2,
    SizeLimit: 3,
    Cancelled: 4,
    Unknown: 5,
    UnknownAction: 6
  };
  var EventCode = {
    Pause: 1,
    Resume: 2,
    MuteChanged: 3,
    FocusChanged: 4
  };
  var BridgeError = class extends Error {
    constructor(code, message) {
      super(message);
      this.code = code;
    }
  };

  // src/core/bridge.ts
  var Bridge = class {
    constructor(platform) {
      this.available = true;
      this.invoker = new CallbackInvoker();
      this.queued = [];
      this.platform = platform;
    }
    register(pointer) {
      this.invoker.register(pointer);
      while (this.queued.length > 0) {
        const message = this.queued.shift();
        this.deliver(message.requestId, message.code, message.payload);
      }
    }
    call(moduleName, action, payloadText, requestId) {
      let result;
      try {
        const handler = this.actionHandler(moduleName, action);
        result = handler(this.parse(payloadText));
      } catch (error) {
        this.fail(requestId, error);
        return;
      }
      Promise.resolve(result).then((value) => this.emit(requestId, ResultCode.Ok, value)).catch((error) => this.fail(requestId, error));
    }
    query(moduleName, action, payloadText) {
      var _a;
      try {
        const adapter = this.platform.modules[moduleName];
        const handler = (_a = adapter == null ? void 0 : adapter.queries) == null ? void 0 : _a[action];
        if (handler === void 0) {
          throw new BridgeError(ResultCode.UnknownAction, `${moduleName}.${action}`);
        }
        return this.serialize({ code: ResultCode.Ok, value: handler(this.parse(payloadText)) });
      } catch (error) {
        const bridgeError = this.toBridgeError(error);
        return this.serialize({ code: bridgeError.code, message: bridgeError.message });
      }
    }
    event(code, payload) {
      this.emit(0, code, payload);
    }
    actionHandler(moduleName, action) {
      const adapter = this.platform.modules[moduleName];
      const handler = adapter == null ? void 0 : adapter.actions[action];
      if (handler === void 0) {
        throw new BridgeError(ResultCode.UnknownAction, `${moduleName}.${action}`);
      }
      return handler;
    }
    emit(requestId, code, value) {
      this.deliver(requestId, code, this.serialize(value === void 0 ? {} : value));
    }
    fail(requestId, error) {
      const bridgeError = this.toBridgeError(error);
      this.deliver(requestId, bridgeError.code, this.serialize({ message: bridgeError.message }));
    }
    deliver(requestId, code, payload) {
      if (this.invoker.isRegistered === false) {
        this.queued.push({ requestId, code, payload });
        return;
      }
      try {
        this.invoker.invoke(requestId, code, payload);
      } catch (error) {
        console.error("[JTL SDK] Failed to deliver a message to the game.", error);
      }
    }
    parse(payloadText) {
      if (payloadText === void 0 || payloadText === null || payloadText === "") {
        return {};
      }
      const parsed = JSON.parse(payloadText);
      if (parsed !== null && typeof parsed === "object" && Array.isArray(parsed) === false) {
        return parsed;
      }
      throw new BridgeError(ResultCode.InvalidParams, "Payload must be a JSON object.");
    }
    serialize(value) {
      try {
        return JSON.stringify(value === void 0 ? {} : value);
      } catch (e) {
        return "{}";
      }
    }
    toBridgeError(error) {
      if (error instanceof BridgeError) {
        return error;
      }
      const message = error instanceof Error ? error.message : String(error);
      return new BridgeError(ResultCode.Unknown, message);
    }
  };
  function install(platform) {
    var _a;
    const bridge = new Bridge(platform);
    platform.bind((code, payload) => bridge.event(code, payload));
    Module.JTLSDKBridges = (_a = Module.JTLSDKBridges) != null ? _a : {};
    Module.JTLSDKBridges[platform.name] = bridge;
    return bridge;
  }

  // src/core/adblock.ts
  var BaitClasses = "adsbox ad adsbygoogle ad-banner ad-unit text-ad pub_300x250";
  var DetectionDelayMilliseconds = 100;
  function detectAdBlock() {
    if (typeof document === "undefined" || document.body === null) {
      return Promise.resolve(false);
    }
    const bait = document.createElement("div");
    bait.className = BaitClasses;
    bait.style.position = "absolute";
    bait.style.left = "-10000px";
    bait.style.top = "-10000px";
    bait.style.width = "1px";
    bait.style.height = "1px";
    bait.style.pointerEvents = "none";
    bait.innerHTML = "&nbsp;";
    document.body.appendChild(bait);
    return new Promise((resolve) => {
      window.setTimeout(() => {
        const style = window.getComputedStyle(bait);
        const blocked = bait.offsetParent === null || bait.offsetHeight === 0 || bait.offsetWidth === 0 || style.display === "none" || style.visibility === "hidden";
        bait.remove();
        resolve(blocked);
      }, DetectionDelayMilliseconds);
    });
  }

  // src/core/continuePrompt.ts
  var OverlayId = "jtlsdk-continue-prompt";
  var Messages = {
    en: "Click this area to continue.",
    ru: "\u0427\u0442\u043E\u0431\u044B \u043F\u0440\u043E\u0434\u043E\u043B\u0436\u0438\u0442\u044C, \u043D\u0430\u0436\u043C\u0438 \u043D\u0430 \u044D\u0442\u0443 \u043E\u0431\u043B\u0430\u0441\u0442\u044C.",
    tr: "Devam etmek i\xE7in bu alana t\u0131kla."
  };
  function showContinuePrompt(languageCode) {
    return new Promise((resolve) => {
      var _a;
      const existing = document.getElementById(OverlayId);
      if (existing !== null) {
        existing.remove();
      }
      const overlay = document.createElement("div");
      overlay.id = OverlayId;
      overlay.setAttribute("role", "button");
      overlay.tabIndex = 0;
      overlay.style.cssText = "position:fixed;inset:0;display:flex;align-items:center;justify-content:center;background:rgba(0,0,0,0.3);color:#F2F2F3;font:500 clamp(18px,3vw,32px) Inter,system-ui,sans-serif;text-align:center;cursor:pointer;z-index:2147483647;user-select:none;";
      overlay.textContent = (_a = Messages[languageCode]) != null ? _a : Messages.en;
      const close = () => {
        overlay.remove();
        window.focus();
        resolve();
      };
      overlay.addEventListener("click", close);
      overlay.addEventListener("keydown", (event) => {
        if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          close();
        }
      });
      document.body.appendChild(overlay);
      overlay.focus();
    });
  }

  // src/core/script.ts
  var pending = /* @__PURE__ */ new Map();
  function loadScript(url) {
    const existing = pending.get(url);
    if (existing !== void 0) {
      return existing;
    }
    const promise = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = url;
      script.async = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error(`Failed to load ${url}`));
      document.head.appendChild(script);
    });
    pending.set(url, promise);
    return promise;
  }

  // src/platforms/yandex.ts
  var SdkScriptUrl = "/sdk.js";
  var DataKey = "jtlsdk";
  var YandexPlatform = class {
    constructor() {
      this.name = "yandex";
      this.sdkPromise = null;
      this.playerPromise = null;
      this.paymentsPromise = null;
      this.emit = () => void 0;
      this.eventsBound = false;
      this.modules = {
        platform: {
          actions: {
            initialize: () => this.initializePlatform(),
            continuePrompt: (payload) => showContinuePrompt(this.text(payload, "language", "en"))
          }
        },
        ads: {
          actions: {
            showInterstitial: (payload) => this.showInterstitial(payload),
            showRewarded: (payload) => this.showRewarded(payload),
            showBanner: () => this.sdk().then((sdk) => sdk.adv.showBannerAdv()).then((status) => ({ visible: (status == null ? void 0 : status.stickyAdvIsShowing) === true })),
            hideBanner: () => this.sdk().then((sdk) => sdk.adv.hideBannerAdv()).then(() => ({})),
            bannerStatus: () => this.sdk().then((sdk) => sdk.adv.getBannerAdvStatus()).then((status) => ({ visible: status.stickyAdvIsShowing }))
          }
        },
        data: {
          actions: {
            load: () => this.loadData(),
            save: (payload) => this.saveData(payload)
          }
        },
        payments: {
          actions: {
            initialize: () => this.initializePayments(),
            purchase: (payload) => this.purchase(payload),
            consume: (payload) => this.payments().then((payments) => payments.consumePurchase(this.text(payload, "token"))).then(() => ({ consumed: true })),
            refresh: () => this.payments().then((payments) => payments.getPurchases()).then((purchases) => ({ purchases: purchases.map(this.mapPurchase) }))
          }
        },
        player: {
          actions: {
            info: () => this.playerInfo(),
            authorize: () => this.authorize()
          }
        },
        leaderboards: {
          actions: {
            setScore: (payload) => this.leaderboards().then((boards) => {
              var _a;
              return boards.setScore(this.text(payload, "id"), Number((_a = payload.score) != null ? _a : 0));
            }).then(() => ({})),
            playerEntry: (payload) => this.playerEntry(payload),
            load: (payload) => this.loadLeaderboard(payload)
          }
        },
        flags: {
          actions: {
            get: (payload) => this.sdk().then((sdk) => {
              var _a;
              return sdk.getFlags({ defaultFlags: (_a = payload.defaults) != null ? _a : {} });
            }).then((flags) => ({ flags }))
          }
        },
        time: {
          actions: {
            server: () => this.sdk().then((sdk) => ({ milliseconds: sdk.serverTime() }))
          }
        },
        gameplay: {
          actions: {
            ready: () => this.sdk().then((sdk) => {
              var _a;
              (_a = sdk.features.LoadingAPI) == null ? void 0 : _a.ready();
              return {};
            }),
            start: () => this.sdk().then((sdk) => {
              var _a;
              (_a = sdk.features.GameplayAPI) == null ? void 0 : _a.start();
              return {};
            }),
            stop: () => this.sdk().then((sdk) => {
              var _a;
              (_a = sdk.features.GameplayAPI) == null ? void 0 : _a.stop();
              return {};
            })
          }
        },
        review: {
          actions: {
            canRequest: () => this.sdk().then((sdk) => sdk.feedback.canReview()).then((result) => ({ value: result.value })),
            request: () => this.sdk().then((sdk) => sdk.feedback.requestReview()).then((result) => ({ sent: result.feedbackSent }))
          }
        },
        shortcut: {
          actions: {
            canRequest: () => this.sdk().then((sdk) => sdk.shortcut.canShowPrompt()).then((result) => ({ value: result.canShow })),
            request: () => this.sdk().then((sdk) => sdk.shortcut.showPrompt()).then((result) => ({ created: result.outcome === "accepted" }))
          }
        }
      };
    }
    bind(emit) {
      this.emit = emit;
    }
    sdk() {
      if (this.sdkPromise !== null) {
        return this.sdkPromise;
      }
      const preload = window.JTLSDK_PAGE;
      if ((preload == null ? void 0 : preload.ysdk) !== void 0) {
        this.sdkPromise = Promise.resolve(preload.ysdk);
      } else {
        this.sdkPromise = this.loadSdk();
      }
      this.sdkPromise = this.sdkPromise.then((sdk) => {
        this.bindEvents(sdk);
        return sdk;
      });
      return this.sdkPromise;
    }
    async loadSdk() {
      if (typeof YaGames === "undefined") {
        await loadScript(SdkScriptUrl);
      }
      if (typeof YaGames === "undefined") {
        throw new BridgeError(ResultCode.Unavailable, "Yandex Games SDK is not available.");
      }
      return YaGames.init();
    }
    bindEvents(sdk) {
      if (this.eventsBound) {
        return;
      }
      this.eventsBound = true;
      sdk.on("game_api_pause", () => this.emit(EventCode.Pause, {}));
      sdk.on("game_api_resume", () => this.emit(EventCode.Resume, {}));
    }
    player() {
      if (this.playerPromise === null) {
        this.playerPromise = this.sdk().then((sdk) => sdk.getPlayer());
      }
      return this.playerPromise;
    }
    payments() {
      if (this.paymentsPromise === null) {
        this.paymentsPromise = this.sdk().then((sdk) => sdk.getPayments({ signed: false }));
      }
      return this.paymentsPromise;
    }
    leaderboards() {
      return this.sdk().then((sdk) => sdk.leaderboards);
    }
    async initializePlatform() {
      const sdk = await this.sdk();
      return {
        appId: sdk.environment.app.id,
        deviceType: sdk.deviceInfo.type,
        language: sdk.environment.i18n.lang,
        serverTime: sdk.serverTime()
      };
    }
    async showInterstitial(payload) {
      if (payload.adBlockDetection === true && await detectAdBlock()) {
        return { result: "blocked" };
      }
      const sdk = await this.sdk();
      return new Promise((resolve, reject) => {
        sdk.adv.showFullscreenAdv({
          callbacks: {
            onClose: (wasShown) => resolve({ result: wasShown ? "shown" : "notShown" }),
            onOffline: () => resolve({ result: "notShown" }),
            onError: (error) => reject(new BridgeError(ResultCode.Unknown, String(error)))
          }
        });
      });
    }
    async showRewarded(payload) {
      if (payload.adBlockDetection === true && await detectAdBlock()) {
        return { result: "blocked" };
      }
      const sdk = await this.sdk();
      return new Promise((resolve, reject) => {
        let rewarded = false;
        sdk.adv.showRewardedVideo({
          callbacks: {
            onRewarded: () => {
              rewarded = true;
            },
            onClose: () => resolve({ result: rewarded ? "rewarded" : "closed" }),
            onError: (error) => reject(new BridgeError(ResultCode.Unknown, String(error)))
          }
        });
      });
    }
    async loadData() {
      const player = await this.player();
      const data = await player.getData([DataKey]);
      const value = data[DataKey];
      if (typeof value !== "string" || value.length === 0) {
        return { result: "empty", data: "" };
      }
      return { result: "loaded", data: value };
    }
    async saveData(payload) {
      const player = await this.player();
      await player.setData({ [DataKey]: this.text(payload, "data") }, true);
      return { saved: true };
    }
    async initializePayments() {
      const payments = await this.payments();
      const [catalog, purchases] = await Promise.all([payments.getCatalog(), payments.getPurchases()]);
      return {
        products: catalog.map((product) => ({
          id: product.id,
          price: Number(product.priceValue),
          currency: product.priceCurrencyCode,
          formatted: product.price
        })),
        purchases: purchases.map(this.mapPurchase)
      };
    }
    async purchase(payload) {
      const payments = await this.payments();
      try {
        const purchase = await payments.purchase({ id: this.text(payload, "id") });
        return this.mapPurchase(purchase);
      } catch (error) {
        throw new BridgeError(ResultCode.Cancelled, error instanceof Error ? error.message : String(error));
      }
    }
    mapPurchase(purchase) {
      return { productId: purchase.productID, token: purchase.purchaseToken };
    }
    async playerInfo() {
      const player = await this.player();
      const authorized = player.isAuthorized();
      return {
        authorized,
        id: authorized ? player.getUniqueID() : "",
        name: authorized ? player.getName() : "",
        avatar: authorized ? player.getPhoto("medium") : ""
      };
    }
    async authorize() {
      const sdk = await this.sdk();
      try {
        await sdk.auth.openAuthDialog();
      } catch (e) {
        return this.playerInfo();
      }
      this.playerPromise = null;
      return this.playerInfo();
    }
    async playerEntry(payload) {
      const boards = await this.leaderboards();
      try {
        const entry = await boards.getPlayerEntry(this.text(payload, "id"));
        return __spreadValues({ found: true }, this.mapEntry(entry, true));
      } catch (e) {
        return { found: false };
      }
    }
    async loadLeaderboard(payload) {
      var _a, _b;
      const boards = await this.leaderboards();
      const result = await boards.getEntries(this.text(payload, "id"), {
        quantityTop: Math.min(20, Math.max(1, Number((_a = payload.top) != null ? _a : 10))),
        quantityAround: Math.min(10, Math.max(1, Number((_b = payload.around) != null ? _b : 5))),
        includeUser: true
      });
      return {
        entries: result.entries.map((entry) => this.mapEntry(entry, entry.rank === result.userRank)),
        currentRank: result.userRank
      };
    }
    mapEntry(entry, isCurrentPlayer) {
      return {
        rank: entry.rank,
        score: entry.score,
        name: entry.player.publicName,
        avatar: entry.player.getAvatarSrc("small"),
        isCurrentPlayer
      };
    }
    text(payload, key, fallback = "") {
      const value = payload[key];
      return typeof value === "string" ? value : fallback;
    }
  };

  // src/entry/yandex.ts
  install(new YandexPlatform());
})();
