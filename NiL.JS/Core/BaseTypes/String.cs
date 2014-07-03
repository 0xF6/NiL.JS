﻿using System;
using System.Globalization;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class String : JSObject
    {
        [Serializable]
        private sealed class StringAllowUnsafeCallAttribute : AllowUnsafeCallAttribute
        {
            public StringAllowUnsafeCallAttribute()
                : base(typeof(JSObject))
            { }

            protected internal override object Convert(object arg)
            {
                if (arg is JSObject && (arg as JSObject).valueType == JSObjectType.String)
                    return arg;
                return new String(arg.ToString());
            }
        }

        [DoNotEnumerate]
        public static JSObject fromCharCode(JSObject[] code)
        {
            int chc = 0;
            if (code == null || code.Length == 0)
                return new String();
            string res = "";
            for (int i = 0; i < code.Length; i++)
            {
                chc = Tools.JSObjectToInt32(code[i]);
                res += ((char)chc).ToString();
            }
            return res;
        }

        [DoNotEnumerate]
        public String()
            : this("")
        {
        }

        [DoNotEnumerate]
        public String(JSObject args)
            : this(Tools.JSObjectToInt32(args.GetMember("length")) == 0 ? "" : args.GetMember("0").ToString())
        {
        }

        [DoNotEnumerate]
        public String(string s)
        {
            oValue = s;
            valueType = JSObjectType.String;
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                throw new InvalidOperationException("Try to assign to String");
        }

        [Hidden]
        public JSObject this[int pos]
        {
            [Hidden]
            get
            {
                if ((pos < 0) || (pos >= (oValue as string).Length))
                {
                    notExist.valueType = JSObjectType.NotExistInObject;
                    return JSObject.notExist;
                }
                return new JSObject(false) { valueType = JSObjectType.String, oValue = (oValue as string)[pos].ToString(), attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete };
            }
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public String charAt(JSObject pos)
        {
            int p = Tools.JSObjectToInt32(pos.GetMember("0"));
            if ((p < 0) || (p >= (oValue as string).Length))
                return "";
            return (oValue as string)[p].ToString();
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public Number charCodeAt(JSObject pos)
        {
            int p = Tools.JSObjectToInt32(pos.GetMember("0"));
            if ((p < 0) || (p >= (oValue as string).Length))
                return double.NaN;
            return (int)(oValue as string)[p];
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject concat(JSObject[] args)
        {
            string res = oValue.ToString();
            for (var i = 0; i < args.Length; i++)
                res += args[i].ToString();
            return res;
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject indexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].ToString();
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), pos, out d, Tools.ParseNumberOptions.Default);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).IndexOf(fstr, pos, StringComparison.CurrentCulture);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject lastIndexOf(JSObject[] args)
        {
            if (args.Length == 0)
                return -1;
            string fstr = args[0].ToString();
            int pos = 0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), pos, out d, Tools.ParseNumberOptions.Default);
                            pos = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).LastIndexOf(fstr, (oValue as string).Length, StringComparison.CurrentCulture);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject localeCompare(JSObject[] args)
        {
            string str0 = oValue.ToString();
            string str1 = args.Length > 0 ? args[0].ToString() : "";
            return string.CompareOrdinal(str0, str1);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject match(JSObject args)
        {
            if (valueType <= JSObjectType.Undefined || (valueType >= JSObjectType.Object && oValue == null))
                throw new JSException(new TypeError("String.prototype.match called on null or undefined"));
            var a0 = args.GetMember("0");
            if (a0.valueType == JSObjectType.Object && a0.oValue is RegExp)
            {
                var regex = a0.oValue as RegExp;
                if (!regex._global)
                {
                    args.GetMember("0", true, true).Assign(this);
                    return regex.exec(args);
                }
                else
                {
                    var groups = regex.regEx.Match(oValue as string ?? this.ToString()).Groups;
                    var res = new Array(groups.Count);
                    for (int i = 0; i < groups.Count; i++)
                        res.data[(uint)i] = groups[i].Value;
                    return res;
                }
            }
            else
            {
                var match = new System.Text.RegularExpressions.Regex((a0.valueType > JSObjectType.Undefined ? (object)a0 : "").ToString(), System.Text.RegularExpressions.RegexOptions.ECMAScript).Match(oValue as string ?? this.ToString());
                var res = new Array(match.Groups.Count);
                for (int i = 0; i < match.Groups.Count; i++)
                    res.data[(uint)i] = match.Groups[i].Value;
                res.GetMember("index", true, true).Assign(match.Index);
                res.GetMember("input", true, true).Assign(this);
                return res;
            }
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject search(JSObject args)
        {
            if (valueType <= JSObjectType.Undefined || (valueType >= JSObjectType.Object && oValue == null))
                throw new JSException(new TypeError("String.prototype.match called on null or undefined"));
            var a0 = args.GetMember("0");
            if (a0.valueType == JSObjectType.Object && a0.oValue is RegExp)
            {
                var regex = a0.oValue as RegExp;
                if (!regex._global)
                {
                    args.GetMember("0", true, true).Assign(this);
                    return regex.exec(args)["index"];
                }
                else
                {
                    return regex.regEx.Match(oValue as string ?? this.ToString()).Index;
                }
            }
            else
            {
                var match = new System.Text.RegularExpressions.Regex((a0.valueType > JSObjectType.Undefined ? (object)a0 : "").ToString(), System.Text.RegularExpressions.RegexOptions.ECMAScript).Match(oValue as string ?? this.ToString());
                return match.Index;
            }
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject replace(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            if (args[0].valueType == JSObjectType.Object && args[0].oValue is RegExp)
            {
                if (args.Length > 1 && args[1].oValue is Function)
                {
                    var oac = assignCallback;
                    assignCallback = null;
                    string temp = this.oValue as string;
                    var f = args[1].oValue as Function;
                    var match = new String();
                    match.assignCallback = null;
                    var margs = CreateObject();
                    JSObject len = 1;
                    len.assignCallback = null;
                    len.attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly;
                    margs.fields["length"] = len;
                    margs.fields["0"] = match;
                    match.oValue = (args[0].oValue as RegExp).regEx.Replace(oValue.ToString(), new System.Text.RegularExpressions.MatchEvaluator(
                        (m) =>
                        {
                            this.oValue = temp;
                            this.valueType = JSObjectType.String;
                            len.iValue = 1 + m.Groups.Count + 1 + 1;
                            match.oValue = m.Value;
                            JSObject t = m.Index;
                            for (int i = 0; i < m.Groups.Count; i++)
                            {
                                t = m.Groups[i].Value;
                                t.assignCallback = null;
                                margs.fields[(i + 1).ToString(CultureInfo.InvariantCulture)] = t;
                            }
                            t = m.Index;
                            t.assignCallback = null;
                            margs.fields[(len.iValue - 2).ToString()] = t;
                            margs.fields[(len.iValue - 1).ToString()] = this;
                            return f.Invoke(margs).ToString();
                        }), (args[0].oValue as RegExp)._global ? int.MaxValue : 1);
                    this.oValue = temp;
                    this.valueType = JSObjectType.String;
                    assignCallback = oac;
                    return match;
                }
                else
                {
                    return (args[0].oValue as RegExp).regEx.Replace(oValue.ToString(), args.Length > 1 ? args[1].ToString() : "undefined", (args[0].oValue as RegExp)._global ? int.MaxValue : 1);
                }
            }
            else
            {
                string pattern = args.Length > 0 ? args[0].ToString() : "";
                if (args.Length > 1 && args[1].oValue is Function)
                {
                    var oac = assignCallback;
                    assignCallback = null;
                    string othis = this.oValue as string;
                    var f = args[1].oValue as Function;
                    var margs = CreateObject();
                    margs.oValue = Arguments.Instance;
                    JSObject alen = 3;
                    alen.assignCallback = null;
                    alen.attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly;
                    margs.fields["length"] = alen;
                    margs.fields["0"] = pattern;
                    margs.fields["2"] = this;
                    int index = oValue.ToString().IndexOf(pattern);
                    if (index == -1)
                        return this;
                    margs.fields["1"] = index;
                    var res = othis.Substring(0, index) + f.Invoke(margs).ToString() + othis.Substring(index + pattern.Length);
                    oValue = othis;
                    valueType = JSObjectType.String;
                    assignCallback = oac;
                    return res;
                }
                else
                {
                    string replace = args.Length > 1 ? args[1].ToString() : "undefined";
                    if (string.IsNullOrEmpty(pattern))
                        return replace + oValue;
                    return oValue.ToString().Replace(pattern, replace);
                }
            }
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject slice(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            int pos0 = 0;
            switch (args[0].valueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    {
                        pos0 = args[0].iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        pos0 = (int)args[0].dValue;
                        break;
                    }
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.String:
                    {
                        double d;
                        Tools.ParseNumber(args[0].ToString(), pos0, out d, Tools.ParseNumberOptions.Default);
                        pos0 = (int)d;
                        break;
                    }
            }
            int pos1 = 0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos1 = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos1 = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), pos1, out d, Tools.ParseNumberOptions.Default);
                            pos1 = (int)d;
                            break;
                        }
                }
            }
            else
                pos1 = (oValue as string).Length;
            return (oValue as string).Substring(pos0, pos1 - pos0);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject split(JSObject[] args)
        {
            if (args.Length == 0)
                return new Array(new object[] { this });
            string fstr = args[0].ToString();
            int limit = int.MaxValue;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            limit = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            limit = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), limit, out d, Tools.ParseNumberOptions.Default);
                            limit = (int)d;
                            break;
                        }
                }
            }
            string[] res = null;
            if (string.IsNullOrEmpty(fstr))
                return new Array(System.Text.UTF8Encoding.UTF8.GetChars(System.Text.UTF8Encoding.UTF8.GetBytes(oValue as string)));
            else
                res = (oValue as string).Split(new string[] { fstr }, limit, StringSplitOptions.None);
            return new Array(res);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject substring(JSObject[] args)
        {
            return slice(args);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject substr(JSObject[] args)
        {
            if (args.Length == 0)
                return this;
            int pos0 = 0;
            if (args.Length > 0)
            {
                switch (args[0].valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            pos0 = args[0].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            pos0 = (int)args[0].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[0].ToString(), pos0, out d, Tools.ParseNumberOptions.Default);
                            pos0 = (int)d;
                            break;
                        }
                }
            }
            int len = (oValue as string).Length - pos0;
            if (args.Length > 1)
            {
                switch (args[1].valueType)
                {
                    case JSObjectType.Int:
                    case JSObjectType.Bool:
                        {
                            len = args[1].iValue;
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            len = (int)args[1].dValue;
                            break;
                        }
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.String:
                        {
                            double d;
                            Tools.ParseNumber(args[1].ToString(), len, out d, Tools.ParseNumberOptions.Default);
                            len = (int)d;
                            break;
                        }
                }
            }
            return (oValue as string).Substring(pos0, len);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject toLocaleLowerCase()
        {
            return (oValue as string).ToLower(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject toLocaleUpperCase()
        {
            return (oValue as string).ToUpper(System.Threading.Thread.CurrentThread.CurrentUICulture);
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject toLowerCase()
        {
            return (oValue as string).ToLowerInvariant();
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject toUpperCase()
        {
            return (oValue as string).ToUpperInvariant();
        }

        [StringAllowUnsafeCallAttribute()]
        [DoNotEnumerate]
        public JSObject trim()
        {
            return (oValue as string).Trim();
        }

        [CLSCompliant(false)]
        [AllowUnsafeCall(typeof(JSObject))]
        [ParametersCount(0)]
        [DoNotEnumerate]
        public new JSObject toString(JSObject args)
        {
            if (typeof(String) == this.GetType() && valueType == JSObjectType.Object) // prototype instance
                return "";
            if (this.valueType == JSObjectType.String)
                return this;
            else
                throw new JSException(new TypeError("Try to call String.toString for not string object."));
        }

        [DoNotEnumerate]
        [StringAllowUnsafeCall]
        public override JSObject valueOf()
        {
            if (typeof(String) == this.GetType() && valueType == JSObjectType.Object) // prototype instance
                return "";
            if (this.valueType == JSObjectType.String)
                return this;
            else
                throw new JSException(new TypeError("Try to call String.valueOf for not string object."));
        }

        private Number _length = null;

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public JSObject length
        {
            [StringAllowUnsafeCallAttribute()]
            [Hidden]
            get
            {
                if (this.GetType() == typeof(String))
                {
                    if (_length == null)
                        _length = new Number((oValue as string).Length) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum };
                    else
                        _length.iValue = (oValue as string).Length;
                    return _length;
                }
                else
                    return (oValue as string).Length;
            }
        }

        [Hidden]
        public override string ToString()
        {
            if (this.GetType() == typeof(String))
                return oValue as string;
            else
                throw new JSException(new TypeError("Try to call String.toString for not string object."));
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            if (obj is String)
                return oValue.Equals((obj as String).oValue);
            return false;
        }

        [Hidden]
        public override int GetHashCode()
        {
            return oValue.GetHashCode();
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool create, bool own)
        {
            create &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(typeof(String));
            int index = 0;
            double dindex = Tools.JSObjectToDouble(name);
            if (!double.IsInfinity(dindex)
                && !double.IsNaN(dindex)
                && ((index = (int)dindex) == dindex)
                && ((index = (int)dindex) == dindex)
                && index < (oValue as string).Length
                && index >= 0)
            {
                return this[index];
            }
            return DefaultFieldGetter(name, create, own); // обращение идёт к Объекту String, а не к значению string, поэтому члены создавать можно
        }

        #region HTML Wraping
        [DoNotEnumerate]
        public JSObject anchor(JSObject arg)
        {
            return "<a name=\"" + arg.Value + "\">" + oValue + "</a>";
        }

        [DoNotEnumerate]
        public JSObject big()
        {
            return "<big>" + oValue + "</big>";
        }

        [DoNotEnumerate]
        public JSObject blink()
        {
            return "<blink>" + oValue + "</blink>";
        }

        [DoNotEnumerate]
        public JSObject bold()
        {
            return "<bold>" + oValue + "</bold>";
        }

        [DoNotEnumerate]
        public JSObject @fixed()
        {
            return "<tt>" + oValue + "</tt>";
        }

        [DoNotEnumerate]
        public JSObject fontcolor(JSObject arg)
        {
            return "<font color=\"" + arg.Value + "\">" + oValue + "</font>";
        }

        [DoNotEnumerate]
        public JSObject fontsize(JSObject arg)
        {
            return "<font size=\"" + arg.Value + "\">" + oValue + "</font>";
        }

        [DoNotEnumerate]
        public JSObject italics()
        {
            return "<i>" + oValue + "</i>";
        }

        [DoNotEnumerate]
        public JSObject link(JSObject arg)
        {
            return "<a href=\"" + arg.Value + "\">" + oValue + "</a>";
        }

        [DoNotEnumerate]
        public JSObject small()
        {
            return "<small>" + oValue + "</small>";
        }

        [DoNotEnumerate]
        public JSObject strike()
        {
            return "<strike>" + oValue + "</strike>";
        }

        [DoNotEnumerate]
        public JSObject sub()
        {
            return "<sub>" + oValue + "</sub>";
        }

        [DoNotEnumerate]
        public JSObject sup()
        {
            return "<sup>" + oValue + "</sup>";
        }
        #endregion

        protected internal override System.Collections.Generic.IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (!hideNonEnum)
            {
                var len = (oValue as string).Length;
                for (var i = 0; i < len; i++)
                    yield return i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture);
                yield return "length";
            }
            for (var e = base.GetEnumeratorImpl(hideNonEnum); e.MoveNext(); )
                yield return e.Current;
        }

        [Hidden]
        public static implicit operator String(string val)
        {
            return new String(val);
        }
    }
}