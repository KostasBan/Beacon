using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KBanakakis.Beacon.Evaluation
{
    internal static class JsonLite
    {
        public static bool IsValidObjectJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var i = SkipWhitespace(json, 0);
            if (i >= json.Length || json[i] != '{')
            {
                return false;
            }

            if (!TryReadValue(json, i, out var endExclusive))
            {
                return false;
            }

            i = SkipWhitespace(json, endExclusive);
            return i == json.Length;
        }

        public static bool TryExtractObject(string json, string propertyName, out string objectJsonSubstring)
        {
            if (!TryExtractObject(json, propertyName, out objectJsonSubstring, out var present))
            {
                return false;
            }

            return present;
        }

        public static bool TryExtractObject(string json, string propertyName, out string objectJsonSubstring, out bool present)
        {
            objectJsonSubstring = null;
            if (!TryFindPropertyValueRange(json, propertyName, out var start, out var endExclusive, out present))
            {
                return false;
            }

            if (!present)
            {
                return true;
            }

            if (start >= endExclusive || json[start] != '{')
            {
                return false;
            }

            objectJsonSubstring = json.Substring(start, endExclusive - start);
            return true;
        }

        public static bool TryExtractBool(string json, string propertyName, out bool value, out bool present)
        {
            value = false;
            if (!TryFindPropertyValueRange(json, propertyName, out var start, out var endExclusive, out present))
            {
                return false;
            }

            if (!present)
            {
                return true;
            }

            var token = json.Substring(start, endExclusive - start);
            if (token == "true")
            {
                value = true;
                return true;
            }

            if (token == "false")
            {
                value = false;
                return true;
            }

            return false;
        }

        public static bool TryExtractInt(string json, string propertyName, out int value, out bool present)
        {
            value = 0;
            if (!TryFindPropertyValueRange(json, propertyName, out var start, out var endExclusive, out present))
            {
                return false;
            }

            if (!present)
            {
                return true;
            }

            var token = json.Substring(start, endExclusive - start);
            return int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryExtractString(string json, string propertyName, out string value, out bool present)
        {
            value = null;
            if (!TryFindPropertyValueRange(json, propertyName, out var start, out var endExclusive, out present))
            {
                return false;
            }

            if (!present)
            {
                return true;
            }

            if (start >= endExclusive || json[start] != '"')
            {
                return false;
            }

            var i = start;
            if (!TryReadString(json, ref i, out value))
            {
                return false;
            }

            return i == endExclusive;
        }

        public static bool TryExtractStringArray(string json, string propertyName, out HashSet<string> values, out bool present)
        {
            values = null;
            if (!TryFindPropertyValueRange(json, propertyName, out var start, out var endExclusive, out present))
            {
                return false;
            }

            if (!present)
            {
                return true;
            }

            if (start >= endExclusive || json[start] != '[')
            {
                return false;
            }

            var set = new HashSet<string>(StringComparer.Ordinal);
            var i = start + 1;
            while (true)
            {
                i = SkipWhitespace(json, i);
                if (i >= endExclusive)
                {
                    return false;
                }

                if (json[i] == ']')
                {
                    values = set;
                    return i + 1 == endExclusive;
                }

                if (json[i] != '"')
                {
                    return false;
                }

                if (!TryReadString(json, ref i, out var item))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(item))
                {
                    set.Add(item);
                }

                i = SkipWhitespace(json, i);
                if (i >= endExclusive)
                {
                    return false;
                }

                if (json[i] == ',')
                {
                    i++;
                    continue;
                }

                if (json[i] == ']')
                {
                    values = set;
                    return i + 1 == endExclusive;
                }

                return false;
            }
        }

        private static bool TryFindPropertyValueRange(string json, string propertyName, out int valueStart, out int valueEndExclusive, out bool present)
        {
            valueStart = 0;
            valueEndExclusive = 0;
            present = false;

            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            var i = SkipWhitespace(json, 0);
            if (i >= json.Length || json[i] != '{')
            {
                return false;
            }

            i++;
            while (true)
            {
                i = SkipWhitespace(json, i);
                if (i >= json.Length)
                {
                    return false;
                }

                if (json[i] == '}')
                {
                    return true;
                }

                if (json[i] != '"')
                {
                    return false;
                }

                if (!TryReadString(json, ref i, out var key))
                {
                    return false;
                }

                i = SkipWhitespace(json, i);
                if (i >= json.Length || json[i] != ':')
                {
                    return false;
                }

                i = SkipWhitespace(json, i + 1);
                if (i >= json.Length)
                {
                    return false;
                }

                if (!TryReadValue(json, i, out var endExclusive))
                {
                    return false;
                }

                if (key == propertyName)
                {
                    valueStart = i;
                    valueEndExclusive = endExclusive;
                    present = true;
                    return true;
                }

                i = SkipWhitespace(json, endExclusive);
                if (i >= json.Length)
                {
                    return false;
                }

                if (json[i] == ',')
                {
                    i++;
                    continue;
                }

                if (json[i] == '}')
                {
                    return true;
                }

                return false;
            }
        }

        private static bool TryReadValue(string json, int start, out int endExclusive)
        {
            endExclusive = start;
            if (start >= json.Length)
            {
                return false;
            }

            var c = json[start];
            if (c == '"')
            {
                var i = start;
                if (!TryReadString(json, ref i, out _))
                {
                    return false;
                }

                endExclusive = i;
                return true;
            }

            if (c == '{' || c == '[')
            {
                return TryReadComposite(json, start, c, c == '{' ? '}' : ']', out endExclusive);
            }

            var iToken = start;
            while (iToken < json.Length)
            {
                c = json[iToken];
                if (c == ',' || c == '}' || c == ']' || char.IsWhiteSpace(c))
                {
                    break;
                }

                iToken++;
            }

            if (iToken == start)
            {
                return false;
            }

            endExclusive = iToken;
            return true;
        }

        private static bool TryReadComposite(string json, int start, char open, char close, out int endExclusive)
        {
            endExclusive = start;
            var depth = 0;
            var i = start;
            while (i < json.Length)
            {
                var c = json[i];
                if (c == '"')
                {
                    i++;
                    while (i < json.Length)
                    {
                        if (json[i] == '\\')
                        {
                            i += 2;
                            continue;
                        }

                        if (json[i] == '"')
                        {
                            i++;
                            break;
                        }

                        i++;
                    }

                    if (i >= json.Length)
                    {
                        return false;
                    }

                    continue;
                }

                if (c == open)
                {
                    depth++;
                }
                else if (c == close)
                {
                    depth--;
                    if (depth == 0)
                    {
                        endExclusive = i + 1;
                        return true;
                    }

                    if (depth < 0)
                    {
                        return false;
                    }
                }

                i++;
            }

            return false;
        }

        private static bool TryReadString(string json, ref int index, out string value)
        {
            value = null;
            if (index >= json.Length || json[index] != '"')
            {
                return false;
            }

            index++;
            var sb = new StringBuilder();
            while (index < json.Length)
            {
                var c = json[index++];
                if (c == '"')
                {
                    value = sb.ToString();
                    return true;
                }

                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                if (index >= json.Length)
                {
                    return false;
                }

                var esc = json[index++];
                switch (esc)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (index + 4 > json.Length)
                        {
                            return false;
                        }

                        if (!ushort.TryParse(json.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
                        {
                            return false;
                        }

                        sb.Append((char)code);
                        index += 4;
                        break;
                    default:
                        return false;
                }
            }

            return false;
        }

        private static int SkipWhitespace(string json, int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }

            return index;
        }
    }
}
