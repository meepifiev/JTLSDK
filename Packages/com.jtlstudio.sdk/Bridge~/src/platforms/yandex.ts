import { detectAdBlock } from "../core/adblock";
import { showContinuePrompt } from "../core/continuePrompt";
import { loadScript } from "../core/script";
import { BridgeError, EventCode, EventEmitter, ModuleAdapter, Payload, PlatformAdapter, ResultCode } from "../core/types";

const SdkScriptUrl = "/sdk.js";
const DataKey = "jtlsdk";

interface YandexPlayer {
  getMode(): string;
  getUniqueID(): string;
  getName(): string;
  getPhoto(size: string): string;
  getData(keys?: string[]): Promise<Record<string, unknown>>;
  setData(data: Record<string, unknown>, flush?: boolean): Promise<void>;
}

interface YandexProduct {
  id: string;
  title: string;
  price: string;
  priceValue: string;
  priceCurrencyCode: string;
}

interface YandexPurchase {
  productID: string;
  purchaseToken: string;
}

interface YandexPayments {
  getCatalog(): Promise<YandexProduct[]>;
  getPurchases(): Promise<YandexPurchase[]>;
  purchase(options: { id: string }): Promise<YandexPurchase>;
  consumePurchase(token: string): Promise<void>;
}

interface YandexLeaderboardEntry {
  score: number;
  rank: number;
  player: { publicName: string; uniqueID: string; getAvatarSrc(size: string): string };
}

interface YandexLeaderboards {
  setLeaderboardScore(name: string, score: number): Promise<void>;
  getLeaderboardPlayerEntry(name: string): Promise<YandexLeaderboardEntry>;
  getLeaderboardEntries(name: string, options: { quantityTop: number; quantityAround: number; includeUser: boolean }): Promise<{ entries: YandexLeaderboardEntry[]; userRank: number }>;
}

interface YandexAdCallbacks {
  onOpen?: () => void;
  onClose?: (wasShown: boolean) => void;
  onRewarded?: () => void;
  onError?: (error: unknown) => void;
  onOffline?: () => void;
}

interface YandexSdk {
  environment: { i18n: { lang: string }; app: { id: string } };
  deviceInfo: { type: string };
  features: { LoadingAPI?: { ready(): void }; GameplayAPI?: { start(): void; stop(): void } };
  adv: {
    showFullscreenAdv(options: { callbacks: YandexAdCallbacks }): void;
    showRewardedVideo(options: { callbacks: YandexAdCallbacks }): void;
    showBannerAdv(): Promise<unknown>;
    hideBannerAdv(): Promise<unknown>;
    getBannerAdvStatus(): Promise<{ stickyAdvIsShowing: boolean }>;
  };
  auth: { openAuthDialog(): Promise<void> };
  feedback: { canReview(): Promise<{ value: boolean }>; requestReview(): Promise<{ feedbackSent: boolean }> };
  shortcut: { canShowPrompt(): Promise<{ canShow: boolean }>; showPrompt(): Promise<{ outcome: string }> };
  getPlayer(options: { scopes: boolean }): Promise<YandexPlayer>;
  getPayments(options: { signed: boolean }): Promise<YandexPayments>;
  getLeaderboards(): Promise<YandexLeaderboards>;
  getFlags(options: { defaultFlags: Record<string, string> }): Promise<Record<string, string>>;
  serverTime(): number;
  on(event: string, handler: () => void): void;
}

declare const YaGames: { init(): Promise<YandexSdk> } | undefined;

interface PagePreload {
  ysdk?: Promise<YandexSdk> | YandexSdk;
}

export class YandexPlatform implements PlatformAdapter {
  public readonly name = "yandex";
  public readonly modules: Record<string, ModuleAdapter>;

  private sdkPromise: Promise<YandexSdk> | null = null;
  private playerPromise: Promise<YandexPlayer> | null = null;
  private paymentsPromise: Promise<YandexPayments> | null = null;
  private leaderboardsPromise: Promise<YandexLeaderboards> | null = null;
  private emit: EventEmitter = () => undefined;
  private eventsBound = false;

