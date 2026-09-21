export type Payload = Record<string, unknown>;

export type ActionHandler = (payload: Payload) => Promise<unknown> | unknown;

export type QueryHandler = (payload: Payload) => unknown;

export interface ModuleAdapter {
  readonly actions: Record<string, ActionHandler>;
  readonly queries?: Record<string, QueryHandler>;
}

export interface PlatformAdapter {
  readonly name: string;
  readonly modules: Record<string, ModuleAdapter>;
}

export const ResultCode = {
  Ok: 0,
  Unavailable: 1,
  InvalidParams: 2,
  SizeLimit: 3,
  Cancelled: 4,
  Unknown: 5,
  UnknownAction: 6,
} as const;

export const EventCode = {
  Pause: 1,
  Resume: 2,
  MuteChanged: 3,
  FocusChanged: 4,
} as const;

export class BridgeError extends Error {
  public readonly code: number;

  public constructor(code: number, message: string) {
    super(message);
    this.code = code;
  }
}
