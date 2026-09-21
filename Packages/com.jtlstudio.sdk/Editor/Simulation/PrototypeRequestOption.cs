using System;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class PrototypeRequestOption
    {
        public PrototypeRequestOption(string label, string result, bool primary, Action onChosen)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Result = result ?? throw new ArgumentNullException(nameof(result));
            IsPrimary = primary;
            OnChosen = onChosen ?? throw new ArgumentNullException(nameof(onChosen));
        }

        public string Label { get; }
        public string Result { get; }
        public bool IsPrimary { get; }
        public Action OnChosen { get; }
    }
}