  public constructor() {
    this.modules = {
      platform: {
        actions: {
          initialize: () => this.initializePlatform(),
          continuePrompt: (payload) => showContinuePrompt(this.text(payload, "language", "en")),
        },
      },
      ads: {
        actions: {
          showInterstitial: (payload) => this.showInterstitial(payload),
          showRewarded: (payload) => this.showRewarded(payload),
          showBanner: () => this.sdk().then((sdk) => sdk.adv.showBannerAdv()).then(() => ({})),
          hideBanner: () => this.sdk().then((sdk) => sdk.adv.hideBannerAdv()).then(() => ({})),
          bannerStatus: () => this.sdk().then((sdk) => sdk.adv.getBannerAdvStatus()).then((status) => ({ visible: status.stickyAdvIsShowing })),
        },
      },
      data: {
        actions: {
          load: () => this.loadData(),
          save: (payload) => this.saveData(payload),
        },
      },
      payments: {
        actions: {
          initialize: () => this.initializePayments(),
          purchase: (payload) => this.purchase(payload),
          consume: (payload) => this.payments().then((payments) => payments.consumePurchase(this.text(payload, "token"))).then(() => ({ consumed: true })),
          refresh: () => this.payments().then((payments) => payments.getPurchases()).then((purchases) => ({ purchases: purchases.map(this.mapPurchase) })),
        },
      },
      player: {
        actions: {
          info: () => this.playerInfo(),
          authorize: () => this.authorize(),
        },
      },
      leaderboards: {
        actions: {
          setScore: (payload) => this.leaderboards().then((boards) => boards.setLeaderboardScore(this.text(payload, "id"), Number(payload.score ?? 0))).then(() => ({})),
          playerEntry: (payload) => this.playerEntry(payload),
          load: (payload) => this.loadLeaderboard(payload),
        },
      },
      flags: {
        actions: {
          get: (payload) => this.sdk().then((sdk) => sdk.getFlags({ defaultFlags: (payload.defaults as Record<string, string>) ?? {} })).then((flags) => ({ flags })),
        },
      },
      time: {
        actions: {
          server: () => this.sdk().then((sdk) => ({ milliseconds: sdk.serverTime() })),
        },
      },
      gameplay: {
        actions: {
          ready: () => this.sdk().then((sdk) => { sdk.features.LoadingAPI?.ready(); return {}; }),
          start: () => this.sdk().then((sdk) => { sdk.features.GameplayAPI?.start(); return {}; }),
          stop: () => this.sdk().then((sdk) => { sdk.features.GameplayAPI?.stop(); return {}; }),
        },
      },
      review: {
        actions: {
          canRequest: () => this.sdk().then((sdk) => sdk.feedback.canReview()).then((result) => ({ value: result.value })),
          request: () => this.sdk().then((sdk) => sdk.feedback.requestReview()).then((result) => ({ sent: result.feedbackSent })),
        },
      },
      shortcut: {
        actions: {
          canRequest: () => this.sdk().then((sdk) => sdk.shortcut.canShowPrompt()).then((result) => ({ value: result.canShow })),
          request: () => this.sdk().then((sdk) => sdk.shortcut.showPrompt()).then((result) => ({ created: result.outcome === "accepted" })),
        },
      },
    };
  }

  public bind(emit: EventEmitter): void {
    this.emit = emit;
  }

