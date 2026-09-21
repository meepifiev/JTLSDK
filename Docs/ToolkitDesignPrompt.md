# JTL SDK Toolkit: UI design brief

Prompt for a design tool. Reference style: minimalist near-monochrome dark admin interface (left sidebar, hairline borders, thin line icons, Inter, color only for meaning).

## What you are designing

A settings and tooling window for game developers that lives inside the Unity Editor. It is called "JTL SDK Toolkit". JTL SDK is a Unity package that lets one WebGL game be published to several web game portals (Yandex Games, YouTube Playables, later CrazyGames and others) without changing game code. The toolkit is where a developer configures everything: which portal is active, build settings, the loading page template, languages, in-app products, leaderboards, remote flags, build output, editor simulation of ads and purchases, a save-data editor, package updates, and a code analyzer.

Users are Unity developers. They open this window many times a day, so it must be calm, dense enough to be useful, and fast to scan. It is a tool, not a marketing dashboard.

## Visual direction

Reference: a minimalist, near-monochrome dark admin interface. Think of a dark dashboard with a left sidebar, near-black background, panels one step lighter than the background, 1px hairline borders, thin line icons, Inter typeface, white primary text, gray secondary text, and almost no color. Color is used only for meaning: success, warning, error, and one restrained accent for the active state and primary actions.

Rules:

