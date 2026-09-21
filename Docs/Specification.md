# JTL SDK. Техническая сводка

Версия документа: 0.1 (21.09.2026). Статус: решения согласованы, разработка не начата.

Документ описывает, что такое JTL SDK, как он устроен, как им пользоваться из игры, как он реализован на каждой площадке и как выглядит тулкит. Всё, что здесь написано, обязательно к исполнению при разработке. Спорные места вынесены в раздел «Открытые вопросы».

---

## 1. Цель и принципы

JTL SDK - собственный платформенный слой студии JTL Studio для WebGL-игр на веб-порталах. Он заменяет Prime SDK, PluginYG2 и обёртку YouTube Playables одним пакетом с единым API.

Что даёт:

- Одна ветка игры вместо ветки на площадку. Площадка выбирается в тулките, код игры не меняется.
- Один API для рекламы, покупок, сохранений, языка, паузы, лидербордов, игрока, флагов, времени.
- Одинаковое поведение в редакторе и в билде. Всё, что можно проверить в Play Mode, проверяется в Play Mode.
- Нет ситуации «вызов до готовности роняет игру».

Принципы:

1. **Код игры не знает площадку.** Он работает со статическим фасадом `JTLSDK` и проверяет возможности через `Supports(...)`.
2. **Общая логика пишется один раз.** Площадка реализует только тонкий адаптер. Частота показов, паузы, учёт покупок живут в общем слое или в настройках адаптера площадки, а не в игре.
3. **Ошибки не подменяются значениями.** Каждый вызов возвращает результат (`AdResult`, `PurchaseResult`), по которому видно, что произошло.
4. **Готовность - это состояние, а не соглашение.** До `IsReady` любой вызов безопасен и предсказуем.
5. **Редактор равен билду.** Прототипы в редакторе проходят через тот же общий слой, что и площадки.

Что взято из изученных SDK:

| Источник | Взято |
|---|---|
| Prime SDK | схема инициализации (модули, провайдеры, готовность), статический фасад, пауза по источникам с владением timeScale/звуком/курсором, машина состояний gameplay, восстановление покупок по счётчику выдач, симуляция оверлеями, определение AdBlock, оверлей «Нажми, чтобы продолжить», анализатор API |
| PluginYG2 | предзагрузка данных на странице до старта Unity, насыщенные настройки WebGL-шаблона, симуляция отказов, пресеты сборки на площадку, плашка тестовой сборки, zip и нумерация билдов, скрытие UI под возможности площадки |
| YouTube Playables | `firstFrameReady` со страницы, звук площадки главнее звука игры, стабильный id награды, проверка лимитов площадки при сборке, `FromJsonOverwrite` поверх значений по умолчанию |

Чего не повторяем: колбэки-поля вместо событий, `SendMessage` по имени объекта, строки `"true"` через мост, разное поведение редактора и билда, consume до выдачи, глобальная перезапись define-символов на все платформы, закрытая кодогенерация.

---

## 2. Имена, репозиторий, установка

| Что | Значение |
|---|---|
| Отображаемое имя | JTL SDK |
| Статический фасад | `JTLSDK` |
| Namespace | `JTLStudio.SDK` |
| Пакет | `com.jtlstudio.sdk` |
| Репозиторий | `github.com/meepifiev/JTLSDK` (публичный, MIT) |
| Скачиваемые модули | `github.com/meepifiev/JTLSDK-<Module>`, например `JTLSDK-Analytics` |
| Папка проекта | `~/repos/JTLSDK` |
| Unity | минимум 2021.3.18f1; разработка на 2021.3.45f2; проверка на 2022.3 LTS и Unity 6 |

Репозиторий - это Unity-проект, пакет лежит внутри как embedded:

```text
JTLSDK/
  Assets/                          интеграционные тесты, тестовые ассеты
  Packages/com.jtlstudio.sdk/      сам пакет (то, что ставится в игры)
  ProjectSettings/
  Docs/                            этот документ и рабочие заметки
  modules.json                     список скачиваемых модулей
  .github/workflows/               CI
  LICENSE
```

Демо-сцена лежит внутри пакета как UPM-сэмпл (раздел 3.2), а не в `Assets`.

Установка в игру через Package Manager по ссылке с путём и тегом:

```text
https://github.com/meepifiev/JTLSDK.git?path=Packages/com.jtlstudio.sdk#v1.0.0
```

Обновление через окно тулкита (раздел 13).

---

## 3. Архитектура

### 3.1. Слои

```text
Игра
  └─ JTLSDK (статический фасад)
       └─ Модули: интерфейсы IAds, IData, IPayments, ...
            └─ Общие сервисы: AdsService, DataService, PaymentsService, PauseService, ...
                 └─ Провайдеры площадки: IAdsProvider, IDataProvider, ...
                      ├─ Editor (прототипы)
                      ├─ YandexGames (C# + jslib)
                      └─ YouTubePlayables (C# + jslib)
                           └─ Мост C# ⇄ JS (jtlsdk.jspre, TypeScript)
```

- **Фасад** отдаёт модули и управляет жизненным циклом.
- **Модуль** - публичный интерфейс для игры.
- **Общий сервис** реализует модуль и держит всю логику, которая одинакова на всех площадках: единственный показ рекламы за раз, пауза, учёт покупок, ревизии сейвов, машина состояний gameplay.
- **Провайдер** реализует только вызовы конкретной площадки. Провайдер не знает про паузу, счётчики и очереди.
- **Мост** переводит вызовы в JS и обратно.

### 3.2. Структура пакета и сборки

```text
Packages/com.jtlstudio.sdk/
  package.json
  Runtime/
    JTLStudio.SDK.asmdef
    Facade/            JTLSDK.cs, SdkInstance.cs, SdkSettings.cs
    Modules/           интерфейсы модулей, enum, структуры результатов
    Services/          общие сервисы
    Providers/         интерфейсы провайдеров
    Bridge/            C#-сторона моста
    Prototype/         провайдеры-прототипы (код под UNITY_EDITOR)
    Plugins/WebGL/     jtlsdk.jspre (собирается из Bridge~)
    Platforms/
      YandexGames/     JTLStudio.SDK.YandexGames.asmdef, провайдеры, yandexgames.jslib
      YouTubePlayables/ JTLStudio.SDK.YouTubePlayables.asmdef, провайдеры, youtube.jslib
  Editor/
    JTLStudio.SDK.Editor.asmdef
    Toolkit/           окно, разделы, UXML, USS, локализация RU/EN
    Simulation/        прототипы, оверлей во вкладке Game
    Configuration/     применение конфигураций, define-символы, шаблон
    Build/             сборка, нумерация, проверки, zip, плашка
    Updates/           GitHub Releases, модули
    Analyzer/          анализатор API
    SaveEditor/        окно сохранений
  Bridge~/             TypeScript-исходники моста, package.json, tsconfig.json
  Samples~/Demo/       демо-сцена (UPM-сэмпл)
  Tests/               EditMode и PlayMode тесты
  Documentation~/      документация RU и EN
```

**Демо-сцена как сэмпл.** Папка `Samples~/Demo` объявлена в `package.json`:

```json
"samples": [
  {
    "displayName": "Demo",
    "description": "Scene with a button for every module and on-screen results.",
    "path": "Samples~/Demo"
  }
]
```

В Package Manager у пакета есть вкладка Samples с кнопками Import и Remove. Import копирует сцену в `Assets/Samples/JTL SDK/<версия>/Demo`, Remove удаляет папку. В билд игры демо не попадает, пока его не импортировали. В репозитории разработки демо импортируется так же, а правки переносятся в `Samples~/Demo` вручную. Папка `Assets/Samples` в репозитории разработки в `.gitignore`.

WebGL-шаблон в пакете не лежит. Он живёт в отдельном репозитории `meepifiev/JTLSDK-WebGLTemplate` со своими релизами, тулкит скачивает его с GitHub и ставит в `Assets/WebGLTemplates/JTLSDK` (раздел 11). Так же устроен шаблон PrimeGames у Prime.

Сборки:

| asmdef | Платформы | Содержимое |
|---|---|---|
| `JTLStudio.SDK` | все | фасад, модули, сервисы, мост, прототипы (`Runtime/Prototype`, код под `UNITY_EDITOR`) |
| `JTLStudio.SDK.YandexGames` | Editor, WebGL | провайдеры Яндекса |
| `JTLStudio.SDK.YouTubePlayables` | Editor, WebGL | провайдеры YouTube |
| `JTLStudio.SDK.Editor` | Editor | тулкит и всё редакторское |
| `JTLStudio.SDK.Tests.EditMode` | Editor, `UNITY_INCLUDE_TESTS` | тесты |

Сборки площадок компилируются всегда, без define constraints: провайдеры хранятся в конфигурациях через `SerializeReference` и должны существовать до активации, иначе конфигурацию нельзя ни создать, ни показать в тулките. Define-символ активной конфигурации гейтит только тела `[DllImport]` (`#if JTLSDK_YANDEX_GAMES && UNITY_WEBGL && !UNITY_EDITOR`) и `.jslib` через `PluginImporter.DefineConstraints`. Провайдер неактивной площадки в билд не попадает благодаря managed stripping, потому что на него никто не ссылается.

Asmdef обязательны: Unity не компилирует скрипты пакета из `Packages` без asmdef (проверено на 2021.3.45f2: `Script '...' will not be compiled because it exists outside the Assets folder and does not belong to any assembly definition file`). Модель та же, что у Prime.

У `JTLStudio.SDK` стоит `"autoReferenced": true`, как у Prime. Код игры в `Assembly-CSharp` видит `JTLSDK` без настроек. Ссылку на `JTLStudio.SDK` добавляют только проекты, где код игры разбит на свои asmdef. Это стандартное поведение Unity для любого пакета.

`.jslib` площадок получают те же define constraints через `PluginImporter.DefineConstraints`, поэтому в билд попадает JS только активной площадки. `jtlsdk.jspre` общий и попадает всегда.

Define-символ активной конфигурации ставится только для WebGL (`BuildTargetGroup.WebGL`) и только один. При смене конфигурации старый символ снимается.

### 3.3. Модули и провайдеры

Каждый модуль реализован общим сервисом, который получает провайдер площадки:

```csharp
public interface IAdsProvider
{
    AdsCapabilities Capabilities { get; }
    void Initialize(Action<ProviderState> onInitialized);
    void ShowInterstitial(Action<AdResult> onResult);
    void ShowRewarded(string platformRewardId, Action<AdResult> onResult);
    void ShowBanner();
    void HideBanner();
}
```

Провайдер хранится в конфигурации через `[SerializeReference]` вместе со своими настройками. Провайдеры находятся через `TypeCache.GetTypesDerivedFrom<IAdsProvider>()`, поэтому провайдер из любой сборки проекта появляется в выпадающем списке тулкита. Нет строковых имён провайдеров и нет кодогенерации.

Если у площадки нет модуля, в конфигурации стоит `Unsupported`-провайдер. Он отвечает `IsSupported == false`, а на вызовы возвращает результат `NotSupported` без ошибок.

### 3.4. Конфигурации

Конфигурация - ScriptableObject `SdkConfiguration`. Их несколько, активная одна.

