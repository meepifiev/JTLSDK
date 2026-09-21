(function () {
  "use strict";

  var body = document.body;
  var data = body.dataset;
  var canvas = document.getElementById("unity-canvas");
  var loader = document.getElementById("jtlsdk-loader");
  var progressFill = document.getElementById("jtlsdk-progress-fill");
  var isMobile = /Mobi|Android|iPhone|iPad|iPod/i.test(navigator.userAgent);
  var page = window.JTLSDK_PAGE || (window.JTLSDK_PAGE = {});

  page.platform = data.platform;

  function resolvePixelRatio(setting) {
    var native = window.devicePixelRatio || 1;

    if (!setting || setting === "auto") {
      return native;
    }

    if (setting.indexOf("max:") === 0) {
      return Math.min(native, parseFloat(setting.substring(4)) || native);
    }

    return parseFloat(setting) || native;
  }

  function parseAspect(value) {
    if (!value || value === "free") {
      return 0;
    }

    var parts = value.split("/");
    var width = parseFloat(parts[0]);
    var height = parseFloat(parts[1]);
    return width > 0 && height > 0 ? width / height : 0;
  }

  function fitCanvas() {
    var aspect = isMobile && data.aspectMobile === "free" ? 0 : parseAspect(data.aspect);
    var width = window.innerWidth;
    var height = window.innerHeight;

    if (aspect > 0 && width / height > aspect) {
      canvas.style.width = Math.round(height * aspect) + "px";
      canvas.style.height = height + "px";
      return;
    }

    canvas.style.width = "100%";
    canvas.style.height = "100%";
  }

  function showBanner(message, type) {
    if (type === "error") {
      console.error(message);
    } else {
      console.warn(message);
    }
  }

  function sendFirstFrameReady() {
    if (page.firstFrameReadySent || typeof ytgame === "undefined" || !ytgame.game || !ytgame.game.firstFrameReady) {
      return;
    }

    try {
      ytgame.game.firstFrameReady();
      page.firstFrameReadySent = true;
    } catch (error) {
      page.firstFrameReadySent = false;
    }
  }

  function preloadPlatform() {
    if (data.platform === "yandex" && typeof YaGames !== "undefined") {
      page.ysdk = YaGames.init();
    }

    if (data.platform === "youtube") {
      sendFirstFrameReady();
    }
  }

  function hideLoader() {
    loader.classList.add("jtlsdk-loader--hidden");
    window.setTimeout(function () {
      loader.style.display = "none";
    }, 300);
  }

  var config = {
    dataUrl: data.data,
    frameworkUrl: data.framework,
    codeUrl: data.code,
    memoryUrl: data.memory,
    symbolsUrl: data.symbols,
    streamingAssetsUrl: "StreamingAssets",
    companyName: data.company,
    productName: data.product,
    productVersion: data.version,
    devicePixelRatio: resolvePixelRatio(isMobile ? data.dprMobile : data.dprDesktop),
    showBanner: showBanner
  };

  window.addEventListener("resize", fitCanvas);
  window.addEventListener("orientationchange", fitCanvas);
  fitCanvas();
  preloadPlatform();

  var script = document.createElement("script");
  script.src = data.loader;
  script.onload = function () {
    createUnityInstance(canvas, config, function (progress) {
      progressFill.style.width = Math.round(progress * 100) + "%";
    }).then(function (instance) {
      page.unityInstance = instance;
      hideLoader();
    }).catch(function (message) {
      console.error(message);
    });
  };
  document.body.appendChild(script);
})();
