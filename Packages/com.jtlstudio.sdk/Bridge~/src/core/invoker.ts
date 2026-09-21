type NativeCallback = (requestId: number, code: number, payloadPointer: number) => void;

export class CallbackInvoker {
  private pointer = 0;

  public get isRegistered(): boolean {
    return this.pointer !== 0;
  }

  public register(pointer: number): void {
    this.pointer = pointer;
  }

  public invoke(requestId: number, code: number, payload: string): void {
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

  private resolve(): NativeCallback {
    if (typeof getWasmTableEntry === "function") {
      const entry = getWasmTableEntry(this.pointer);

      if (entry !== undefined) {
        return entry as NativeCallback;
      }
    }

    if (typeof wasmTable !== "undefined" && wasmTable !== undefined) {
      const entry = wasmTable.get(this.pointer);

      if (entry !== undefined) {
        return entry as NativeCallback;
      }
    }

    const dynamic = (Module as Record<string, unknown>)["dynCall_viii"];

    if (typeof dynamic === "function") {
      const pointer = this.pointer;
      return (requestId, code, payloadPointer) => (dynamic as (...args: unknown[]) => unknown)(pointer, requestId, code, payloadPointer);
    }

    const pointer = this.pointer;
    return (requestId, code, payloadPointer) => dynCall("viii", pointer, [requestId, code, payloadPointer]);
  }
}
