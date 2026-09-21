namespace JTLStudio.SDK.Editor.Analyzer
{
    public class AnalyzerFinding
    {
        public AnalyzerFinding(string path, int line, string code, string replacement, bool simple)
        {
            Path = path;
            Line = line;
            Code = code;
            Replacement = replacement;
            IsSimple = simple;
        }

        public string Path { get; }
        public int Line { get; }
        public string Code { get; }
        public string Replacement { get; }
        public bool IsSimple { get; }
        public bool HasReplacement => string.IsNullOrEmpty(Replacement) == false;
    }
}
