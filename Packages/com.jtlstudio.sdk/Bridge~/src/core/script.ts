const pending = new Map<string, Promise<void>>();

export function loadScript(url: string): Promise<void> {
  const existing = pending.get(url);

  if (existing !== undefined) {
    return existing;
  }

  const promise = new Promise<void>((resolve, reject) => {
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
