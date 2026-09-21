using System;
using System.Collections.Generic;
using JTLStudio.SDK.Prototype;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class GameViewOverlay : VisualElement
    {
        private readonly SimulationSession _session;
        private readonly SimulationTexts _texts = new SimulationTexts();
        private readonly VisualElement _panel;
        private readonly Button _collapsedButton;
        private readonly DropdownField _languageField;
        private readonly DropdownField _deviceField;
        private readonly VisualElement _pauseRow;
        private readonly Toggle _pauseToggle;
        private readonly VisualElement _muteRow;
        private readonly Toggle _muteToggle;
        private readonly Label _caption;
        private readonly List<Language> _languageChoices = new List<Language>();
        private bool _collapsed;
        private bool _refreshing;

        public GameViewOverlay(SimulationSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;

            _panel = new VisualElement();
            _panel.AddToClassList("jtl-overlay-panel");
            Add(_panel);

            VisualElement header = new VisualElement();
            header.AddToClassList("jtl-overlay-header");
            Label title = new Label("JTL SDK");
            title.AddToClassList("jtl-overlay-title");
            Button collapse = new Button(OnCollapseClicked) { text = "–" };
            collapse.AddToClassList("jtl-icon-button");
            header.Add(title);
            header.Add(collapse);
            _panel.Add(header);

            _languageField = new DropdownField();
            _languageField.RegisterValueChangedCallback(OnLanguageChanged);
            _panel.Add(CreateRow(_texts.Get("language"), _languageField));

            _deviceField = new DropdownField(new List<string>(Enum.GetNames(typeof(DeviceType))), 0);
            _deviceField.RegisterValueChangedCallback(OnDeviceChanged);
            _panel.Add(CreateRow(_texts.Get("device"), _deviceField));

            _pauseToggle = new Toggle();
            _pauseToggle.RegisterValueChangedCallback(OnPauseChanged);
            _pauseRow = CreateRow(_texts.Get("platformPause"), _pauseToggle);
            _panel.Add(_pauseRow);

            _muteToggle = new Toggle();
            _muteToggle.RegisterValueChangedCallback(OnMuteChanged);
            _muteRow = CreateRow(_texts.Get("platformMuted"), _muteToggle);
            _panel.Add(_muteRow);

            _caption = new Label("");
            _caption.AddToClassList("jtl-overlay-caption");
            _panel.Add(_caption);

            _collapsedButton = new Button(OnExpandClicked) { text = "JTL SDK" };
            _collapsedButton.AddToClassList("jtl-overlay-collapsed");
            _collapsedButton.style.display = DisplayStyle.None;
            Add(_collapsedButton);

            Refresh();
        }

        public void Refresh()
        {
            _refreshing = true;

            try
            {
                RefreshLanguages();
                RefreshDevice();
                RefreshPlatform();
                RefreshCaption();
            }
            finally
            {
                _refreshing = false;
            }
        }

        private VisualElement CreateRow(string labelText, VisualElement control)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("jtl-overlay-row");
            Label label = new Label(labelText);
            label.AddToClassList("jtl-overlay-label");
            row.Add(label);
            row.Add(control);
            return row;
        }

        private void RefreshLanguages()
        {
            _languageChoices.Clear();
            Language current = _session.Settings.StartLanguage;

            if (JTLSDK.IsCreated)
            {
                _languageChoices.AddRange(JTLSDK.Language.Supported);
                current = JTLSDK.Language.Current;
            }
            else
            {
                _languageChoices.AddRange((Language[])Enum.GetValues(typeof(Language)));
            }

            List<string> names = new List<string>();

            foreach (Language language in _languageChoices)
            {
                names.Add(language.ToString());
            }

            _languageField.choices = names;
            _languageField.SetValueWithoutNotify(current.ToString());
        }

        private void RefreshDevice()
        {
            _deviceField.SetValueWithoutNotify(_session.Settings.DeviceType.ToString());
        }

        private void RefreshPlatform()
        {
            PrototypePlatformProvider platform = PrototypeBridge.ActivePlatform;
            bool hasPlatform = platform != null;
            _pauseRow.style.display = hasPlatform ? DisplayStyle.Flex : DisplayStyle.None;
            _muteRow.style.display = hasPlatform && platform.SupportsPlatformMute ? DisplayStyle.Flex : DisplayStyle.None;

            if (hasPlatform == false)
            {
                return;
            }

            _pauseToggle.SetValueWithoutNotify(platform.IsPlatformPaused);
            _muteToggle.SetValueWithoutNotify(platform.IsPlatformMuted);
        }

        private void RefreshCaption()
        {
            if (JTLSDK.IsCreated == false)
            {
                _caption.text = _texts.Get("notCreated");
                return;
            }

            string platform = JTLSDK.Platform.Current.ToString();
            string state = _texts.Get(JTLSDK.IsReady ? "ready" : "initializing");
            string data = JTLSDK.Data.LoadState.ToString().ToLowerInvariant();
            _caption.text = platform + " · " + state + " · " + _texts.Get("save") + " " + data;
        }

        private void OnLanguageChanged(ChangeEvent<string> changeEvent)
        {
            if (_refreshing)
            {
                return;
            }

            int index = _languageField.choices.IndexOf(changeEvent.newValue);

            if (index < 0 || index >= _languageChoices.Count)
            {
                return;
            }

            Language language = _languageChoices[index];
            _session.Settings.StartLanguage = language;
            _session.Settings.Save();

            if (JTLSDK.IsCreated && IsSupported(language))
            {
                JTLSDK.Language.Set(language);
            }
        }

        private bool IsSupported(Language language)
        {
            foreach (Language supported in JTLSDK.Language.Supported)
            {
                if (supported == language)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDeviceChanged(ChangeEvent<string> changeEvent)
        {
            if (_refreshing)
            {
                return;
            }

            if (Enum.TryParse(changeEvent.newValue, out DeviceType deviceType) == false)
            {
                return;
            }

            _session.Settings.DeviceType = deviceType;
            _session.Settings.Save();
        }

        private void OnPauseChanged(ChangeEvent<bool> changeEvent)
        {
            if (_refreshing)
            {
                return;
            }

            PrototypeBridge.ActivePlatform?.SetPlatformPaused(changeEvent.newValue);
        }

        private void OnMuteChanged(ChangeEvent<bool> changeEvent)
        {
            if (_refreshing)
            {
                return;
            }

            PrototypeBridge.ActivePlatform?.SetPlatformMuted(changeEvent.newValue);
        }

        private void OnCollapseClicked()
        {
            SetCollapsed(true);
        }

        private void OnExpandClicked()
        {
            SetCollapsed(false);
        }

        private void SetCollapsed(bool collapsed)
        {
            _collapsed = collapsed;
            _panel.style.display = _collapsed ? DisplayStyle.None : DisplayStyle.Flex;
            _collapsedButton.style.display = _collapsed ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
