using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JTLStudio.SDK.Services.Json
{
    public class JsonReader
    {
        private string _text;
        private int _position;

        public object Parse(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _position = 0;

            SkipWhitespace();
            object value = ReadValue();
            SkipWhitespace();

            if (_position != _text.Length)
            {
                throw new FormatException(nameof(text));
            }

            return value;
        }

        private object ReadValue()
        {
            if (_position >= _text.Length)
            {
                throw new FormatException(nameof(_text));
            }

            char current = _text[_position];

            switch (current)
            {
                case '{':
                    return ReadObject();

                case '[':
                    return ReadArray();

                case '"':
                    return ReadString();

                case 't':
                    ReadLiteral("true");
                    return true;

                case 'f':
                    ReadLiteral("false");
                    return false;

                case 'n':
                    ReadLiteral("null");
                    return null;

                default:
                    return ReadNumber();
            }
        }

        private Dictionary<string, object> ReadObject()
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
                string key = ReadString();
                SkipWhitespace();
                Expect(':');
                SkipWhitespace();
                result[key] = ReadValue();
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

        private List<object> ReadArray()
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
                SkipWhitespace();
                result.Add(ReadValue());
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

        private string ReadString()
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

                    case 'b':
                        builder.Append('\b');
                        break;

                    case 'f':
                        builder.Append('\f');
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

                    case 'u':
                        builder.Append(ReadUnicodeEscape());
                        break;

                    default:
                        throw new FormatException(nameof(_text));
                }
            }
        }

        private char ReadUnicodeEscape()
        {
            if (_position + 4 > _text.Length)
            {
                throw new FormatException(nameof(_text));
            }

            string hex = _text.Substring(_position, 4);
            _position += 4;
            return (char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        private object ReadNumber()
        {
            int start = _position;

            while (_position < _text.Length && IsNumberCharacter(_text[_position]))
            {
                _position++;
            }

            if (start == _position)
            {
                throw new FormatException(nameof(_text));
            }

            string token = _text.Substring(start, _position - start);

            if (token.IndexOfAny(new[] { '.', 'e', 'E' }) < 0 && long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
            {
                return integer;
            }

            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                return number;
            }

            throw new FormatException(nameof(_text));
        }

        private bool IsNumberCharacter(char character)
        {
            return char.IsDigit(character) || character == '-' || character == '+' || character == '.' || character == 'e' || character == 'E';
        }

        private void ReadLiteral(string literal)
        {
            if (string.CompareOrdinal(_text, _position, literal, 0, literal.Length) != 0)
            {
                throw new FormatException(nameof(_text));
            }

            _position += literal.Length;
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
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
            char current = Peek();
            _position++;
            return current;
        }

        private void Expect(char expected)
        {
            if (Next() != expected)
            {
                throw new FormatException(nameof(_text));
            }
        }
    }
}
