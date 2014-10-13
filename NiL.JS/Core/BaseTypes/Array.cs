using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class Array : JSObject
    {
        private sealed class _lengthField : JSObject
        {
            private Array array;

            public _lengthField(Array owner)
            {
                attributes |= JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.NotConfigurable;
                array = owner;
                if ((long)(int)array._length == array._length)
                {
                    this.iValue = (int)array._length;
                    this.valueType = JSObjectType.Int;
                }
                else
                {
                    this.dValue = array._length;
                    this.valueType = JSObjectType.Double;
                }
            }

            public override void Assign(JSObject value)
            {
                var nlenD = Tools.JSObjectToDouble(value);
                var nlen = (uint)nlenD;
                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                    throw new JSException(new RangeError("Invalid array length"));
                if ((attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                    return;
                try
                {
                    if (!array.setLength(nlen))
                        throw new JSException(new TypeError("Unable to reduce length because not configurable elements"));
                }
                finally
                {
                    if ((long)(int)array._length == array._length)
                    {
                        this.iValue = (int)array._length;
                        this.valueType = JSObjectType.Int;
                    }
                    else
                    {
                        this.dValue = array._length;
                        this.valueType = JSObjectType.Double;
                    }
                }
            }
        }

        [Hidden]
        internal BinaryTree<long, JSObject> data;
        [Hidden]
        internal long _length;

        private Array(BinaryTree<long, JSObject> data, long length)
        {
            valueType = JSObjectType.Object;
            oValue = this;
            this.data = data;
            _length = length;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [DoNotEnumerate]
        public Array()
        {
            oValue = this;
            valueType = JSObjectType.Object;
            data = new BinaryTree<long, JSObject>();
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [DoNotEnumerate]
        public Array(int length)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (length < 0)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new BinaryTree<long, JSObject>();
            this._length = (long)length;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        public Array(long length)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (length < 0)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new BinaryTree<long, JSObject>();
            this._length = length;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [DoNotEnumerate]
        public Array(double d)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (((long)d != d) || (d < 0) || (d > 0xffffffff))
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array length.")));
            data = new BinaryTree<long, JSObject>();
            this._length = (long)d;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [DoNotEnumerate]
        public Array(Arguments args)
            : this(MethodProxy.argumentsToArray<object>(args))
        {

        }

        [Hidden]
        public Array(ICollection collection)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (collection == null)
                throw new ArgumentNullException("collection");
            data = new BinaryTree<long, JSObject>();
            this._length = (long)collection.Count;
            var index = 0U;
            foreach (var e in collection)
                data[index++] = e is JSObject ? (e as JSObject).CloneImpl() : TypeProxy.Proxy(e);
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [Hidden]
        public Array(IEnumerable enumerable)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");
            data = new BinaryTree<long, JSObject>();
            var index = 0U;
            foreach (var e in enumerable)
                data[index++] = e is JSObject ? (e as JSObject).CloneImpl() : TypeProxy.Proxy(e);
            _length = (long)index;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [Hidden]
        internal Array(IEnumerator enumerator)
        {
            oValue = this;
            valueType = JSObjectType.Object;
            if (enumerator == null)
                throw new ArgumentNullException("enumerator");
            data = new BinaryTree<long, JSObject>();
            var index = 0U;
            while (enumerator.MoveNext())
            {
                var e = enumerator.Current;
                data[index++] = e is JSObject ? (e as JSObject).CloneImpl() : TypeProxy.Proxy(e);
            }
            _length = (long)index;
            attributes |= JSObjectAttributesInternal.SystemObject;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            _lengthObj = new _lengthField(this);
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
                throw new InvalidOperationException("Try to assign to array");
        }

        [Hidden]
        public void Add(JSObject obj)
        {
            data.Add(_length++, obj);
        }

        private _lengthField _lengthObj;
        [Hidden]
        public JSObject length
        {
            [Hidden]
            get
            {
                if (_length <= int.MaxValue)
                {
                    _lengthObj.iValue = (int)_length;
                    _lengthObj.valueType = JSObjectType.Int;
                }
                else
                {
                    _lengthObj.dValue = _length > uint.MaxValue ? 0 : (uint)_length;
                    _lengthObj.valueType = JSObjectType.Double;
                }
                return _lengthObj;
            }
        }

        internal bool setLength(long nlen)
        {
            if (_length == nlen)
                return true;
            if (nlen < 0)
                throw new JSException(new RangeError("Invalid array length"));
            if (_length > nlen)
            {
                var res = true;
                foreach (var element in data.NotLess(nlen))
                    if (element.Value.valueType >= JSObjectType.Undefined && (element.Value.attributes & JSObjectAttributesInternal.DoNotDelete) != 0)
                    {
                        nlen = element.Key;
                        res = false;
                    }
                if (!res)
                {
                    setLength(nlen + 1); // ����������� �������� �� ����� ����.
                    return false;
                }
            }
            foreach (var i in data.NotLess(nlen))
                data.Remove(i.Key);
            //if (_length < nlen)
            //{
            //    for (var i = _length; i < nlen; i++)
            //        data[i] = __proto__[i.ToString()];
            //}
            _length = nlen;
            return true;
        }

        [DoNotEnumerateAttribute]
        public JSObject concat(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var res = new BinaryTree<long, JSObject>();
            long resLen = _length;
            foreach (var element in data)
                res[element.Key] = element.Value.CloneImpl();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.valueType == JSObjectType.Object && arg.oValue is Array)
                {
                    Array arr = arg.oValue as Array;
                    foreach (var element in arr.data)
                        res[element.Key + resLen] = element.Value.CloneImpl();
                    resLen += arr._length;
                }
                else
                    res[resLen++] = args[i].CloneImpl();
            }
            return new Array(res, resLen);
        }

        [DoNotEnumerate]
        public JSObject every(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = undefined;
            ao[1] = undefined;
            ao[2] = this;
            bool res = true;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                foreach (var element in data)
                {
                    if (element.Value.valueType == JSObjectType.Property)
                        ao[0].Assign((element.Value.oValue as Function[])[1] != null ? (element.Value.oValue as Function[])[1].Invoke(this, null) : undefined);
                    else
                        ao[0].Assign(element.Value);
                    ao[1].Assign(context.wrap(element.Key));
                    res &= (bool)f.Invoke(tb, ao);
                    if (!res)
                        break;
                }
            }
            else
            {
                var len = (long)Tools.JSObjectToDouble(this.length);
                for (var i = 0L; i < len && res; i++)
                {
                    var v = this[i.ToString(CultureInfo.InvariantCulture)];
                    if (v.valueType < JSObjectType.Undefined)
                        continue;
                    ao[0] = v;
                    ao[1].Assign(context.wrap(i));
                    res &= (bool)f.Invoke(tb, ao);
                }
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject some(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = undefined;
            ao[1] = undefined;
            ao[2] = this;
            bool res = false;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                foreach (var element in data)
                {
                    if (element.Value.valueType == JSObjectType.Property)
                        ao[0].Assign((element.Value.oValue as Function[])[1] != null ? (element.Value.oValue as Function[])[1].Invoke(this, null) : undefined);
                    else
                        ao[0].Assign(element.Value);
                    ao[1].Assign(context.wrap(element.Key));
                    res |= (bool)f.Invoke(tb, ao);
                    if (res)
                        break;
                }
            }
            else
            {
                var len = (long)Tools.JSObjectToDouble(this.length);
                for (var i = 0L; i < len && !res; i++)
                {
                    var v = this[i.ToString(CultureInfo.InvariantCulture)];
                    if (v.valueType < JSObjectType.Undefined)
                        continue;
                    ao[0] = v;
                    ao[1].Assign(context.wrap(i));
                    res |= (bool)f.Invoke(tb, ao);
                }
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject filter(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = undefined;
            ao[1] = undefined;
            ao[2] = this;
            var res = new Array();
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                foreach (var element in data)
                {
                    ao[0].Assign(element.Value);
                    ao[1].Assign(context.wrap(element.Key));
                    if ((bool)f.Invoke(tb, ao))
                        res.Add(element.Value.CloneImpl());
                }
            }
            else
            {
                var len = (long)Tools.JSObjectToDouble(this.length);
                for (var i = 0L; i < len; i++)
                {
                    var v = this[i.ToString(CultureInfo.InvariantCulture)];
                    if (v.valueType < JSObjectType.Undefined)
                        continue;
                    ao[0] = v;
                    ao[1].Assign(context.wrap(i));
                    if ((bool)f.Invoke(tb, ao))
                        res.Add(v.CloneImpl());
                }
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject map(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[0] = undefined;
            ao[1] = undefined;
            ao[2] = this;
            var res = new Array(_length);
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            int rindex = 0;
            if (isArray)
            {
                foreach (var element in data)
                {
                    if (element.Value.valueType == JSObjectType.Property)
                        ao[0].Assign((element.Value.oValue as Function[])[1] != null ? (element.Value.oValue as Function[])[1].Invoke(this, null) : undefined);
                    else
                        ao[0].Assign(element.Value);
                    ao[1].Assign(context.wrap(element.Key));
                    res._length = element.Key;
                    res.data[rindex++] = f.Invoke(tb, ao).CloneImpl();
                }
            }
            else
            {
                var len = (long)Tools.JSObjectToDouble(this.length);
                for (var i = 0L; i < len; i++)
                {
                    var v = this[i.ToString(CultureInfo.InvariantCulture)];
                    if (v.valueType < JSObjectType.Undefined)
                        continue;
                    ao[0] = v;
                    ao[1].Assign(context.wrap(i));
                    res.data[rindex++] = f.Invoke(tb, ao).CloneImpl();
                }
            }
            return res;
        }

        [DoNotEnumerate]
        public JSObject forEach(Arguments args)
        {
            if (args.Length < 1)
                return undefined;
            var f = args[0] == null ? null : args[0].oValue as Function;
            if (f == null)
                throw new JSException(new TypeError("Callback argument is not a function."));
            var tb = args.Length > 1 ? args[1] : null;
            var ao = new Arguments();
            ao.length = 3;
            ao[1] = 0;
            ao[2] = this;
            bool isArray = this.GetType() == typeof(Array);
            var context = Context.CurrentContext;
            if (isArray)
            {
                foreach (var element in data)
                {
                    ao[0].Assign(element.Value);
                    ao[1].Assign(context.wrap(element.Key));
                    f.Invoke(tb, ao);
                }
            }
            else
            {
                var len = (long)Tools.JSObjectToDouble(this.length);
                for (var i = 0L; i < len; i++)
                {
                    var v = this[i.ToString(CultureInfo.InvariantCulture)];
                    if (v.valueType < JSObjectType.Undefined)
                        continue;
                    ao[0] = v;
                    ao[1].Assign(context.wrap(i));
                    f.Invoke(tb, ao);
                }
            }
            return undefined;
        }

        [DoNotEnumerateAttribute]
        public JSObject indexOf(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return -1;
            var el = args[0];
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
            foreach (var element in data)
            {
                if (element.Value.isExist && Expressions.StrictEqual.Check(element.Value, el, null))
                    return Context.CurrentContext.wrap(element.Key);
            }
            return -1;
        }

        [DoNotEnumerateAttribute]
        public static JSObject isArray(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            return args[0].Value is Array || args[0].Value == TypeProxy.GetPrototype(typeof(Array));
        }

        [DoNotEnumerate]
        public JSObject join(Arguments separator)
        {
            //if (separator == null)
            //    throw new ArgumentNullException("separator");
            //if  separator.Length == 0)
            //    return ToString();
            var el = separator == null || separator.length == 0 || !separator[0].isDefinded ? "," : separator[0].ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            JSObject t;
            for (var i = 0U; i < _length - 1; i++)
            {
                if ((data.TryGetValue(i, out t) || null != (t = this.GetMember(i.ToString(), false, false))) && t.isDefinded)
                {
                    if (t.valueType < JSObjectType.String || t.oValue != null)
                        sb.Append(t);
                }
                sb.Append(el);
            }
            if ((data.TryGetValue(_length - 1, out t) || null != (t = this.GetMember((_length - 1).ToString(), false, false))) && t.isDefinded)
                if (t.valueType < JSObjectType.String || t.oValue != null)
                    sb.Append(t);
            return sb.ToString();
        }

        [DoNotEnumerate]
        public JSObject lastIndexOf(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return -1;
            var el = args[0];
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
            foreach (var element in data.Reversed)
            {
                if (element.Value.isExist && Expressions.StrictEqual.Check(element.Value, el, null))
                    return Context.CurrentContext.wrap(element.Key);
            }
            return -1;
        }

        [DoNotEnumerate]
        public JSObject pop()
        {
            if (_length == 0)
                return JSObject.undefined;
            var res = JSObject.undefined;
            if (data.TryGetValue(--_length, out res))
                data.Remove(_length);
            return res;
        }

        [DoNotEnumerate]
        public JSObject push(Arguments args)
        {
            for (var i = 0; i < args.Length; i++)
                data.Add(_length++, args[i].CloneImpl());
            return this.length;
        }

        [DoNotEnumerate]
        public JSObject reverse()
        {
            var newData = new BinaryTree<long, JSObject>();
            foreach (var element in data)
                newData[_length - element.Key - 1U] = element.Value;
            data = newData;
            return this;
        }

        [DoNotEnumerate]
        public JSObject reduce(Arguments args)
        {
            var funco = args[0];
            if (funco.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("First argument on reduce mast be a function."));
            var func = funco.oValue as Function;
            var accum = args[1];
            if ((accum.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                accum = accum.CloneImpl();
            var index = 0;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (_length == 0)
                    throw new JSException(new TypeError("Array is empty."));
                index++;
                accum.attributes = 0;
                accum.Assign(data[0]);
            }
            if (index >= data.Count)
                return accum;
            if (accum.GetType() != typeof(JSObject))
                accum = accum.CloneImpl();
            args.length = 4;
            args[2] = new JSObject();
            var context = Context.CurrentContext;
            foreach (var element in data)
            {
                args[0] = accum;
                if (element.Value.valueType == JSObjectType.Property)
                    args[1] = (element.Value.oValue as Function[])[1] == null ? undefined : (element.Value.oValue as Function[])[1].Invoke(this, null);
                else
                    args[1] = element.Value;
                args[2].Assign(element.Key);
                args[3] = this;
                accum.Assign(func.Invoke(args));
            }
            return accum;
        }

        [DoNotEnumerate]
        public JSObject reduceRight(Arguments args)
        {
            var funco = args[0];
            if (funco.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("First argument on reduce mast be a function."));
            var func = funco.oValue as Function;
            var accum = args[1];
            if ((accum.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                accum = accum.CloneImpl();
            var index = 0;
            if (accum.valueType < JSObjectType.Undefined)
            {
                if (_length == 0)
                    throw new JSException(new TypeError("Array is empty."));
                index++;
                accum.attributes = 0;
                accum.Assign(data[0]);
            }
            if (index >= data.Count)
                return accum;
            if (accum.GetType() != typeof(JSObject))
                accum = accum.CloneImpl();
            args.length = 4;
            args[2] = new JSObject();
            var context = Context.CurrentContext;
            foreach (var element in data.Reversed)
            {
                args[0] = accum;
                if (element.Value.valueType == JSObjectType.Property)
                    args[1] = (element.Value.oValue as Function[])[1] == null ? undefined : (element.Value.oValue as Function[])[1].Invoke(this, null);
                else
                    args[1] = element.Value;
                args[2].Assign(element.Key);
                args[3] = this;
                accum.Assign(func.Invoke(args));
            }
            return accum;
        }

        [DoNotEnumerate]
        public JSObject shift()
        {
            if (_length == 0)
                return JSObject.undefined;
            var res = JSObject.undefined;
            data.TryGetValue(0, out res);
            _length--;
            foreach (var o in data.Nodes)
                o.key--;
            return res;
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject slice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (this.GetType() == typeof(Array)) // ��, Array sealed, �� ��� � �� ����� ��������.
            {
                long pos0 = Tools.JSObjectToInt64(args[0], 0, true);
                long pos1 = 0;
                if (args.Length > 1)
                    pos1 = Tools.JSObjectToInt64(args[1], _length, true);
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = _length + pos1;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = System.Math.Min(pos0, _length);
                if (pos0 >= 0 && pos1 >= 0 && pos1 > pos0)
                {
                    var res = new Array();
                    Array src = this;
                    while (src != null)
                    {
                        foreach (var node in src.data.Nodes)
                        {
                            if (node.key >= pos1)
                                break;
                            if (pos0 <= node.key)
                            {
                                var temp = node.value;
                                if (temp.isExist)
                                    res.Add(temp.CloneImpl());
                            }
                        }
                        if (src.__proto__ is TypeProxy
                            && src.__proto__ == TypeProxy.GetPrototype(typeof(Array)))
                            src = (src.__proto__ as TypeProxy).prototypeInstance as Array;
                        else
                            src = src.__proto__.oValue as Array;
                    }
                    return res;
                }
                return new Array();
            }
            else // ���-�� �������� ������ � ����� length
            {
                long _length = (uint)Tools.JSObjectToInt64(this.GetMember("length"));
                long pos0 = Tools.JSObjectToInt64(args[0], 0, true);
                long pos1 = 0;
                if (args.Length > 1)
                    pos1 = Tools.JSObjectToInt64(args[1], _length, true);
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = _length + pos1;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = System.Math.Min(pos0, _length);
                if (pos0 >= 0 && pos1 >= 0 && pos1 > pos0)
                {
                    var res = new Array();
                    foreach (var i in this)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        if (Tools.ParseNumber(i, ref pindex, out dindex) && (pindex == i.Length)
                            && dindex < _length
                            && dindex >= pos0
                            && dindex < pos1)
                        {
                            var temp = this[i];
                            if (temp.isExist)
                                res.Add(temp.CloneImpl());
                        }
                    }
                    return res;
                }
                return new Array();
            }
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject splice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return this;
            if (this.GetType() == typeof(Array)) // ��, Array sealed, �� ��� � �� ����� ��������.
            {
                long pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = (long)System.Math.Min(Tools.JSObjectToDouble(args[1]), _length);
                }
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = (uint)System.Math.Min(pos0, _length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, _length);
                var res = new Array();
                List<long> keysToRemove = new List<long>();
                long prewKey = -1;
                foreach (var node in data.Nodes)
                {
                    if (prewKey == -1)
                        prewKey = node.key;
                    if (node.key - prewKey > 1 && node.key < pos1)
                    {
                        for (var i = prewKey + 1; i < node.key; i++)
                            res.data[i] = __proto__[i.ToString()].CloneImpl();
                    }
                    if (node.key >= pos1)
                    {
                        node.key -= pos1 - pos0 - args.length + 2;
                    }
                    else if (pos0 <= node.key)
                    {
                        res.Add(node.value.CloneImpl());
                        keysToRemove.Add(node.key);
                    }
                    prewKey = node.key;
                }
                if (prewKey == -1)
                {
                    for (var i = 0; i < pos1; i++)
                        res.Add(__proto__[i.ToString()].CloneImpl());
                }
                for (var i = keysToRemove.Count; i-- > 0; )
                    data.Remove(keysToRemove[i]);
                _length -= pos1 - pos0 - args.length + 2;
                for (var i = 2; i < args.length; i++)
                    data[i - 2 + pos0] = args[i].CloneImpl();
                return res;
            }
            else // ���-�� �������� ������ � ����� length
            {
                var _length = (long)(uint)Tools.JSObjectToInt64(this["length"]);
                var pos0 = (long)System.Math.Min(Tools.JSObjectToDouble(args[0]), _length);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = (long)System.Math.Min(Tools.JSObjectToDouble(args[1]), _length);
                }
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = (uint)System.Math.Min(pos0, _length);
                pos1 += pos0;
                pos1 = (uint)System.Math.Min(pos1, _length);
                var res = new Array();
                List<string> keysToRemove = new List<string>();
                List<KeyValuePair<uint, JSObject>> itemsToReplace = new List<KeyValuePair<uint, JSObject>>();
                long prewKey = -1;
                foreach (var keyS in this)
                {
                    var pindex = 0;
                    var dkey = 0.0;
                    if (Tools.ParseNumber(keyS, ref pindex, out dkey) && (pindex == keyS.Length))
                    {
                        if (prewKey == -1)
                            prewKey = (uint)dkey;
                        if (dkey - prewKey > 1 && dkey < pos1)
                        {
                            for (var i = prewKey + 1; i < dkey; i++)
                                res.data[i] = __proto__[i.ToString()].CloneImpl();
                        }
                        if (dkey >= pos1)
                        {
                            keysToRemove.Add(keyS);
                            itemsToReplace.Add(new KeyValuePair<uint, JSObject>((uint)dkey, this[keyS].CloneImpl()));
                        }
                        else if (pos0 <= dkey)
                        {
                            res.Add(this[keyS].CloneImpl());
                            keysToRemove.Add(keyS);
                        }
                        prewKey = (long)dkey;
                    }
                }
                if (prewKey == -1)
                {
                    for (var i = 0; i < pos1; i++)
                        res.Add(__proto__[i.ToString()].CloneImpl());
                }
                for (var i = keysToRemove.Count; i-- > 0; )
                    this[keysToRemove[i]].valueType = JSObjectType.NotExists;
                _length -= pos1 - pos0 - args.length + 2;
                for (var i = 2; i < args.length; i++)
                    this[(i - 2 + pos0).ToString()] = args[i].CloneImpl();
                for (var i = 0; i < itemsToReplace.Count; i++)
                    this[(itemsToReplace[i].Key - (pos1 - pos0 - args.length + 2)).ToString()] = itemsToReplace[i].Value;
                this["length"] = _length;
                return res;
            }
        }

        private sealed class JSComparer : IComparer<JSObject>
        {
            Arguments args;
            JSObject first;
            JSObject second;
            Function comparer;

            public JSComparer(Arguments args, JSObject first, JSObject second, Function comparer)
            {
                this.args = args;
                this.first = first;
                this.second = second;
                this.comparer = comparer;
            }

            public int Compare(JSObject x, JSObject y)
            {
                first.Assign(x);
                second.Assign(y);
                args[0] = first;
                args[1] = second;
                var res = Tools.JSObjectToInt32(comparer.Invoke(JSObject.undefined, args));
                return res;
            }
        }

        [DoNotEnumerate]
        [AllowUnsafeCall(typeof(JSObject))]
        public JSObject sort(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var comparer = args[0].oValue as Function;
            if (this.GetType() == typeof(Array))
            {
                var len = _length;
                if (comparer != null)
                {
                    var second = new JSObject();
                    var first = new JSObject();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSObject, List<JSObject>>(new JSComparer(args, first, second, comparer));
                    Array src = this;
                    while (src != null)
                    {
                        foreach (var node in src.data.Nodes)
                        {
                            if (node.value.isDefinded)
                            {
                                if (src != this
                                    && data.ContainsKey(node.key))
                                    continue;
                                List<JSObject> els = null;
                                if (!tt.TryGetValue(node.value, out els))
                                    tt[node.value] = els = new List<JSObject>();
                                els.Add(node.value);
                            }
                        }
                        if (src.__proto__ is TypeProxy
                            && src.__proto__ == TypeProxy.GetPrototype(typeof(Array)))
                            src = (src.__proto__ as TypeProxy).prototypeInstance as Array;
                        else
                            src = src.__proto__.oValue as Array;
                    }
                    data.Clear();
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            data.Add(index++, node.value[i]);
                    }
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSObject>>(StringComparer.Ordinal);
                    Array src = this;
                    while (src != null)
                    {
                        foreach (var node in src.data.Nodes)
                        {
                            if (node.value.isDefinded)
                            {
                                if (src != this
                                    && data.ContainsKey(node.key))
                                    continue;
                                List<JSObject> els = null;
                                var key = node.value.ToString();
                                if (!tt.TryGetValue(key, out els))
                                    tt[key] = els = new List<JSObject>();
                                els.Add(node.value);
                            }
                        }
                        if (src.__proto__ is TypeProxy
                            && src.__proto__ == TypeProxy.GetPrototype(typeof(Array)))
                            src = (src.__proto__ as TypeProxy).prototypeInstance as Array;
                        else
                            src = src.__proto__.oValue as Array;
                    }
                    data.Clear();
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            data.Add(index++, node.value[i]);
                    }
                }
            }
            else
            {
                var len = (uint)Tools.JSObjectToInt64(this["length"]);
                if (comparer != null)
                {
                    var second = new JSObject();
                    var first = new JSObject();
                    args.length = 2;
                    args[0] = first;
                    args[1] = second;

                    var tt = new BinaryTree<JSObject, List<JSObject>>(new JSComparer(args, first, second, comparer));
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in this)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        if (Tools.ParseNumber(key, ref pindex, out dindex) && (pindex == key.Length)
                            && dindex < len)
                        {
                            keysToRemove.Add(key);
                            var value = this[key];
                            if (value.isDefinded)
                            {
                                value = value.CloneImpl();
                                List<JSObject> els = null;
                                if (!tt.TryGetValue(value, out els))
                                    tt[value] = els = new List<JSObject>();
                                els.Add(value);
                            }
                        }
                    }
                    for (var i = keysToRemove.Count; i-- > 0; )
                        this[keysToRemove[i]].valueType = JSObjectType.NotExists;
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            this[(index++).ToString()] = node.value[i];
                    }
                }
                else
                {
                    var tt = new BinaryTree<string, List<JSObject>>(StringComparer.Ordinal);
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in this)
                    {
                        var pindex = 0;
                        var dindex = 0.0;
                        if (Tools.ParseNumber(key, ref pindex, out dindex) && (pindex == key.Length)
                            && dindex < len)
                        {
                            keysToRemove.Add(key);
                            var value = this[key];
                            if (value.isDefinded)
                            {
                                value = value.CloneImpl();
                                List<JSObject> els = null;
                                var skey = value.ToString();
                                if (!tt.TryGetValue(skey, out els))
                                    tt[skey] = els = new List<JSObject>();
                                els.Add(value);
                            }
                        }
                    }
                    for (var i = keysToRemove.Count; i-- > 0; )
                        this[keysToRemove[i]].valueType = JSObjectType.NotExists;
                    var index = 0u;
                    foreach (var node in tt.Nodes)
                    {
                        for (var i = node.value.Count; i-- > 0; )
                            this[(index++).ToString()] = node.value[i];
                    }
                }
            }
            return this;
        }

        [DoNotEnumerate]
        public JSObject unshift(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException();
            foreach (var node in data.Nodes)
                node.key += (long)args.Length;
            _length += (long)args.Length;
            for (var i = 0; i < args.Length; i++)
                data[i] = args[i].CloneImpl();
            return length;
        }

        [Hidden]
        public override string ToString()
        {
            if (_length == 0)
                return "";
            return join(null).oValue as string;
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        public new JSObject toString(Arguments args)
        {
            if (this.GetType() != typeof(Array) && !this.GetType().IsSubclassOf(typeof(Array)))
                throw new JSException(new TypeError("Try to call Array.toString on not Array object."));
            return this.ToString();
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(this.GetType());
            foreach (var node in data.Nodes)
            {
                if (node.value.isExist && (!hideNonEnum || (node.value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return node.key.ToString();
            }
            if (!hideNonEnum)
                yield return "length";
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.isExist && (!hideNonEnum || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            if (name.valueType == JSObjectType.String && "length".Equals(name.oValue))
                return length;
            bool isIndex = false;
            uint index = 0;
            JSObject tname = name;
            if (tname.valueType >= JSObjectType.Object)
                tname = tname.ToPrimitiveValue_String_Value();
            switch (tname.valueType)
            {
                case JSObjectType.Object:
                case JSObjectType.Bool:
                    break;
                case JSObjectType.Int:
                    {
                        isIndex = tname.iValue >= 0;
                        index = (uint)tname.iValue;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        isIndex = (index = (uint)tname.dValue) == tname.dValue && index < uint.MaxValue;
                        break;
                    }
                case JSObjectType.String:
                    {
                        var fc = (tname.oValue as string)[0];
                        if ('0' <= fc && '9' >= fc)
                        {
                            var dindex = 0.0;
                            int si = 0;
                            if (Tools.ParseNumber(tname.oValue.ToString(), ref si, out dindex)
                                && (si == tname.oValue.ToString().Length)
                                && (index = (uint)dindex) == dindex
                                && index < uint.MaxValue)
                            {
                                isIndex = true;
                            }
                        }
                        break;
                    }
            }
            if (isIndex)
            {
                forWrite &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
                if (_length <= index)
                {
                    if (forWrite)
                    {
                        if ((_lengthObj.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                        {
                            if (own)
                                throw new JSException(new TypeError("Cannot add element in fixed size array"));
                            return notExists;
                        }
                        setLength(index + 1);
                        return data[index] = new JSObject() { valueType = JSObjectType.NotExistsInObject };
                    }
                    else
                    {
                        JSObject element = null;
                        if (!data.TryGetValue(index, out element))
                        {
                            return DefaultFieldGetter(name, forWrite, own);
                        }
                        else
                        {
                            notExists.valueType = JSObjectType.NotExistsInObject;
                            return element ?? notExists;
                        }
                    }
                }
                else
                {
                    JSObject element = null;
                    bool exists = data.TryGetValue(index, out element) && (element ?? (element = notExists)).isExist;
                    if (!exists)
                    {
                        if (forWrite)
                        {
                            element = new JSObject() { valueType = JSObjectType.NotExistsInObject };
                            data[index] = element;
                            return element;
                        }
                        else
                        {
                            if (own)
                            {
                                notExists.valueType = JSObjectType.NotExistsInObject;
                                return notExists;
                            }
                            else
                            {
                                if (__proto__ == null)
                                    __proto__ = TypeProxy.GetPrototype(this.GetType());
                                return __proto__.GetMember(name, false, false);
                            }
                        }
                    }
                    else
                    {
                        var t = element;
                        if (forWrite && (t.attributes & (JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly)) == JSObjectAttributesInternal.SystemObject)
                            data[index] = t = t.CloneImpl();
                        return t;
                    }
                }
            }

            //if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0)
            //    return __proto__.GetMember(name, create, own);
            return DefaultFieldGetter(name, forWrite, own);
        }

        [Hidden]
        public override JSObject valueOf()
        {
            return base.valueOf();
        }
    }
}