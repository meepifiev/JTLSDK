using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace JTLStudio.SDK.Editor.Configuration
{
    public class DefineSymbolService
    {
        public const string Prefix = "JTLSDK_";

        private readonly NamedBuildTarget _target = NamedBuildTarget.WebGL;

        public void Apply(string activeSymbol)
        {
            List<string> symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbols(_target).Split(';'));
            symbols.RemoveAll(symbol => symbol.StartsWith(Prefix) || string.IsNullOrWhiteSpace(symbol));

            if (string.IsNullOrEmpty(activeSymbol) == false)
            {
                symbols.Add(activeSymbol);
            }

            PlayerSettings.SetScriptingDefineSymbols(_target, string.Join(";", symbols));
        }

        public string Current()
        {
            foreach (string symbol in PlayerSettings.GetScriptingDefineSymbols(_target).Split(';'))
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
