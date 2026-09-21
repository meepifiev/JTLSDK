using System;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class PrototypeRequestOption
    {
        public PrototypeRequestOption(string label, string callback, string hint, bool primary, Action onChosen)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            Hint = hint ?? "";
            IsPrimary = primary;
            OnChosen = onChosen ?? throw new ArgumentNullException(nameof(onChosen));
        }

        public string Label { get; }
        public string Callback { get; }
        public string Hint { get; }
        public bool IsPrimary { get; }
        public Action OnChosen { get; }
    }
}
