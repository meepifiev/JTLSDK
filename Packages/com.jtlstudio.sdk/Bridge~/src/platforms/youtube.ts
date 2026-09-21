import { showContinuePrompt } from "../core/continuePrompt";
import { BridgeError, EventCode, EventEmitter, ModuleAdapter, Payload, PlatformAdapter, ResultCode } from "../core/types";

interface YouTubeSdk {
  IN_PLAYABLES_ENV: boolean;
  SDK_VERSION: string;
  game: {
    firstFrameReady(): void;
    gameReady(): void;
    loadData(): Promise<string>;
    saveData(data: string): Promise<void>;
  };
  ads: {
    requestInterstitialAd(): Promise<void>;
    requestRewardedAd(rewardId: string): Promise<boolean>;
  };
  engagement: {
    sendScore(score: { value: number }): Promise<void>;
  };
  system: {
    isAudioEnabled(): boolean;
    onAudioEnabledChange(callback: (enabled: boolean) => void): () => void;
    onPause(callback: () => void): () => void;
    onResume(callback: () => void): () => void;
    getLanguage(): Promise<string>;
  };
  health: {
    logError(): void;
    logWarning(): void;
  };
}

declare const ytgame: YouTubeSdk | undefined;

interface PageState {
  firstFrameReadySent?: boolean;
}

const DefaultRewardId = "reward";

export class YouTubePlatform implements PlatformAdapter {
  public readonly name = "youtube";
  public readonly modules: Record<string, ModuleAdapter>;

  private emit: EventEmitter = () => undefined;
  private eventsBound = false;
  private gameReadySent = false;

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
          showInterstitial: () => this.sdk().ads.requestInterstitialAd().then(() => ({ result: "shown" })),
          showRewarded: (payload) => this.sdk().ads.requestRewardedAd((this.text(payload, "rewardId") || DefaultRewardId)).then((rewarded) => ({ result: rewarded ? "rewarded" : "closed" })),
        },
      },
      data: {
        actions: {
          load: () => this.loadData(),
          save: (payload) => this.sdk().game.saveData(this.text(payload, "data")).then(() => ({ saved: true })),
        },
      },
      leaderboards: {
        actions: {
          setScore: (payload) => this.sdk().engagement.sendScore({ value: Number(payload.score ?? 0) }).then(() => ({})),
        },
      },
      gameplay: {
        actions: {
          ready: () => this.gameReady(),
          start: () => ({}),
          stop: () => ({}),
        },
      },
    };
  }

  public bind(emit: EventEmitter): void {
    this.emit = emit;
  }

  private sdk(): YouTubeSdk {
    if (typeof ytgame === "undefined") {
      throw new BridgeError(ResultCode.Unavailable, "YouTube Playables SDK is not available.");
    }

    return ytgame;
  }

  private bindEvents(sdk: YouTubeSdk): void {
    if (this.eventsBound) {
      return;
    }

    this.eventsBound = true;
    sdk.system.onPause(() => this.emit(EventCode.Pause, {}));
    sdk.system.onResume(() => this.emit(EventCode.Resume, {}));
    sdk.system.onAudioEnabledChange((enabled) => this.emit(EventCode.MuteChanged, { muted: enabled === false }));
  }

  private async initializePlatform(): Promise<unknown> {
    const sdk = this.sdk();
    this.bindEvents(sdk);
    const language = await sdk.system.getLanguage().catch(() => "");

    return {
      appId: "",
      deviceType: this.detectDeviceType(),
      language,
      audioEnabled: sdk.system.isAudioEnabled(),
      inPlayablesEnvironment: sdk.IN_PLAYABLES_ENV,
      sdkVersion: sdk.SDK_VERSION,
    };
  }

  private async loadData(): Promise<unknown> {
    const data = await this.sdk().game.loadData();

    if (typeof data !== "string" || data.length === 0) {
      return { result: "empty", data: "" };
    }

    return { result: "loaded", data };
  }

  private gameReady(): unknown {
    const sdk = this.sdk();
    const page = window as unknown as { JTLSDK_PAGE?: PageState };

    if (page.JTLSDK_PAGE?.firstFrameReadySent !== true) {
      sdk.game.firstFrameReady();
      page.JTLSDK_PAGE = { ...(page.JTLSDK_PAGE ?? {}), firstFrameReadySent: true };
    }

    if (this.gameReadySent === false) {
      this.gameReadySent = true;
      sdk.game.gameReady();
    }

    return {};
  }

  private detectDeviceType(): string {
    const userAgent = navigator.userAgent.toLowerCase();

    if (/ipad|tablet/.test(userAgent)) {
      return "tablet";
    }

    if (/mobile|android|iphone/.test(userAgent)) {
      return "mobile";
    }

    return "desktop";
  }

  private text(payload: Payload, key: string, fallback = ""): string {
    const value = payload[key];
    return typeof value === "string" ? value : fallback;
  }
}
