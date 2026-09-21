# JTL SDK · Руководство

Один API для WebGL-площадок. Код игры не меняется между Yandex Games, YouTube Playables и редактором.

## Установка

Unity 2021.3.18f1 и новее. Проверено на 2021.3, 2022.3 и Unity 6.

`Window › Package Manager › + › Add package from git URL`:

```text
https://github.com/meepifiev/JTLSDK.git?path=Packages/com.jtlstudio.sdk
```

Затем `JTL SDK › Toolkit`.

## Тулкит

| Раздел | Что делает |
|---|---|
| Configurations | Конфигурация на каждую площадку. Активная конфигурация включает свой define-символ и пресет Player Settings. Одна сборка = одна конфигурация. |
| Simulation | Ответы площадки в редакторе: устройство, задержка и ошибка инициализации, реклама, покупки, игрок, сохранения. |
| Template | Экран загрузки, логотип, фон, прогресс, пропорции, pixel ratio. |
| Build | Проверки, путь, ZIP или папка, номер сборки, Development-плашка. |
| Package Manager | Обновления SDK из GitHub Releases, шаблон, модули. |
| Code Analyzer | Находит в коде игры `Time.timeScale`, `AudioListener`, `PlayerPrefs`, `Cursor` и `Application.OpenURL` и заменяет их на вызовы SDK. |
| Modules | Провайдер каждого модуля в каждой конфигурации. Покупки, лидерборды, флаги и языки описываются здесь. |

После правки товаров, лидербордов и флагов нажмите «Generate constants». Появятся классы `ProductIds`, `LeaderboardIds` и `FlagKeys`.

## Запуск

```csharp
private void Awake()
{
    JTLSDK.Create();
    JTLSDK.Payments.Granted += OnGranted;
    JTLSDK.WhenReady(OnReady);
}

private void OnReady()
{
    JTLSDK.Gameplay.GameReady();
}
```

`Create` читает `Resources/JTLSDK/JTLSDKSettings.asset`. `WhenReady` вызывается один раз, когда готовы все модули или истёк таймаут. Каждый модуль тоже имеет `IsReady`, `IsSupported` и `WhenReady`.

## Модули

**Реклама.** Пауза и звук на время показа управляются SDK.

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

**Сохранения.** Ключ-значение, запись в облако с задержкой. `Flush` пишет сразу.

```csharp
int money = JTLSDK.Data.GetInt("money");
JTLSDK.Data.SetInt("money", money + 100);
JTLSDK.Data.Flush();
```

**Покупки.** Товар выдаётся в `Granted`, затем SDK сохраняет данные и только после этого списывает расходуемую покупку. Подпишитесь на `Granted` сразу после `Create`: незавершённые покупки прошлых запусков придут в первый обработчик. Обработчик должен добавлять, а не устанавливать значение.

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

**Язык.** Язык выбирает площадка. `Set` действует до конца сессии.

```csharp
Language language = JTLSDK.Language.Current;
JTLSDK.Language.Changed += OnLanguageChanged;
```

**Пауза, время и звук.** SDK владеет `Time.timeScale` и громкостью. Меняйте их через SDK.

```csharp
JTLSDK.Time.Scale = 0.5f;
JTLSDK.Audio.Volume = 0.8f;

using (JTLSDK.Pause.Hold("Menu"))
{
}
```

**Геймплей.** `GameReady` один раз после загрузки, `Start` и `Stop` вокруг активной игры.

**Лидерборды, игрок, флаги, отзыв, ярлык.**

```csharp
JTLSDK.Leaderboards.SetScore(LeaderboardIds.Levels, 27);
JTLSDK.Player.Authorize(success => { });
bool hardMode = JTLSDK.Flags.GetBool(FlagKeys.HardMode);
JTLSDK.Review.Request(sent => { });
JTLSDK.Shortcut.Request(created => { });
```

**Аналитика.** Без модуля события игнорируются.

```csharp
JTLSDK.Analytics.Report("level_complete", new Dictionary<string, object> { { "level", 3 } });
```

**Площадка и устройство.**

```csharp
PlatformId platform = JTLSDK.Platform.Current;
bool mobile = JTLSDK.Device.IsMobile;
```

## Редактор

В Play Mode провайдеры конфигурации заменяются прототипами. Реклама и покупки показывают окно с вариантами ответа, язык меняется в углу вкладки Game. Сохранения лежат в `PlayerPrefs` и редактируются в разделе Saves.

## Сборка

`Build › Build` собирает WebGL для активной конфигурации. Красная проверка блокирует сборку. Для YouTube Playables дополнительно проверяются размер файлов, их количество, сжатие и внешние скрипты.

## Модули пакета

Package Manager в тулките показывает модули из `modules.json`. Модуль добавляет провайдер, и его выбирают в конфигурации.

| Модуль | Площадки | Что даёт |
|---|---|---|
| Yandex Metrica | Yandex Games | `JTLSDK.Analytics` через `reachGoal` |

## Поддержка

[t.me/jtlstudio](https://t.me/jtlstudio)