```csharp
public class SdkConfiguration : ScriptableObject
{
    [SerializeField] private string _displayName;
    [SerializeField] private PlatformId _platform;
    [SerializeField] private string _defineSymbol;
    [SerializeField] private PlayerSettingsPreset _playerSettings;
    [SerializeField] private TemplateOverrides _templateOverrides;
    [SerializeField] private Language[] _supportedLanguages;
    [SerializeReference] private IAdsProvider _ads;
    [SerializeReference] private IPaymentsProvider _payments;
    [SerializeReference] private IDataProvider _data;
    [SerializeReference] private ILanguageProvider _language;
    [SerializeReference] private IPlayerProvider _player;
    [SerializeReference] private ILeaderboardsProvider _leaderboards;
    [SerializeReference] private IFlagsProvider _flags;
    [SerializeReference] private ITimeProvider _time;
    [SerializeReference] private IPlatformProvider _platformProvider;
    [SerializeReference] private IReviewProvider _review;
    [SerializeReference] private IGameLabelProvider _gameLabel;
}
```

`PlayerSettingsPreset` - что применяется к проекту при активации. Каждое поле разработчик выбирает сам в тулките (выпадающие списки и галки), у каждого есть галка «Применять». Что не отмечено, тулкит не трогает. Значения по умолчанию для новой конфигурации:

| Поле | Yandex Games | YouTube Playables |
|---|---|---|
| WebGL Template | JTLSDK | JTLSDK |
| Compression Format | Brotli | Disabled |
| Decompression Fallback | выкл | выкл |
| Data Caching | вкл | вкл |
| Managed Stripping Level | Medium | High |
| Run In Background | вкл | вкл |
| Memory Size (MB) | 512 | 512 |

Для YouTube Playables значение Compression Format = Disabled обязательно по требованиям площадки: другое значение подсвечивается красным и блокирует сборку.

Активация конфигурации (кнопка «Сделать активной» в тулките):

1. Снять define-символ прошлой конфигурации, поставить новый.
2. Применить отмеченные поля `PlayerSettingsPreset`.
3. Если шаблон не установлен, предложить скачать его с GitHub (раздел 11).
4. Записать ссылку на активную конфигурацию в `JTLSDKSettings`.
5. Unity перекомпилирует проект. Это нормально и ожидаемо, как смена платформы.

Одна сборка = одна конфигурация. Определения площадки по URL нет.

### 3.5. Где лежат настройки

| Файл | Что хранит | В VCS | В билде |
|---|---|---|---|
| `Assets/Resources/JTLSDK/JTLSDKSettings.asset` | ссылка на активную конфигурацию, языки, каталог покупок, лидерборды, флаги по умолчанию, таймаут инициализации, задержка автосохранения, логирование | да | да |
| `Assets/Settings/JTLSDK/Configurations/*.asset` | конфигурации | да | только активная (по ссылке) |
| `Assets/WebGLTemplates/JTLSDK/` | шаблон, скачанный тулкитом с GitHub, можно править | да | при сборке |
| `ProjectSettings/JTLSDKEditorSettings.asset` | шаблон (общие настройки), сборка, симуляция по умолчанию, язык тулкита (по умолчанию английский), счётчик билдов, путь и namespace генерации констант | да | нет |
| `UserSettings/JTLSDKUserSettings.asset` | стартовый язык Play Mode, запомненные результаты прототипов, кеш проверки обновлений | нет | нет |

`Create()` читает только `Resources/JTLSDK/JTLSDKSettings.asset`. Это единственное, что SDK кладёт в Resources.

---

## 4. Инициализация и жизненный цикл

### 4.1. Фасад

```csharp
namespace JTLStudio.SDK
{
    public static class JTLSDK
    {
        public const string Version = "0.1.0";

        public static bool IsCreated { get; }
        public static bool IsReady { get; }

        public static IAds Ads { get; }
        public static IData Data { get; }
        public static IPayments Payments { get; }
        public static ILanguage Language { get; }
        public static IPause Pause { get; }
        public static ITime Time { get; }
        public static IAudio Audio { get; }
        public static IGameEvents GameEvents { get; }
        public static ILeaderboards Leaderboards { get; }
        public static IPlayer Player { get; }
        public static IFlags Flags { get; }
        public static IPlatform Platform { get; }
        public static IDevice Device { get; }
        public static IReview Review { get; }
        public static IGameLabel GameLabel { get; }

        public static void Create();
        public static void WhenReady(Action onReady);
    }
}
```

Все модули наследуют базовый интерфейс:

```csharp
public enum ModuleState
{
    Pending,
    Ready,
    Failed,
    Unsupported
}

public interface IModule
{
    ModuleState State { get; }
    bool IsSupported { get; }
    bool IsReady { get; }
    void WhenReady(Action onReady);
}
```

`IsReady` истинно, когда инициализация модуля закончилась любым исходом (`Ready` или `Failed`). Ошибка видна по `State`.

### 4.2. Что делает `Create()`

1. Загружает `JTLSDKSettings` из Resources. Если файла нет, бросает `InvalidOperationException`.
2. Создаёт модули из активной конфигурации.
3. Создаёт скрытый объект `JTLSDK` с `DontDestroyOnLoad`. Он даёт SDK `Update`, `OnApplicationFocus`, `OnApplicationPause`, `OnApplicationQuit`. Это единственный MonoBehaviour в рантайме SDK.
4. Регистрирует в мосте один колбэк для всех ответов JS и забирает события, накопленные на странице до старта Unity.
5. Вызывает `Initialize` у каждого провайдера. Провайдеры Яндекса и YouTube через мост забирают то, что страница уже предзагрузила: язык, окружение, сейв, каталог, покупки, игрока, флаги, серверное время.
6. По мере ответов модули переходят в `Ready` или `Failed`.
7. Когда все модули не `Pending`, `IsReady = true` и вызываются колбэки `WhenReady` в порядке регистрации.

Повторный `Create()` бросает `InvalidOperationException`. Обращение к любому модулю до `Create()` бросает `InvalidOperationException`. Оба правила действуют и в редакторе, и в билде, чтобы ошибка была видна сразу.

### 4.3. Таймаут

`InitializationTimeoutSeconds` (по умолчанию 15). По истечении все модули в `Pending` переходят в `Failed`, `IsReady` становится истинным, игра стартует. Модуль `Data` в `Failed` работает в памяти без записи в облако и повторяет загрузку (раздел 6.2). Мост продолжает ждать ответы: если площадка ответила позже, модуль переходит в `Ready` и вызывает свои `WhenReady`.

### 4.4. Поведение до готовности

| Вызов | До `Create()` | После `Create()`, модуль `Pending` |
|---|---|---|
| `JTLSDK.Ads` | исключение | доступен |
| `Ads.ShowRewarded(...)` | исключение | колбэк сразу с `AdResult.NotReady` |
| `Data.GetInt(...)` | исключение | значение по умолчанию + предупреждение в лог |
| `Data.SetInt(...)` | исключение | игнорируется + ошибка в лог |
| `Payments.Purchase(...)` | исключение | колбэк сразу с `PurchaseResult.NotReady` |
| `GameEvents.GameReady()` | исключение | запоминается и отправляется после готовности |
| `Pause.Set(...)`, `Time.Scale`, `Audio.Volume` | исключение | работают сразу, площадка не нужна |

На стороне JS объект `Module.JTLSDK` создаётся в `jtlsdk.jspre` в момент загрузки страницы, до запуска любого C#. Ни одна функция `.jslib` не обращается к неопределённому объекту. Это закрывает краш Prime `reading data at _primeSDK_data_getInt`.

### 4.5. Пример входной точки

```csharp
public class Bootstrapper : MonoBehaviour
{
    private void Awake()
    {
        JTLSDK.Create();
        JTLSDK.WhenReady(StartGame);
    }

    private void StartGame()
    {
        JTLSDK.Language.Changed += OnLanguageChanged;
        ApplySettings();
        LoadFirstScene();
    }

    private void ApplySettings()
    {
        float musicVolume = JTLSDK.Data.GetFloat(SaveKeys.MusicVolume, 0.5f);
        _audio.SetMusicVolume(musicVolume);
    }
}
```

`GameEvents.GameReady()` вызывается игрой, когда исчез экран загрузки и игрок может нажимать. Не раньше.

---

## 5. Использование в игре

Короткие примеры на каждый модуль. Подробности в разделе 6.

```csharp
JTLSDK.Ads.ShowRewarded(RewardIds.DoubleMoney, result =>
{
    if (result == AdResult.Rewarded)
    {
        _wallet.Add(_reward);
    }
    else if (result == AdResult.Blocked)
    {
        _alerts.Show(AlertIds.DisableAdBlock);
    }
});

JTLSDK.Ads.ShowInterstitial();

int money = JTLSDK.Data.GetInt(SaveKeys.Money);
JTLSDK.Data.SetInt(SaveKeys.Money, money + 100);

JTLSDK.Payments.Granted += OnProductGranted;
JTLSDK.Payments.Purchase(ProductIds.RemoveAds, result => { });

if (JTLSDK.Payments.TryGetPrice(ProductIds.RemoveAds, out ProductPrice price))
{
    _priceLabel.text = price.Formatted;
}

Language language = JTLSDK.Language.Current;
JTLSDK.Language.Set(Language.Russian);

JTLSDK.Time.Scale = 0.3f;
JTLSDK.Audio.Volume = 0.8f;

using (JTLSDK.Pause.Hold(PauseSources.Menu))
{
}

JTLSDK.GameEvents.GameReady();
JTLSDK.GameEvents.Start();
JTLSDK.GameEvents.Stop();

JTLSDK.Leaderboards.SetScore(LeaderboardIds.Levels, currentLevel);

JTLSDK.Player.Authorize(success => { });

bool tutorialEnabled = JTLSDK.Flags.GetBool(FlagKeys.Tutorial, true);

DateTimeOffset now = JTLSDK.Time.Now;

JTLSDK.Review.Request(sent => { });
JTLSDK.GameLabel.ShowDialog(created => { });

if (JTLSDK.Platform.Supports(Capability.Purchases) == false)
{
    _shopButton.gameObject.SetActive(false);
}
```

Идентификаторы (`RewardIds`, `ProductIds`, `SaveKeys`, `LeaderboardIds`, `FlagKeys`, `PauseSources`) - константы в коде игры. SDK принимает строки, но игра держит их в одном месте.

Готовый компонент для UI: `CapabilityFilter` (MonoBehaviour) с полем `Capability` и режимом «Скрыть, если не поддерживается» или «Показать, если не поддерживается». Вешается на кнопку магазина, лидерборда, авторизации.

---

## 6. Модули

### 6.1. Реклама

```csharp
public enum AdResult
{
    Shown,
    Rewarded,
    Closed,
    NotShown,
    Blocked,
    NotSupported,
    NotReady,
    Failed
}

public interface IAds : IModule
{
    bool IsShowing { get; }
    bool IsInterstitialSupported { get; }
    bool IsRewardedSupported { get; }
    bool IsBannerSupported { get; }
    bool IsBannerVisible { get; }

    event Action Opened;
    event Action Closed;

    void ShowInterstitial(Action<AdResult> onResult = null);
    void ShowRewarded(string rewardId, Action<AdResult> onResult);
    void ShowBanner();
    void HideBanner();
}
```

