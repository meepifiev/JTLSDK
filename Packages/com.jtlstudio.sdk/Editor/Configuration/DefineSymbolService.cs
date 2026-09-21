using System.Collections.Generic;
using UnityEditor;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class DefineSymbolService
    {
        public const string Prefix = "JTLSDK_";

        private const BuildTargetGroup TargetGroup = BuildTargetGroup.WebGL;

        public void Apply(string activeSymbol)
        {
            List<string> symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(TargetGroup).Split(';'));
            symbols.RemoveAll(symbol => symbol.StartsWith(Prefix) || string.IsNullOrWhiteSpace(symbol));

            if (string.IsNullOrEmpty(activeSymbol) == false)
            {
                symbols.Add(activeSymbol);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(TargetGroup, string.Join(";", symbols));
        }

        public string Current()
        {
            foreach (string symbol in PlayerSettings.GetScriptingDefineSymbolsForGroup(TargetGroup).Split(';'))
            {
                if (symbol.StartsWith(Prefix))
                {
                    return symbol;
                }
            }

            return "";
        }
    }
}
