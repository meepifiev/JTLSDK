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
            RegisterLiveData();
            RegisterCommon();
            RegisterConfigurations();
            RegisterConfigurationDetails();
            RegisterTemplate();
            RegisterLanguages();
            RegisterPurchases();
            RegisterLeaderboardsAndFlags();
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
            Add("nav.configurations", "Configurations", "Конфигурации");
            Add("nav.simulation", "Simulation", "Симуляция");
            Add("nav.template", "Template", "Шаблон");
            Add("nav.build", "Build", "Сборка");
            Add("nav.packageManager", "Package Manager", "Менеджер пакетов");
            Add("nav.analyzer", "Code Analyzer", "Анализатор кода");
            Add("nav.modules", "MODULES", "МОДУЛИ");
            Add("nav.languages", "Languages", "Языки");
            Add("nav.purchases", "Purchases", "Покупки");
            Add("nav.leaderboards", "Leaderboards", "Лидерборды");
            Add("nav.flags", "Flags", "Флаги");
            Add("nav.saves", "Saves", "Сохранения");
            Add("nav.documentation", "Documentation", "Документация");
            Add("nav.support", "Support", "Поддержка");
            Add("nav.ads", "Ads", "Реклама");
            Add("nav.player", "Player", "Игрок");
            Add("nav.time", "Time", "Время");
            Add("nav.gameEvents", "Game events", "Игровые события");
            Add("nav.review", "Review", "Отзыв");
            Add("nav.gameLabel", "Game label", "Ярлык игры");
            Add("nav.configurable", "Configurable", "Настраиваемые");
            Add("nav.outOfTheBox", "Out of the box", "Из коробки");
            Add("nav.pause", "Pause", "Пауза");
            Add("nav.audio", "Audio", "Звук");
            Add("nav.platform", "Platform", "Площадка");
            Add("module.providers", "Providers", "Провайдеры");
            Add("package.notInstalled", "Not installed", "Не установлен");
            Add("package.requiresFormat", "requires JTL SDK {0}", "нужен JTL SDK {0}");
            Add("package.modulesFailed", "Module list unavailable: {0}", "Список модулей недоступен: {0}");
            Add("package.remove", "Remove", "Удалить");
            Add("package.removeMessage", "Remove {0} from the project?", "Удалить {0} из проекта?");
            Add("package.installing", "Installing {0}…", "Установка {0}…");
            Add("package.removing", "Removing {0}…", "Удаление {0}…");
            Add("build.development", "Development build", "Development-сборка");
            Add("build.browse", "Browse", "Выбрать");
            Add("build.number", "Build number", "Build number");
            Add("build.openFolder", "Open folder after build", "Открывать папку после сборки");
            Add("build.heroTitle", "Build Once.", "Собери один раз.");
            Add("build.heroAccent", "Publish Across Platforms.", "Публикуй везде.");
            Add("build.heroDescription", "Export your Unity game to Yandex Games, YouTube Playables and more — with one streamlined workflow.", "Выгружай Unity-игру в Яндекс Игры, YouTube Playables и не только — в одном простом процессе.");
            Add("build.start", "Get Started", "Начать");
            Add("build.recentBuilds", "Recent Builds", "Последние сборки");
            Add("build.noBuilds", "No builds yet.", "Сборок пока нет.");
            Add("build.clearHistory", "Clear", "Очистить");
            Add("build.clearHistoryTitle", "Clear recent builds?", "Очистить последние сборки?");
            Add("build.clearHistoryHint", "Only the list is cleared. Build files stay on disk.", "Очищается только список. Файлы сборок остаются на диске.");
            Add("build.columnName", "NAME", "ИМЯ");
            Add("build.columnPlatform", "TARGET PLATFORM", "ПЛОЩАДКА");
            Add("build.columnSize", "SIZE", "РАЗМЕР");
            Add("build.columnStatus", "STATUS", "СТАТУС");
            Add("build.columnDate", "DATE", "ДАТА");
            Add("build.size", "{0} MB", "{0} МБ");
            Add("build.statusSuccess", "Success", "Успешно");
            Add("build.statusFailed", "Failed", "Ошибка");
            Add("build.today", "Today, {0}", "Сегодня, {0}");
            Add("build.yesterday", "Yesterday, {0}", "Вчера, {0}");
            Add("build.reveal", "Show in folder", "Показать в папке");
            Add("build.activeTarget", "Active Target", "Active Target");
            Add("build.noTarget", "Create a configuration for a portal to build.", "Создай конфигурацию площадки, чтобы собрать игру.");
            Add("build.createConfiguration", "Create configuration", "Создать конфигурацию");
            Add("build.tabs", "Build Settings|Player Settings", "Build Settings|Player Settings");
            Add("build.platformConfiguration", "Platform Configuration", "Platform Configuration");
            Add("build.buildTarget", "Build Target", "Build Target");
            Add("build.compression", "Compression Format", "Compression Format");
            Add("build.outputSettings", "Output Settings", "Output Settings");
            Add("build.buildFolder", "Build Folder", "Build Folder");
            Add("build.openOutput", "Open Folder", "Открыть папку");
            Add("build.reset", "Reset", "Сбросить");
            Add("build.buildFileName", "Build File Name", "Build File Name");
            Add("build.outputs", "folder|zip", "folder|zip");
            Add("build.productSettings", "Product", "Product");
            Add("build.productName", "Product Name", "Product Name");
            Add("build.companyName", "Company Name", "Company Name");
            Add("build.version", "Version", "Version");
            Add("build.runtimeSettings", "Runtime", "Runtime");
            Add("build.stripping", "Managed Stripping", "Managed Stripping");
            Add("build.advanced", "Advanced", "Advanced");
            Add("build.buildProject", "Build Project", "Собрать проект");
            Add("build.preChecks", "Pre-build checks", "Проверки перед сборкой");
            Add("build.postChecks", "Post-build checks", "Проверки после сборки");
            Add("build.check.configuration", "Active configuration is set", "Активная конфигурация выбрана");
            Add("build.check.webgl", "WebGL module is installed", "Модуль WebGL установлен");
            Add("build.check.scenes", "Build Settings have an enabled scene", "В Build Settings есть включённая сцена");
            Add("build.check.settings", "JTLSDKSettings exists in Resources", "JTLSDKSettings лежит в Resources");
            Add("build.check.define", "Define {0} is applied", "Define {0} применён");
            Add("build.check.issue", "{0}", "{0}");
            Add("build.check.compression", "Compression is disabled", "Сжатие выключено");
            Add("build.check.fileSize", "Each file is under 30 MiB · largest {0} MiB", "Каждый файл меньше 30 МиБ · самый большой {0} МиБ");
            Add("build.check.fileCount", "At most 8000 files · {0}", "Не больше 8000 файлов · {0}");
            Add("build.check.scripts", "No external scripts in index.html", "В index.html нет внешних скриптов");
            Add("build.check.variables", "Template variables are substituted", "Переменные шаблона подставлены");
            Add("build.done", "Build {0} · {1} MB · {2}", "Сборка {0} · {1} МБ · {2}");
            Add("build.failed", "Build failed: {0}", "Сборка не удалась: {0}");
            Add("template.title", "Template", "Шаблон");
            Add("template.installed", "Installed", "Установлен");
            Add("template.install", "Install template", "Установить шаблон");
            Add("template.reinstall", "Reinstall", "Переустановить");
            Add("template.installedTo", "Template installed to {0}.", "Шаблон установлен в {0}.");
            Add("template.logo", "Logo", "Логотип");
            Add("template.logoSource", "Source", "Источник");
            Add("template.fillStyle", "Fill style", "Заливка");
            Add("template.fillStyles", "Solid|Gradient", "Цвет|Градиент");
            Add("template.borderWidth", "Border", "Рамка");
            Add("template.borderColor", "Border color", "Цвет рамки");
            Add("template.padding", "Inner padding", "Внутренний отступ");
            Add("template.notInstalled", "Template is not installed", "Шаблон не установлен");
            Add("template.remove", "Remove", "Удалить");
            Add("template.removeTitle", "Remove template", "Удалить шаблон");
            Add("template.removeMessage", "Delete Assets/WebGLTemplates/JTLSDK? Player Settings switch to the Unity default template.", "Удалить Assets/WebGLTemplates/JTLSDK? В Player Settings включится стандартный шаблон Unity.");
            Add("template.removed", "Template removed.", "Шаблон удалён.");
            Add("build.check.template", "WebGL template is installed", "WebGL-шаблон установлен");
            Add("template.logoModes", "JTL SDK|Custom|None", "JTL SDK|Свой|Без логотипа");
            Add("template.gradientShape", "Shape", "Форма");
            Add("template.gradientShapes", "Linear|Radial", "Линейный|Радиальный");
            Add("template.progressBar", "Progress bar", "Прогресс-бар");
            Add("template.fill", "Fill", "Заполнение");
            Add("template.track", "Track", "Фон полосы");
            Add("template.progressWidth", "Width", "Ширина");
            Add("template.progressHeight", "Height", "Высота");
            Add("template.progressRadius", "Corner radius", "Скругление");
            Add("template.logoFile", "Image", "Картинка");
            Add("template.logoSize", "Width", "Ширина");
            Add("template.loadingScreen", "Loading screen", "Экран загрузки");
            Add("template.background", "Background", "Фон");
            Add("template.backgroundKinds", "Color|Gradient|Image", "Цвет|Градиент|Картинка");
            Add("template.backgroundColor", "Color", "Цвет");
            Add("template.gradientFrom", "From", "От");
            Add("template.gradientTo", "To", "До");
            Add("template.radial", "Radial", "Радиальный");
            Add("template.angle", "Angle", "Угол");
            Add("template.image", "Image", "Картинка");
            Add("template.progressFill", "Progress fill", "Заполнение");
            Add("template.progressTrack", "Progress track", "Фон полосы");
            Add("template.progressSize", "Width · height · radius", "Ширина · высота · скругление");
            Add("template.progressPosition", "Progress position", "Позиция полосы");
            Add("template.positions", "Below logo|Bottom", "Под логотипом|Внизу");
            Add("template.loadingText", "Loading text", "Текст загрузки");
            Add("template.pageBackground", "Page background", "Фон страницы");
            Add("template.canvas", "Canvas", "Canvas");
            Add("template.fixedAspect", "Fixed aspect ratio", "Фиксированные пропорции");
            Add("template.aspectRatio", "Aspect ratio", "Пропорции");
            Add("template.freeOnMobile", "Free on mobile", "Свободно на мобильных");
            Add("template.pixelRatioDesktop", "Pixel ratio · desktop", "Pixel ratio · десктоп");
            Add("template.pixelRatioMobile", "Pixel ratio · mobile", "Pixel ratio · мобильные");
            Add("template.pixelRatioModes", "Auto|Fixed|Auto, limited", "Авто|Фиксированный|Авто с лимитом");
            Add("template.preview", "Preview", "Превью");
            Add("template.previewDevices", "Desktop|Mobile", "Десктоп|Мобильный");
            Add("template.progress", "Progress", "Прогресс");
        }

        private void RegisterLiveData()
        {
            Add("platform.yandexDescription", "Yandex Games: ads, purchases, cloud saves, leaderboards, flags and server time.", "Яндекс Игры: реклама, покупки, облачные сохранения, лидерборды, флаги и серверное время.");
            Add("platform.youtubeDescription", "YouTube Playables: ads, cloud saves and score. No purchases, no authorization.", "YouTube Playables: реклама, облачные сохранения и счёт. Без покупок и авторизации.");
            Add("platform.editorDescription", "Editor only: prototypes answer every call.", "Только редактор: на все вызовы отвечают прототипы.");
            Add("module.platform", "Platform", "Площадка");
            Add("module.language", "Language", "Язык");
            Add("module.player", "Player", "Игрок");
            Add("module.flags", "Flags", "Флаги");
            Add("module.time", "Time", "Время");
            Add("module.gameEvents", "Game events", "Игровые события");
            Add("module.review", "Review", "Отзыв");
            Add("module.gameLabel", "Game label", "Ярлык игры");
            Add("topbar.noConfiguration", "No configuration", "Нет конфигурации");
            Add("topbar.packageVersion", "Package", "Пакет");
            Add("topbar.manageConfigurations", "Manage configurations", "Управлять конфигурациями");
            Add("configurations.ready", "Configurations loaded from the project.", "Конфигурации загружены из проекта.");
            Add("configurations.emptyTitle", "No configurations yet", "Конфигураций пока нет");
            Add("configurations.emptyDescription", "Create one per portal. The first one becomes active.", "Создай по одной на площадку. Первая станет активной.");
            Add("configurations.createYandex", "Create Yandex Games", "Создать Yandex Games");
            Add("configurations.createYouTube", "Create YouTube Playables", "Создать YouTube Playables");
            Add("configurations.notApplied", "Not applied", "Не применяется");
            Add("configurations.languagesCount", "{0} of {1}", "{0} из {1}");
            Add("configurations.created", "{0} configuration created.", "Конфигурация {0} создана.");
            Add("configurations.activated", "{0} is active.", "{0} активна.");
            Add("configurations.copied", "Copied {0}.", "Скопировано: {0}.");
            Add("details.ready", "Changes are saved to the configuration asset immediately.", "Изменения сразу сохраняются в ассет конфигурации.");
            Add("details.noConfigurationTitle", "No configuration selected", "Конфигурация не выбрана");
            Add("details.noConfigurationDescription", "Open one from the list or create a new one.", "Открой конфигурацию из списка или создай новую.");
            Add("details.backToList", "Back to configurations", "К списку конфигураций");
            Add("details.general", "Configuration", "Конфигурация");
            Add("details.name", "Name", "Название");
            Add("details.unsupportedNote", "Not available on this portal", "Недоступно на этой площадке");
            Add("details.hasSettings", "Has settings", "Есть настройки");
            Add("details.noSettings", "No settings", "Без настроек");
            Add("details.pause", "Pause", "Пауза");
            Add("details.pauseOnFocusLoss", "Pause on focus loss", "Пауза при потере фокуса");
            Add("details.languages", "Languages on this configuration", "Языки этой конфигурации");
            Add("details.languagesDescription", "Only languages enabled in the project are listed.", "В списке только языки, включённые в проекте.");
            Add("details.noProjectLanguages", "Enable languages in the Languages section first.", "Сначала включи языки в разделе «Языки».");
            Add("details.delete", "Delete", "Удалить");
            Add("details.cancel", "Cancel", "Отмена");
            Add("details.deleteTitle", "Delete configuration?", "Удалить конфигурацию?");
            Add("details.deleteMessage", "{0} will be deleted from the project. This cannot be undone.", "Конфигурация {0} будет удалена из проекта. Это нельзя отменить.");
            Add("details.deleted", "{0} deleted.", "{0} удалена.");
            Add("common.constantsGenerated", "Constants written to {0}.", "Константы записаны в {0}.");
            Add("languages.ready", "Languages are stored in JTLSDKSettings.", "Языки хранятся в JTLSDKSettings.");
            Add("languages.selectedCountFormat", "{0} of {1} selected", "Выбрано {0} из {1}");
            Add("languages.noConfigurations", "No configurations yet.", "Конфигураций пока нет.");
            Add("languages.lastLanguage", "At least one language must stay enabled.", "Хотя бы один язык должен остаться включённым.");
            Add("languages.startLanguageSaved", "Play Mode starts in {0}.", "Play Mode стартует на языке: {0}.");
            Add("purchases.ready", "Products are stored in JTLSDKSettings.", "Товары хранятся в JTLSDKSettings.");
            Add("purchases.emptyTitle", "No products yet", "Товаров пока нет");
            Add("purchases.emptyDescription", "Declare a product id here, then map it to each portal.", "Объяви id товара здесь и сопоставь его с id на площадках.");
            Add("purchases.unsupportedWarning", "Purchases are not supported on the active configuration {0}.", "Покупки не поддерживаются в активной конфигурации {0}.");
            Add("purchases.yandexIdNote", "Leave empty to reuse the product id.", "Оставь пустым, чтобы использовать id товара.");
            Add("purchases.added", "{0} added.", "{0} добавлен.");
            Add("leaderboards.ready", "Leaderboards are stored in JTLSDKSettings.", "Лидерборды хранятся в JTLSDKSettings.");
            Add("leaderboards.empty", "No leaderboards yet.", "Лидербордов пока нет.");
            Add("flags.ready", "Flag defaults are stored in JTLSDKSettings.", "Значения флагов по умолчанию хранятся в JTLSDKSettings.");
            Add("flags.empty", "No flags yet.", "Флагов пока нет.");
            Add("simulation.ready", "Simulation settings are stored per user in UserSettings.", "Настройки симуляции хранятся у каждого пользователя в UserSettings.");
            Add("simulation.saved", "Saved.", "Сохранено.");
            Add("simulation.loadFailure", "Simulate load failure", "Имитировать ошибку загрузки");
            Add("saves.ready", "Editor save data lives in PlayerPrefs under JTLSDK.Data.", "Сейв редактора хранится в PlayerPrefs под ключом JTLSDK.Data.");
            Add("saves.sizeWithLimit", "{0} KB of {1} KB", "{0} КБ из {1} КБ");
            Add("saves.corrupted", "Save data is not valid JSON.", "Сейв не является корректным JSON.");
            Add("saves.invalidValue", "{0}: the value does not match its type.", "{0}: значение не подходит под тип.");
            Add("saves.valueSaved", "{0} saved.", "{0} сохранён.");
            Add("saves.keyDeleted", "{0} deleted.", "{0} удалён.");
            Add("saves.resetTitle", "Reset all save data?", "Сбросить весь сейв?");
            Add("saves.resetMessage", "All editor save keys will be deleted.", "Все ключи сейва редактора будут удалены.");
            Add("saves.resetDone", "Editor save data cleared.", "Сейв редактора очищен.");
            Add("saves.exported", "Saved to {0}.", "Сохранено в {0}.");
            Add("saves.imported", "Imported from {0}.", "Импортировано из {0}.");
            Add("saves.copyJson", "Copy JSON", "Скопировать JSON");
            Add("saves.copied", "Save JSON copied.", "JSON сейва скопирован.");
            Add("analyzer.ready", "Scan a folder to find engine calls that the SDK replaces.", "Просканируй папку, чтобы найти вызовы движка, которые заменяет SDK.");
            Add("analyzer.foundCount", "{0} places found", "Найдено мест: {0}");
            Add("analyzer.scannedFormat", "Scanned {0} files in {1} s", "Проверено файлов: {0} за {1} с");
            Add("analyzer.nothingFound", "Nothing to replace.", "Заменять нечего.");
            Add("analyzer.manual", "Review manually", "Проверить вручную");
            Add("analyzer.scanDone", "{0} places found in {1}.", "Найдено мест: {0} в {1}.");
            Add("analyzer.replaced", "{0}:{1} replaced.", "{0}:{1} заменено.");
            Add("analyzer.replaceFailed", "{0}:{1} changed since the scan. Scan again.", "{0}:{1} изменился после сканирования. Просканируй заново.");
            Add("analyzer.replaceAllConfirm", "Replace {0} places in {1} files?", "Заменить {0} мест в {1} файлах?");
            Add("analyzer.replacedCount", "{0} places replaced.", "Заменено мест: {0}.");
            Add("package.ready", "Versions come from GitHub Releases.", "Версии берутся из GitHub Releases.");
            Add("package.checking", "checking GitHub…", "проверка GitHub…");
            Add("package.checkedAtFormat", "Checked at {0}", "Проверено в {0}");
            Add("package.installedFormat", "Installed {0}", "Установлено {0}");
            Add("package.availableFormat", "available {0}", "доступно {0}");
            Add("package.upToDate", "up to date", "актуально");
            Add("package.noReleases", "no releases published yet", "релизов пока нет");
            Add("package.repositoryMissing", "repository not found", "репозиторий не найден");
            Add("package.checkFailed", "check failed", "проверка не удалась");
            Add("package.updateToFormat", "Update to {0}", "Обновить до {0}");
            Add("package.updateTitle", "Update JTL SDK?", "Обновить JTL SDK?");
            Add("package.updateMessage", "Switch JTL SDK to {0}?", "Переключить JTL SDK на {0}?");
            Add("package.updating", "Updating to {0}…", "Обновление до {0}…");
            Add("package.webglTemplate", "WebGL Template", "WebGL-шаблон");
            Add("package.templateInstalled", "Installed in {0}", "Установлен в {0}");
            Add("package.templateMissing", "Not installed", "Не установлен");
            Add("package.noModules", "No modules published yet.", "Модули пока не опубликованы.");
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
            Add("configurations.defineSymbol", "Define symbol", "Define symbol");
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
            Add("details.portal", "Portal: {0}", "Площадка: {0}");
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
            Add("template.description", "The loading page that wraps the build: logo, progress bar, page background and canvas rules.", "Страница загрузки вокруг сборки: логотип, прогресс-бар, фон страницы и правила канваса.");
            Add("template.backgroundTypes", "Color|Gradient|Image", "Цвет|Градиент|Картинка");
            Add("template.progressSizeNote", "Width, height, corner radius.", "Ширина, высота, скругление.");
            Add("template.progressPositions", "Below logo|Bottom of screen", "Под логотипом|Внизу экрана");
            Add("template.type", "Type", "Тип");
            Add("template.colors", "Colors", "Цвета");
            Add("template.aspectRatios", "Free|Fixed 16 / 9", "Свободное|Фиксированное 16 / 9");
            Add("template.disableOnMobile", "Disable on mobile", "Выключить на мобильных");
            Add("template.letterboxFill", "Letterbox fill", "Заливка полей");
            Add("template.letterboxFills", "Page background|Custom color", "Фон страницы|Свой цвет");
            Add("template.pixelRatios", "Auto|Fixed 1.0|Auto up to 2.0", "Авто|Фиксированный 1.0|Авто, не выше 2.0");
            Add("template.overriddenNote", "Overridden in YouTube Playables: aspect ratio, pixel ratio.", "Переопределено в YouTube Playables: соотношение сторон, pixel ratio.");
            Add("template.previewModes", "Desktop|Mobile", "Десктоп|Мобильный");
            Add("template.previewLogo", "LOGO 160px", "ЛОГО 160px");
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
            Add("saves.revision", "Revision {0}", "Ревизия {0}");
            Add("saves.size", "{0} KB", "{0} КБ");
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