Результаты:

| Результат | Интерстишл | Rewarded |
|---|---|---|
| `Shown` | показан и закрыт | не используется |
| `Rewarded` | не используется | досмотрен, награду выдавать |
| `Closed` | не используется | закрыт раньше, награду не выдавать |
| `NotShown` | площадка не показала: кулдаун, нет заполнения, уже идёт другой показ | то же |
| `Blocked` | обнаружен блокировщик рекламы | то же |
| `NotSupported` | у площадки нет формата | то же |
| `NotReady` | SDK ещё не готов | то же |
| `Failed` | ошибка площадки | то же |

Общий слой (`AdsService`) гарантирует:

1. Одновременно идёт один показ. Второй вызов сразу получает `NotShown`. Это закрывает двойную награду по двойному клику.
2. На время показа ставится пауза с источником `Ads` (раздел 6.5). Пауза снимается в `finally`, даже если колбэк игры бросил исключение.
3. На время показа gameplay останавливается (`GameEvents.GameplayStopped`), после показа восстанавливается прежнее состояние.
4. Колбэк вызывается ровно один раз.
5. Если включено определение AdBlock, оно выполняется до запроса рекламы.

Частота показов, первый показ, пропуск интерстишла после rewarded - правила площадки. Общий слой их не знает. Они живут в настройках провайдера площадки, значения по умолчанию взяты из требований площадки. Провайдер отвечает `NotShown`, если правило не выполнено.

Настройки провайдера Яндекса:

| Поле | По умолчанию | Смысл |
|---|---|---|
| `AdBlockDetection` | вкл | проверка приманкой перед показом |
| `MinimumInterstitialIntervalSeconds` | 60 | не чаще, чем требует Яндекс; можно только увеличить |
| `SkipInterstitialAfterRewardedSeconds` | 60 | после rewarded интерстишл пропускается |
| `StickyBanner` | выкл | показывать sticky-баннер при старте |

Настройки провайдера YouTube: полей нет. Интерстишл и rewarded показывает само приложение YouTube по своим правилам. AdBlock не определяется.

**Определение AdBlock.** Перед показом на страницу добавляется невидимый `<div>` с классами, которые блокировщики прячут (`ad-banner`, `adsbox`). Через 100 мс проверяется `offsetHeight` и `display`. Если элемент скрыт, реклама не запрашивается, результат `Blocked`. Игра показывает своё сообщение «Отключите блокировщик».

**Оверлей «Нажми, чтобы продолжить».** HTML-оверлей поверх канваса на странице, не Unity UI: тёмная полупрозрачная заливка и крупный текст. Как у Prime, два режима:

1. Ручной: `JTLSDK.Pause.ShowContinuePrompt(Action onContinue = null)`. Нужен там, где площадка показала свой оверлей (например, уведомление CrazyGames об AdBlock), игра потеряла фокус и стоит на паузе. Клик возвращает фокус окну, снимает паузу с источником `Platform` и вызывает `onContinue`.
2. Автоматический: настройка конфигурации «Показывать оверлей на паузе», по умолчанию выключена. Оверлей появляется при любой паузе от площадки и исчезает по клику.

Текст локализован по `Language.Current`, а не по языку браузера. Водяного знака нет.

Id награды: игра передаёт свой `rewardId`. На YouTube он уходит как есть (стабильный id обязателен). На Яндексе не используется.

### 6.2. Сохранения

API как у Prime: ключ-значение.

```csharp
public enum DataState
{
    Pending,
    Loaded,
    Empty,
    Failed
}

public interface IData : IModule
{
    DataState LoadState { get; }
    bool IsDirty { get; }

    event Action Loaded;
    event Action<bool> Flushed;

    bool HasKey(string key);
    int GetInt(string key, int defaultValue = 0);
    float GetFloat(string key, float defaultValue = 0f);
    bool GetBool(string key, bool defaultValue = false);
    string GetString(string key, string defaultValue = "");
    T GetObject<T>(string key, T defaultValue = null) where T : class;

    void SetInt(string key, int value);
    void SetFloat(string key, float value);
    void SetBool(string key, bool value);
    void SetString(string key, string value);
    void SetObject<T>(string key, T value) where T : class;

    void DeleteKey(string key);
    void DeleteAll();

    void Save();
    void Flush(Action<bool> onFlushed = null);
}
```

**Формат.** Один JSON-документ:

```json
{
  "format": 1,
  "revision": 42,
  "savedAt": "2026-09-21T14:03:11Z",
  "values": {
    "Money": 1200,
    "Level": 27,
    "MusicVolume": 0.5,
    "Profile": { "name": "Player", "skins": [1, 4] }
  }
}
```

`SetObject<T>` сериализует объект через `JsonUtility` и кладёт его вложенным JSON, а не строкой. `GetObject<T>` создаёт `new T()` и накладывает JSON через `FromJsonOverwrite`, поэтому поля, которых не было в старом сейве, получают значения по умолчанию. JSON-парсер свой, внутри пакета, без Newtonsoft.

**Где меняются сохранения.**

```text
игра: Set*(key, value)
  → память (словарь значений)
  → IsDirty = true, revision не меняется
  → таймер отложенной записи (AutosaveDelaySeconds, по умолчанию 2)
    → Flush: revision++, savedAt, сериализация
      → провайдер площадки: Яндекс player.setData, YouTube saveData, Editor PlayerPrefs
      → Flushed(success)
```

Хранилище одно: то, что даёт площадка. Второй копии в `localStorage` нет: она создаёт конфликты у игрока с двумя устройствами, а Яндекс для неавторизованных игроков и так хранит данные локально сам.

Запись происходит:

- по таймеру после последнего `Set*`;
- сразу при `Save()`;
- сразу при паузе площадки, потере фокуса, скрытии вкладки (`visibilitychange`, `pagehide`), выходе из Play Mode;
- сразу после выдачи покупки, перед consume (раздел 6.3).

Игра обычно не вызывает `Save()`. Он нужен перед действием, после которого потеря прогресса недопустима, например перед переходом на другую страницу.

**Загрузка.**

```text
Create()
  → провайдер: загрузить сейв площадки
  → LoadState = Loaded | Empty
  → Loaded, State = Ready
```

Если площадка не ответила или ответила ошибкой:

- `LoadState = Failed`, `State = Failed`;
- значения в памяти - значения по умолчанию;
- запись в облако запрещена, чтобы не затереть настоящий сейв значениями по умолчанию; `Set*` работает только в памяти;
- каждые 30 секунд повтор загрузки; при успехе загруженные значения заменяют память, `LoadState = Loaded`, запись разрешается.

«Пусто» и «ошибка» различаются всегда. Пустой сейв - нормальный первый запуск, запись после него разрешена.

**Авторизация.** После успешного `Player.Authorize` сейв площадки загружается заново. Если сейв аккаунта пуст, в него записывается текущий прогресс. Если не пуст, он заменяет текущий. До завершения перезагрузки запись приостановлена.

**Лимиты.** Провайдер сообщает `MaxBytes` и `RecommendedBytes`. Яндекс: 200 КБ. YouTube: 3 МиБ, рекомендовано 500 КиБ. При превышении `RecommendedBytes` предупреждение в лог. При превышении `MaxBytes` запись не выполняется, `Flushed(false)`, ошибка в лог. В редакторе те же лимиты активной конфигурации.

**Редактор.** Хранилище - `PlayerPrefs`, ключ `JTLSDK.Data`, тот же JSON. Правится в окне «Сохранения» тулкита (раздел 9.9).

**Что не делаем.** Никаких partial-классов и глобальных объектов сейва. Игра сама решает, какие ключи ей нужны.

### 6.3. Покупки

```csharp
public enum ProductType
{
    NonConsumable,
    Consumable
}

public enum PurchaseResult
{
    Purchased,
    Cancelled,
    NotSupported,
    NotReady,
    Failed
}

public readonly struct ProductPrice
{
    public decimal Value { get; }
    public string CurrencyCode { get; }
    public string Formatted { get; }
}

public interface IPayments : IModule
{
    event Action<string> Granted;

    bool IsPurchased(string productId);
    bool TryGetPrice(string productId, out ProductPrice price);
    void Purchase(string productId, Action<PurchaseResult> onResult);
}
```

**Каталог** задаётся в тулките (раздел 9.5): id в игре, тип, id на каждой площадке, тестовая цена для редактора. Провайдер получает id площадки, игра работает только со своим.

**Порядок выдачи: сначала выдать, потом consume.**

```text
Purchase(productId)
  → провайдер: покупка на площадке
  → успех: Granted(productId)            игра выдаёт товар и пишет в Data
  → Data.Flush                            запись сейва
  → успех записи: провайдер consume       только для Consumable
  → onResult(Purchased)
```

Если игра бросила исключение в обработчике `Granted` или запись сейва не удалась, consume не вызывается. При следующем запуске площадка вернёт покупку как незавершённую, и `Granted` вызовется снова. Обработчик `Granted` для расходуемых товаров должен быть «добавить», а не «установить».

**Восстановление.** При инициализации провайдер запрашивает список покупок площадки. Разовые попадают в `IsPurchased`. Расходуемые без consume вызывают `Granted` по одной и проходят тот же путь выдачи. Отдельного `Restore` в API нет, это происходит всегда.

**Готовность.** `Granted` никогда не вызывается до `Data.IsReady`, чтобы игре было куда записать выдачу. Незавершённые покупки выдаются, когда на `Granted` подписан первый обработчик. Без обработчика расходуемая покупка не списывается и ждёт его.

**YouTube.** Покупок нет. Провайдер `Unsupported`, `Purchase` отвечает `NotSupported`, кнопки магазина скрываются через `CapabilityFilter`.

**Редактор.** Прототип держит покупки в `PlayerPrefs` и проходит через тот же `PaymentsService`. Кнопка «Оплатить, игра упала до выдачи» сохраняет покупку без consume, и при следующем запуске Play Mode `Granted` придёт снова.

### 6.4. Язык

Язык - enum с явными номерами. Новые значения добавляются только в конец.

```csharp
public enum Language
{
    English = 0,
    Russian = 1,
    Turkish = 2,
    Spanish = 3,
    Portuguese = 4,
    German = 5,
    French = 6,
    Italian = 7,
    Polish = 8,
    Ukrainian = 9,
    Belarusian = 10,
    Kazakh = 11,
    Uzbek = 12,
    Azerbaijani = 13,
    Armenian = 14,
    Georgian = 15,
    Romanian = 16,
    Arabic = 17,
    Hebrew = 18,
    Hindi = 19,
    Indonesian = 20,
    Japanese = 21,
    Korean = 22,
    ChineseSimplified = 23,
    Vietnamese = 24,
    Thai = 25
}

public interface ILanguage : IModule
{
    Language Current { get; }
    IReadOnlyList<Language> Supported { get; }

    event Action<Language> Changed;

    void Set(Language language);
}
```

Через мост язык идёт строкой-кодом (BCP-47), а не номером. Разбор в одном месте в C#:

```csharp
public class LanguageCodes
{
    public bool TryParse(string code, out Language language);
    public string ToCode(Language language);
}
```

