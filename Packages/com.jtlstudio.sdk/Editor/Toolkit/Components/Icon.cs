using JTLStudio.SDK.Editor.Toolkit.Icons;
using UnityEngine;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class Icon : VisualElement
    {
        public const string DefaultTone = "secondary";
        public const int DefaultSize = 16;
        private const string ClassName = "jtl-icon";
        private const string TonePrefix = "jtl-icon--";

        public new class UxmlFactory : UxmlFactory<Icon, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _icon = new UxmlStringAttributeDescription { name = "icon" };
            private readonly UxmlIntAttributeDescription _size = new UxmlIntAttributeDescription { name = "size", defaultValue = DefaultSize };
            private readonly UxmlStringAttributeDescription _tone = new UxmlStringAttributeDescription { name = "tone", defaultValue = DefaultTone };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                Icon icon = (Icon)element;
                icon.Size = _size.GetValueFromBag(attributes, context);
                icon.Tone = _tone.GetValueFromBag(attributes, context);
                icon.IconName = _icon.GetValueFromBag(attributes, context);
            }
        }

        private readonly IconLibrary _library = new IconLibrary();
        private string _iconName;
        private int _size = DefaultSize;
        private string _tone = DefaultTone;

        public Icon()
        {
            AddToClassList(ClassName);
            AddToClassList(TonePrefix + DefaultTone);
            pickingMode = PickingMode.Ignore;
            ApplySize();
        }

        public Icon(string iconName, int size, string tone) : this()
        {
            Size = size;
            Tone = tone;
            IconName = iconName;
        }

        public string IconName
        {
            get => _iconName;
            set
            {
                _iconName = value;
                ApplyTexture();
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                ApplySize();
                ApplyTexture();
            }
        }

        public string Tone
        {
            get => _tone;
            set
            {
                RemoveFromClassList(TonePrefix + _tone);
                _tone = string.IsNullOrEmpty(value) ? DefaultTone : value;
                AddToClassList(TonePrefix + _tone);
            }
        }

        private void ApplySize()
        {
            style.width = _size;
            style.height = _size;
            style.minWidth = _size;
            style.minHeight = _size;
        }

        private void ApplyTexture()
        {
            Texture2D texture = _library.Load(_iconName, _size);
            style.backgroundImage = texture;
            style.display = texture == null ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
