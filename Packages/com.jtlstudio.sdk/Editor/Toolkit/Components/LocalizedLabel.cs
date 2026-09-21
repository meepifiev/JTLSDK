using JTLStudio.SDK.Editor.Toolkit.Localization;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit.Components
{
    public class LocalizedLabel : Label, ILocalizedElement
    {
        public new class UxmlFactory : UxmlFactory<LocalizedLabel, UxmlTraits>
        {
        }

        public new class UxmlTraits : Label.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _key = new UxmlStringAttributeDescription { name = "key" };

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext context)
            {
                base.Init(element, attributes, context);
                ((LocalizedLabel)element).Key = _key.GetValueFromBag(attributes, context);
            }
        }

        public LocalizedLabel()
        {
            AddToClassList("jtl-text");
        }

        public LocalizedLabel(string key) : this()
        {
            Key = key;
        }

        public string Key { get; set; }

        public void ApplyLocalization(ToolkitLocalization localization)
        {
            if (string.IsNullOrEmpty(Key) == false)
            {
                text = localization.Get(Key);
            }
        }
    }
}