`TryParse` отрезает регион (`en-US` → `en`), `zh-CN` и `zh-Hans` сводит к `ChineseSimplified`.

**Выбор языка при старте.** Язык определяет площадка, как у Prime.

```text
1. Код площадки → TryParse.
2. Если языка нет среди Supported → таблица замен (например Belarusian → Russian).
3. Если и после замены нет → язык по умолчанию.
```

`Supported` - пересечение списка языков игры и списка активной конфигурации. `Set` с языком вне `Supported` бросает `ArgumentOutOfRangeException`.

`Set` меняет `Current` до конца сессии и вызывает `Changed`. SDK выбор игрока не запоминает. Если в игре есть переключатель языка, игра сама хранит выбор в `Data` и вызывает `Set` после `WhenReady`.

Переводы остаются в игре. SDK отвечает за определение языка площадки и событие смены.

### 6.5. Пауза, время, звук, курсор

SDK владеет `Time.timeScale`, `AudioListener.volume`, `AudioListener.pause`, `Cursor.visible`, `Cursor.lockState`. Игра пишет в свойства SDK, а SDK применяет итог с учётом паузы.

```csharp
public interface IPause : IModule
{
    bool IsPaused { get; }
    IReadOnlyCollection<string> Sources { get; }

    event Action<bool> Changed;

    void Set(string source, bool paused);
    IDisposable Hold(string source);
    void ShowContinuePrompt(Action onContinue = null);
}

public interface ITime : IModule
{
    float Scale { get; set; }
    DateTimeOffset Now { get; }
    bool IsServerTime { get; }
}

public interface IAudio : IModule
{
    float Volume { get; set; }
    bool IsPlatformMuted { get; }

    event Action<bool> PlatformMuteChanged;
}

public interface IDevice : IModule
{
    bool IsMobile { get; }
    DeviceType Type { get; }
    bool CursorVisible { get; set; }
    CursorLockMode CursorLock { get; set; }
}
```

Источники паузы, которые ставит сам SDK: `Platform` (потеря фокуса, скрытие вкладки, `onPause` YouTube, `game_api_pause` Яндекса), `Ads`, `Purchase`. Игра добавляет свои строки (`Menu`, `Settings`).

Применение:

```text
Time.timeScale       = IsPaused ? 0 : Scale
AudioListener.pause  = IsPaused
AudioListener.volume = (IsPaused || IsPlatformMuted) ? 0 : Volume
Cursor               = IsPaused ? видимый и свободный : значения игры
```

Геттеры `Scale`, `Volume`, `CursorVisible` возвращают значение игры, а не применённое. Во время паузы `Time.Scale` читается как 0.3, если игра поставила 0.3. У Prime это непоследовательно, здесь единообразно.

Почему так, на примере: игра включила замедление 0.3 и через секунду по unscaled-таймеру вернёт 1. Открылась реклама. Если бы игра писала `Time.timeScale` напрямую, таймер во время рекламы поставил бы 1, и игра поехала бы под рекламой. А после рекламы SDK вернул бы «то, что было», то есть 0.3, и игра застряла бы в замедлении. С `Sdk.Time.Scale` записанная единица запоминается, применяется после снятия паузы, и оба бага невозможны.

Повторная пауза от того же источника ничего не меняет. Снятие паузы источником, которого нет, ничего не меняет. Игра не может застрять на паузе из-за двойного `onPause`.

`PauseOnFocusLoss` - настройка конфигурации, по умолчанию включена.

### 6.6. Игровые события

```csharp
public interface IGameEvents : IModule
{
    bool IsGameReady { get; }
    bool IsGameplayActive { get; }

    void GameReady();
    void GameplayStarted();
    void GameplayRestarted();
    void GameplayStopped();
}
```

Машина состояний в общем слое:

- `GameReady()` отправляется один раз; повторные вызовы игнорируются. До готовности SDK запоминается.
- `GameplayStarted()` при активном геймплее игнорируется, `GameplayStopped()` при неактивном тоже.
- `GameplayRestarted()` останавливает активный геймплей и сразу запускает снова. Если геймплей не шёл, просто запускает.
- На время рекламы геймплей останавливается автоматически, после рекламы состояние восстанавливается.
- На время паузы площадки то же самое.

Площадки: Яндекс `LoadingAPI.ready()`, `GameplayAPI.start()/stop()`; YouTube `game.gameReady()`, у start/stop реализации нет.

`firstFrameReady` для YouTube вызывает страница шаблона в момент показа экрана загрузки. Игре ничего делать не нужно.

### 6.7. Лидерборды

```csharp
public readonly struct LeaderboardEntry
{
    public int Rank { get; }
    public long Score { get; }
    public string PlayerName { get; }
    public string AvatarUrl { get; }
    public bool IsCurrentPlayer { get; }
}

public readonly struct LeaderboardPage
{
    public IReadOnlyList<LeaderboardEntry> Entries { get; }
    public LeaderboardEntry? CurrentPlayer { get; }
}

public interface ILeaderboards : IModule
{
    bool CanLoad { get; }

    void SetScore(string leaderboardId, long score);
    void GetPlayerEntry(string leaderboardId, Action<LeaderboardEntry?> onResult);
    void Load(string leaderboardId, int topCount, int aroundCount, Action<LeaderboardPage> onResult);
}
```

Список лидербордов задаётся в тулките: id в игре и id на каждой площадке.

Яндекс: `setScore` только для авторизованных; неавторизованному `SetScore` откладывается до авторизации и отправляется после неё. YouTube: `engagement.sendScore`, один лидерборд, `CanLoad == false`, `Load` возвращает пустую страницу.

### 6.8. Игрок

```csharp
public interface IPlayer : IModule
{
    bool IsAuthorized { get; }
    string Id { get; }
    string Name { get; }
    string AvatarUrl { get; }

    event Action Authorized;

    void Authorize(Action<bool> onResult);
}
```

После успешной авторизации: перезагрузка сейва (раздел 6.2), обновление покупок, отправка отложенных очков лидерборда, событие `Authorized`. YouTube: `IsSupported == false`, игрок анонимный.

### 6.9. Флаги (remote config)

```csharp
public interface IFlags : IModule
{
    bool HasKey(string key);
    bool GetBool(string key, bool defaultValue = false);
    int GetInt(string key, int defaultValue = 0);
    float GetFloat(string key, float defaultValue = 0f);
    string GetString(string key, string defaultValue = "");
}
```

Значения по умолчанию задаются в тулките и отдаются, если площадка флаг не прислала. Яндекс: `getFlags()` с этими же значениями как `defaultFlags`. YouTube: только значения по умолчанию.

### 6.10. Серверное время

`JTLSDK.Time.Now` и `JTLSDK.Time.IsServerTime`. Яндекс: `serverTime()` при инициализации, дальше локальный монотонный отсчёт от него. YouTube и редактор: локальные часы, `IsServerTime == false`. Игра решает сама, доверять ли локальному времени для таймеров.

### 6.11. Отзыв и ярлык

```csharp
public interface IReview : IModule
{
    bool CanRequest { get; }
    void Request(Action<bool> onResult);
}

public interface IGameLabel : IModule
{
    bool CanRequest { get; }
    void Request(Action<bool> onResult);
}
```

Яндекс: `feedback.canReview` и `requestReview`; `shortcut.canShowPrompt` и `showPrompt`. YouTube: `IsSupported == false`.

### 6.12. Площадка и устройство

```csharp
public enum PlatformId
{
    Editor = 0,
    YandexGames = 1,
    YouTubePlayables = 2
}

public enum DeviceType
{
    Desktop,
    Mobile,
    Tablet,
    TV
}

public enum Capability
{
    Interstitial,
    Rewarded,
    Banner,
    Purchases,
    Leaderboards,
    LeaderboardsLoad,
    Authorization,
    Flags,
    ServerTime,
    Review,
    GameLabel,
    PlatformMute
}

public interface IPlatform : IModule
{
    PlatformId Current { get; }
    string AppId { get; }
    bool Supports(Capability capability);
}
```

В Play Mode `Platform.Current` возвращает площадку активной конфигурации, а `Supports` её возможности. `Editor` в `Current` не бывает при активной конфигурации Яндекса или YouTube; он нужен для проекта без конфигураций.

---

## 7. Мост C# ⇄ JS

Исходники на TypeScript в `Bridge~/src`, сборка в один файл `Runtime/Plugins/WebGL/jtlsdk.jspre`. Файл лежит в репозитории, CI проверяет, что он совпадает со сборкой из исходников.

**C# → JS.** `[DllImport("__Internal")]` функции в `.jslib` вызывают `Module.JTLSDK.<module>.<action>(...)`. Аргументы: числа как есть, строки UTF-8, сложные структуры JSON-строкой. У каждого вызова, который ждёт ответ, есть `requestId`.

**JS → C#.** Один колбэк на всё:

```csharp
[MonoPInvokeCallback(typeof(BridgeCallback))]
private static void OnBridgeMessage(int requestId, int code, string payload)
```

C# регистрирует его при `Create()`. Ответы на запросы приходят с `requestId > 0` и удаляются из таблицы ожидания. События площадки (пауза, возобновление, звук, видимость) приходят с фиксированными отрицательными `requestId`. JS вызывает указатель через `getWasmTableEntry`, затем `wasmTable.get`, затем `dynCall_viii`, смотря что даёт версия Unity. Строки выделяются на стороне JS и освобождаются после возврата.

**Очередь событий.** События, пришедшие до регистрации колбэка (например `onPause` до старта Unity), копятся в JS и отдаются сразу после регистрации.

**Предзагрузка на странице.** Файл `jtlsdk-page.js` подключается шаблоном и запускается параллельно с загрузкой Unity: подключает скрипт площадки, инициализирует SDK площадки, запрашивает язык, окружение, сейв, каталог, покупки, игрока, флаги, серверное время. К моменту `Create()` ответы обычно уже есть, и инициализация занимает один кадр.

**Ошибки.** Каждый ответ несёт код: `0` успех, дальше коды ошибок площадки (`Unavailable`, `InvalidParams`, `SizeLimit`, `Cancelled`, `Unknown`). Ошибка никогда не превращается в «пустую строку» или «ноль».

---

## 8. Площадки

### 8.1. Editor

Провайдеры-прототипы. Работают только в редакторе, лежат в `Runtime/Prototype` под `UNITY_EDITOR`, в билд не попадают. Реклама и покупки рисуют оверлей во вкладке Game (раздел 10). Сейв в `PlayerPrefs`. Язык, устройство, пауза площадки и её звук переключаются в оверлее вкладки Game. Прототипы проходят через те же общие сервисы, что и площадки.

### 8.2. Yandex Games

