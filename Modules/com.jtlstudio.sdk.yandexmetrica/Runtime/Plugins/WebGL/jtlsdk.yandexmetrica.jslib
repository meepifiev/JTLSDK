mergeInto(LibraryManager.library, {
  JTLSDK_Metrica_Initialize: function (counterId, options) {
    if (typeof window === "undefined" || typeof window.ytgame !== "undefined") {
      return 0;
    }

    if (typeof window.ym !== "function") {
      window.ym = function () {
        (window.ym.a = window.ym.a || []).push(arguments);
      };
      window.ym.l = Date.now();
      var script = document.createElement("script");
      script.async = true;
      script.src = "https://mc.yandex.ru/metrika/tag.js";
      (document.head || document.documentElement).appendChild(script);
    }

    window.ym(counterId, "init", {
      webvisor: (options & 1) !== 0,
      clickmap: (options & 2) !== 0,
      trackLinks: (options & 4) !== 0,
      accurateTrackBounce: (options & 8) !== 0
    });
    return 1;
  },

  JTLSDK_Metrica_ReachGoal: function (counterId, goal, parametersJson) {
    if (typeof window.ym !== "function") {
      return;
    }

    var json = UTF8ToString(parametersJson);
    var parameters;

    if (json) {
      try {
        parameters = JSON.parse(json);
      } catch (error) {
        parameters = undefined;
      }
    }

    window.ym(counterId, "reachGoal", UTF8ToString(goal), parameters);
  }
});
