using System;
using System.Collections.Generic;

namespace JTLStudio.SDK
{
    public interface ILanguage : IModule
    {
        Language Current { get; }
        IReadOnlyList<Language> Supported { get; }

        event Action<Language> Changed;

        void Set(Language language);
    }
}