| Модуль | Реализация |
|---|---|
| Инициализация | `<script src="/sdk.js">` в шаблоне, `YaGames.init()` на странице до старта Unity |
| Интерстишл | `adv.showFullscreenAdv` |
| Rewarded | `adv.showRewardedVideo`, награда по `onRewarded` |
| Баннер | `adv.showBannerAdv` / `hideBannerAdv` (sticky) |
| Сейвы | `player.getData` / `setData`, лимит 200 КБ |
| Покупки | `payments.getCatalog`, `purchase`, `getPurchases`, `consumePurchase` после выдачи |
| Лидерборды | `leaderboards.setLeaderboardScore`, `getLeaderboardPlayerEntry`, `getLeaderboardEntries` |
| Игрок | `getPlayer`, `auth.openAuthDialog` |
| Язык | `environment.i18n.lang` |
| Флаги | `getFlags` |
| Время | `serverTime()` |
| Игровые события | `features.LoadingAPI.ready`, `features.GameplayAPI.start/stop` |
| Пауза | `game_api_pause` / `game_api_resume`, плюс фокус страницы |
| Отзыв, ярлык | `feedback.*`, `shortcut.*` |
| Устройство | `deviceInfo` |

### 8.3. YouTube Playables

| Модуль | Реализация |
|---|---|
| Инициализация | `<script src="https://www.youtube.com/game_api/v1">` в `<head>` шаблона |
| `firstFrameReady` | страница, в момент показа экрана загрузки |
| Игровые события | `game.gameReady()` |
| Интерстишл | `ads.requestInterstitialAd()` → `Shown`, отказ → `Failed` |
| Rewarded | `ads.requestRewardedAd(rewardId)` → `true` `Rewarded`, `false` `Closed`, отказ `Failed` |
| Баннер, покупки, авторизация, отзыв, ярлык, флаги, серверное время | `Unsupported` |
| Сейвы | `game.loadData` / `saveData`, лимит 3 МиБ; запись до успешной загрузки отклоняется площадкой, SDK её и не делает |
| Лидерборд | `engagement.sendScore`, только лучший результат, без загрузки |
| Язык | `system.getLanguage()` |
| Звук | `system.isAudioEnabled` + `onAudioEnabledChange` → `Audio.IsPlatformMuted` |
| Пауза | `system.onPause` / `onResume` |
| Ошибки | `SdkError.errorType` доходит до C# кодом ошибки |

Требования площадки, которые SDK берёт на себя:

- сжатие Unity выключено (пресет конфигурации);
- проверка размера файлов при сборке: каждый файл меньше 30 МиБ, файлов не больше 8000 (раздел 12);
- никаких внешних запросов: скачиваемый модуль аналитики на этой конфигурации отключается сам;
- звук площадки главнее звука игры;
- `firstFrameReady` раньше `gameReady`, оба вызываются ровно один раз;
- шаблон поддерживает соотношения сторон от 9:32 до 32:9 и не блокирует ориентацию;
- шаблон не ставит `devicePixelRatio = 1` по умолчанию.

---

## 9. Тулкит: UI/UX и макеты

### 9.1. Принципы

- Одно окно `Window → JTL SDK`, UI Toolkit. Слева навигация, справа раздел. Ширина не меньше 900 px, окно можно докнуть.
- В шапке всегда: активная конфигурация, площадка, версия пакета, переключатель RU/EN, значок обновления.
- Все изменения сохраняются сразу, с Undo. Кнопки «Применить» нет, кроме опасных действий: активация конфигурации, сброс сейва, замена кода анализатором.
- Каждое поле с подсказкой на языке тулкита. Валидация прямо под полем красным текстом. Ошибки, которые сломают сборку, дублируются в разделе «Сборка» и блокируют кнопку.
- Никаких модальных окон, кроме подтверждения необратимых действий.
- Внизу строка состояния: последнее действие и его результат.
- Своя тёмная тема независимо от темы редактора. Эталон - макеты в Claude Design (`https://claude.ai/artifact/V2pEgoX88r48Wpifq7epib`): 15 экранов, окно 900×600, состояния, токены, компоненты, иконки, redlines.
- Текст интерфейса, подсказки и сообщения в двух языках. Переключатель хранится в `JTLSDKEditorSettings`.

**Токены дизайна** (из макета):

| Токен | Значение | Где |
|---|---|---|
| `bg` | `#0E0E10` | фон окна |
| `surface` | `#151518` | сайдбар, карточки, статус-бар |
| `raised` | `#1C1C20` | кнопки, выпадающие списки, чипы |
| `field-bg` | `#111114` | поля ввода |
| `border` | `#26262B` | рамки 1 px |
| `border-strong` | `#33333A` | рамка при наведении |
| `text` | `#F2F2F3` | основной текст |
| `text-secondary` | `#9A9AA3` | подписи, описания |
| `text-muted` | `#66666E` | подсказки, выключенное |
| `accent` | `#5C8CC2` | активный пункт меню, ссылки, фокус |
| `accent-strong` | `#3E6B9D` | заливка основной кнопки |
| `accent-border` | `#33506F` | рамка активной карточки |
| `accent-surface` | `#1A2430` | фон активного пункта меню |
| `success` | `#5CA872` | пройденные проверки |
| `warning` | `#C9973F` | пре-релиз, предупреждения |
| `error` | `#CF5F59` | ошибки валидации, проваленные проверки |
| `danger-fill` | `#8E3A35` | разрушающая кнопка |

Шрифт Inter: 20/600 заголовок страницы, 15/500 заголовок карточки, 13 основной, 12 подписи, 11 сноски, моноширинный 12 для кода и идентификаторов. В пакете лежат Inter Medium и SemiBold, в редакторе есть только Regular и Bold. Радиусы: 4 бейдж, 6 поле и кнопка, 8 карточка. Иконки тонкие линейные 16 px в меню и строках, 20 px в заголовках, 24 px логотипы площадок.

**Размеры** (из redlines): сайдбар 220 px, свёрнутый 52 px при ширине окна меньше 1040 px; шапка 44 px; отступ страницы 24 px (16 px в свёрнутом окне); карточка: отступ 16 px, промежуток 12 px; строки таблиц и списков 32 px, поля 28 px; колонка подписей 190 px на широких страницах и 150 px в карточках; статус-бар 28 px.

**Замены при вёрстке.** USS не знает `gap`, `line-height`, `text-transform` и пунктирных рамок: промежутки делаются отступами, заголовки групп пишутся заглавными в тексте, пустые состояния получают сплошную рамку `border-strong`, чекбоксы и переключатели стилизуются свои.

**Меню** в две группы, как на макете: основная (Configurations, Simulation, Template, Build, Package Manager, Analyzer) и Modules (Languages, Purchases, Leaderboards, Flags, Saves). Раздел «Пакет» в сводке ниже соответствует Package Manager.

### 9.2. Каркас и раздел «Конфигурации»

```text
┌ JTL SDK ───────────────────────────────────────────────────────────────────────┐
│ Активная: Yandex Games ▾    Пакет 1.0.0  ● есть 1.1.0          [RU] EN          │
├──────────────┬─────────────────────────────────────────────────────────────────┤
│ Конфигурации │ Конфигурации                                                    │
│ Шаблон       │                                                                 │
│ Языки        │ ┌──────────────────────────────┐ ┌────────────────────────────┐ │
│ Покупки      │ │ ● Yandex Games     активная  │ │ ○ YouTube Playables        │ │
│ Лидерборды   │ │   JTLSDK_YANDEX_GAMES        │ │   JTLSDK_YOUTUBE_PLAYABLES │ │
│ Флаги        │ │   Brotli · Medium · 9 языков │ │   Disabled · High · 1 язык │ │
│ Сборка       │ │   [Открыть]                  │ │   [Сделать активной]       │ │
│ Симуляция    │ └──────────────────────────────┘ └────────────────────────────┘ │
│ Сохранения   │ [+ Новая конфигурация ▾]  Yandex Games / YouTube Playables      │
│ Анализатор   │                                                                 │
│ Пакет        │ Общие настройки                                                 │
│              │  Таймаут инициализации, с   [15   ]                             │
│              │  Задержка автосохранения, с [2    ]                             │
│              │  Логирование                 [Ошибки и предупреждения ▾]        │
├──────────────┴─────────────────────────────────────────────────────────────────┤
│ Конфигурация Yandex Games применена 14:03. Проект перекомпилирован.            │
└────────────────────────────────────────────────────────────────────────────────┘
```

Открытая конфигурация:

```text
│ Конфигурации › Yandex Games                                   [Сделать активной]│
│                                                                                 │
│ Название       [Yandex Games        ]   Площадка  Yandex Games                  │
│ Define-символ  JTLSDK_YANDEX_GAMES     (только чтение)                          │
│                                                                                 │
│ Настройки проекта при активации                    Применять                    │
│  WebGL Template          JTLSDK                       [x]                       │
│  Compression Format      [Brotli ▾]                   [x]                       │
│  Managed Stripping       [Medium ▾]                   [x]                       │
│  Memory Size, MB         [512   ]                     [ ]                       │
│                                                                                 │
│ Модули                                                                          │
│  Реклама      [Yandex Games Ads ▾]        ▸ AdBlock [x]  Интервал [60] с        │
│  Сохранения   [Yandex Games Data ▾]       ▸ Лимит 200 КБ                        │
│  Покупки      [Yandex Games Payments ▾]                                         │
│  Лидерборды   [Yandex Games Leaderboards ▾]                                     │
│  Игрок        [Yandex Games Player ▾]                                           │
│  Язык         [Yandex Games Language ▾]                                         │
│  Флаги        [Yandex Games Flags ▾]                                            │
│  Время        [Yandex Games Time ▾]                                             │
│  Отзыв        [Yandex Games Review ▾]                                           │
│  Ярлык        [Yandex Games Game Label ▾]                                         │
│                                                                                 │
│ Пауза при потере фокуса  [x]                                                    │
│ Языки конфигурации       [x] English  [x] Russian  [x] Turkish  ...             │
│ Шаблон                   [Переопределить ▾]  Логотип, фон, пиксельное соотношение│
```

Выпадающий список модуля показывает все провайдеры, найденные через `TypeCache`, включая `Unsupported`. Под провайдером раскрываются его поля.

### 9.3. Раздел «Шаблон»

```text
│ Шаблон WebGL                                            [Превью]  [Сбросить]    │
│                                                                                 │
│ Логотип        [logo.png              ▾]   Размер  [160] px   Формат PNG/JPG/GIF│
│                                                                                 │
│ Экран загрузки                                                                  │
│  Фон           (●) Цвет  ( ) Градиент  ( ) Картинка                             │
│                [■ #1A1A1A]                                                      │
│  Прогресс-бар  Цвет заполнения [■ #FFFFFF]  Цвет фона [■ #333333]               │
│                Ширина [40] %   Высота [8] px   Скругление [0] px                │
│                Положение (●) Под логотипом  ( ) Внизу экрана                    │
│  Текст         [Загрузка...            ]  показывать [ ]                        │
│                                                                                 │
│ Страница                                                                        │
│  Фон           ( ) Цвет  (●) Градиент  ( ) Картинка                             │
│                Радиальный [x]  Угол [140]  [■ #2B1B6B] → [■ #0D0A1F]             │
│  Соотношение   ( ) Свободное  (●) Фиксированное [16/9]  выкл. на мобильных [x]  │
│                Поля: (●) фон страницы  ( ) цвет [■]                             │
│                                                                                 │
│ Рендер                                                                          │
│  Pixel ratio десктоп  (●) Авто  ( ) Фиксированный [1.0]  ( ) Авто, не выше [2.0]│
│  Pixel ratio мобильные ( ) Авто  ( ) Фиксированный [1.0]  (●) Авто, не выше [1.5]│
│                                                                                 │
│ Переопределено в конфигурации YouTube Playables: соотношение сторон, pixel ratio│
│                                                                                 │
│ ┌ Превью ────────────────────────────────────────────┐                          │
│ │            ░░░░░░░░░░░░░░░░░░░░░░░░░░░             │  [Десктоп] [Мобильный]   │
│ │            ░░░░░░  [ЛОГО]  ░░░░░░░░░░░             │  Прогресс [====----] 55% │
│ │            ░░░░░░ ████████░░░ ░░░░░░░░             │                          │
│ └────────────────────────────────────────────────────┘                          │
```

