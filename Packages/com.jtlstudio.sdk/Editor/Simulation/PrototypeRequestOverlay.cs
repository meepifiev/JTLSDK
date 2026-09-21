using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class PrototypeRequestOverlay : VisualElement
    {
        private readonly Toggle _rememberToggle;

        public PrototypeRequestOverlay(string title, string caption, IReadOnlyList<PrototypeRequestOption> options, Action<PrototypeRequestOption, bool> onChosen)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (onChosen == null)
            {
                throw new ArgumentNullException(nameof(onChosen));
            }

            AddToClassList("jtl-request-dim");

            VisualElement card = new VisualElement();
            card.AddToClassList("jtl-request-card");
            Add(card);

            Label titleLabel = new Label(title);
            titleLabel.AddToClassList("jtl-request-title");
            card.Add(titleLabel);

            Label captionLabel = new Label(caption);
            captionLabel.AddToClassList("jtl-request-caption");
            card.Add(captionLabel);

            _rememberToggle = new Toggle("Remember until Play Mode ends");
            _rememberToggle.AddToClassList("jtl-request-remember");

            foreach (PrototypeRequestOption option in options)
            {
                Button button = new Button(() => onChosen(option, _rememberToggle.value));
                button.AddToClassList("jtl-request-option");

                if (option.IsPrimary)
                {
                    button.AddToClassList("jtl-request-option--primary");
                }

                Label label = new Label(option.Label);
                label.AddToClassList("jtl-request-option__label");
                Label result = new Label(option.Result);
                result.AddToClassList("jtl-request-option__result");
                button.Add(label);
                button.Add(result);
                card.Add(button);
            }

            card.Add(_rememberToggle);
        }
    }
}
