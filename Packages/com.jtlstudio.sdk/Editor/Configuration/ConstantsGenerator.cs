using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class ConstantsGenerator
    {
        public const string DefaultPath = "Assets/Scripts/Generated/JTLSDKIds.cs";

        private const string Indent = "    ";

        public string Generate(JTLSDKSettings settings, string path)
        {
            string namespaceName = string.IsNullOrWhiteSpace(EditorSettings.projectGenerationRootNamespace) ? "Game" : EditorSettings.projectGenerationRootNamespace.Trim();
            List<string> products = new List<string>();
            List<string> leaderboards = new List<string>();
            List<string> flags = new List<string>();

            foreach (ProductDefinition product in settings.Products)
            {
                products.Add(product.Id);
            }

            foreach (LeaderboardDefinition leaderboard in settings.Leaderboards)
            {
                leaderboards.Add(leaderboard.Id);
            }

            foreach (FlagDefinition flag in settings.Flags)
            {
                flags.Add(flag.Key);
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("namespace ").Append(namespaceName).Append('\n');
            builder.Append("{\n");
            AppendClass(builder, "ProductIds", products);
            builder.Append('\n');
            AppendClass(builder, "LeaderboardIds", leaderboards);
            builder.Append('\n');
            AppendClass(builder, "FlagKeys", flags);
            builder.Append("}\n");

            string directory = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, builder.ToString());
            AssetDatabase.ImportAsset(path);
            return path;
        }

        public string ToIdentifier(string id)
        {
            StringBuilder builder = new StringBuilder();
            bool upperNext = true;

            foreach (char character in id ?? "")
            {
                if (char.IsLetterOrDigit(character) == false)
                {
                    upperNext = true;
                    continue;
                }

                builder.Append(upperNext ? char.ToUpperInvariant(character) : character);
                upperNext = false;
            }

            if (builder.Length == 0)
            {
                return "Unnamed";
            }

            if (char.IsDigit(builder[0]))
            {
                builder.Insert(0, "Id");
            }

            return builder.ToString();
        }

        private void AppendClass(StringBuilder builder, string className, List<string> ids)
        {
            HashSet<string> used = new HashSet<string>();
            builder.Append(Indent).Append("public static class ").Append(className).Append('\n');
            builder.Append(Indent).Append("{\n");

            foreach (string id in ids)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                string identifier = ToIdentifier(id);
                string unique = identifier;
                int suffix = 2;

                while (used.Add(unique) == false)
                {
                    unique = identifier + suffix;
                    suffix++;
                }

                builder.Append(Indent).Append(Indent).Append("public const string ").Append(unique).Append(" = \"").Append(id.Replace("\"", "\\\"")).Append("\";\n");
            }

            builder.Append(Indent).Append("}\n");
        }
    }
}