Общие настройки хранятся в `JTLSDKEditorSettings`, переопределения полей в конфигурации. Превью рисуется тем же CSS, что и шаблон, через `WebView` недоступно, поэтому превью - UI Toolkit-копия разметки с теми же значениями.

### 9.4. Раздел «Языки»

```text
│ Языки                                                                           │
│                                                                                 │
│ Языки игры                                                                      │
│  [x] English   [x] Russian   [x] Turkish   [ ] Spanish   [ ] Portuguese         │
│  [ ] German    [ ] French    [ ] Italian   [ ] Polish    [ ] Ukrainian          │
│  ...                                                                            │
│                                                                                 │
│ Язык по умолчанию   [English ▾]                                                 │
│                                                                                 │
│ Замены (язык площадки → язык игры)                              [+ Добавить]    │
│  Belarusian  → [Russian ▾]                                          [x]         │
│  Kazakh      → [Russian ▾]                                          [x]         │
│  Ukrainian   → [Russian ▾]                                          [x]         │
│  Uzbek       → [Russian ▾]                                          [x]         │
│                                                                                 │
│ Языки на конфигурациях                                                          │
│  Yandex Games       [x] English [x] Russian [x] Turkish                         │
│  YouTube Playables  [x] English [ ] Russian [ ] Turkish                         │
│                                                                                 │
│ Play Mode                                                                       │
│  Стартовый язык  [Russian ▾]   (меняется в углу вкладки Game)                   │
```

### 9.5. Раздел «Покупки»

```text
│ Покупки                                                          [+ Товар]      │
│                                                                                 │
│ ┌ remove_ads ───────────────────────────────────────────────────────────────┐   │
│ │ Id в игре    [remove_ads      ]   Тип  [Разовая ▾]                        │   │
│ │ Yandex Games [remove_ads_yg   ]                                           │   │
│ │ Тестовая цена [49] [YAN ▾]        (для редактора)                          │   │
│ │                                                                 [Удалить] │   │
│ └───────────────────────────────────────────────────────────────────────────┘   │
│ ┌ coins_1000 ───────────────────────────────────────────────────────────────┐   │
│ │ Id в игре    [coins_1000      ]   Тип  [Расходуемая ▾]                    │   │
│ │ Yandex Games [coins_1000      ]                                           │   │
│ │ Тестовая цена [15] [YAN ▾]                                                 │   │
│ └───────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
│ ⚠ На конфигурации YouTube Playables покупки не поддерживаются.                  │
│ [Сгенерировать константы]  → Assets/Scripts/Generated/JTLSDKIds.cs              │
```

**Генерация констант.** Кнопка есть в разделах «Покупки», «Лидерборды» и «Флаги», генерирует один файл со всеми идентификаторами. Путь и namespace задаются в настройках тулкита, по умолчанию `Assets/Scripts/Generated/JTLSDKIds.cs` и корневой namespace проекта. Имена констант делаются из id: `remove_ads` → `RemoveAds`. Файл перезаписывается целиком, править его вручную нельзя, об этом написано в его шапке.

```csharp
public static class ProductIds
{
    public const string RemoveAds = "remove_ads";
    public const string Coins1000 = "coins_1000";
}

public static class LeaderboardIds
{
    public const string Levels = "levels";
}

public static class FlagKeys
{
    public const string TutorialEnabled = "tutorial_enabled";
}
```

### 9.6. Разделы «Лидерборды» и «Флаги»

```text
│ Лидерборды                                                       [+ Лидерборд]  │
│  Id в игре [levels    ]  Yandex Games [levels_board]  YouTube [единственный]    │
│                                                                                 │
│ Флаги                                                            [+ Флаг]       │
│  Ключ [tutorial_enabled]  Тип [bool ▾]  По умолчанию [x]                        │
│  Ключ [ads_interval    ]  Тип [int  ▾]  По умолчанию [60]                       │
```

### 9.7. Раздел «Сборка»

```text
│ Сборка                                                                          │
│                                                                                 │
│ Конфигурация      Yandex Games (активная)                                       │
│ Development Build [ ]   Плашка номера сборки показывается только в Development  │
│                                                                                 │
│ Вывод             (●) Папка  ( ) ZIP                                            │
│ Путь              [~/Builds/MyGame                               ] [Выбрать]    │
│ Имя               [{product}_{configuration}_b{build}] → MyGame_YandexGames_b43     │
│ Номер сборки      42  → следующая 43   [Изменить]                               │
│ После сборки      [x] Открыть папку   [x] Строка в консоль                      │
│                                                                                 │
│ Проверки перед сборкой                                                          │
│  ✔ Define-символ соответствует конфигурации                                     │
│  ✔ WebGL Template = JTLSDK                                                      │
│  ✔ Compression Format = Brotli                                                  │
│  ✔ JTLSDKSettings.asset есть в Resources                                        │
│                                                                                 │
│ Проверки после сборки (YouTube Playables)                                       │
│  каждый файл < 30 МиБ · файлов ≤ 8000 · сжатие выключено · нет внешних скриптов │
│                                                                                 │
│                                                              [Собрать]          │
```

### 9.8. Раздел «Симуляция»

```text
│ Симуляция (Play Mode)                                                           │
│                                                                                 │
│ Площадка в Play Mode   Yandex Games (по активной конфигурации)                  │
│ Устройство             [Десктоп ▾]                                              │
│ Задержка инициализации [0.0] с    Имитировать ошибку инициализации [ ]          │
│                                                                                 │
│ Реклама                                                                         │
│  Спрашивать результат  (●) Каждый раз  ( ) Использовать выбранное:              │
│   Интерстишл [Показана ▾]   Rewarded [Награда ▾]                                │
│  Длительность показа   [1.0] с                                                  │
│                                                                                 │
│ Покупки                                                                         │
│  Спрашивать результат  (●) Каждый раз  ( ) Использовать выбранное: [Оплачена ▾] │
│                                                                                 │
│ Игрок                                                                           │
│  Авторизован [ ]   Имя [Тестовый игрок]   Id [editor-player]                    │
│                                                                                 │
│ Сейвы                                                                           │
│  Имитировать ошибку загрузки [ ]   Пустой сейв при старте [ ]                   │
│                                                                                 │
│ Оверлей во вкладке Game  [x]                                                    │
```

### 9.9. Раздел «Сохранения»

```text
│ Сохранения (редактор)                  Ревизия 42 · 1.8 КБ из 200 КБ · Loaded   │
│                                                                                 │
│ Поиск [money            ]                                       [+ Ключ]        │
│ ┌──────────────────┬────────┬──────────────────────────────────────┬─────────┐  │
│ │ Ключ             │ Тип    │ Значение                             │         │  │
│ ├──────────────────┼────────┼──────────────────────────────────────┼─────────┤  │
│ │ Money            │ int    │ [1200        ]                       │ [x]     │  │
│ │ Level            │ int    │ [27          ]                       │ [x]     │  │
│ │ MusicVolume      │ float  │ [0.5         ]                       │ [x]     │  │
│ │ Profile          │ object │ ▸ { "name": "Player", "skins": [1,4] }│ [x]     │  │
│ └──────────────────┴────────┴──────────────────────────────────────┴─────────┘  │
│                                                                                 │
│ [Сбросить всё]  [Экспорт JSON]  [Импорт JSON]  [Открыть JSON]                   │
```

Работает и в Play Mode: правка значения сразу видна игре, а `Set*` из игры сразу виден в таблице.

### 9.10. Раздел «Пакет»

```text
│ Пакет                                                                           │
│                                                                                 │
│ JTL SDK   установлено 1.0.0   доступно 1.1.0        [Обновить до 1.1.0]         │
│  Показывать пре-релизы [ ]                                                      │
│  ▸ 1.1.0  (14.10.2026)                                                          │
│    • Добавлен провайдер баннера для Yandex Games                                 │
│    • Исправлено ...                                                             │
│  ▸ 1.0.1  (02.10.2026)                                                          │
│                                                                                 │
│ Шаблон WebGL      1.0.0   установлен 1.0.0   [Обновить]                         │
│                                                                                 │
│ Модули                                                                          │
│  Analytics        1.0.0   не установлен      [Установить]                       │
│  CrazyGames       0.9.0   не установлен      [Установить]   пре-релиз           │
│                                                                                 │
│ Проверено 14:02   [Проверить сейчас]                                            │
```

### 9.11. Раздел «Анализатор»

```text
│ Анализатор API                                            [Сканировать]         │
│                                                                                 │
│ Папки  [Assets/Game/Scripts]  исключить [Assets/Plugins]                        │
│                                                                                 │
│ 7 мест                                                                          │
│  Assets/Game/.../GlobalTimeScaler.cs:23   Time.timeScale = 0.3f                 │
│     → JTLSDK.Time.Scale = 0.3f                            [Открыть] [Заменить]  │
│  Assets/Game/.../SettingHandler.cs:41     AudioListener.volume = v              │
│     → JTLSDK.Audio.Volume = v                             [Открыть] [Заменить]  │
│  Assets/Game/.../SavesService.cs:88       PlayerPrefs.GetInt("Level")           │
│     → JTLSDK.Data.GetInt("Level")                         [Открыть] [Заменить]  │
│                                                                                 │
│ [Заменить все простые]  (только однозначные замены, с подтверждением)           │
```

---

## 10. Симуляция в редакторе

### 10.1. Кнопка во вкладке Game

Всегда, и в Play Mode, и вне его, только в редакторе. В полосе инструментов вкладки Game, слева от «Play Focused», стоит кнопка `JTL · RU · Mobile ▾`. Картинку игры она не закрывает. По нажатию под ней открывается панель, клик мимо закрывает её:

```text
[ JTL · RU · Mobile ▾ ] [Play Focused ▾] [🔈] [Stats] [Gizmos ▾]
┌──────────────────────────────┐
│ Language   [Russian ▾]       │
│ Device     [Mobile  ▾]       │
│ Platform audio muted [ ]     │  только YouTube
└──────────────────────────────┘
```

Вне Play Mode список языков берётся из языков проекта, а выбор становится стартовым языком симуляции. В Play Mode смена языка вызывает `Language.Changed`. Переключатель звука меняет `Audio.IsPlatformMuted` и есть только на конфигурациях с `Capability.PlatformMute`. Если окно Game слишком узкое, кнопка встаёт под полосу инструментов в правый угол.

### 10.2. Оверлеи прототипов

Появляются поверх игры, игра при этом на паузе через `Ads`/`Purchase`. Кнопки крупные, под каждой написано, что получит код игры.

