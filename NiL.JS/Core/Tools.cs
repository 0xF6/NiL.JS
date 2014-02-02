﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    public static class Tools
    {
        private static readonly char[] NumChars = new[] { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static double JSObjectToDouble(JSObject arg)
        {
            if (arg == null)
                return double.NaN;
            switch (arg.ValueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        return arg.iValue;
                    }
                case JSObjectType.Double:
                    {
                        return arg.dValue;
                    }
                case JSObjectType.String:
                    {
                        double x = double.NaN;
                        int ix = 0;
                        string s = (arg.oValue as string).Trim();
                        if (Tools.ParseNumber(s, ref ix, true, out x) && ix < s.Length)
                            return double.NaN;
                        return x;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (arg.oValue == null)
                            return 0;
                        arg = arg.ToPrimitiveValue_Value_String();
                        return JSObjectToDouble(arg);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return double.NaN;
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }

        public static int JSObjectToInt(JSObject arg)
        {
            if (arg == null)
                return 0;
            var r = arg;
            switch (r.ValueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        return r.iValue;
                    }
                case JSObjectType.Double:
                    {
                        if (double.IsNaN(r.dValue) || double.IsInfinity(r.dValue))
                            return 0;
                        return (int)((long)r.dValue & 0xFFFFFFFF);
                    }
                case JSObjectType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (r.oValue as string).Trim();
                        if (!Tools.ParseNumber(s, ref ix, true, out x) || ix < s.Length)
                            return 0;
                        return (int)x;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (r.oValue == null)
                            return 0;
                        r = r.ToPrimitiveValue_Value_String();
                        return JSObjectToInt(r);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return 0;
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }

        public static JSObject JSObjectToNumber(JSObject arg)
        {
            if (arg == null)
                return 0;
            switch (arg.ValueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        return arg;
                    }
                case JSObjectType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (arg.oValue as string).Trim();
                        if (!Tools.ParseNumber(s, ref ix, true, out x) || ix < s.Length)
                            return 0;
                        return x;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (arg.oValue == null)
                            return 0;
                        arg = arg.ToPrimitiveValue_Value_String();
                        return JSObjectToNumber(arg);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return 0;
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }

        internal static string DoubleToString(double d)
        {
            if (0.0 == d || double.IsInfinity(d) || double.IsNaN(d))
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (Math.Abs(d) < 1.0)
            {
                if (d == (d % 0.000001))
                    return d.ToString("0.####e-0", System.Globalization.CultureInfo.InvariantCulture);
                if (d == (d % 0.001))
                {
                    var t = Math.Sign(d);
                    d = Math.Abs(d);
                    if (t < 0)
                        return d.ToString("-0.##########", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            else if (Math.Abs(d) >= 1e+21)
                return d.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            return d.ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool ParseNumber(string code, ref int index, bool move, out double value)
        {
            return ParseNumber(code, ref index, move, out value, 0, false);
        }

        public static bool ParseNumber(string code, ref int index, bool move, out double value, int radix, bool allowOctal)
        {
            value = double.NaN;
            if (radix != 0 && (radix < 2 || radix > 36))
                return true;
            if (code.Length == 0)
            {
                value = 0.0;
                return true;
            }
            int i = index;
            while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
            if (i == code.Length)
            {
                value = 0.0;
                return true;
            }
            int sig = 1;
            if (code[i] == '-' || code[i] == '+')
                sig = 44 - code[i++];
            const string infinity = "Infinity";
            for (int j = i; ((j - i) < infinity.Length) && (j < code.Length); j++)
            {
                if (code[j] != infinity[j - i])
                    break;
                else if (code[j] == 'y')
                {
                    if (move)
                        index = j + 1;
                    value = sig * double.PositiveInfinity;
                    return true;
                }
            }
            bool res = false;
            if (i + 1 < code.Length)
            {
                if ((radix == 0 || radix == 16) && (code[i] == '0') && (code[i + 1] == 'x' || code[i + 1] == 'X'))
                {
                    i += 2;
                    radix = 16;
                }
                else if (allowOctal && (radix == 0 && code[i] == '0' && char.IsDigit(code[i + 1])))
                {
                    i += 1;
                    radix = 8;
                    res = true;
                }
            }
            if (radix == 0)
            {
                long temp = 0;
                int scount = 0;
                int deg = 0;
                while (i < code.Length)
                {
                    if (!char.IsDigit(code[i]))
                        break;
                    else
                    {
                        if (scount <= 18)
                        {
                            temp = temp * 10 + (code[i++] - '0');
                            scount++;
                        }
                        else
                        {
                            deg++;
                            i++;
                        }
                        res = true;
                    }
                }
                if (!res && (i >= code.Length || code[i] != '.'))
                    return false;
                if (i < code.Length && code[i] == '.')
                {
                    i++;
                    while (i < code.Length)
                    {
                        if (!char.IsDigit(code[i]))
                            break;
                        else
                        {
                            if (scount <= 18)
                            {
                                temp = temp * 10 + (code[i++] - '0');
                                scount++;
                                deg--;
                            }
                            else
                                i++;
                            res = true;
                        }
                    }
                }
                if (!res)
                    return false;
                if (i < code.Length && (code[i] == 'e' || code[i] == 'E'))
                {
                    i++;
                    int td = 0;
                    int esign = code[i] >= 43 && code[i] <= 45 ? 44 - code[i++] : 1;
                    while (i < code.Length)
                    {
                        if (!char.IsDigit(code[i]))
                            break;
                        else
                        {
                            if (scount <= 18)
                                td = td * 10 + (code[i++] - '0');
                        }
                    }
                    deg += td * esign;
                }
                if (deg != 0)
                {
                    if (deg < 0)
                    {
                        if (deg < -16)
                        {
                            value = (double)((decimal)temp * 0.0000000000000001M);
                            deg += 16;
                        }
                        else
                        {
                            value = (double)((decimal)temp * (decimal)Math.Pow(10.0, deg));
                            deg = 0;
                        }
                    }
                    else
                    {
                        if (deg > 10)
                        {
                            value = (double)((decimal)temp * 10000000000M);
                            deg -= 10;
                        }
                        else
                        {
                            value = (double)((decimal)temp * (decimal)Math.Pow(10.0, deg));
                            deg = 0;
                        }
                    }
                    if (deg != 0)
                    {
                        var exp = Math.Pow(10.0, deg);
                        value *= exp;
                    }
                }
                else
                    value = temp;
                value *= sig;
                if (move)
                    index = i;
                return true;
            }
            else
            {
                value = 0;
                long temp = 0;
                int starti = i;
                while (i < code.Length)
                {
                    var sign = ((code[i] % 'a' % 'A' + 10) % ('0' + 10));
                    if (sign >= radix || (NumChars[sign] != code[i] && (NumChars[sign] - ('a' - 'A')) != code[i]))
                    {
                        if (radix < 10 && i - starti > 1)
                        {
                            i = starti;
                            temp = 0;
                            radix = 10;
                        }
                        else
                            break;
                    }
                    else
                    {
                        temp = temp * radix + sign;
                        res = true;
                    }
                    i++;
                }
                if (!res)
                {
                    value = double.NaN;
                    return false;
                }
                value = temp;
                value *= sig;
                if (move)
                    index = i;
                return true;
            }
        }

        public static string Unescape(string code)
        {
            return Unescape(code, true);
        }

        public static string Unescape(string code, bool defaultUnescape)
        {
            StringBuilder res = new StringBuilder(code.Length);
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\\')
                {
                    i++;
                    switch (code[i])
                    {
                        case 'x':
                        case 'u':
                            {
                                if (i + (code[i] == 'u' ? 5 : 3) > code.Length)
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid escape code (\"" + code + "\")")));
                                string c = code.Substring(i + 1, code[i] == 'u' ? 4 : 2);
                                ushort chc = 0;
                                if (ushort.TryParse(c, System.Globalization.NumberStyles.HexNumber, null, out chc))
                                {
                                    char ch = (char)chc;
                                    res.Append(ch);
                                    i += c.Length;
                                }
                                else
                                {
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid escape sequence '\\" + code[i] + c + "'")));
                                    //res.Append(code[i - 1]);
                                    //res.Append(code[i]);
                                }
                                break;
                            }
                        case 't':
                            {
                                res.Append('\t');
                                break;
                            }
                        case 'f':
                            {
                                res.Append('\f');
                                break;
                            }
                        case 'v':
                            {
                                res.Append('\v');
                                break;
                            }
                        case 'b':
                            {
                                res.Append('\b');
                                break;
                            }
                        case 'n':
                            {
                                res.Append('\n');
                                break;
                            }
                        case 'r':
                            {
                                res.Append('\r');
                                break;
                            }
                        default:
                            {
                                if (char.IsDigit(code[i]))
                                {
                                    var ccode = code[i] - '0';
                                    if (i + 1 < code.Length && char.IsDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    if (i + 1 < code.Length && char.IsDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    res.Append((char)ccode);
                                }
                                else if (defaultUnescape)
                                    res.Append(code[i]);
                                else
                                {
                                    res.Append('\\');
                                    res.Append(code[i]);
                                }
                                break;
                            }
                    }
                }
                else
                    res.Append(code[i]);
            }
            return res.ToString();
        }

        internal static bool isLineTerminator(char c)
        {
            return (c == '\u000A') || (c == '\u000D') || (c == '\u2028') || (c == '\u2029');
        }

        internal static void skipComment(string code, ref int index, bool skipSpaces)
        {
            bool work;
            do
            {
                if (code.Length <= index)
                    return;
                work = false;
                if (code[index] == '/')
                {
                    switch (code[index + 1])
                    {
                        case '/':
                            {
                                index += 2;
                                while (index < code.Length && !Tools.isLineTerminator(code[index])) index++;
                                while (index < code.Length && char.IsWhiteSpace(code[index])) index++;
                                work = true;
                                break;
                            }
                        case '*':
                            {
                                index += 2;
                                while (code[index] != '*' || code[index + 1] != '/')
                                    index++;
                                index += 2;
                                work = true;
                                break;
                            }
                    }
                }
            } while (work);
            if (skipSpaces)
                while ((index < code.Length) && (char.IsWhiteSpace(code[index]))) index++;
        }

        internal static string RemoveComments(string code)
        {
            StringBuilder res = new StringBuilder(code.Length);
            for (int i = 0; i < code.Length; )
            {
                while (i < code.Length && char.IsWhiteSpace(code[i])) res.Append(code[i++]);
                var s = i;
                skipComment(code, ref i, false);
                for (; s < i; s++)
                    res.Append(' ');
                if (i >= code.Length)
                    continue;
                if (Parser.ValidateName(code, ref i, true)
                    || Parser.ValidateNumber(code, ref i, true)
                    || Parser.ValidateRegex(code, ref i, true, false)
                    || Parser.ValidateString(code, ref i, true))
                {
                    for (; s < i; s++)
                        res.Append(code[s]);
                }
                else
                    res.Append(code[i++]);
            }
            return res.ToString();
        }

        internal static JSObject RaiseIfNotExist(JSObject obj)
        {
            if (obj != null && obj.ValueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible is not defined.")));
            return obj;
        }

        internal sealed class TextCord
        {
            internal int line;
            internal int column;

            public override string ToString()
            {
                return "(" + line + ": " + column + ")"; 
            }
        }

        internal static TextCord PositionToTextcord(string text, int position)
        {
            int line = 1;
            int column = 1;
            for (int i = 0; i < position; i++)
            {
                if (text[i] == '\n')
                {
                    column = 0;
                    line++;
                    if (text[i + 1] == '\r')
                        i++;
                }
                else if (text[i] == '\r')
                {
                    column = 0;
                    line++;
                    if (text[i + 1] == '\n')
                        i++;
                }
                column++;
            }
            return new TextCord() { line = line, column = column };
        }
    }
}
