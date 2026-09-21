// JTL SDK bridge (youtube). Generated from Bridge~/src, do not edit.
"use strict";
(() => {
  var __defProp = Object.defineProperty;
  var __defProps = Object.defineProperties;
  var __getOwnPropDescs = Object.getOwnPropertyDescriptors;
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
  var __spreadProps = (a, b) => __defProps(a, __getOwnPropDescs(b));

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

  // src/platforms/youtube.ts
  var YouTubePlatform = class {
    constructor() {
      this.name = "youtube";
      this.emit = () => void 0;
      this.eventsBound = false;
      this.gameReadySent = false;
      this.modules = {
        platform: {
          actions: {
            initialize: () => this.initializePlatform(),
            continuePrompt: (payload) => showContinuePrompt(this.text(payload, "language", "en"))
          }
        },
        ads: {
          actions: {
            showInterstitial: () => this.sdk().ads.requestInterstitialAd().then(() => ({ result: "shown" })),
            showRewarded: (payload) => this.sdk().ads.requestRewardedAd(this.text(payload, "rewardId")).then((rewarded) => ({ result: rewarded ? "rewarded" : "closed" }))
          }
        },
        data: {
          actions: {
            load: () => this.loadData(),
            save: (payload) => this.sdk().game.saveData(this.text(payload, "data")).then(() => ({ saved: true }))
          }
        },
        leaderboards: {
          actions: {
            setScore: (payload) => {
              var _a;
              return this.sdk().engagement.sendScore({ value: Number((_a = payload.score) != null ? _a : 0) }).then(() => ({}));
            }
          }
        },
        gameplay: {
          actions: {
            ready: () => this.gameReady(),
            start: () => ({}),
            stop: () => ({})
          }
        }
      };
    }
    bind(emit) {
      this.emit = emit;
    }
    sdk() {
      if (typeof ytgame === "undefined") {
        throw new BridgeError(ResultCode.Unavailable, "YouTube Playables SDK is not available.");
      }
      return ytgame;
    }
    bindEvents(sdk) {
      if (this.eventsBound) {
        return;
      }
      this.eventsBound = true;
      sdk.system.onPause(() => this.emit(EventCode.Pause, {}));
      sdk.system.onResume(() => this.emit(EventCode.Resume, {}));
      sdk.system.onAudioEnabledChange((enabled) => this.emit(EventCode.MuteChanged, { muted: enabled === false }));
    }
    async initializePlatform() {
      const sdk = this.sdk();
      this.bindEvents(sdk);
      const language = await sdk.system.getLanguage().catch(() => "");
      return {
        appId: "",
        deviceType: this.detectDeviceType(),
        language,
        audioEnabled: sdk.system.isAudioEnabled(),
        inPlayablesEnvironment: sdk.IN_PLAYABLES_ENV,
        sdkVersion: sdk.SDK_VERSION
      };
    }
    async loadData() {
      const data = await this.sdk().game.loadData();
      if (typeof data !== "string" || data.length === 0) {
        return { result: "empty", data: "" };
      }
      return { result: "loaded", data };
    }
    gameReady() {
      var _a, _b;
      const sdk = this.sdk();
      const page = window;
      if (((_a = page.JTLSDK_PAGE) == null ? void 0 : _a.firstFrameReadySent) !== true) {
        sdk.game.firstFrameReady();
        page.JTLSDK_PAGE = __spreadProps(__spreadValues({}, (_b = page.JTLSDK_PAGE) != null ? _b : {}), { firstFrameReadySent: true });
      }
      if (this.gameReadySent === false) {
        this.gameReadySent = true;
        sdk.game.gameReady();
      }
      return {};
    }
    detectDeviceType() {
      const userAgent = navigator.userAgent.toLowerCase();
      if (/ipad|tablet/.test(userAgent)) {
        return "tablet";
      }
      if (/mobile|android|iphone/.test(userAgent)) {
        return "mobile";
      }
      return "desktop";
    }
    text(payload, key, fallback = "") {
      const value = payload[key];
      return typeof value === "string" ? value : fallback;
    }
  };

  // src/entry/youtube.ts
  install(new YouTubePlatform());
})();
