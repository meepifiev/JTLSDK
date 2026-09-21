using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JTLStudio.SDK.Editor.Analyzer
{
    public class ApiAnalyzer
    {
        private const string SdkNamespaceUsing = "using JTLStudio.SDK;";
        private const string SdkPackageFolder = "Packages/com.jtlstudio.sdk";
        private const string CommentPrefix = "//";
        private const int SearchRadius = 2;

        private readonly List<AnalyzerRule> _rules = new List<AnalyzerRule>
        {
            new AnalyzerRule(@"\bTime\.timeScale\b", "JTLSDK.Time.Scale", true),
            new AnalyzerRule(@"\bAudioListener\.volume\b", "JTLSDK.Audio.Volume", true),
            new AnalyzerRule(@"\bAudioListener\.pause\s*=\s*(true|false)", "JTLSDK.Pause.Set(\"Game\", $1)", false),
            new AnalyzerRule(@"\bPlayerPrefs\.(GetInt|SetInt|GetFloat|SetFloat|GetString|SetString|HasKey|DeleteKey|DeleteAll|Save)\b", "JTLSDK.Data.$1", true),
            new AnalyzerRule(@"\bCursor\.visible\b", "JTLSDK.Device.CursorVisible", true),
            new AnalyzerRule(@"\bCursor\.lockState\b", "JTLSDK.Device.CursorLock", true),
            new AnalyzerRule(@"\bApplication\.OpenURL\b", "", false)
        };

        public int ScannedFiles { get; private set; }

        public List<AnalyzerFinding> Scan(string folder, IReadOnlyList<string> excludedFolders)
        {
            List<AnalyzerFinding> findings = new List<AnalyzerFinding>();
            ScannedFiles = 0;

            if (Directory.Exists(folder) == false)
            {
                return findings;
            }

            foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');

                if (IsExcluded(path, excludedFolders))
                {
                    continue;
                }

                ScannedFiles++;
                string[] lines = File.ReadAllLines(path);

                for (int index = 0; index < lines.Length; index++)
                {
                    AnalyzeLine(path, index + 1, lines[index], findings);
                }
            }

            return findings;
        }

        public bool Replace(AnalyzerFinding finding)
        {
            if (finding.HasReplacement == false || File.Exists(finding.Path) == false)
            {
                return false;
            }

            List<string> lines = new List<string>(File.ReadAllLines(finding.Path));
            int index = FindLine(lines, finding);

            if (index < 0)
            {
                return false;
            }

            lines[index] = lines[index].Replace(finding.Code, finding.Replacement);
            EnsureUsing(lines);
            File.WriteAllText(finding.Path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
            return true;
        }

        private int FindLine(List<string> lines, AnalyzerFinding finding)
        {

            for (int offset = 0; offset <= SearchRadius; offset++)
            {
                int below = finding.Line - 1 + offset;
                int above = finding.Line - 1 - offset;

                if (below >= 0 && below < lines.Count && lines[below].Trim() == finding.Code)
                {
                    return below;
                }

                if (above >= 0 && above < lines.Count && lines[above].Trim() == finding.Code)
                {
                    return above;
                }
            }

            return -1;
        }

        private void AnalyzeLine(string path, int lineNumber, string line, List<AnalyzerFinding> findings)
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith(CommentPrefix))
            {
                return;
            }

            foreach (AnalyzerRule rule in _rules)
            {
                if (rule.Pattern.IsMatch(trimmed) == false)
                {
                    continue;
                }

                string replacement = rule.HasReplacement ? rule.Pattern.Replace(trimmed, rule.Replacement) : "";
                findings.Add(new AnalyzerFinding(path, lineNumber, trimmed, replacement, rule.IsSimple));
                return;
            }
        }

        private bool IsExcluded(string path, IReadOnlyList<string> excludedFolders)
        {
            if (path.StartsWith(SdkPackageFolder, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (string excluded in excludedFolders)
            {
                string normalized = excluded.Trim().TrimEnd('/');

                if (normalized.Length > 0 && path.StartsWith(normalized + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureUsing(List<string> lines)
        {
            foreach (string line in lines)
            {
                if (line.Trim() == SdkNamespaceUsing)
                {
                    return;
                }
            }

            int insertAt = 0;

            for (int index = 0; index < lines.Count; index++)
            {
                if (lines[index].TrimStart().StartsWith("using ") && lines[index].TrimEnd().EndsWith(";"))
                {
                    insertAt = index + 1;
                }
            }

            lines.Insert(insertAt, SdkNamespaceUsing);
        }
    }
}