Rewarded:

```text
┌──────────────────────────────────────────────────────────┐
│  Rewarded · double_money · Yandex Games                  │
│  Игра на паузе. Выбери результат.                        │
│                                                          │
│  [ Досмотрел, выдать награду ]     AdResult.Rewarded     │
│  [ Закрыл раньше, без награды ]    AdResult.Closed       │
│  [ Реклама недоступна ]            AdResult.NotShown     │
│  [ Ошибка показа ]                 AdResult.Failed       │
│                                                          │
│  [ ] Запомнить до конца Play Mode                        │
└──────────────────────────────────────────────────────────┘
```

Интерстишл:

```text
│  [ Показана и закрыта ]            AdResult.Shown        │
│  [ Площадка не показала ]          AdResult.NotShown     │
│  [ Ошибка показа ]                 AdResult.Failed       │
```

Покупка:

```text
┌──────────────────────────────────────────────────────────┐
│  Покупка · coins_1000 · 15 YAN                           │
│                                                          │
│  [ Оплатить ]                      Granted + Purchased   │
│  [ Оплатить, игра упала до выдачи ] Granted при след. запуске│
│  [ Отменить ]                      PurchaseResult.Cancelled│
│  [ Ошибка оплаты ]                 PurchaseResult.Failed  │
└──────────────────────────────────────────────────────────┘
```

Оверлей «Нажми, чтобы продолжить» и плашка Development-сборки в редакторе рисуются тем же UI Toolkit, в билде это HTML на странице.

---

## 11. Шаблон WebGL

За основу берётся шаблон PluginYG2 (CC0), переписывается по стилю проекта и без инициализации площадки внутри `index.html`. Шаблон отвечает за экран загрузки, запуск Unity, подключение `jtlsdk-page.js` и вставку площадки.

**Доставка.** В первой версии шаблон лежит в пакете, в `Editor/Template~/JTLSDK`. Кнопка «Установить шаблон» в разделе «Шаблон» копирует его в `Assets/WebGLTemplates/JTLSDK` и выбирает в Player Settings. Без установленного шаблона раздел «Шаблон» показывает только кнопку установки, а сборку блокирует проверка. Кнопка «Удалить» стирает папку шаблона и возвращает в Player Settings стандартный шаблон Unity. Позже шаблон переедет в отдельный репозиторий `meepifiev/JTLSDK-WebGLTemplate` с релизами: раздел «Пакет» покажет установленную и доступную версию, кнопки «Установить» и «Обновить», а перед обновлением вручную правленых файлов тулкит спросит.

Файлы:

```text
Assets/WebGLTemplates/JTLSDK/
  index.html
  thumbnail.png
  TemplateData/
    style.css
    jtlsdk-page.js        экран загрузки, pixel ratio, вписывание canvas, firstFrameReady
    logo.png              копируется из настроек тулкита
    loader-background.png если у экрана загрузки фон-картинка
    page-background.png   если у страницы фон-картинка
```

Значения из тулкита попадают в шаблон через переменные Unity (`PlayerSettings.SetTemplateCustomValue`) перед сборкой. Правки `index.html` после сборки нет.

| Переменная | Источник |
|---|---|
| `JTLSDK_LOGO_DISPLAY`, `JTLSDK_LOGO_SIZE` | Шаблон › Логотип |
| `JTLSDK_LOADER_BACKGROUND` | Шаблон › Экран загрузки › Фон (готовый CSS) |
| `JTLSDK_PAGE_BACKGROUND` | Шаблон › Фон страницы |
| `JTLSDK_PROGRESS_FILL`, `_FILL_TO`, `_TRACK`, `_BORDER_WIDTH`, `_BORDER_COLOR`, `_PADDING`, `_WIDTH`, `_HEIGHT`, `_RADIUS`, `_POSITION` | прогресс-бар: заливка цветом или градиентом, рамка, внутренний отступ, цвета с прозрачностью |
| `JTLSDK_LOADING_TEXT` | текст под прогрессом |
| `JTLSDK_ASPECT`, `JTLSDK_ASPECT_MOBILE` | пропорции канваса и их отключение на мобильных |
| `JTLSDK_DPR_DESKTOP`, `JTLSDK_DPR_MOBILE` | `auto`, число или `max:2` |
| `JTLSDK_PLATFORM`, `JTLSDK_PLATFORM_HEAD` | площадка активной конфигурации и её скрипт в `<head>` |
| `JTLSDK_DEV_BADGE` | плашка Development-сборки: `DEV · b43 · Yandex Games · v1.3.0` |

Особенности YouTube в шаблоне: скрипт площадки первым в `<head>`, `firstFrameReady` сразу после показа экрана загрузки, поля под любое соотношение сторон, без блокировки ориентации, без `preventDefault` на Esc.

---

## 12. Сборка

Кнопка «Собрать» в разделе «Сборка» тулкита.

1. Проверки перед сборкой (раздел 9.7). Красная проверка блокирует сборку.
2. Подстановка переменных шаблона, копирование картинок.
3. Номер сборки увеличивается и записывается в `JTLSDKEditorSettings`.
4. `BuildPipeline.BuildPlayer` в папку `{path}/{name}`.
5. Проверки после сборки для конфигурации: размер файлов, количество, сжатие, внешние скрипты в `index.html`.
6. При выводе ZIP папка архивируется, папка удаляется.
7. Строка в консоль: `JTL SDK build 43 · Yandex Games · 12.4 MB · ~/Builds/MyGame/MyGame_YandexGames_b43.zip`.
8. Открыть папку в Finder, если отмечено.

Плашка `DEV · b43 · Yandex Games · v1.3.0` в углу страницы только при `Development Build`. Это HTML-элемент, в Unity-сцене её нет.

---

## 13. Пакет, модули и обновления

**Источник версий** - GitHub Releases репозитория `meepifiev/JTLSDK`, публичный API без токена. Проверка раз при старте редактора, кеш на сутки в `UserSettings`. Кнопка «Проверить сейчас».

**Обновление** - `Client.Add("https://github.com/meepifiev/JTLSDK.git?path=Packages/com.jtlstudio.sdk#v1.1.0")`. Откат на любой релиз тем же способом.

**Скачиваемые модули** описаны в `modules.json` в корне репозитория. Тулкит читает его с `main` через `raw.githubusercontent.com`.

```json
{
  "modules": [
    {
      "id": "example",
      "name": "Example",
      "package": "com.jtlstudio.sdk.example",
      "repository": "meepifiev/JTLSDK",
      "path": "Modules/com.jtlstudio.sdk.example",
      "requires": "0.1.0",
      "platforms": ["YandexGames"]
    }
  ]
}
```

Модуль - отдельный UPM-пакет. Он лежит в своём репозитории или в папке `Modules/` этого репозитория, тогда в записи указан `path`. Установка - `Client.Add` по git-ссылке с тегом последнего релиза, удаление - `Client.Remove`. Кнопка «Установить» неактивна, если установленный JTL SDK ниже `requires`.

Модуль ничего не регистрирует в рантайме. Он добавляет провайдер для слота ядра, и провайдер выбирают в конфигурации, как любой другой. Атрибут `[ProviderPlatforms(...)]` ограничивает площадки: тулкит не предлагает такой провайдер в чужой конфигурации, а валидатор сборки считает его ошибкой.

**Аналитика** будет отдельным скачиваемым модулем со своим API. В ядре её нет.

---

## 14. Анализатор API

Сканирует `.cs` в выбранных папках `Assets` (по умолчанию всё, кроме `Packages` и `Plugins`) и находит:

| Шаблон | Замена |
|---|---|
| `Time.timeScale` | `JTLSDK.Time.Scale` |
| `AudioListener.volume` | `JTLSDK.Audio.Volume` |
| `AudioListener.pause` | пауза через `JTLSDK.Pause` |
| `Cursor.visible`, `Cursor.lockState` | `JTLSDK.Device.CursorVisible`, `CursorLock` |
| `PlayerPrefs.*` | `JTLSDK.Data.*` |
| `Application.OpenURL` | предупреждение: на YouTube внешние ссылки запрещены |

Результат - список с файлом, строкой, найденным кодом и предлагаемой заменой. «Заменить» правит одну строку. «Заменить все простые» правит только однозначные случаи (присваивание и чтение) после подтверждения со списком файлов. Проект должен быть под VCS, тулкит это проверяет и предупреждает.

---

## 15. Разработка SDK

**Стиль кода.** В репозитории лежит `.claude/CODE_STYLE.md` студии. Исключение одно: фасад `JTLSDK` - статический класс, единственный в пакете. Комментариев в коде нет, имена без сокращений.

**Тесты.**

- EditMode: общие сервисы с фейковыми провайдерами. Единственный показ рекламы за раз, снятие паузы при исключении в колбэке, запрет записи в облако до успешной загрузки, перезагрузка сейва после авторизации, выдача до consume и повтор `Granted`, выбор языка, машина состояний gameplay, `WhenReady` в порядке регистрации.
- PlayMode: демо-сцена из `Samples~/Demo` проходит полный цикл на конфигурации Editor.
- Ручная матрица перед релизом: черновик на Яндексе, McPlay для YouTube, три версии Unity.

**CI (GitHub Actions).**

- `ci.yml`: на каждый push и PR собрать `Bridge~` (Node 20, `tsc`), сравнить результат с `jtlsdk.jspre` в репозитории, прогнать EditMode-тесты на 2021.3.45f2.
- `release.yml`: на тег `v*` проверить, что `package.json` совпадает с тегом, создать GitHub Release с текстом из `CHANGELOG.md`.
- Матрица версий Unity (2022.3, 6000.0) запускается вручную и раз в неделю.

**Версии.** SemVer. `package.json` и тег совпадают. `CHANGELOG.md` ведётся вручную.

**Демо-сцена** в `Samples~/Demo`: кнопки на каждый модуль, вывод результата на экран. Она же используется как ручной тест на площадках.

---

## 16. План работ

| Этап | Содержание | Результат |
|---|---|---|
| 1 | Фасад, модули, общие сервисы, Editor-провайдеры, `JTLSDKSettings`, конфигурации, define-символы | игра запускается в Play Mode на прототипах |
| 2 | Тулкит: конфигурации, языки, покупки, лидерборды, флаги, симуляция, сохранения, оверлей Game | всё настраивается без инспектора |
| 3 | Мост на TypeScript, шаблон, провайдеры Яндекса | черновик на Яндексе проходит модерацию |
| 4 | Провайдеры YouTube, проверки лимитов, `firstFrameReady` | McPlay проходит |
| 5 | Раздел «Сборка», плашка, zip, нумерация | сборка одной кнопкой |
| 6 | Обновления, `modules.json`, модуль аналитики, анализатор API | пакет обновляется из тулкита |
| 7 | CI, тесты, документация RU/EN, релиз 1.0.0 | тег `v1.0.0` |

---

## 17. Журнал решений

