# JTL SDK · Guide

One API for WebGL portals. Game code stays the same on Yandex Games, YouTube Playables and in the editor.

## Install

Unity 2021.3.18f1 or newer. Tested on 2021.3, 2022.3 and Unity 6.

`Window › Package Manager › + › Add package from git URL`:

```text
https://github.com/meepifiev/JTLSDK.git?path=Packages/com.jtlstudio.sdk
```

Then open `JTL SDK › Toolkit`.

## Toolkit

| Section | What it does |
|---|---|
| Configurations | One configuration per portal. The active one applies its define symbol and Player Settings preset. One build is one configuration. |
| Build | Active configuration picker, its build and player settings, checks, output path, ZIP or folder, build number, development badge, recent builds. |
| Simulation | Portal answers in the editor: device, initialization delay and failure, ads, purchases, player, saves. |
| Template | Loading screen, logo, background, progress bar, aspect ratio, pixel ratio. |
| Package Manager | SDK updates from GitHub Releases, the template, modules. |
| Code Analyzer | Finds `Time.timeScale`, `AudioListener`, `PlayerPrefs`, `Cursor` and `Application.OpenURL` in game code and replaces them with SDK calls. |
| Modules | Configurable and out-of-the-box: the provider of every module in every configuration. Products, leaderboards, flags and languages are declared here. |

After editing products, leaderboards or flags, press "Generate constants". It writes the `ProductIds`, `LeaderboardIds` and `FlagKeys` classes.

## Start

```csharp
private void Awake()
{
    JTLSDK.Create();
    JTLSDK.Payments.Granted += OnGranted;
    JTLSDK.WhenReady(OnReady);
}

private void OnReady()
{
    JTLSDK.GameEvents.GameReady();
}
```

`Create` reads `Resources/JTLSDK/JTLSDKSettings.asset`. `WhenReady` fires once, when every module is ready or the timeout expires. Every module also has `IsReady`, `IsSupported` and `WhenReady`.

## Modules

**Ads.** The SDK pauses the game and mutes audio while an ad is shown.

```csharp
JTLSDK.Ads.ShowInterstitial();
JTLSDK.Ads.ShowRewarded("double_money", result =>
{
    if (result == AdResult.Rewarded)
    {
        AddMoney(100);
    }
});
```

**Saves.** Key-value storage with delayed cloud writes. `Flush` writes immediately.

```csharp
int money = JTLSDK.Data.GetInt("money");
JTLSDK.Data.SetInt("money", money + 100);
JTLSDK.Data.Flush();
```

**Purchases.** The game grants the product in `Granted`, the SDK saves, and only then consumes a consumable purchase. Subscribe to `Granted` right after `Create`: unfinished purchases from earlier sessions go to the first handler. The handler must add to a value, not set it.

```csharp
private void OnGranted(string productId)
{
    if (productId == ProductIds.Coins1000)
    {
        JTLSDK.Data.SetInt("money", JTLSDK.Data.GetInt("money") + 1000);
    }
}

JTLSDK.Payments.Purchase(ProductIds.RemoveAds, result => { });
bool removed = JTLSDK.Payments.IsPurchased(ProductIds.RemoveAds);
```

**Language.** The portal picks the language. `Set` lasts until the end of the session.

```csharp
Language language = JTLSDK.Language.Current;
JTLSDK.Language.Changed += OnLanguageChanged;
```

**Pause, time and audio.** The SDK owns `Time.timeScale` and the volume. Change them through the SDK.

```csharp
JTLSDK.Time.Scale = 0.5f;
JTLSDK.Audio.Volume = 0.8f;

using (JTLSDK.Pause.Hold("Menu"))
{
}
```

**Game events.** Call `GameReady` once after loading. Call `GameplayStarted`, `GameplayStopped` and `GameplayRestarted` around active play.

```csharp
JTLSDK.GameEvents.GameplayStarted();
JTLSDK.GameEvents.GameplayRestarted();
JTLSDK.GameEvents.GameplayStopped();
```

**Leaderboards, player, flags, review, game label.**

```csharp
JTLSDK.Leaderboards.SetScore(LeaderboardIds.Levels, 27);
JTLSDK.Player.Authorize(success => { });
bool hardMode = JTLSDK.Flags.GetBool(FlagKeys.HardMode);
JTLSDK.Review.Request(sent => { });
JTLSDK.GameLabel.ShowDialog(created => { });
```

**Platform and device.**

```csharp
PlatformId platform = JTLSDK.Platform.Current;
bool mobile = JTLSDK.Device.IsMobile;
```

## Editor

In Play Mode the configuration's providers are replaced with prototypes. Ads and purchases show a dialog with the possible answers, and the language switch sits in the corner of the Game view. Saves live in `PlayerPrefs` and are edited in the Saves section.

## Build

`Build › Build` makes a WebGL build of the active configuration. A red check blocks the build. YouTube Playables builds are also checked for file size, file count, compression and external scripts.

## Package modules

The toolkit's Package Manager lists modules from `modules.json` and installs them from git. No modules are published yet.

## Support

[t.me/jtlstudio](https://t.me/jtlstudio)
