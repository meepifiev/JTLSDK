using System;
using System.Text.RegularExpressions;

namespace JTLStudio.SDK.Editor.Analyzer
{
    public class AnalyzerRule
    {
        public AnalyzerRule(string pattern, string replacement, bool simple)
        {
            Pattern = new Regex(pattern ?? throw new ArgumentNullException(nameof(pattern)), RegexOptions.Compiled);
            Replacement = replacement;
            IsSimple = simple;
        }

        public Regex Pattern { get; }
        public string Replacement { get; }
        public bool IsSimple { get; }
        public bool HasReplacement => string.IsNullOrEmpty(Replacement) == false;
    }
}
