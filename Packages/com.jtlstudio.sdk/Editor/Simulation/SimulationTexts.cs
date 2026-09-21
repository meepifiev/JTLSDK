using System.Collections.Generic;
using UnityEditor;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class SimulationTexts
    {
        public const string LanguagePreferenceKey = "JTLSDK.ToolkitLanguage";

        private readonly Dictionary<string, string> _english = new Dictionary<string, string>
        {
            { "language", "Language" },
            { "device", "Device" },
            { "platformPause", "Platform pause" },
            { "platformMuted", "Platform audio muted" },
            { "notCreated", "JTLSDK.Create() has not been called yet" },
            { "ready", "ready" },
            { "initializing", "initializing" },
            { "save", "save" },
            { "chooseAd", "Choose what happened. The game receives the callback written on the button." },
            { "choosePurchase", "Choose what happened. The game receives the events written on the button." },
            { "remember", "Use this answer for the rest of Play Mode" },
            { "rewardedWatched", "Player watched the ad to the end" },
            { "rewardedWatchedHint", "Give the reward." },
            { "rewardedClosed", "Player closed the ad early" },
            { "rewardedClosedHint", "No reward." },
            { "adNotShown", "Platform had no ad to show" },
            { "adNotShownHint", "Nothing was shown, no reward." },
            { "adFailed", "Platform error while showing" },
            { "adFailedHint", "Nothing was shown, no reward." },
            { "interstitialShown", "Ad was shown and closed" },
            { "interstitialShownHint", "Continue the game." },
            { "interstitialNotShownHint", "Continue the game." },
            { "interstitialFailedHint", "Continue the game." },
            { "purchasePaid", "Player paid" },
            { "purchasePaidHint", "Give the product in the Granted handler, the SDK consumes it after the save." },
            { "purchaseCrash", "Player paid, game crashed before Granted" },
            { "purchaseCrashHint", "Nothing now. Granted fires again on the next Play Mode start." },
            { "purchaseCancelled", "Player cancelled" },
            { "purchaseCancelledHint", "Nothing to give." },
            { "purchaseFailed", "Payment failed" },
            { "purchaseFailedHint", "Show an error if the game needs one." },
        };

        private readonly Dictionary<string, string> _russian = new Dictionary<string, string>
        {
            { "language", "Язык" },
            { "device", "Устройство" },
            { "platformPause", "Пауза площадки" },
            { "platformMuted", "Звук площадки выключен" },
            { "notCreated", "JTLSDK.Create() ещё не вызван" },
            { "ready", "готов" },
            { "initializing", "инициализация" },
            { "save", "сейв" },
            { "chooseAd", "Выбери, что произошло. Игра получит колбэк, написанный на кнопке." },
            { "choosePurchase", "Выбери, что произошло. Игра получит события, написанные на кнопке." },
            { "remember", "Отвечать так до конца Play Mode" },
            { "rewardedWatched", "Игрок досмотрел рекламу до конца" },
            { "rewardedWatchedHint", "Выдай награду." },
            { "rewardedClosed", "Игрок закрыл рекламу раньше" },
            { "rewardedClosedHint", "Награды нет." },
            { "adNotShown", "У площадки не нашлось рекламы" },
            { "adNotShownHint", "Ничего не показано, награды нет." },
            { "adFailed", "Ошибка площадки при показе" },
            { "adFailedHint", "Ничего не показано, награды нет." },
            { "interstitialShown", "Реклама показана и закрыта" },
            { "interstitialShownHint", "Продолжай игру." },
            { "interstitialNotShownHint", "Продолжай игру." },
            { "interstitialFailedHint", "Продолжай игру." },
            { "purchasePaid", "Игрок оплатил" },
            { "purchasePaidHint", "Выдай товар в обработчике Granted, SDK подтвердит покупку после сохранения." },
            { "purchaseCrash", "Игрок оплатил, игра упала до Granted" },
            { "purchaseCrashHint", "Сейчас ничего. Granted придёт снова при следующем запуске Play Mode." },
            { "purchaseCancelled", "Игрок отменил" },
            { "purchaseCancelledHint", "Выдавать нечего." },
            { "purchaseFailed", "Оплата не прошла" },
            { "purchaseFailedHint", "Покажи ошибку, если игре это нужно." },
        };

        public string Get(string key)
        {
            Dictionary<string, string> table = IsRussian ? _russian : _english;
            return table.TryGetValue(key, out string text) ? text : key;
        }

        private bool IsRussian => EditorPrefs.GetString(LanguagePreferenceKey, "en") == "ru";
    }
}
