using System.Collections.Generic;

namespace JTLStudio.SDK
{
    public class LanguageCodes
    {
        private readonly Dictionary<string, Language> _byCode = new Dictionary<string, Language>();
        private readonly Dictionary<Language, string> _codes = new Dictionary<Language, string>();

        public LanguageCodes()
        {
            Register(Language.English, "en");
            Register(Language.Russian, "ru");
            Register(Language.Turkish, "tr");
            Register(Language.Spanish, "es");
            Register(Language.Portuguese, "pt");
            Register(Language.German, "de");
            Register(Language.French, "fr");
            Register(Language.Italian, "it");
            Register(Language.Polish, "pl");
            Register(Language.Ukrainian, "uk");
            Register(Language.Belarusian, "be");
            Register(Language.Kazakh, "kk");
            Register(Language.Uzbek, "uz");
            Register(Language.Azerbaijani, "az");
            Register(Language.Armenian, "hy");
            Register(Language.Georgian, "ka");
            Register(Language.Romanian, "ro");
            Register(Language.Arabic, "ar");
            Register(Language.Hebrew, "he");
            Register(Language.Hindi, "hi");
            Register(Language.Indonesian, "id");
            Register(Language.Japanese, "ja");
            Register(Language.Korean, "ko");
            Register(Language.ChineseSimplified, "zh");
            Register(Language.Vietnamese, "vi");
            Register(Language.Thai, "th");

            _byCode["iw"] = Language.Hebrew;
            _byCode["in"] = Language.Indonesian;
            _byCode["zh-cn"] = Language.ChineseSimplified;
            _byCode["zh-hans"] = Language.ChineseSimplified;
            _byCode["zh-sg"] = Language.ChineseSimplified;
        }

        public bool TryParse(string code, out Language language)
        {
            language = Language.English;

            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            string normalized = code.Trim().ToLowerInvariant().Replace('_', '-');

            if (_byCode.TryGetValue(normalized, out language))
            {
                return true;
            }

            int separatorIndex = normalized.IndexOf('-');

            if (separatorIndex > 0)
            {
                string primary = normalized.Substring(0, separatorIndex);

                if (_byCode.TryGetValue(primary, out language))
                {
                    return true;
                }
            }

            language = Language.English;
            return false;
        }

        public string ToCode(Language language)
        {
            return _codes[language];
        }

        private void Register(Language language, string code)
        {
            _byCode[code] = language;
            _codes[language] = code;
        }
    }
}
