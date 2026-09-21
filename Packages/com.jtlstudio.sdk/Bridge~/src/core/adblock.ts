const BaitClasses = "adsbox ad adsbygoogle ad-banner ad-unit text-ad pub_300x250";
const DetectionDelayMilliseconds = 100;

export function detectAdBlock(): Promise<boolean> {
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

  return new Promise<boolean>((resolve) => {
    window.setTimeout(() => {
      const style = window.getComputedStyle(bait);
      const blocked = bait.offsetParent === null || bait.offsetHeight === 0 || bait.offsetWidth === 0 || style.display === "none" || style.visibility === "hidden";
      bait.remove();
      resolve(blocked);
    }, DetectionDelayMilliseconds);
  });
}
