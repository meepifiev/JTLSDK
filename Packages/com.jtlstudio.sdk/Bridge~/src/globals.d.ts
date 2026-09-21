declare const Module: {
  JTLSDK?: unknown;
  JTLSDKBridges?: Record<string, unknown>;
  [key: string]: unknown;
};

declare function _malloc(size: number): number;
declare function _free(pointer: number): void;
declare function lengthBytesUTF8(text: string): number;
declare function stringToUTF8(text: string, pointer: number, maxBytes: number): void;
declare function dynCall(signature: string, pointer: number, args: unknown[]): unknown;
declare function getWasmTableEntry(pointer: number): ((...args: unknown[]) => unknown) | undefined;
declare const wasmTable: { get(pointer: number): ((...args: unknown[]) => unknown) | undefined } | undefined;
