﻿using System;
using System.Text;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    /// <summary>
    /// Содержит функции, используемые на разных этапах выполнения скрипта.
    /// </summary>
    public static class Tools
    {
        [Flags]
        public enum ParseNumberOptions
        {
            None = 0,
            RaiseIfOctal = 1,
            ProcessOctal = 2,
            AllowFloat = 4,
            AllowAutoRadix = 8,
            Default = 2 + 4 + 8
        }

        internal static readonly char[] NumChars = new[] 
        { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' 
        };
        internal static readonly string[] NumString = new[] 
		{ 
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15"
		};
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static double JSObjectToDouble(JSObject arg)
        {
            if (arg == null)
                return double.NaN;
            switch (arg.valueType)
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
                        if (Tools.ParseNumber(s, ref ix, out x) && ix < s.Length)
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
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt(JSObject arg)
        {
            return JSObjectToInt(arg, 0, false);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="alternateInfinity">Если истина, для значений +Infinity и -Infinity будут возвращены значения int.MaxValue и int.MinValue соответственно.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt(JSObject arg, bool alternateInfinity)
        {
            return JSObjectToInt(arg, 0, alternateInfinity);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndef">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt(JSObject arg, int nullOrUndef)
        {
            return JSObjectToInt(arg, nullOrUndef, false);
        }

        /// <summary>
        /// Преобразует JSObject в значение типа integer.
        /// </summary>
        /// <param name="arg">JSObject, значение которого нужно преобразовать.</param>
        /// <param name="nullOrUndef">Значение, которое будет возвращено, если значение arg null или undefined.</param>
        /// <param name="alternateInfinity">Если истина, для значений +Infinity и -Infinity будут возвращены значения int.MaxValue и int.MinValue соответственно.</param>
        /// <returns>Целочисленное значение, представленное в объекте arg.</returns>
#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int JSObjectToInt(JSObject arg, int nullOrUndef, bool alternateInfinity)
        {
            if (arg == null)
                return nullOrUndef;
            var r = arg;
            switch (r.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                    {
                        return r.iValue;
                    }
                case JSObjectType.Double:
                    {
                        if (double.IsNaN(r.dValue))
                            return 0;
                        if (double.IsInfinity(r.dValue))
                            return alternateInfinity ? double.IsPositiveInfinity(r.dValue) ? int.MaxValue : int.MinValue : 0;
                        return (int)((long)r.dValue & 0xFFFFFFFF);
                    }
                case JSObjectType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (r.oValue as string).Trim();
                        if (!Tools.ParseNumber(s, ref ix, out x) || ix < s.Length)
                            return 0;
                        return (int)x;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object:
                    {
                        if (r.oValue == null)
                            return nullOrUndef;
                        r = r.ToPrimitiveValue_Value_String();
                        return JSObjectToInt(r);
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return nullOrUndef;
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static JSObject JSObjectToNumber(JSObject arg)
        {
            if (arg == null)
                return 0;
            switch (arg.valueType)
            {
                case JSObjectType.Bool:
                case JSObjectType.Int:
                case JSObjectType.Double:
                    return arg;
                case JSObjectType.String:
                    {
                        double x = 0;
                        int ix = 0;
                        string s = (arg.oValue as string).Trim();
                        if (!Tools.ParseNumber(s, ref ix, out x) || ix < s.Length)
                            return Number.NaN;
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
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object convertJStoObj(JSObject jsobj, Type targetType)
        {
            if (jsobj == null)
                return null;
            if (targetType.IsAssignableFrom(jsobj.GetType()))
                return jsobj;
            var res = targetType.IsValueType || jsobj.valueType >= JSObjectType.String ? jsobj.Value : null;
            if (res == null)
                return res;
            if ((targetType.IsEnum && Enum.IsDefined(targetType, res)) || targetType.IsAssignableFrom(res.GetType()))
                return res;
            if (res is TypeProxy && targetType.IsAssignableFrom((res as TypeProxy).hostedType))
                return (res as TypeProxy).prototypeInstance;
            return null;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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

        internal static bool ParseNumber(string code, out double value, int radix)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, radix, ParseNumberOptions.Default);
        }

        internal static bool ParseNumber(string code, out double value, ParseNumberOptions options)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, 0, options);
        }

        internal static bool ParseNumber(string code, out double value, int radix, ParseNumberOptions options)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, radix, options);
        }

        internal static bool ParseNumber(string code, ref int index, out double value)
        {
            return ParseNumber(code, ref index, out value, 0, ParseNumberOptions.Default);
        }

        internal static bool ParseNumber(string code, int index, out double value, ParseNumberOptions options)
        {
            return ParseNumber(code, ref index, out value, 0, options);
        }

        private static bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        internal static bool ParseNumber(string code, ref int index, out double value, int radix, ParseNumberOptions options)
        {
            bool raiseOctal = (options & ParseNumberOptions.RaiseIfOctal) != 0;
            bool processOctal = (options & ParseNumberOptions.ProcessOctal) != 0;
            bool allowAutoRadix = (options & ParseNumberOptions.AllowAutoRadix) != 0;
            bool allowFloat = (options & ParseNumberOptions.AllowFloat) != 0;
            if (code == null)
                throw new ArgumentNullException("code");
            value = double.NaN;
            if (radix != 0 && (radix < 2 || radix > 36))
                return false;
            if (code.Length == 0)
            {
                value = 0;
                return true;
            }
            int i = index;
            while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
            if (i == code.Length)
            {
                value = 0.0;
                return true;
            }
            const string nan = "NaN";
            for (int j = i; ((j - i) < nan.Length) && (j < code.Length); j++)
            {
                if (code[j] != nan[j - i])
                    break;
                else if (j > i && code[j] == 'N')
                {
                    index = j + 1;
                    value = double.NaN;
                    return true;
                }
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
                    index = j + 1;
                    value = sig * double.PositiveInfinity;
                    return true;
                }
            }
            bool res = false;
            bool skiped = false;
            if (allowAutoRadix && i + 1 < code.Length)
            {
                while (code[i] == '0' && i + 1 < code.Length && code[i + 1] == '0')
                {
                    skiped = true;
                    i++;
                }
                if ((radix == 0 || radix == 16) && (code[i] == '0') && (code[i + 1] == 'x' || code[i + 1] == 'X'))
                {
                    i += 2;
                    radix = 16;
                }
                else if (radix == 0 && code[i] == '0' && isDigit(code[i + 1]))
                {
                    if (raiseOctal)
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Octal literals not allowed in strict mode")));
                    i += 1;
                    if (processOctal)
                        radix = 8;
                    res = true;
                }
            }
            if (allowFloat && radix == 0)
            {
                long temp = 0;
                int scount = 0;
                int deg = 0;
                while (i < code.Length)
                {
                    if (!isDigit(code[i]))
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
                        if (!isDigit(code[i]))
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
                    scount = 0;
                    while (i < code.Length)
                    {
                        if (!isDigit(code[i]))
                            break;
                        else
                        {
                            if (scount <= 18)
                                td = td * 10 + (code[i++] - '0');
                            else
                                i++;
                        }
                    }
                    deg += td * esign;
                }
                if (deg != 0)
                {
                    if (deg < 0)
                    {
                        if (deg < -18)
                        {
                            value = (double)((decimal)temp * 0.000000000000000001M);
                            deg += 18;
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
                        if (deg == -324)
                        {
                            value *= 1e-323;
                            value *= 0.1;
                        }
                        else
                        {
                            var exp = Math.Pow(10.0, deg);
                            value *= exp;
                        }
                    }
                }
                else
                    value = temp;
                if (value == 0 && skiped)
                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Octal literals not allowed in strict mode")));
                value *= sig;
                index = i;
                return true;
            }
            else
            {
                if (radix == 0)
                    radix = 10;
                value = 0;
                bool extended = false;
                double doubleTemp = 0.0;
                ulong temp = 0;
                int starti = i;
                while (i < code.Length)
                {
                    var sign = ((code[i] % 'a' % 'A' + 10) % ('0' + 10));
                    if (sign >= radix || (NumChars[sign] != code[i] && (NumChars[sign] - ('a' - 'A')) != code[i]))
                    {
                        break;
                    }
                    else
                    {
                        if (extended)
                            doubleTemp = doubleTemp * radix + sign;
                        else
                        {
                            temp = temp * (uint)radix + (uint)sign;
                            if ((temp & 0xFE00000000000000) != 0)
                            {
                                extended = true;
                                doubleTemp = temp;
                            }
                        }
                        res = true;
                    }
                    i++;
                }
                if (!res)
                {
                    value = double.NaN;
                    return false;
                }
                value = extended ? doubleTemp : temp;
                value *= sig;
                index = i;
                return true;
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string Unescape(string code, bool strict)
        {
            return Unescape(code, strict, true);
        }

        public static string Unescape(string code, bool strict, bool processUnknown)
        {
            if (code == null)
                throw new ArgumentNullException("code");
            StringBuilder res = null;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\\' && i + 1 < code.Length)
                {
                    if (res == null)
                    {
                        res = new StringBuilder(code.Length);
                        for (var j = 0; j < i; j++)
                            res.Append(code[j]);
                    }
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
                                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid escape sequence '\\" + code[i] + c + "'")));
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
                                if (isDigit(code[i]))
                                {
                                    if (strict)
                                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Octal literals are not allowed in strict mode.")));
                                    var ccode = code[i] - '0';
                                    if (i + 1 < code.Length && isDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    if (i + 1 < code.Length && isDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    res.Append((char)ccode);
                                }
                                else if (processUnknown)
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
                else if (res != null)
                    res.Append(code[i]);
            }
            return (res as object ?? code).ToString();
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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
                if (code[index] == '/' && index + 1 < code.Length)
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
                                while (index + 1 < code.Length && (code[index] != '*' || code[index + 1] != '/'))
                                    index++;
                                if (index + 1 >= code.Length)
                                    throw new JSException(new SyntaxError("Unexpected end of source."));
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
            StringBuilder res = null;// new StringBuilder(code.Length);
            for (int i = 0; i < code.Length; )
            {
                while (i < code.Length && char.IsWhiteSpace(code[i]))
                    if (res != null)
                        res.Append(code[i++]);
                    else i++;
                var s = i;
                skipComment(code, ref i, false);
                if (s != i && res == null)
                {
                    res = new StringBuilder(code.Length);
                    for (var j = 0; j < s; j++)
                        res.Append(code[j]);
                }
                for (; s < i; s++)
                    res.Append(' ');
                if (i >= code.Length)
                    continue;
                if (Parser.ValidateName(code, ref i, false)
                    || Parser.ValidateNumber(code, ref i)
                    || Parser.ValidateRegex(code, ref i, false)
                    || Parser.ValidateString(code, ref i))
                {
                    if (res != null)
                        for (; s < i; s++)
                            res.Append(code[s]);
                }
                else if (res != null)
                    res.Append(code[i++]);
                else i++;
            }
            return (res as object ?? code).ToString();
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static JSObject RaiseIfNotExist(JSObject obj)
        {
            if (obj != null && obj.valueType == JSObjectType.NotExist)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable \"" + obj.lastRequestedName + "\" is not defined.")));
            return obj;
        }

        public sealed class TextCord
        {
            public int Line { get; private set; }
            public int Column { get; private set; }

            public TextCord(int line, int column)
            {
                Line = line;
                Column = column;
            }

            public override string ToString()
            {
                return "(" + Line + ": " + Column + ")";
            }
        }

        public static TextCord PositionToTextcord(string text, int position)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (position < 0)
                throw new ArgumentOutOfRangeException("position");
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
            return new TextCord(line, column);
        }
    }
}
