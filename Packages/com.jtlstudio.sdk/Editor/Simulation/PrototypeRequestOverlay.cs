using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class PrototypeRequestOverlay : VisualElement
    {
        private readonly Toggle _rememberToggle;

        public PrototypeRequestOverlay(string title, string caption, string rememberText, IReadOnlyList<PrototypeRequestOption> options, Action<PrototypeRequestOption, bool> onChosen)
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

            _rememberToggle = new Toggle(rememberText);
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
                Label callback = new Label(option.Callback);
                callback.AddToClassList("jtl-request-option__result");
                button.Add(label);
                button.Add(callback);

                if (string.IsNullOrEmpty(option.Hint) == false)
                {
                    Label hint = new Label(option.Hint);
                    hint.AddToClassList("jtl-request-option__hint");
                    button.Add(hint);
                }

                card.Add(button);
            }

            card.Add(_rememberToggle);
        }
    }
}
