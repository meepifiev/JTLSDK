using System;
using System.Collections.Generic;
using JTLStudio.SDK.Prototype;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class GameViewOverlay : VisualElement
    {
        private const float ToolbarHeight = 21f;
        private const float LeftToolbarWidth = 430f;
        private const float FallbackRightToolbarWidth = 300f;
        private const float IconButtonWidth = 28f;
        private const float ToolbarGap = 12f;
        private const string Separator = " · ";

        private readonly SimulationSession _session;
        private readonly LanguageCodes _codes = new LanguageCodes();
        private readonly Button _button;
        private readonly VisualElement _backdrop;
        private readonly VisualElement _panel;
        private readonly DropdownField _languageField;
        private readonly DropdownField _deviceField;
        private readonly VisualElement _muteRow;
        private readonly Toggle _muteToggle;
        private readonly List<Language> _languageChoices = new List<Language>();
        private float _rightToolbarWidth = -1f;
        private bool _open;
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

            _backdrop = new VisualElement();
            _backdrop.AddToClassList("jtl-overlay-backdrop");
            _backdrop.RegisterCallback<PointerDownEvent>(pointerEvent => SetOpen(false));
            Add(_backdrop);

            _button = new Button(() => SetOpen(_open == false));
            _button.AddToClassList("jtl-overlay-button");
            Add(_button);

            _panel = new VisualElement();
            _panel.AddToClassList("jtl-overlay-panel");
            Add(_panel);

            _languageField = new DropdownField();
            _languageField.RegisterValueChangedCallback(OnLanguageChanged);
            _panel.Add(CreateRow("Language", _languageField));

            _deviceField = new DropdownField(new List<string>(Enum.GetNames(typeof(DeviceType))), 0);
            _deviceField.RegisterValueChangedCallback(OnDeviceChanged);
            _panel.Add(CreateRow("Device", _deviceField));

            _muteToggle = new Toggle();
            _muteToggle.RegisterValueChangedCallback(OnMuteChanged);
            _muteRow = CreateRow("Platform audio muted", _muteToggle);
            _panel.Add(_muteRow);

            RegisterCallback<GeometryChangedEvent>(geometryEvent => Arrange());
            SetOpen(false);
            Refresh();
        }

        public void Refresh()
        {
            _refreshing = true;

            try
            {
                Language language = RefreshLanguages();
                RefreshDevice();
                RefreshMute();
                _button.text = "JTL" + Separator + _codes.ToCode(language).ToUpperInvariant() + Separator + _session.Settings.DeviceType + "  ▾";
            }
            finally
            {
                _refreshing = false;
            }
        }

        private void Arrange()
        {
            float width = resolvedStyle.width;

            if (float.IsNaN(width) || width <= 0f)
            {
                return;
            }

            float right = RightToolbarWidth() + ToolbarGap;
            float buttonWidth = _button.resolvedStyle.width;
            bool fitsToolbar = float.IsNaN(buttonWidth) || width - right - buttonWidth > LeftToolbarWidth;

            _button.style.right = fitsToolbar ? right : 6f;
            _button.style.top = fitsToolbar ? 1f : ToolbarHeight + 4f;
            _panel.style.right = fitsToolbar ? right : 6f;
            _panel.style.top = fitsToolbar ? ToolbarHeight : ToolbarHeight + 28f;
        }

        private float RightToolbarWidth()
        {
            if (_rightToolbarWidth > 0f)
            {
                return _rightToolbarWidth;
            }

            try
            {
                float width = EditorStyles.toolbarDropDown.CalcSize(new GUIContent("Play Unfocused")).x;
                width += IconButtonWidth * 2f;
                width += EditorStyles.toolbarButton.CalcSize(new GUIContent("Stats")).x;
                width += EditorStyles.toolbarDropDown.CalcSize(new GUIContent("Gizmos")).x;
                _rightToolbarWidth = width > 0f ? width : FallbackRightToolbarWidth;
            }
            catch (Exception)
            {
                _rightToolbarWidth = FallbackRightToolbarWidth;
            }

            return _rightToolbarWidth;
        }

        private void SetOpen(bool open)
        {
            _open = open;
            _panel.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            _backdrop.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            _button.EnableInClassList("jtl-overlay-button--open", open);
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

        private Language RefreshLanguages()
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
                JTLSDKSettings settings = Resources.Load<JTLSDKSettings>(JTLSDKSettings.ResourcePath);

                if (settings != null && settings.SupportedLanguages.Count > 0)
                {
                    _languageChoices.AddRange(settings.SupportedLanguages);
                }
                else
                {
                    _languageChoices.AddRange((Language[])Enum.GetValues(typeof(Language)));
                }

                if (_languageChoices.Contains(current) == false && _languageChoices.Count > 0)
                {
                    current = _languageChoices[0];
                }
            }

            List<string> names = new List<string>();

            foreach (Language language in _languageChoices)
            {
                names.Add(language.ToString());
            }

            _languageField.choices = names;
            _languageField.SetValueWithoutNotify(current.ToString());
            return current;
        }

        private void RefreshDevice()
        {
            _deviceField.SetValueWithoutNotify(_session.Settings.DeviceType.ToString());
        }

        private void RefreshMute()
        {
            PrototypePlatformProvider platform = PrototypeBridge.ActivePlatform;
            bool visible = platform != null && platform.SupportsPlatformMute;
            _muteRow.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            if (visible)
            {
                _muteToggle.SetValueWithoutNotify(platform.IsPlatformMuted);
            }
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

            Refresh();
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
            Refresh();
        }

        private void OnMuteChanged(ChangeEvent<bool> changeEvent)
        {
            if (_refreshing)
            {
                return;
            }

            PrototypeBridge.ActivePlatform?.SetPlatformMuted(changeEvent.newValue);
        }
    }
}
