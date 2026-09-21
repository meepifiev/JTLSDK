using System;
using UnityEngine;

namespace JTLStudio.SDK
{
    [Serializable]
    public struct LanguageReplacement
    {
        [SerializeField] private Language _from;
        [SerializeField] private Language _to;

        public LanguageReplacement(Language from, Language to)
        {
            _from = from;
            _to = to;
        }

        public Language From => _from;
        public Language To => _to;
    }
}