| Дата | Решение |
|---|---|
| 21.09.2026 | Модель распространения как у Prime: UPM-пакет в `Packages`, asmdef всегда, `autoReferenced`. |
| 21.09.2026 | Шаблон WebGL в отдельном репозитории, ставится тулкитом с GitHub. |
| 21.09.2026 | Поля пресета сборки разработчик выбирает сам; для YouTube сжатие Disabled обязательно. |
| 21.09.2026 | Сейв без локального зеркала: одно хранилище площадки, запись запрещена до успешной загрузки. |
| 21.09.2026 | Язык определяет площадка; `Set` действует до конца сессии, выбор игрока хранит игра. |
| 21.09.2026 | Язык тулкита по умолчанию английский. |
| 21.09.2026 | `CurrencyImageUrl` не нужен. |
| 21.09.2026 | Генерация констант входит в первую версию. |
| 21.09.2026 | Оверлей «Нажми, чтобы продолжить»: ручной и автоматический режимы, как у Prime. |
| 21.09.2026 | Сборки площадок компилируются всегда; define-символ гейтит только `[DllImport]` и `.jslib`. |
| 21.09.2026 | `IPaymentsProvider.Configure(products, platform)`: провайдер получает каталог до инициализации, чтобы знать тип товара и тестовые цены. |
| 21.09.2026 | Прототипы в Play Mode: `PrototypeFactory` оборачивает провайдеры активной конфигурации, сохраняя её возможности; выключается флагом `UsePrototypesInEditor` в настройках. |
| 21.09.2026 | Настройки симуляции хранятся в `UserSettings/JTLSDKSimulation.json`, а не в ассете. |
| 21.09.2026 | Статические методы разрешены в точках входа редактора (`[InitializeOnLoad]`, `[MenuItem]`), они только создают экземпляр. |
| 21.09.2026 | Сообщения коммитов: одна строка, без описания и без соавторов. |
| 21.09.2026 | Тулкит без пояснений: только значения и действия. Нет переключателя темы, кнопки помощи и версии пакета в сайдбаре. Поддержка и документация ведут на `t.me/jtlstudio`. |
| 21.09.2026 | Конфигурацию не переименовывают. Карточка конфигурации показывает логотип площадки, название, define-символ и одну кнопку. Строки WebGL Template в конфигурации нет: шаблон один на все площадки. |
| 21.09.2026 | В сайдбаре под MODULES перечислены все модули; у каждого своя страница с провайдерами по конфигурациям. |
| 21.09.2026 | Шаблон WebGL в первой версии лежит в пакете (`Editor/Template~/JTLSDK`) и ставится тулкитом в `Assets/WebGLTemplates/JTLSDK`. Перенос в отдельный репозиторий остаётся в плане. |
| 21.09.2026 | Настройки шаблона и сборки хранятся в `ProjectSettings/JTLSDKEditorSettings.asset` (`ScriptableSingleton`): они общие для проекта и не нужны в рантайме. |
| 21.09.2026 | Скрипт площадки в `<head>` подставляет сборка через переменную шаблона `JTLSDK_PLATFORM_HEAD`: `/sdk.js` для Яндекса, `game_api/v1` для YouTube. |
| 21.09.2026 | Unity не принимает значения незарегистрированных переменных шаблона. Сервис шаблона сам находит `{{{ JTLSDK_* }}}` в файлах шаблона и регистрирует их через `PlayerSettings.templateCustomKeys`, затем проверяет каждое значение. |
| 21.09.2026 | Модуль - это провайдер для слота ядра, а не рантайм-регистрация. Аналитика стала слотом ядра, Yandex Metrica - первым модулем. |
| 21.09.2026 | Модули можно держать в папке `Modules/` репозитория JTLSDK и ставить по git-ссылке с `path`. Отдельный репозиторий для модуля не обязателен. |
| 21.09.2026 | Атрибут `[ProviderPlatforms]` ограничивает провайдер площадками. Провайдеры Яндекса и YouTube помечены им. |
| 21.09.2026 | IJ-SmashAndHit на JTL SDK не переводится. Пункт убран из плана. |
| 21.09.2026 | Аналитика убрана из ядра вместе с модулем Yandex Metrica. Она появится позже отдельным модулем. Механизм `modules.json` остаётся, список модулей пуст. |
| 21.09.2026 | Шаблон по умолчанию фирменный: логотип JTL SDK (`Editor/Toolkit/Icons/Brand/jtlsdk-template-logo.png`), радиальный тёмно-синий градиент, синий прогресс-бар. Логотип выбирается режимом: JTL SDK, свой или без логотипа. Превью в тулките рисует настоящий градиент. |
| 21.09.2026 | Настройки шаблона доступны только после установки. Шаблон можно удалить из тулкита. Сборка больше не ставит шаблон сама, вместо этого проверка «WebGL-шаблон установлен». |
| 21.09.2026 | Шаблон повторяет рабочий шаблон yt-ветки IJ-SmashAndHit. Canvas вписывается по высоте, только если окно шире пропорции игры, иначе заполняет экран. Canvas прозрачный, pixel ratio по умолчанию 1, баннеры Unity уходят в консоль. Прогресс-бар умеет рамку, внутренний отступ и градиент. Кнопки полного экрана нет, страница и так на весь экран. |
| 21.09.2026 | Мост площадки выбирает C# по конфигурации: каждый `.jspre` регистрирует себя в `Module.JTLSDKBridges[name]`, а `JTLSDK_Select` делает нужный активным. Раньше в сборку попадали оба моста, и побеждал последний подключённый, то есть YouTube даже на Яндексе: ограничения по define-символу для `.jspre` на это не влияли. |
| 21.09.2026 | Панель симуляции во вкладке Game стала кнопкой в полосе инструментов с выпадающим меню: язык, устройство, звук площадки. Пауза площадки, строка состояния и настройка «оверлей на паузе» удалены. |
| 21.09.2026 | Модуль Gameplay стал «Игровыми событиями»: `JTLSDK.GameEvents.GameReady()`, `GameplayStarted()`, `GameplayRestarted()`, `GameplayStopped()`, `IsGameplayActive`. Ярлык назван как в PluginYG2: `JTLSDK.GameLabel.CanShow` и `ShowDialog`. Старые конфигурации переносятся через `FormerlySerializedAs` и `MovedFrom`. |
| 21.09.2026 | В меню тулкита две категории: «Возможности» (покупки, сохранения, языки, лидерборды, флаги) и «Модули» (реклама, игровые события, пауза, время, звук, игрок, площадка, отзыв, ярлык игры). |
| 21.09.2026 | В пресет конфигурации добавлен флажок Debug Symbols (внешний файл символов). |
| 21.09.2026 | Значения шаблона больше не переменные Unity `{{{ }}}`, а маркеры `%JTLSDK_...%`: их подставляет обработчик сборки в готовый `index.html`, в Player Settings они не показываются. Картинки шаблона готовятся перед сборкой, старые удаляются, JPG копируется как есть. Работает и при сборке из стандартного окна Build Settings. |
| 21.09.2026 | Мост площадки попадает в сборку только для площадки активной конфигурации: `PluginImporter.SetIncludeInBuildDelegate`. Ограничения по define-символам для `.jspre` убраны. Проверено сборками: у Яндекса только мост Яндекса, у YouTube только мост YouTube. |
| 21.09.2026 | На площадку только одна конфигурация. Занятые площадки в меню «Новая конфигурация» неактивны. |
| 21.09.2026 | Сверка мостов с документацией. Яндекс: платежи с `signed: false`, потому что при `true` данные покупок приходят только в зашифрованном `signature`. Лидерборды через `ysdk.leaderboards.*` вместо устаревшего `getLeaderboards()`, авторизация через `player.isAuthorized()` вместо устаревшего `getMode()`, `getPlayer()` без устаревшего `scopes`. Видимость баннера берётся из ответа SDK, закрытое окно входа не сбрасывает данные игрока, размеры страниц лидерборда ограничены 1–20 и 1–10. YouTube: у награды есть запасной id. |
| 21.09.2026 | Потеря фокуса ставит свой источник паузы `Focus`, а не `Platform`, и окно «продолжить» снимает только его. Пауза площадки снимается только сигналом площадки, как требует YouTube. На YouTube пауза по фокусу выключена по умолчанию, а включённую не пропустит проверка перед сборкой. |
| 21.09.2026 | В меню `JTL SDK` только пункт Toolkit. Настройки создаёт тулкит сам, демо ставится из вкладки Samples в Package Manager. Пункты «Create Settings» и Development удалены. |
| 21.09.2026 | Восстановление расходуемых покупок ждёт первого обработчика `Granted`, а покупка без обработчика не списывается. Раньше покупка, восстановленная до подписки игры, списывалась без выдачи. |
| 21.09.2026 | EditMode-тесты проходят на 2021.3.45f2, 2022.3.62f2 и 6000.3.8f1. Настройки площадки пишутся через `NamedBuildTarget`, фон превью - через `BackgroundPropertyHelper` на 2022.2+. На Unity 6 остаются предупреждения об устаревших `UxmlFactory`/`UxmlTraits`: переход на `[UxmlElement]` ломает 2021.3, поэтому он отложен до поднятия минимальной версии. |
| 22.09.2026 | Тулкит переведён на обновлённый дизайн: тёмно-синяя палитра, сайдбар 236 px с плитками иконок и залитым активным пунктом, верхняя панель 48 px, поля и кнопки 30 px со скруглением 9, карточки со скруглением 14 и отступом 18. Оверлей в окне Game тоже на новой палитре. |
| 22.09.2026 | Раздел Build переделан по новому дизайну и стоит в меню вторым, после конфигураций: баннер, последние сборки, проверки перед сборкой и после неё, панель активной цели. В панели выбор активной конфигурации, её настройки сборки и проекта, путь и имя сборки, а под «Advanced» Development-сборка, номер сборки и открытие папки. История сборок хранится у каждого пользователя в `UserSettings/JTLSDKBuildHistory.asset`, последние 20 записей. Выбор конфигурации из верхней панели убран, он теперь только в разделе Build. |
| 22.09.2026 | Правка настроек проекта у активной конфигурации сразу применяется к Player Settings, а не только при следующей активации. |
| 22.09.2026 | Во время сборки SDK больше не меняет файлы в `Assets`: логотип и фоны копируются сразу в папку готовой сборки вместе с подстановкой маркеров. Раньше они пересоздавались в `Assets/WebGLTemplates/JTLSDK` вместе с `.meta`, и Unity падала с «Backend has requested a buildprogram run 6 times». Установленный шаблон сверяется с пакетом при загрузке редактора и перед сборкой из тулкита и обновляется, если отличается; сборка из стандартного окна с устаревшим шаблоном останавливается с понятной ошибкой. Раньше шаблон, установленный до перехода на маркеры `%JTLSDK_...%`, ронял сборку ошибкой `JTLSDK_PLATFORM_HEAD is not defined`. |
| 22.09.2026 | Размер сборки считается по тому, что загружается на площадку: у ZIP это размер архива, у папки сумма файлов. Мегабайт = 1 000 000 байт, как в Finder. Проверка YouTube на размер одного файла по-прежнему в МиБ. |
| 22.09.2026 | Development-сборка в SDK — это только плашка с номером сборки в шаблоне. Unity собирает её как обычную сборку, без `BuildOptions.Development`. Раньше включался настоящий Development Build, а вместе с Name Files As Hashes Unity не могла его собрать: «Backend has requested a buildprogram run 6 times». |

Открытых вопросов нет.
