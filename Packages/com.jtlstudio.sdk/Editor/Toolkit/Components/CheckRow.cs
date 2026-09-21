using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class CheckRow : VisualElement, ILocalizedElement
    {
        public const string PassState = "pass";
        public const string FailState = "fail";
        public const string PendingState = "pending";
        public const string WarningState = "warning";
        private const int IconSize = 14;
        private const string ClassName = "jtl-check-row";
        private const string StatePrefix = "jtl-check-row--";

        public new class UxmlFactory : UxmlFactory<CheckRow, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _textKey = new UxmlStringAttributeDescription { name = "text-key" };
            private readonly UxmlStringAttributeDescription _text = new UxmlStringAttributeDescription { name = "text" };
            private readonly UxmlStringAttributeDescription _state = new UxmlStringAttributeDescription { name = "state", defaultValue = PendingState };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                CheckRow row = (CheckRow)element;
                row.State = _state.GetValueFromBag(attributes, context);
                row.Text = _text.GetValueFromBag(attributes, context);
                row.TextKey = _textKey.GetValueFromBag(attributes, context);
            }
        }

        private readonly Icon _icon = new Icon();
        private readonly Label _text = new Label();
        private string _state = PendingState;

        public CheckRow()
        {
            AddToClassList(ClassName);
            AddToClassList(StatePrefix + PendingState);
            _icon.AddToClassList("jtl-check-row__icon");
            _icon.Size = IconSize;
            _text.AddToClassList("jtl-check-row__text");
            Add(_icon);
            Add(_text);
            ApplyIcon();
        }

        public CheckRow(string textKey, string state) : this()
        {
            TextKey = textKey;
            State = state;
        }

        public string TextKey { get; set; }

        public string Text
        {
            get => _text.text;
            set => _text.text = value;
        }

        public string State
        {
            get => _state;
            set
            {
                RemoveFromClassList(StatePrefix + _state);
                _state = string.IsNullOrEmpty(value) ? PendingState : value;
                AddToClassList(StatePrefix + _state);
                ApplyIcon();
            }
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(TextKey) == false)
            {
                _text.text = localization.Get(TextKey);
            }
        }

        private void ApplyIcon()
        {
            switch (_state)
            {
                case PassState:
                    _icon.IconName = "check";
                    _icon.Tone = "success";
                    break;
                case FailState:
                    _icon.IconName = "close";
                    _icon.Tone = "error";
                    break;
                case WarningState:
                    _icon.IconName = "alert-triangle";
                    _icon.Tone = "warning";
                    break;
                default:
                    _icon.IconName = "minus";
                    _icon.Tone = "muted";
                    break;
            }
        }
    }
}
