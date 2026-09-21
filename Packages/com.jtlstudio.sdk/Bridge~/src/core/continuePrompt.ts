const OverlayId = "jtlsdk-continue-prompt";

const Messages: Record<string, string> = {
  en: "Click this area to continue.",
  ru: "Чтобы продолжить, нажми на эту область.",
  tr: "Devam etmek için bu alana tıkla.",
};

export function showContinuePrompt(languageCode: string): Promise<void> {
  return new Promise<void>((resolve) => {
    const existing = document.getElementById(OverlayId);

    if (existing !== null) {
      existing.remove();
    }

    const overlay = document.createElement("div");
    overlay.id = OverlayId;
    overlay.setAttribute("role", "button");
    overlay.tabIndex = 0;
    overlay.style.cssText = "position:fixed;inset:0;display:flex;align-items:center;justify-content:center;background:rgba(0,0,0,0.3);color:#F2F2F3;font:500 clamp(18px,3vw,32px) Inter,system-ui,sans-serif;text-align:center;cursor:pointer;z-index:2147483647;user-select:none;";
    overlay.textContent = Messages[languageCode] ?? Messages.en;

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
