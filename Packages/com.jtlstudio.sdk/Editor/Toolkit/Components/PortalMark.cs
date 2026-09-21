using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class PortalMark : VisualElement
    {
        public const string YandexGames = "yandex";
        public const string YouTubePlayables = "youtube";
        public const int DefaultSize = 24;
        private const string ClassName = "jtl-portal-mark";
        private const string PortalPrefix = "jtl-portal-mark--";
        private const float RadiusScale = 0.25f;

        public new class UxmlFactory : UxmlFactory<PortalMark, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _portal = new UxmlStringAttributeDescription { name = "portal", defaultValue = YandexGames };
            private readonly UxmlIntAttributeDescription _size = new UxmlIntAttributeDescription { name = "size", defaultValue = DefaultSize };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                PortalMark mark = (PortalMark)element;
                mark.Size = _size.GetValueFromBag(attributes, context);
                mark.Portal = _portal.GetValueFromBag(attributes, context);
            }
        }

        private string _portal = YandexGames;
        private int _size = DefaultSize;

        public PortalMark()
        {
            AddToClassList(ClassName);
            AddToClassList(PortalPrefix + YandexGames);
            pickingMode = PickingMode.Ignore;
            ApplySize();
        }

        public PortalMark(string portal, int size) : this()
        {
            Size = size;
            Portal = portal;
        }

        public string Portal
        {
            get => _portal;
            set
            {
                RemoveFromClassList(PortalPrefix + _portal);
                _portal = string.IsNullOrEmpty(value) ? YandexGames : value;
                AddToClassList(PortalPrefix + _portal);
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                ApplySize();
            }
        }

        private void ApplySize()
        {
            style.width = _size;
            style.height = _size;
            style.minWidth = _size;
            style.minHeight = _size;
            float radius = _size * RadiusScale;
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
        }
    }
}
