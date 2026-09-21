using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JTLStudio.SDK.Services.Json
{
    public class JsonParser
    {
        private string _text = "";
        private int _position;

        public object Parse(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _position = 0;

            object value = ParseValue();
            SkipWhitespace();

            if (_position != _text.Length)
            {
                throw new FormatException(nameof(text));
            }

            return value;
        }

        private object ParseValue()
        {
            SkipWhitespace();

            if (_position >= _text.Length)
            {
                throw new FormatException(nameof(_text));
            }

            char current = _text[_position];

            switch (current)
            {
                case '{':
                    return ParseObject();

                case '[':
                    return ParseArray();

                case '"':
                    return ParseString();

                case 't':
                    ExpectLiteral("true");
                    return true;

                case 'f':
                    ExpectLiteral("false");
                    return false;

                case 'n':
                    ExpectLiteral("null");
                    return null;

                default:
                    return ParseNumber();
            }
        }

        private Dictionary<string, object> ParseObject()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            _position++;
            SkipWhitespace();

            if (Peek() == '}')
            {
                _position++;
                return result;
            }

            while (true)
            {
                SkipWhitespace();

                if (Peek() != '"')
                {
                    throw new FormatException(nameof(_text));
                }

                string key = ParseString();
                SkipWhitespace();
                Expect(':');
                result[key] = ParseValue();
                SkipWhitespace();

                char separator = Next();

                if (separator == '}')
                {
                    return result;
                }

                if (separator != ',')
                {
                    throw new FormatException(nameof(_text));
                }
            }
        }

        private List<object> ParseArray()
        {
            List<object> result = new List<object>();
            _position++;
            SkipWhitespace();

            if (Peek() == ']')
            {
                _position++;
                return result;
            }

            while (true)
            {
                result.Add(ParseValue());
                SkipWhitespace();

                char separator = Next();

                if (separator == ']')
                {
                    return result;
                }

                if (separator != ',')
                {
                    throw new FormatException(nameof(_text));
                }
            }
        }

        private string ParseString()
        {
            Expect('"');
            StringBuilder builder = new StringBuilder();

            while (true)
            {
                char current = Next();

                if (current == '"')
                {
                    return builder.ToString();
                }

                if (current != '\\')
                {
                    builder.Append(current);
                    continue;
                }

                char escaped = Next();

                switch (escaped)
                {
                    case '"':
                    case '\\':
                    case '/':
                        builder.Append(escaped);
                        break;

                    case 'n':
                        builder.Append('\n');
                        break;

                    case 'r':
                        builder.Append('\r');
                        break;

                    case 't':
                        builder.Append('\t');
                        break;

                    case 'b':
                        builder.Append('\b');
                        break;

                    case 'f':
                        builder.Append('\f');
                        break;

                    case 'u':
                        builder.Append(ParseUnicodeEscape());
                        break;

                    default:
                        throw new FormatException(nameof(_text));
                }
            }
        }

        private char ParseUnicodeEscape()
        {
            if (_position + 4 > _text.Length)
            {
                throw new FormatException(nameof(_text));
            }

            string hex = _text.Substring(_position, 4);
            _position += 4;
            return (char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        private object ParseNumber()
        {
            int start = _position;
            bool isInteger = true;

            while (_position < _text.Length)
            {
                char current = _text[_position];

                if (char.IsDigit(current) || current == '-' || current == '+')
                {
                    _position++;
                    continue;
                }

                if (current == '.' || current == 'e' || current == 'E')
                {
                    isInteger = false;
                    _position++;
                    continue;
                }

                break;
            }

            string token = _text.Substring(start, _position - start);

            if (token.Length == 0)
            {
                throw new FormatException(nameof(_text));
            }

            if (isInteger && long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
            {
                return integer;
            }

            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                return number;
            }

            throw new FormatException(nameof(_text));
        }

        private void ExpectLiteral(string literal)
        {
            if (string.CompareOrdinal(_text, _position, literal, 0, literal.Length) != 0)
            {
                throw new FormatException(nameof(_text));
            }

            _position += literal.Length;
        }

        private void Expect(char expected)
        {
            if (Next() != expected)
            {
                throw new FormatException(nameof(_text));
            }
        }

        private char Peek()
        {
            if (_position >= _text.Length)
            {
                throw new FormatException(nameof(_text));
            }

            return _text[_position];
        }

        private char Next()
        {
            if (_position >= _text.Length)
            {
                throw new FormatException(nameof(_text));
            }

            return _text[_position++];
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }
        }
    }
}