  private sdk(): Promise<YandexSdk> {
    if (this.sdkPromise !== null) {
      return this.sdkPromise;
    }

    const preload = (window as unknown as { JTLSDK_PAGE?: PagePreload }).JTLSDK_PAGE;

    if (preload?.ysdk !== undefined) {
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

  private async loadSdk(): Promise<YandexSdk> {
    if (typeof YaGames === "undefined") {
      await loadScript(SdkScriptUrl);
    }

    if (typeof YaGames === "undefined") {
      throw new BridgeError(ResultCode.Unavailable, "Yandex Games SDK is not available.");
    }

    return YaGames.init();
  }

  private bindEvents(sdk: YandexSdk): void {
    if (this.eventsBound) {
      return;
    }

    this.eventsBound = true;
    sdk.on("game_api_pause", () => this.emit(EventCode.Pause, {}));
    sdk.on("game_api_resume", () => this.emit(EventCode.Resume, {}));
  }

  private player(): Promise<YandexPlayer> {
    if (this.playerPromise === null) {
      this.playerPromise = this.sdk().then((sdk) => sdk.getPlayer({ scopes: false }));
    }

    return this.playerPromise;
  }

  private payments(): Promise<YandexPayments> {
    if (this.paymentsPromise === null) {
      this.paymentsPromise = this.sdk().then((sdk) => sdk.getPayments({ signed: true }));
    }

    return this.paymentsPromise;
  }

  private leaderboards(): Promise<YandexLeaderboards> {
    if (this.leaderboardsPromise === null) {
      this.leaderboardsPromise = this.sdk().then((sdk) => sdk.getLeaderboards());
    }

    return this.leaderboardsPromise;
  }

  private async initializePlatform(): Promise<unknown> {
    const sdk = await this.sdk();
    return {
      appId: sdk.environment.app.id,
      deviceType: sdk.deviceInfo.type,
      language: sdk.environment.i18n.lang,
      serverTime: sdk.serverTime(),
    };
  }

  private async showInterstitial(payload: Payload): Promise<unknown> {
    if (payload.adBlockDetection === true && (await detectAdBlock())) {
      return { result: "blocked" };
    }

    const sdk = await this.sdk();

    return new Promise((resolve, reject) => {
      sdk.adv.showFullscreenAdv({
        callbacks: {
          onClose: (wasShown) => resolve({ result: wasShown ? "shown" : "notShown" }),
          onOffline: () => resolve({ result: "notShown" }),
          onError: (error) => reject(new BridgeError(ResultCode.Unknown, String(error))),
        },
      });
    });
  }

  private async showRewarded(payload: Payload): Promise<unknown> {
    if (payload.adBlockDetection === true && (await detectAdBlock())) {
      return { result: "blocked" };
    }

    const sdk = await this.sdk();

    return new Promise((resolve, reject) => {
      let rewarded = false;
      sdk.adv.showRewardedVideo({
        callbacks: {
          onRewarded: () => { rewarded = true; },
          onClose: () => resolve({ result: rewarded ? "rewarded" : "closed" }),
          onError: (error) => reject(new BridgeError(ResultCode.Unknown, String(error))),
        },
      });
    });
  }

  private async loadData(): Promise<unknown> {
    const player = await this.player();
    const data = await player.getData([DataKey]);
    const value = data[DataKey];

    if (typeof value !== "string" || value.length === 0) {
      return { result: "empty", data: "" };
    }

    return { result: "loaded", data: value };
  }

  private async saveData(payload: Payload): Promise<unknown> {
    const player = await this.player();
    await player.setData({ [DataKey]: this.text(payload, "data") }, true);
    return { saved: true };
  }

  private async initializePayments(): Promise<unknown> {
    const payments = await this.payments();
    const [catalog, purchases] = await Promise.all([payments.getCatalog(), payments.getPurchases()]);

    return {
      products: catalog.map((product) => ({
        id: product.id,
        price: Number(product.priceValue),
        currency: product.priceCurrencyCode,
        formatted: product.price,
      })),
      purchases: purchases.map(this.mapPurchase),
    };
  }

  private async purchase(payload: Payload): Promise<unknown> {
    const payments = await this.payments();

    try {
      const purchase = await payments.purchase({ id: this.text(payload, "id") });
      return this.mapPurchase(purchase);
    } catch (error) {
      throw new BridgeError(ResultCode.Cancelled, error instanceof Error ? error.message : String(error));
    }
  }

  private mapPurchase(purchase: YandexPurchase): { productId: string; token: string } {
    return { productId: purchase.productID, token: purchase.purchaseToken };
  }

  private async playerInfo(): Promise<unknown> {
    const player = await this.player();
    const authorized = player.getMode() !== "lite";

    return {
      authorized,
      id: authorized ? player.getUniqueID() : "",
      name: authorized ? player.getName() : "",
      avatar: authorized ? player.getPhoto("medium") : "",
    };
  }

  private async authorize(): Promise<unknown> {
    const sdk = await this.sdk();

    try {
      await sdk.auth.openAuthDialog();
    } catch {
      return { authorized: false, id: "", name: "", avatar: "" };
    }

    this.playerPromise = null;
    return this.playerInfo();
  }

  private async playerEntry(payload: Payload): Promise<unknown> {
    const boards = await this.leaderboards();

    try {
      const entry = await boards.getLeaderboardPlayerEntry(this.text(payload, "id"));
      return { found: true, ...this.mapEntry(entry, true) };
    } catch {
      return { found: false };
    }
  }

  private async loadLeaderboard(payload: Payload): Promise<unknown> {
    const boards = await this.leaderboards();
    const result = await boards.getLeaderboardEntries(this.text(payload, "id"), {
      quantityTop: Number(payload.top ?? 10),
      quantityAround: Number(payload.around ?? 5),
      includeUser: true,
    });

    return {
      entries: result.entries.map((entry) => this.mapEntry(entry, entry.rank === result.userRank)),
      currentRank: result.userRank,
    };
  }

  private mapEntry(entry: YandexLeaderboardEntry, isCurrentPlayer: boolean): Record<string, unknown> {
    return {
      rank: entry.rank,
      score: entry.score,
      name: entry.player.publicName,
      avatar: entry.player.getAvatarSrc("small"),
      isCurrentPlayer,
    };
  }

  private text(payload: Payload, key: string, fallback = ""): string {
    const value = payload[key];
    return typeof value === "string" ? value : fallback;
  }
}
