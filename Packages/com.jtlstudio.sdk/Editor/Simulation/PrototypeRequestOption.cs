using System;

namespace JTLStudio.SDK.Editor.Simulation
{
    public class PrototypeRequestOption
    {
        public PrototypeRequestOption(string callback, string note, bool primary, Action onChosen)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
            Note = note ?? "";
            IsPrimary = primary;
            OnChosen = onChosen ?? throw new ArgumentNullException(nameof(onChosen));
        }

        public string Callback { get; }
        public string Note { get; }
        public bool IsPrimary { get; }
        public Action OnChosen { get; }
    }
}