- Palette: near-black background (around #0E0E10), surface one step lighter (around #151518), raised surface (around #1C1C20), hairline border (around #26262B), primary text (around #F2F2F3), secondary text (around #9A9AA3), muted text (around #66666E). One accent (a desaturated blue or a light neutral, your choice, but only one). Semantic colors: green for success, amber for warning, red for error. No other hues.
- Typography: Inter. Base size 13px (Unity Editor scale), 12px for secondary, 11px for captions, 20px for page titles, 15px for card titles. Weights 400 and 500 only, 600 for page titles.
- Shape: 6px radius on fields and buttons, 8px on cards, 4px on badges. 1px borders everywhere, no shadows, no blur, no gradients, no glass effects, no illustrations, no decorative shapes.
- Icons: thin line icons, 16px in navigation and rows, 20px for section headers, 1.5px stroke, monochrome. Portal logos (Yandex Games, YouTube) are the only full-color icons, shown small (24px) inside a neutral rounded square.
- Spacing: 4px grid. Page padding 24px, card padding 16px, row height 32px, gap between cards 12px. Denser than a consumer dashboard.
- Motion: only opacity and color transitions, 120ms. No movement animations.
- Always dark. There is no light theme.
- Language: all UI text in English. The window also supports Russian, so labels must survive 30% longer strings. Do not truncate labels.

## Hard technical constraints (Unity UI Toolkit)

The design will be implemented in Unity UI Toolkit (UXML and USS), which is a limited subset of CSS. Do not design anything that needs:

- box shadows, blur, backdrop filters;
- CSS gradients (only flat colors);
- custom web fonts other than Inter;
- CSS grid (only flexbox: rows and columns);
- overlapping absolutely positioned decorative layers;
- animated illustrations or Lottie.

Allowed: flat colors, 1px borders, rounded corners, flexbox layouts, raster or simple SVG icons, hover, pressed, focused and disabled states, opacity and color transitions.

Window facts: it is a dockable, resizable editor window. Minimum size 900 by 600 px. Design the main layout at 1280 by 800 and show how it holds at 900 by 600 (sidebar collapses to icons at less than 1040 px). The sidebar is fixed 220 px wide; the content area scrolls vertically. The window has no macOS or Windows title bar controls of its own.

## Layout skeleton

- Left sidebar, 220 px: logo mark and "JTL SDK" at the top with the caption "Build once. Play everywhere." Navigation list: Configurations, Template, Languages, Purchases, Leaderboards, Flags, Build, Simulation, Saves, Package, Analyzer. Bottom of sidebar: package version "v1.0.0", links "Documentation" and "Support".
- Top bar in the content area: "Active configuration: Yandex Games" as a dropdown, "Package 1.0.0 · 1.1.0 available" with a small dot when an update exists, RU / EN language toggle, a settings icon, a help icon.
- Content: page title, one-line description, then cards and forms.
- Bottom status bar, 28 px: icon, last message, time. Example: "Yandex Games configuration applied. Project recompiled. 14:03".

## Screens to design

Use the sample data below on every screen; do not invent product names.

1. Configurations. Cards for each configuration: portal logo, name, badge Active or Inactive, one-line description, three summary rows (Define symbol with copy button, Compression format, Languages count), and a button: "Open configuration" for the active one, "Make active" for others. A "New configuration" dropdown button with options Yandex Games and YouTube Playables. Below: "General settings" card with three fields: Initialization timeout (seconds), Autosave delay (seconds), Logging level dropdown.

2. Configuration details (opened from a card). Header with name, portal, define symbol (read-only) and "Make active" button. Section "Project settings applied on activation": a table with rows WebGL Template, Compression Format, Decompression Fallback, Data Caching, Managed Stripping Level, Run In Background, Memory Size (MB); each row has a value control (dropdown or toggle or number) and an "Apply" checkbox. One row must show a validation error state: for YouTube Playables, "Compression Format must be Disabled. Build is blocked." Section "Modules": rows Ads, Saves, Purchases, Leaderboards, Player, Language, Flags, Time, Review, Shortcut, each with a provider dropdown (values like "Yandex Games Ads", "Unsupported") and an expandable area with the provider's own settings (for Ads: AdBlock detection toggle, Minimum interstitial interval 60 s, Skip interstitial after rewarded 60 s, Sticky banner toggle). Section "Pause": toggle "Pause on focus loss", toggle "Show overlay on pause". Section "Languages on this configuration": checkboxes. Section "Template overrides": collapsed by default.

3. Template. Fields: Logo file picker and size (160 px); Loading screen background (radio: Color, Gradient, Image) with a color swatch; Progress bar: fill color, track color, width 40 %, height 8 px, radius 0 px, position radio (Below logo, Bottom of screen); Loading text with a toggle; Page background (radio, Gradient selected: radial toggle, angle 140, two colors); Aspect ratio (radio: Free, Fixed 16/9, toggle "Disable on mobile", letterbox fill radio); Pixel ratio for desktop and mobile (radio: Auto, Fixed 1.0, Auto up to 2.0); Fullscreen button toggle. A note line: "Overridden in YouTube Playables: aspect ratio, pixel ratio". A live preview panel on the right or below with Desktop and Mobile tabs and a progress slider at 55 %.

4. Languages. A grid of checkboxes for 26 languages (English, Russian, Turkish checked). Default language dropdown. "Replacements" list: rows "Belarusian → Russian", "Kazakh → Russian", "Ukrainian → Russian", "Uzbek → Russian", each removable, with "Add replacement". "Languages per configuration" matrix: Yandex Games (English, Russian, Turkish), YouTube Playables (English). "Play Mode start language" dropdown with the caption "Can be changed in the Game view overlay."

5. Purchases. Cards per product: Product id, Type dropdown (Non-consumable, Consumable), Yandex Games id, Test price with currency dropdown (YAN), Delete. Products: remove_ads (non-consumable, 49 YAN), coins_1000 (consumable, 15 YAN). Warning line: "Purchases are not supported on YouTube Playables." Button "Generate constants" with the caption "Assets/Scripts/Generated/JTLSDKIds.cs".

6. Leaderboards and Flags. Leaderboards: rows with id "levels", Yandex Games id "levels_board", YouTube "single board". Flags: rows with key, type dropdown (bool, int, float, string) and default value: tutorial_enabled bool true, ads_interval int 60. Both with "Add" and "Generate constants".

7. Build. Fields: Configuration (read-only: Yandex Games, active), Development build toggle with the caption "Build number badge is shown only in development builds", Output radio (Folder, ZIP), Path with Browse, Name pattern "{product}_{configuration}_b{build}" with a resolved preview "SmashAndHit_YandexGames_b43", Build number "42 → next 43" with Edit, After build checkboxes (Open folder, Log line). Card "Pre-build checks" with four green check rows. Card "Post-build checks (YouTube Playables)" with four gray rows: each file under 30 MiB, at most 8000 files, compression disabled, no external scripts. Primary button "Build" bottom right. Also design the blocked state: one red check row and a disabled Build button.

8. Simulation. Platform in Play Mode (read-only "Yandex Games"), Device dropdown, Initialization delay seconds, toggle "Simulate initialization failure". Card "Ads": radio "Ask every time" or "Use selected" with dropdowns Interstitial (Shown) and Rewarded (Rewarded), Ad duration 1.0 s. Card "Purchases": same radio with dropdown "Purchased". Card "Player": Authorized toggle, Name "Test player", Id "editor-player". Card "Saves": toggles "Simulate load failure", "Empty save on start". Toggle "Overlay in Game view".

9. Saves. Header line "Revision 42 · 1.8 KB of 200 KB · Loaded". Search field, "Add key" button. Table: Key, Type, Value (editable inline), delete icon. Rows: Money int 1200, Level int 27, MusicVolume float 0.5, Profile object (expandable JSON). Footer buttons: Reset all, Export JSON, Import JSON, Open JSON. Also design the empty state ("No save data yet").

10. Package. Row "JTL SDK · installed 1.0.0 · available 1.1.0" with "Update to 1.1.0" primary button and "Show pre-releases" toggle; an expandable release list with dates and bullet notes. Row "WebGL Template · installed 1.0.0" with Update. "Modules" list: Yandex Metrica 1.0.0 not installed (Install), CrazyGames 0.9.0 pre-release (Install). Footer "Checked at 14:02 · Check now".

11. Analyzer. Folder field "Assets/Game/Scripts", exclude field "Assets/Plugins", "Scan" button. Result list "7 places": each row shows file path with line, the found code, an arrow, the suggested replacement, and buttons Open and Replace. Examples: "GlobalTimeScaler.cs:23 · Time.timeScale = 0.3f → JTLSDK.Time.Scale = 0.3f"; "SettingHandler.cs:41 · AudioListener.volume = v → JTLSDK.Audio.Volume = v"; "SavesService.cs:88 · PlayerPrefs.GetInt("Level") → JTLSDK.Data.GetInt("Level")". Button "Replace all simple" with the caption "Only unambiguous replacements, with confirmation."

12. Game view overlay. A small floating panel in the top-right corner of the Unity Game view (over the running game, dark semi-opaque, 240 px wide): Language dropdown (Russian), Device dropdown (Desktop), toggle "Platform pause", toggle "Platform audio muted (YouTube)", one caption line "Yandex Games · ready · rev. 42". It collapses to a single small icon button.

13. Prototype overlays (shown over the running game in the editor, centered, 420 px wide). Rewarded ad: title "Rewarded · double_money · Yandex Games", caption "Game is paused. Choose the result.", four large buttons with the C# result under each in monospace: "Watched, grant reward" (AdResult.Rewarded), "Closed early, no reward" (AdResult.Closed), "Ad unavailable" (AdResult.NotShown), "Show failed" (AdResult.Failed), and a checkbox "Remember until Play Mode ends". Interstitial: three buttons. Purchase: title "Purchase · coins_1000 · 15 YAN", buttons "Pay" (Granted + Purchased), "Pay, game crashed before grant" (Granted on next launch), "Cancel" (PurchaseResult.Cancelled), "Payment failed" (PurchaseResult.Failed).

14. Page overlays for the built game (HTML, same visual language, but these live on the web page, not in Unity): "Click this area to continue." full-canvas prompt with a dark 30 % overlay and centered text; and the development build badge in a corner: "DEV · b43 · Yandex Games · v1.3.0".

15. Dialogs and states. Confirmation dialog for destructive actions ("Reset all save data?" with Cancel and a red Reset). Inline validation under a field. Disabled state of a whole card when a module is Unsupported. Empty states for lists. Collapsed sidebar at 900 px width.

## Components to deliver as a library

Sidebar nav item (default, hover, selected), top bar, page header, card (default, active-highlighted, disabled), badge (Active, Inactive, Pre-release, Update available), field with label and "?" tooltip icon, number field with unit suffix, text field, dropdown, toggle, checkbox, radio group, color swatch field, file picker field, table row with inline edit, list row with actions, buttons (primary, secondary, ghost, danger, icon-only), expandable section, status bar, inline error and warning text, check row (pass, fail, pending), empty state, confirmation dialog, tooltip.

Show every component in all states: default, hover, pressed, focused, disabled, error.

## Deliverables

1. Design tokens: colors, type scale, spacing, radii, border widths, icon sizes, as a list with hex values and px.
2. Component sheet with all states.
3. All 15 screens at 1280 by 800, plus Configurations and Build at 900 by 600 with the collapsed sidebar.
4. An icon list: name and purpose for every icon used, so they can be sourced from a thin-line icon set.
5. Short redlines for the layout skeleton: sidebar width, paddings, row heights, card gaps.

Keep everything flat, quiet and consistent. When in doubt, remove color and decoration rather than add it.
