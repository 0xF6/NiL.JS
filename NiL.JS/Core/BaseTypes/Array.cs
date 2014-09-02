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
        }

        [DoNotEnumerate]
        public Array()
        {
            oValue = this;
            valueType = JSObjectType.Object;
            data = new BinaryTree<long, JSObject>();
            attributes |= JSObjectAttributesInternal.ReadOnly;
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

        [Field]
        [DoNotDelete]
        [NotConfigurable]
        [DoNotEnumerateAttribute]
        public JSObject length
        {
            [Hidden]
            get
            {
                return _length > 0xFFFFFFFF ? 0 : _length;
            }
            [Hidden]
            set
            {
                var nlenD = Tools.JSObjectToDouble(value);
                var nlen = (long)nlenD;
                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                    throw new JSException(new RangeError("Invalid array length"));
                setLength(nlen);
            }
        }

        internal bool setLength(long nlen)
        {
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
                    setLength(nlen + 1);
                    return false;
                }
            }
            foreach (var i in data.NotLess(nlen))
                data.Remove(i.Key);
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
            var res = new Array();
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
                    res._length = element.Key;
                    res.Add(f.Invoke(tb, ao).CloneImpl());
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
                    res.Add(f.Invoke(tb, ao).CloneImpl());
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
            var left = new NiL.JS.Statements.ImmidateValueStatement(el);
            foreach (var element in data)
            {
                if (element.Value.isExist && Expressions.StrictEqual.Check(element.Value, left, null))
                    return Context.CurrentContext.wrap(element.Key);
            }
            return -1;
        }

        [DoNotEnumerateAttribute]
        public static JSObject isArray(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            return args[0].Value is Array;
        }

        [DoNotEnumerate]
        public JSObject join(Arguments separator)
        {
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (separator.Length == 0)
                return ToString();
            var el = separator[0].ToString();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            JSObject t;
            for (var i = 0U; i < _length - 1; i++)
            {
                if (data.TryGetValue(i, out t) && t.isExist)
                    sb.Append(t);
                sb.Append(el);
            }
            if (data.TryGetValue(_length - 1, out t) && t.isExist)
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
            var left = new NiL.JS.Statements.ImmidateValueStatement(el);
            foreach (var element in data.Reversed)
            {
                if (element.Value.isExist && Expressions.StrictEqual.Check(element.Value, left, null))
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
        public JSObject slice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return this;
            if ((object)this is Array) // ��, Array sealed, �� ��� � �� ����� ��������.
            {
                long pos0 = (long)Tools.JSObjectToDouble(args[0]);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = System.Math.Min((long)Tools.JSObjectToDouble(args[1]), _length);
                }
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
                    foreach (var node in data.Nodes)
                    {
                        if (pos0 <= node.key && pos1 >= node.key)
                            res.Add(node.value.CloneImpl());
                    }
                    return res;
                }
                return new Array();
            }
            else // ���-�� �������� ������ � ����� length
            {
                throw new NotImplementedException();
            }
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        public JSObject splice(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length == 0)
                return this;
            if ((object)this is Array) // ��, Array sealed, �� ��� � �� ����� ��������.
            {
                long pos0 = (long)Tools.JSObjectToDouble(args[0]);
                long pos1 = 0;
                if (args.Length > 1)
                {
                    if (args[1].valueType <= JSObjectType.Undefined)
                        pos1 = 0;
                    else
                        pos1 = System.Math.Min((long)Tools.JSObjectToDouble(args[1]), _length);
                }
                else
                    pos1 = _length;
                if (pos0 < 0)
                    pos0 = _length + pos0;
                if (pos0 < 0)
                    pos0 = 0;
                if (pos1 < 0)
                    pos1 = 0;
                pos0 = System.Math.Min(pos0, _length);
                pos1 += pos0;
                pos1 = System.Math.Min(pos1, _length);
                var res = new Array();
                List<long> keysToRemove = new List<long>();
                foreach (var node in data.Nodes)
                {
                    if (pos0 <= node.key && pos1 >= node.key)
                    {
                        res.Add(node.value.CloneImpl());
                        keysToRemove.Add(node.key);
                    }
                }
                for (var i = keysToRemove.Count; i-- > 0; )
                    data.Remove(keysToRemove[i]);
                _length -= pos1 - pos0;

                return res;
            }
            else // ���-�� �������� ������ � ����� length
            {
                throw new NotImplementedException();
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
        public JSObject sort(Arguments args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var comparer = args[0].oValue as Function;
            var len = Tools.JSObjectToInt32(length);
            if (comparer != null)
            {
                var second = new JSObject();
                var first = new JSObject();
                args.length = 2;
                args[0] = first;
                args[1] = second;

                var tt = new BinaryTree<JSObject, List<JSObject>>(new JSComparer(args, first, second, comparer));
                foreach (var node in data.Nodes)
                {
                    if (node.value.isExist)
                    {
                        List<JSObject> els = null;
                        if (!tt.TryGetValue(node.value, out els))
                            tt[node.value] = els = new List<JSObject>();
                        els.Add(node.value);
                    }
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
                var tt = new BinaryTree<string, List<JSObject>>();
                foreach (var node in data.Nodes)
                {
                    if (node.value.isExist)
                    {
                        List<JSObject> els = null;
                        var key = node.value.ToString();
                        if (!tt.TryGetValue(key, out els))
                            tt[key] = els = new List<JSObject>();
                        els.Add(node.value);
                    }
                }
                data.Clear();
                var index = 0u;
                foreach (var node in tt.Nodes)
                {
                    for (var i = node.value.Count; i-- > 0; )
                        data.Add(index++, node.value[i]);
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
            var res = new StringBuilder();
            for (var i = 0u; i < _length; i++)
            {
                JSObject t = null;
                if (i > 0)
                    res.Append(',');
                if (data.TryGetValue(i, out t) && t.isExist)
                    res.Append(t);
            }
            return res.ToString();
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
        internal protected override JSObject GetMember(JSObject name, bool create, bool own)
        {
            long index = 0;
            double dindex = Tools.JSObjectToDouble(name);
            if (!double.IsNaN(dindex) && !double.IsInfinity(dindex))
            {
                if (dindex >= 0)
                {
                    if (((index = (long)dindex) == dindex))
                    {
                        create &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
                        if (_length <= index)
                        {
                            if (create)
                            {
                                if ((this["length"].oValue as Function[])[0] == null)
                                {
                                    if (own)
                                        throw new JSException(new TypeError("Cannot add element in fixed size array"));
                                    return notExists;
                                }
                                setLength(index + 1);
                                return data[index] = new JSObject() { valueType = JSObjectType.NotExistsInObject };
                            }
                            else
                                return base.GetMember(name, create, own);
                        }
                        else
                        {
                            JSObject element = null;
                            bool exists = data.TryGetValue(index, out element);
                            if (!exists)
                            {
                                if (create)
                                {
                                    element = new JSObject() { valueType = JSObjectType.NotExistsInObject };
                                    data[index] = element;
                                    return element;
                                }
                                else
                                    return base.GetMember(name, create, own);
                            }
                            else
                            {
                                var t = element ?? notExists;
                                if ((t.attributes & (JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly)) == JSObjectAttributesInternal.SystemObject)
                                    data[index] = t = t.CloneImpl();
                                return t;
                            }
                        }
                    }
                }
            }
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(this.GetType());
            if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0)
                return __proto__.GetMember(name, create, own);
            return DefaultFieldGetter(name, create, own);
        }
    }
}