using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class StatusBar : VisualElement, ILocalizedElement
    {
        private const int IconSize = 13;
        private const string ClassName = "jtl-status-bar";

        public new class UxmlFactory : UxmlFactory<StatusBar, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        private readonly Icon _icon = new Icon();
        private readonly Label _text = new Label();
        private readonly VisualElement _spacer = new VisualElement();
        private readonly Label _time = new Label();
        private ToolkitStatus _status;

        public StatusBar()
        {
            AddToClassList(ClassName);
            _icon.AddToClassList("jtl-status-bar__icon");
            _icon.Size = IconSize;
            _text.AddToClassList("jtl-status-bar__text");
            _spacer.AddToClassList("jtl-status-bar__spacer");
            _time.AddToClassList("jtl-status-bar__time");
            Add(_icon);
            Add(_text);
            Add(_spacer);
            Add(_time);
        }

        public ToolkitStatus Status => _status;

        public void Show(ToolkitStatus status)
        {
            _status = status;
            _time.text = status.Time ?? string.Empty;
            ApplyIcon(status.Kind);
            bool empty = string.IsNullOrEmpty(status.Text) && string.IsNullOrEmpty(status.MessageKey);
            _icon.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(_status.Text) == false)
            {
                _text.text = _status.Text;
                return;
            }

            _text.text = string.IsNullOrEmpty(_status.MessageKey) ? string.Empty : localization.Get(_status.MessageKey);
        }

        private void ApplyIcon(StatusKind kind)
        {
            switch (kind)
            {
                case StatusKind.Success:
                    _icon.IconName = "check";
                    _icon.Tone = "success";
                    break;
                case StatusKind.Error:
                    _icon.IconName = "close";
                    _icon.Tone = "error";
                    break;
                case StatusKind.Warning:
                    _icon.IconName = "alert-triangle";
                    _icon.Tone = "warning";
                    break;
                case StatusKind.Pending:
                    _icon.IconName = "clock";
                    _icon.Tone = "muted";
                    break;
                default:
                    _icon.IconName = "info";
                    _icon.Tone = "muted";
                    break;
            }
        }
    }
}
