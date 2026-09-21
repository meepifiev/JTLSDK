import { CallbackInvoker } from "./invoker";
import { BridgeError, Payload, PlatformAdapter, ResultCode } from "./types";

interface QueuedMessage {
  requestId: number;
  code: number;
  payload: string;
}

export class Bridge {
  public readonly available = true;
  public readonly platform: PlatformAdapter;

  private readonly invoker = new CallbackInvoker();
  private readonly queued: QueuedMessage[] = [];

  public constructor(platform: PlatformAdapter) {
    this.platform = platform;
  }

  public register(pointer: number): void {
    this.invoker.register(pointer);

    while (this.queued.length > 0) {
      const message = this.queued.shift() as QueuedMessage;
      this.deliver(message.requestId, message.code, message.payload);
    }
  }

  public call(moduleName: string, action: string, payloadText: string, requestId: number): void {
    let result: Promise<unknown> | unknown;

    try {
      const handler = this.actionHandler(moduleName, action);
      result = handler(this.parse(payloadText));
    } catch (error) {
      this.fail(requestId, error);
      return;
    }

    Promise.resolve(result)
      .then((value) => this.emit(requestId, ResultCode.Ok, value))
      .catch((error) => this.fail(requestId, error));
  }

  public query(moduleName: string, action: string, payloadText: string): string {
    try {
      const adapter = this.platform.modules[moduleName];
      const handler = adapter?.queries?.[action];

      if (handler === undefined) {
        throw new BridgeError(ResultCode.UnknownAction, `${moduleName}.${action}`);
      }

      return this.serialize({ code: ResultCode.Ok, value: handler(this.parse(payloadText)) });
    } catch (error) {
      const bridgeError = this.toBridgeError(error);
      return this.serialize({ code: bridgeError.code, message: bridgeError.message });
    }
  }

  public event(code: number, payload: unknown): void {
    this.emit(0, code, payload);
  }

  private actionHandler(moduleName: string, action: string) {
    const adapter = this.platform.modules[moduleName];
    const handler = adapter?.actions[action];

    if (handler === undefined) {
      throw new BridgeError(ResultCode.UnknownAction, `${moduleName}.${action}`);
    }

    return handler;
  }

  private emit(requestId: number, code: number, value: unknown): void {
    this.deliver(requestId, code, this.serialize(value === undefined ? {} : value));
  }

  private fail(requestId: number, error: unknown): void {
    const bridgeError = this.toBridgeError(error);
    this.deliver(requestId, bridgeError.code, this.serialize({ message: bridgeError.message }));
  }

  private deliver(requestId: number, code: number, payload: string): void {
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

  private parse(payloadText: string): Payload {
    if (payloadText === undefined || payloadText === null || payloadText === "") {
      return {};
    }

    const parsed: unknown = JSON.parse(payloadText);

    if (parsed !== null && typeof parsed === "object" && Array.isArray(parsed) === false) {
      return parsed as Payload;
    }

    throw new BridgeError(ResultCode.InvalidParams, "Payload must be a JSON object.");
  }

  private serialize(value: unknown): string {
    try {
      return JSON.stringify(value === undefined ? {} : value);
    } catch {
      return "{}";
    }
  }

  private toBridgeError(error: unknown): BridgeError {
    if (error instanceof BridgeError) {
      return error;
    }

    const message = error instanceof Error ? error.message : String(error);
    return new BridgeError(ResultCode.Unknown, message);
  }
}

export function install(platform: PlatformAdapter): Bridge {
  const bridge = new Bridge(platform);
  Module.JTLSDK = bridge;
  return bridge;
}
