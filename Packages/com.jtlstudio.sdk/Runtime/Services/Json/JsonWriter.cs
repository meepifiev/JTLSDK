using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JTLStudio.SDK.Services.Json
{
    public class JsonWriter
    {
        private StringBuilder _builder;

        public string Write(object value)
        {
            _builder = new StringBuilder();
            WriteValue(value);
            return _builder.ToString();
        }

        private void WriteValue(object value)
        {
            switch (value)
            {
                case null:
                    _builder.Append("null");
                    break;

                case string text:
                    WriteString(text);
                    break;

                case bool flag:
                    _builder.Append(flag ? "true" : "false");
                    break;

                case int number:
                    _builder.Append(number.ToString(CultureInfo.InvariantCulture));
                    break;

                case long number:
                    _builder.Append(number.ToString(CultureInfo.InvariantCulture));
                    break;

                case float number:
                    _builder.Append(number.ToString("R", CultureInfo.InvariantCulture));
                    break;

                case double number:
                    _builder.Append(number.ToString("R", CultureInfo.InvariantCulture));
                    break;

                case decimal number:
                    _builder.Append(number.ToString(CultureInfo.InvariantCulture));
                    break;

                case IDictionary<string, object> dictionary:
                    WriteObject(dictionary);
                    break;

                case IEnumerable enumerable:
                    WriteArray(enumerable);
                    break;

                default:
                    throw new ArgumentException(nameof(value));
            }
        }

        private void WriteObject(IDictionary<string, object> dictionary)
        {
            _builder.Append('{');
            bool first = true;

            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (first == false)
                {
                    _builder.Append(',');
                }

                first = false;
                WriteString(pair.Key);
                _builder.Append(':');
                WriteValue(pair.Value);
            }

            _builder.Append('}');
        }

        private void WriteArray(IEnumerable enumerable)
        {
            _builder.Append('[');
            bool first = true;

            foreach (object item in enumerable)
            {
                if (first == false)
                {
                    _builder.Append(',');
                }

                first = false;
                WriteValue(item);
            }

            _builder.Append(']');
        }

        private void WriteString(string text)
        {
            _builder.Append('"');

            foreach (char character in text)
            {
                switch (character)
                {
                    case '"':
                        _builder.Append("\\\"");
                        break;

                    case '\\':
                        _builder.Append("\\\\");
                        break;

                    case '\n':
                        _builder.Append("\\n");
                        break;

                    case '\r':
                        _builder.Append("\\r");
                        break;

                    case '\t':
                        _builder.Append("\\t");
                        break;

                    case '\b':
                        _builder.Append("\\b");
                        break;

                    case '\f':
                        _builder.Append("\\f");
                        break;

                    default:
                        if (character < ' ')
                        {
                            _builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            _builder.Append(character);
                        }

                        break;
                }
            }

            _builder.Append('"');
        }
    }
}
