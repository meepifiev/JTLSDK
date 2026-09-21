using System;
using System.Collections.Generic;

namespace JTLStudio.SDK.Editor.Toolkit.Localization
{
    public class ToolkitLocalization
    {
        private const char ListSeparator = '|';
        private const string MissingPrefix = "[";
        private const string MissingSuffix = "]";

        private readonly Dictionary<string, LocalizedText> _entries = new Dictionary<string, LocalizedText>();

        public ToolkitLocalization()
        {
            RegisterShell();
            RegisterCommon();
            RegisterConfigurations();
            RegisterConfigurationDetails();
            RegisterTemplate();
            RegisterLanguages();
            RegisterPurchases();
            RegisterLeaderboardsAndFlags();
            RegisterBuild();
            RegisterSimulation();
            RegisterSaves();
            RegisterPackageManager();
            RegisterAnalyzer();
        }

        public ToolkitLanguage Language { get; set; } = ToolkitLanguage.English;

        public int Count => _entries.Count;

        public bool Contains(string key)
        {
            return key != null && _entries.ContainsKey(key);
        }

        public string Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_entries.TryGetValue(key, out LocalizedText entry) == false)
            {
                return MissingPrefix + key + MissingSuffix;
            }

            return Language == ToolkitLanguage.Russian ? entry.Russian : entry.English;
        }

        public IReadOnlyList<string> GetList(string key)
        {
            return Get(key).Split(ListSeparator);
        }

        private void Add(string key, string english, string russian)
        {
            _entries[key] = new LocalizedText(english, russian);
        }

        private void RegisterShell()
        {
            Add("brand.caption", "Build once. Play everywhere.", "Собери один раз. Играй везде.");
            Add("nav.configurations", "Configurations", "Конфигурации");
            Add("nav.simulation", "Simulation", "Симуляция");
            Add("nav.template", "Template", "Шаблон");
            Add("nav.build", "Build", "Сборка");
            Add("nav.packageManager", "Package Manager", "Менеджер пакетов");
            Add("nav.analyzer", "Analyzer", "Анализатор");
            Add("nav.modules", "MODULES", "МОДУЛИ");
            Add("nav.languages", "Languages", "Языки");
            Add("nav.purchases", "Purchases", "Покупки");
            Add("nav.leaderboards", "Leaderboards", "Лидерборды");
            Add("nav.flags", "Flags", "Флаги");
            Add("nav.saves", "Saves", "Сохранения");
            Add("nav.documentation", "Documentation", "Документация");
            Add("nav.support", "Support", "Поддержка");
            Add("topbar.activeConfiguration", "Active configuration", "Активная конфигурация");
            Add("topbar.package", "Package 1.0.0 · 1.1.0 available", "Пакет 1.0.0 · доступна 1.1.0");
        }

        private void RegisterCommon()
        {
            Add("badge.active", "Active", "Активная");
            Add("badge.inactive", "Inactive", "Неактивная");
            Add("badge.unsupported", "Unsupported", "Не поддерживается");
            Add("badge.preRelease", "Pre-release", "Пре-релиз");
            Add("badge.updateAvailable", "Update available", "Есть обновление");
            Add("badge.notInstalled", "Not installed", "Не установлен");
            Add("badge.loaded", "Loaded", "Загружено");
            Add("badge.empty", "Empty", "Пусто");
            Add("badge.passed4", "4 passed", "4 пройдено");
            Add("badge.failed1", "1 failed", "1 провалена");
            Add("unit.seconds", "s", "с");
            Add("unit.px", "px", "px");
            Add("unit.mb", "MB", "МБ");
            Add("unit.percent", "%", "%");
            Add("unit.deg", "deg", "град");
            Add("common.generateConstants", "Generate constants", "Сгенерировать константы");
            Add("module.ads", "Ads", "Реклама");
            Add("module.saves", "Saves", "Сохранения");
            Add("module.purchases", "Purchases", "Покупки");
            Add("module.leaderboards", "Leaderboards", "Лидерборды");
        }

        private void RegisterConfigurations()
        {
            Add("configurations.title", "Configurations", "Конфигурации");
            Add("configurations.description", "Each configuration targets one web portal. Activating one applies its project settings and its define symbol.", "Каждая конфигурация нацелена на одну площадку. Активация применяет её настройки проекта и define-символ.");
            Add("configurations.newConfiguration", "New configuration", "Новая конфигурация");
            Add("configurations.yandexDescription", "Publishing to Yandex Games: ads, leaderboards, purchases and cloud saves.", "Публикация в Яндекс Играх: реклама, лидерборды, покупки и облачные сохранения.");
            Add("configurations.youtubeDescription", "Publishing to YouTube Playables: no ads, no purchases, local saves only.", "Публикация в YouTube Playables: без рекламы и покупок, только локальные сохранения.");
            Add("configurations.defineSymbol", "Define symbol", "Define-символ");
            Add("configurations.compressionFormat", "Compression format", "Формат сжатия");
            Add("configurations.languages", "Languages", "Языки");
            Add("configurations.yandexLanguages", "3 of 26", "3 из 26");
            Add("configurations.youtubeLanguages", "1 of 26", "1 из 26");
            Add("configurations.openConfiguration", "Open configuration", "Открыть конфигурацию");
            Add("configurations.makeActive", "Make active", "Сделать активной");
            Add("configurations.generalSettings", "General settings", "Общие настройки");
            Add("configurations.generalDescription", "Shared by every configuration.", "Общие для всех конфигураций.");
            Add("configurations.initializationTimeout", "Initialization timeout", "Таймаут инициализации");
            Add("configurations.initializationTimeoutHelp", "How long the SDK waits for the portal to answer before it falls back to the offline provider.", "Сколько SDK ждёт ответа площадки, прежде чем перейти на офлайн-провайдер.");
            Add("configurations.autosaveDelay", "Autosave delay", "Задержка автосохранения");
            Add("configurations.autosaveDelayHelp", "Changes are written to the portal after this delay.", "Изменения записываются на площадку после этой задержки.");
            Add("configurations.loggingLevel", "Logging level", "Логирование");
            Add("configurations.loggingLevels", "Errors only|Warnings and errors|Everything", "Только ошибки|Предупреждения и ошибки|Всё");
            Add("configurations.status", "Yandex Games configuration applied. Project recompiled.", "Конфигурация Yandex Games применена. Проект перекомпилирован.");
        }

        private void RegisterConfigurationDetails()
        {
            Add("details.portal", "Portal: YouTube Playables", "Площадка: YouTube Playables");
            Add("details.projectSettings", "Project settings applied on activation", "Настройки проекта при активации");
            Add("details.projectSettingsCaption", "Apply column controls what activation writes", "Колонка «Применять» задаёт, что записывает активация");
            Add("details.columnSetting", "SETTING", "НАСТРОЙКА");
            Add("details.columnValue", "VALUE", "ЗНАЧЕНИЕ");
            Add("details.columnApply", "APPLY", "ПРИМЕНЯТЬ");
            Add("details.templateChoices", "JTL SDK|Default|Minimal", "JTL SDK|Default|Minimal");
            Add("details.compressionChoices", "Gzip|Brotli|Disabled", "Gzip|Brotli|Disabled");
            Add("details.compressionError", "Compression Format must be Disabled. Build is blocked.", "Compression Format должен быть Disabled. Сборка заблокирована.");
            Add("details.strippingChoices", "Minimal|Low|Medium|High", "Minimal|Low|Medium|High");
            Add("details.modules", "Modules", "Модули");
            Add("details.unsupported", "Unsupported", "Не поддерживается");
            Add("details.noAds", "No ads API on this portal", "На этой площадке нет API рекламы");
            Add("details.noPayments", "No payments on this portal", "На этой площадке нет платежей");
            Add("details.savesProvider", "YouTube Playables Saves", "YouTube Playables Saves");
            Add("details.leaderboardsProvider", "YouTube Playables Leaderboards", "YouTube Playables Leaderboards");
            Add("details.autosaveFocus", "Autosave on focus loss", "Автосохранение при потере фокуса");
            Add("details.flushPause", "Flush before platform pause", "Сброс перед паузой площадки");
            Add("details.localStorageKey", "Local storage key", "Ключ локального хранилища");
            Add("details.status", "Compression Format blocks the build for YouTube Playables.", "Compression Format блокирует сборку для YouTube Playables.");
        }

        private void RegisterTemplate()
        {
            Add("template.title", "Template", "Шаблон");
            Add("template.description", "The loading page that wraps the build: logo, progress bar, page background and canvas rules.", "Страница загрузки вокруг сборки: логотип, прогресс-бар, фон страницы и правила канваса.");
            Add("template.logo", "Logo", "Логотип");
            Add("template.logoFile", "Logo file", "Файл логотипа");
            Add("template.logoSize", "Logo size", "Размер логотипа");
            Add("template.loadingScreen", "Loading screen", "Экран загрузки");
            Add("template.background", "Background", "Фон");
            Add("template.backgroundTypes", "Color|Gradient|Image", "Цвет|Градиент|Картинка");
            Add("template.backgroundColor", "Background color", "Цвет фона");
            Add("template.progressFill", "Progress fill", "Заполнение прогресса");
            Add("template.progressTrack", "Progress track", "Дорожка прогресса");
            Add("template.progressSize", "Progress size", "Размер прогресса");
            Add("template.progressSizeNote", "Width, height, corner radius.", "Ширина, высота, скругление.");
            Add("template.progressPosition", "Progress position", "Положение прогресса");
            Add("template.progressPositions", "Below logo|Bottom of screen", "Под логотипом|Внизу экрана");
            Add("template.loadingText", "Loading text", "Текст загрузки");
            Add("template.pageBackground", "Page background", "Фон страницы");
            Add("template.type", "Type", "Тип");
            Add("template.radial", "Radial", "Радиальный");
            Add("template.angle", "Angle", "Угол");
            Add("template.colors", "Colors", "Цвета");
            Add("template.canvas", "Canvas", "Канвас");
            Add("template.aspectRatio", "Aspect ratio", "Соотношение сторон");
            Add("template.aspectRatios", "Free|Fixed 16 / 9", "Свободное|Фиксированное 16 / 9");
            Add("template.disableOnMobile", "Disable on mobile", "Выключить на мобильных");
            Add("template.letterboxFill", "Letterbox fill", "Заливка полей");
            Add("template.letterboxFills", "Page background|Custom color", "Фон страницы|Свой цвет");
            Add("template.pixelRatioDesktop", "Pixel ratio, desktop", "Pixel ratio, десктоп");
            Add("template.pixelRatioMobile", "Pixel ratio, mobile", "Pixel ratio, мобильные");
            Add("template.pixelRatios", "Auto|Fixed 1.0|Auto up to 2.0", "Авто|Фиксированный 1.0|Авто, не выше 2.0");
            Add("template.fullscreenButton", "Fullscreen button", "Кнопка полного экрана");
            Add("template.overriddenNote", "Overridden in YouTube Playables: aspect ratio, pixel ratio.", "Переопределено в YouTube Playables: соотношение сторон, pixel ratio.");
            Add("template.preview", "Preview", "Превью");
            Add("template.previewModes", "Desktop|Mobile", "Десктоп|Мобильный");
            Add("template.previewLogo", "LOGO 160px", "ЛОГО 160px");
            Add("template.progress", "Progress", "Прогресс");
            Add("template.status", "Template written to Assets/WebGLTemplates/JTLSDK.", "Шаблон записан в Assets/WebGLTemplates/JTLSDK.");
        }

        private void RegisterLanguages()
        {
            Add("languages.title", "Languages", "Языки");
            Add("languages.description", "The languages the game ships with, and how portal locales map onto them.", "Языки, с которыми выходит игра, и как на них ложатся локали площадок.");
            Add("languages.projectLanguages", "Languages in the project", "Языки в проекте");
            Add("languages.selectedCount", "3 of 26 selected", "Выбрано 3 из 26");
            Add("languages.default", "Default language", "Язык по умолчанию");
            Add("languages.defaultChoices", "English|Russian|Turkish", "Английский|Русский|Турецкий");
            Add("languages.replacements", "Replacements", "Замены");
            Add("languages.replacementsNote", "A portal language not in the project falls back to the one on the right.", "Язык площадки, которого нет в проекте, заменяется языком справа.");
            Add("languages.addReplacement", "Add replacement", "Добавить замену");
            Add("languages.perConfiguration", "Languages per configuration", "Языки по конфигурациям");
            Add("languages.playMode", "Play Mode", "Play Mode");
            Add("languages.startLanguage", "Start language", "Стартовый язык");
            Add("languages.startLanguageNote", "Can be changed in the Game view overlay.", "Меняется в оверлее вкладки Game.");
            Add("languages.startChoices", "English|Russian|Turkish", "Английский|Русский|Турецкий");
            Add("languages.status", "Language table regenerated. 3 languages, default English.", "Таблица языков пересобрана. 3 языка, по умолчанию английский.");
            Add("language.English", "English", "Английский");
            Add("language.Russian", "Russian", "Русский");
            Add("language.Turkish", "Turkish", "Турецкий");
            Add("language.Spanish", "Spanish", "Испанский");
            Add("language.Portuguese", "Portuguese", "Португальский");
            Add("language.German", "German", "Немецкий");
            Add("language.French", "French", "Французский");
            Add("language.Italian", "Italian", "Итальянский");
            Add("language.Polish", "Polish", "Польский");
            Add("language.Ukrainian", "Ukrainian", "Украинский");
            Add("language.Belarusian", "Belarusian", "Белорусский");
            Add("language.Kazakh", "Kazakh", "Казахский");
            Add("language.Uzbek", "Uzbek", "Узбекский");
            Add("language.Azerbaijani", "Azerbaijani", "Азербайджанский");
            Add("language.Armenian", "Armenian", "Армянский");
            Add("language.Georgian", "Georgian", "Грузинский");
            Add("language.Romanian", "Romanian", "Румынский");
            Add("language.Arabic", "Arabic", "Арабский");
            Add("language.Hebrew", "Hebrew", "Иврит");
            Add("language.Hindi", "Hindi", "Хинди");
            Add("language.Indonesian", "Indonesian", "Индонезийский");
            Add("language.Japanese", "Japanese", "Японский");
            Add("language.Korean", "Korean", "Корейский");
            Add("language.ChineseSimplified", "Chinese (Simplified)", "Китайский (упрощённый)");
            Add("language.Vietnamese", "Vietnamese", "Вьетнамский");
            Add("language.Thai", "Thai", "Тайский");
        }

        private void RegisterPurchases()
        {
            Add("purchases.title", "Purchases", "Покупки");
            Add("purchases.description", "Products are declared once here and mapped to each portal's own product ids.", "Товары объявляются здесь один раз и сопоставляются с id каждой площадки.");
            Add("purchases.addProduct", "Add product", "Добавить товар");
            Add("purchases.productId", "Product id", "Id товара");
            Add("purchases.type", "Type", "Тип");
            Add("purchases.types", "Non-consumable|Consumable", "Разовая|Расходуемая");
            Add("purchases.yandexId", "Yandex Games id", "Id в Yandex Games");
            Add("purchases.testPrice", "Test price", "Тестовая цена");
            Add("purchases.currencies", "YAN|USD", "YAN|USD");
            Add("purchases.youtubeWarning", "Purchases are not supported on YouTube Playables.", "На YouTube Playables покупки не поддерживаются.");
            Add("purchases.status", "2 products. Constants last generated at 13:48.", "2 товара. Константы сгенерированы в 13:48.");
        }

        private void RegisterLeaderboardsAndFlags()
        {
            Add("leaderboards.title", "Leaderboards", "Лидерборды");
            Add("leaderboards.description", "Ids declared once, mapped per portal, and generated into one constants file.", "Id объявляются один раз, сопоставляются по площадкам и попадают в один файл констант.");
            Add("leaderboards.cardTitle", "Leaderboards", "Лидерборды");
            Add("leaderboards.columnId", "Id", "Id");
            Add("leaderboards.columnYandex", "Yandex Games id", "Id в Yandex Games");
            Add("leaderboards.columnYoutube", "YouTube Playables", "YouTube Playables");
            Add("leaderboards.singleBoard", "single board", "единственная таблица");
            Add("leaderboards.add", "Add leaderboard", "Добавить лидерборд");
            Add("leaderboards.youtubeNote", "YouTube Playables exposes a single board, so every leaderboard id maps onto it.", "У YouTube Playables одна таблица, поэтому все id лидербордов ложатся на неё.");
            Add("leaderboards.status", "Constants generated. 2 leaderboards, 2 flags.", "Константы сгенерированы. 2 лидерборда, 2 флага.");
            Add("flags.title", "Flags", "Флаги");
            Add("flags.description", "Keys declared once with a typed default, overridden remotely at startup.", "Ключи объявляются один раз с типизированным значением по умолчанию и переопределяются удалённо при запуске.");
            Add("flags.cardTitle", "Flags", "Флаги");
            Add("flags.caption", "Remote values override these defaults", "Удалённые значения переопределяют эти");
            Add("flags.columnKey", "Key", "Ключ");
            Add("flags.columnType", "Type", "Тип");
            Add("flags.columnDefault", "Default value", "Значение по умолчанию");
            Add("flags.types", "bool|int|float|string", "bool|int|float|string");
            Add("flags.add", "Add flag", "Добавить флаг");
        }

        private void RegisterBuild()
        {
            Add("build.title", "Build", "Сборка");
            Add("build.description", "Builds the active configuration and runs the portal checks for it.", "Собирает активную конфигурацию и выполняет проверки площадки.");
            Add("build.showBlocked", "Show blocked state", "Показать заблокированное состояние");
            Add("build.showReady", "Show ready state", "Показать готовое состояние");
            Add("build.settings", "Build settings", "Настройки сборки");
            Add("build.configuration", "Configuration", "Конфигурация");
            Add("build.developmentBuild", "Development build", "Development build");
            Add("build.developmentNote", "Build number badge is shown only in development builds.", "Плашка номера сборки видна только в development-сборках.");
            Add("build.output", "Output", "Вывод");
            Add("build.outputTypes", "Folder|ZIP archive", "Папка|ZIP-архив");
            Add("build.path", "Path", "Путь");
            Add("build.browse", "Browse", "Выбрать");
            Add("build.namePattern", "Name pattern", "Шаблон имени");
            Add("build.nameResolves", "Resolves to SmashAndHit_YandexGames_b43", "Получится SmashAndHit_YandexGames_b43");
            Add("build.buildNumber", "Build number", "Номер сборки");
            Add("build.next", "next 43", "следующая 43");
            Add("build.edit", "Edit", "Изменить");
            Add("build.afterBuild", "After build", "После сборки");
            Add("build.openFolder", "Open output folder", "Открыть папку");
            Add("build.writeLog", "Write log line", "Строка в консоль");
            Add("build.preBuildChecks", "Pre-build checks", "Проверки перед сборкой");
            Add("build.checkTarget", "WebGL is the active build target", "Активная платформа сборки — WebGL");
            Add("build.checkTemplate", "Template JTL SDK is selected", "Выбран шаблон JTL SDK");
            Add("build.checkCompression", "Compression format matches the configuration", "Формат сжатия соответствует конфигурации");
            Add("build.checkLanguages", "3 languages configured, default English", "Настроено 3 языка, по умолчанию английский");
            Add("build.blockedHint", "Set Compression Format to Gzip in the configuration, then build again.", "Установите Compression Format = Gzip в конфигурации и соберите снова.");
            Add("build.postBuildChecks", "Post-build checks", "Проверки после сборки");
            Add("build.postBuildCaption", "YouTube Playables", "YouTube Playables");
            Add("build.checkFileSize", "Each file under 30 MiB", "Каждый файл меньше 30 МиБ");
            Add("build.checkFileCount", "At most 8000 files", "Не больше 8000 файлов");
            Add("build.checkCompressionDisabled", "Compression disabled", "Сжатие выключено");
            Add("build.checkNoExternal", "No external scripts", "Нет внешних скриптов");
            Add("build.postBuildNote", "Run after a YouTube Playables build.", "Выполняются после сборки YouTube Playables.");
            Add("build.outputPath", "Output: Builds/YandexGames/SmashAndHit_YandexGames_b43", "Вывод: Builds/YandexGames/SmashAndHit_YandexGames_b43");
            Add("build.cleanOutput", "Clean output", "Очистить вывод");
            Add("build.build", "Build", "Собрать");
            Add("build.status", "Ready to build. Last build b42 finished in 2 m 14 s.", "Готово к сборке. Последняя сборка b42 заняла 2 мин 14 с.");
            Add("build.statusBlocked", "Build blocked: compression format does not match the configuration.", "Сборка заблокирована: формат сжатия не соответствует конфигурации.");
        }

        private void RegisterSimulation()
        {
            Add("simulation.title", "Simulation", "Симуляция");
            Add("simulation.description", "What the SDK answers in Play Mode while the real portal is not there.", "Что SDK отвечает в Play Mode, пока настоящей площадки нет.");
            Add("simulation.overlay", "Overlay in Game view", "Оверлей во вкладке Game");
            Add("simulation.playMode", "Play Mode", "Play Mode");
            Add("simulation.platform", "Platform", "Площадка");
            Add("simulation.device", "Device", "Устройство");
            Add("simulation.devices", "Desktop|Mobile", "Десктоп|Мобильное");
            Add("simulation.initializationDelay", "Initialization delay", "Задержка инициализации");
            Add("simulation.simulateInitFailure", "Simulate initialization failure", "Имитировать ошибку инициализации");
            Add("simulation.ads", "Ads", "Реклама");
            Add("simulation.behaviour", "Behaviour", "Поведение");
            Add("simulation.behaviours", "Ask every time|Use selected result", "Спрашивать каждый раз|Использовать выбранное");
            Add("simulation.interstitial", "Interstitial", "Интерстишл");
            Add("simulation.interstitialResults", "Shown|Closed|Failed", "Показана|Закрыта|Ошибка");
            Add("simulation.rewarded", "Rewarded", "Rewarded");
            Add("simulation.rewardedResults", "Rewarded|Closed|Failed", "Награда|Закрыта|Ошибка");
            Add("simulation.adDuration", "Ad duration", "Длительность показа");
            Add("simulation.purchases", "Purchases", "Покупки");
            Add("simulation.result", "Result", "Результат");
            Add("simulation.purchaseResults", "Purchased|Cancelled|Failed", "Оплачена|Отменена|Ошибка");
            Add("simulation.player", "Player", "Игрок");
            Add("simulation.authorized", "Authorized", "Авторизован");
            Add("simulation.name", "Name", "Имя");
            Add("simulation.id", "Id", "Id");
            Add("simulation.saves", "Saves", "Сейвы");
            Add("simulation.simulateLoadFailure", "Simulate load failure", "Имитировать ошибку загрузки");
            Add("simulation.emptySave", "Empty save on start", "Пустой сейв при старте");
            Add("simulation.status", "Play Mode will use simulated Yandex Games answers.", "В Play Mode будут симулированные ответы Yandex Games.");
        }

        private void RegisterSaves()
        {
            Add("saves.title", "Saves", "Сохранения");
            Add("saves.description", "The editor copy of the player's save data. Edits apply on the next Play Mode start.", "Копия сохранений игрока в редакторе. Правки применяются при следующем запуске Play Mode.");
            Add("saves.showEmpty", "Show empty state", "Показать пустое состояние");
            Add("saves.showFilled", "Show filled state", "Показать заполненное состояние");
            Add("saves.revision", "Revision 42", "Ревизия 42");
            Add("saves.size", "1.8 KB of 200 KB", "1,8 КБ из 200 КБ");
            Add("saves.searchPlaceholder", "Search keys", "Поиск ключей");
            Add("saves.addKey", "Add key", "Добавить ключ");
            Add("saves.columnKey", "Key", "Ключ");
            Add("saves.columnType", "Type", "Тип");
            Add("saves.columnValue", "Value", "Значение");
            Add("saves.profileFields", "4 fields", "4 поля");
            Add("saves.resetAll", "Reset all", "Сбросить всё");
            Add("saves.exportJson", "Export JSON", "Экспорт JSON");
            Add("saves.importJson", "Import JSON", "Импорт JSON");
            Add("saves.openJson", "Open JSON", "Открыть JSON");
            Add("saves.noRevision", "No revision", "Нет ревизии");
            Add("saves.sizeEmpty", "0 KB of 200 KB", "0 КБ из 200 КБ");
            Add("saves.emptyTitle", "No save data yet", "Сохранений пока нет");
            Add("saves.emptyDescription", "Enter Play Mode once so the game writes its first revision, or add a key by hand.", "Запустите Play Mode, чтобы игра записала первую ревизию, или добавьте ключ вручную.");
            Add("saves.status", "Revision 42 loaded from the editor store.", "Ревизия 42 загружена из хранилища редактора.");
            Add("saves.statusEmpty", "No save data in the editor store.", "В хранилище редактора нет сохранений.");
        }

        private void RegisterPackageManager()
        {
            Add("package.title", "Package Manager", "Менеджер пакетов");
            Add("package.description", "Versions of the SDK, the WebGL template and the optional portal modules.", "Версии SDK, WebGL-шаблона и дополнительных модулей площадок.");
            Add("package.sdkVersions", "Installed 1.0.0 · available 1.1.0", "Установлено 1.0.0 · доступно 1.1.0");
            Add("package.releaseNotes", "Release notes", "Что нового");
            Add("package.updateTo", "Update to 1.1.0", "Обновить до 1.1.0");
            Add("package.showPreReleases", "Show pre-releases", "Показывать пре-релизы");
            Add("package.date110", "18 Sep 2026", "18 сен 2026");
            Add("package.note110a", "YouTube Playables leaderboards", "Лидерборды YouTube Playables");
            Add("package.note110b", "Analyzer replaces PlayerPrefs calls", "Анализатор заменяет вызовы PlayerPrefs");
            Add("package.note110c", "Build number is stored per configuration", "Номер сборки хранится для каждой конфигурации");
            Add("package.date101", "2 Sep 2026", "2 сен 2026");
            Add("package.note101a", "Sticky banner no longer survives a scene load", "Sticky-баннер больше не переживает загрузку сцены");
            Add("package.note101b", "Fixed save flush on platform pause", "Исправлен сброс сохранений при паузе площадки");
            Add("package.templateVersions", "Installed 1.0.0 · up to date", "Установлен 1.0.0 · актуален");
            Add("package.update", "Update", "Обновить");
            Add("package.modules", "Modules", "Модули");
            Add("package.install", "Install", "Установить");
            Add("package.checkedAt", "Checked at 14:02", "Проверено в 14:02");
            Add("package.checkNow", "Check now", "Проверить сейчас");
            Add("package.status", "Update 1.1.0 available for JTL SDK.", "Для JTL SDK доступно обновление 1.1.0.");
        }

        private void RegisterAnalyzer()
        {
            Add("analyzer.title", "Analyzer", "Анализатор");
            Add("analyzer.description", "Finds engine calls the portals break and offers the SDK call that replaces them.", "Находит вызовы движка, которые ломают площадки, и предлагает замену из SDK.");
            Add("analyzer.folder", "Folder to scan", "Папка для сканирования");
            Add("analyzer.exclude", "Exclude", "Исключить");
            Add("analyzer.scan", "Scan", "Сканировать");
            Add("analyzer.found", "7 places found", "Найдено 7 мест");
            Add("analyzer.scanned", "Scanned 214 files in 1.2 s", "Проверено 214 файлов за 1,2 с");
            Add("analyzer.open", "Open", "Открыть");
            Add("analyzer.replace", "Replace", "Заменить");
            Add("analyzer.replaceAll", "Replace all simple", "Заменить все простые");
            Add("analyzer.replaceAllNote", "Only unambiguous replacements, with confirmation.", "Только однозначные замены, с подтверждением.");
            Add("analyzer.status", "7 places found in Assets/Game/Scripts.", "Найдено 7 мест в Assets/Game/Scripts.");
        }
    }
}
