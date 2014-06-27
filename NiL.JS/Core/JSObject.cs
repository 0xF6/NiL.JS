using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    public enum JSObjectType : int
    {
        NotExist = 0,
        NotExistInObject = 1,
        Undefined = 2,
        Bool = 6,
        Int = 10,
        Double = 18,
        String = 34,
        Object = 66,
        Function = 130,
        Date = 258,
        Property = 514
    }

    [Serializable]
    [Flags]
    internal enum JSObjectAttributesInternal : int
    {
        None = 0,
        DoNotEnum = 1 << 0,
        DoNotDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        NotConfigurable = 1 << 4,
        Argument = 1 << 16,
        /// <summary>
        /// ������ �������� ���������.
        /// </summary>
        SystemObject = 1 << 17,
        ProxyPrototype = 1 << 18,
        /// <summary>
        /// ��������� �� ��, ��� �������� ������ ������������������ ��� ���� �� ���������.
        /// </summary>
        Field = 1 << 19,
        /// <summary>
        /// ���������, ����������� ��� ������������ ��������.
        /// </summary>
        PrivateAttributes = Immutable | ProxyPrototype | Field
    }

    [Serializable]
    [Flags]
    public enum JSObjectAttributes : int
    {
        None = 0,
        DoNotEnum = 1 << 0,
        DoNotDelete = 1 << 1,
        ReadOnly = 1 << 2,
        Immutable = 1 << 3,
        NotConfigurable = 1 << 4,
    }

    public delegate void AssignCallback(JSObject sender);

    [Serializable]
    /// <summary>
    /// ������� ������ ��� ���� ��������, ����������� � ���������� �������.
    /// ��� �������� ���������������� ��������, � �������� �������� ����, ������������� ������������ ��� NiL.JS.Core.EmbeddedType
    /// </summary>
    public class JSObject : IEnumerable<string>, IEnumerable, ICloneable
    {
        [Hidden]
        internal static readonly AssignCallback ErrorAssignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Invalid left-hand side"))); };
        [Hidden]
        internal static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Hidden]
        internal static readonly JSObject undefined = new JSObject() { valueType = JSObjectType.Undefined, attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject notExist = new JSObject() { valueType = JSObjectType.NotExist, attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject Null = new JSObject() { valueType = JSObjectType.Object, oValue = null, assignCallback = ErrorAssignCallback, attributes = JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject nullString = new JSObject() { valueType = JSObjectType.String, oValue = "null", assignCallback = ErrorAssignCallback, attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static JSObject GlobalPrototype;

        [NonSerialized]
        [Hidden]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.Browsable(false)]
        internal AssignCallback assignCallback;
        [Hidden]
        internal JSObject __proto__;
        [Hidden]
        internal IDictionary<string, JSObject> fields;

        [Hidden]
        internal string lastRequestedName;
        [Hidden]
        internal JSObjectType valueType;
        [Hidden]
        internal int iValue;
        [Hidden]
        internal double dValue;
        [Hidden]
        internal object oValue;
        [Hidden]
        internal JSObjectAttributesInternal attributes;

        /// <summary>
        /// ���������� ���� ������� ��� �������� ��������� ����������� ����� ��������.
        /// </summary>
        /// <param name="name">��� �����.</param>
        /// <returns>���� ������� � ��������� ������.</returns>
        [Hidden]
        public JSObject this[string name]
        {
            [Hidden]
            get
            {
                return this.GetMember(name);
            }
            [Hidden]
            set
            {
                this.GetMember(name, true, true).Assign(value);
            }
        }

        [Hidden]
        public object Value
        {
            [Hidden]
            get
            {
                switch (valueType)
                {
                    case JSObjectType.Bool:
                        return iValue != 0;
                    case JSObjectType.Int:
                        return iValue;
                    case JSObjectType.Double:
                        return dValue;
                    case JSObjectType.String:
                    case JSObjectType.Object:
                    case JSObjectType.Function:
                    case JSObjectType.Property:
                    case JSObjectType.Date:
                        return oValue;
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExistInObject:
                    default:
                        return null;
                }
            }
        }

        [Hidden]
        public JSObjectType ValueType
        {
            [Hidden]
            get
            {
                return valueType;
            }
        }

        [Hidden]
        public JSObjectAttributes Attributes
        {
            [Hidden]
            get
            {
                return (JSObjectAttributes)((int)attributes & 0xffff);
            }
        }

        [Hidden]
        public JSObject()
        {
            valueType = JSObjectType.Undefined;
        }

        [Hidden]
        public JSObject(bool createFields)
        {
            if (createFields)
                fields = new Dictionary<string, JSObject>();
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        public static JSObject create(JSObject args)
        {
            var proto = args["0"];
            if (proto.valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Prototype may be only Object or null.")));
            proto = proto.oValue as JSObject ?? proto;
            var members = args["1"];
            members.fields = (members.oValue as JSObject ?? members).fields;
            if (members.valueType >= JSObjectType.Object && members.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Properties descriptor may be only Object.")));
            var res = CreateObject();
            if (proto.valueType >= JSObjectType.Object && proto.oValue != null)
                res.__proto__ = proto;
            if (members.valueType >= JSObjectType.Object)
                foreach (var member in members)
                {
                    var desc = members[member];
                    if (desc.valueType == JSObjectType.Property)
                    {
                        var getter = (desc.oValue as Function[])[1];
                        if (getter == null || getter.oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Invalid property descriptor for property " + member + " .")));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    if (desc.valueType < JSObjectType.Object || desc.oValue == null)
                        throw new JSException(TypeProxy.Proxy(new TypeError("Invalid property descriptor for property " + member + " .")));
                    var value = desc["value"];
                    if (value.valueType == JSObjectType.Property)
                        value = ((value.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var configurable = desc["configurable"];
                    if (configurable.valueType == JSObjectType.Property)
                        configurable = ((configurable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var enumerable = desc["enumerable"];
                    if (enumerable.valueType == JSObjectType.Property)
                        enumerable = ((enumerable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var writable = desc["writable"];
                    if (writable.valueType == JSObjectType.Property)
                        writable = ((writable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var get = desc["get"];
                    if (get.valueType == JSObjectType.Property)
                        get = ((get.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var set = desc["set"];
                    if (set.valueType == JSObjectType.Property)
                        set = ((set.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    if (value.valueType != JSObjectType.NotExistInObject
                        && (get.valueType != JSObjectType.NotExistInObject
                        || set.valueType != JSObjectType.NotExistInObject))
                        throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and default value.")));
                    if (writable.valueType != JSObjectType.NotExistInObject
                        && (get.valueType != JSObjectType.NotExistInObject
                        || set.valueType != JSObjectType.NotExistInObject))
                        throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and writable attribute.")));
                    JSObject obj = new JSObject();
                    res.fields[member] = obj;
                    obj.attributes |= JSObjectAttributesInternal.DoNotEnum;
                    obj.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
                    if ((bool)enumerable)
                        obj.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                    if ((bool)configurable)
                        obj.attributes &= ~(JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete);
                    if (value.valueType > JSObjectType.Undefined)
                    {
                        obj.Assign(value);
                        if (!(bool)writable)
                            obj.attributes |= JSObjectAttributesInternal.ReadOnly;
                    }
                    else if (get.valueType != JSObjectType.NotExistInObject
                          || set.valueType != JSObjectType.NotExistInObject)
                    {
                        if (get.valueType > JSObjectType.Undefined
                            && get.valueType != JSObjectType.Function)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Getter mast be a function.")));
                        if (set.valueType > JSObjectType.Undefined
                            && set.valueType != JSObjectType.Function)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Setter mast be a function.")));
                        obj.valueType = JSObjectType.Property;
                        obj.oValue = new Function[]
                        {
                            set.valueType > JSObjectType.Undefined ? set.oValue as Function : null,
                            get.valueType > JSObjectType.Undefined ? get.oValue as Function : null
                        };
                    }
                    else if (!(bool)writable) // �� ��� ������, ����� � ����������� �� ������� �� ��������, �� ������/������
                        obj.attributes |= JSObjectAttributesInternal.ReadOnly;
                }
            return res;
        }

        [Hidden]
        public static JSObject CreateObject()
        {
            var t = new JSObject(true)
            {
                valueType = JSObjectType.Object
            };
            t.oValue = t;
            return t;
        }

        [DoNotEnumerate]
        public static JSObject defineProperties(JSObject args)
        {
            var proto = args["0"];
            if (proto.valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Property define may only for Objects.")));
            var res = proto.oValue as JSObject ?? proto;
            var members = args["1"];
            if (members.valueType > JSObjectType.Undefined)
            {
                if (members.valueType < JSObjectType.Object)
                    throw new JSException(TypeProxy.Proxy(new TypeError("Properties descriptor may be only Object.")));
                foreach (var member in members)
                {
                    var desc = members[member];
                    if (desc.valueType == JSObjectType.Property)
                    {
                        var getter = (desc.oValue as Function[])[1];
                        if (getter == null || getter.oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Invalid property descriptor for property " + member + " .")));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    var value = desc["value"];
                    if (value.valueType == JSObjectType.Property)
                        value = ((value.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var configurable = desc["configurable"];
                    if (configurable.valueType == JSObjectType.Property)
                        configurable = ((configurable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var enumerable = desc["enumerable"];
                    if (enumerable.valueType == JSObjectType.Property)
                        enumerable = ((enumerable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var writable = desc["writable"];
                    if (writable.valueType == JSObjectType.Property)
                        writable = ((writable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var get = desc["get"];
                    if (get.valueType == JSObjectType.Property)
                        get = ((get.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    var set = desc["set"];
                    if (set.valueType == JSObjectType.Property)
                        set = ((set.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                    if (value.valueType != JSObjectType.NotExistInObject
                        && (get.valueType != JSObjectType.NotExistInObject
                        || set.valueType != JSObjectType.NotExistInObject))
                        throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and default value.")));
                    if (writable.valueType != JSObjectType.NotExistInObject
                        && (get.valueType != JSObjectType.NotExistInObject
                        || set.valueType != JSObjectType.NotExistInObject))
                        throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and writable attribute.")));
                    JSObject obj = null;
                    if (res.fields == null)
                        res.fields = new Dictionary<string, JSObject>();
                    if (res is TypeProxy)
                        obj = res.DefineMember(member);
                    else
                        if (!res.fields.TryGetValue(member, out obj) || (obj.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                        {
                            if ((res.attributes & JSObjectAttributesInternal.Immutable) != 0)
                                throw new JSException(TypeProxy.Proxy(new TypeError("Can not define property \"" + member + "\". Object is immutable.")));
                            res.fields[member] = obj = new JSObject() { valueType = JSObjectType.NotExist };
                        }
                    var newProp = obj.valueType < JSObjectType.Undefined;
                    var config = (obj.attributes & JSObjectAttributesInternal.NotConfigurable) == 0 || newProp;
                    if (config)
                    {
                        if (newProp)
                            obj.valueType = JSObjectType.Undefined;

                        if (enumerable.valueType >= JSObjectType.Undefined || newProp)
                        {
                            if ((bool)enumerable)
                                obj.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                            else
                                obj.attributes |= JSObjectAttributesInternal.DoNotEnum;
                        }

                        if (configurable.valueType >= JSObjectType.Undefined || newProp)
                        {
                            if ((bool)configurable)
                                obj.attributes &= ~(JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete);
                            else
                                obj.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
                        }

                        if (value.valueType > JSObjectType.Undefined)
                        {
                            obj.valueType = JSObjectType.Undefined; // ��� ����� ���� Property, �� ������� �� ��������
                            obj.Assign(value);
                            if (!(bool)writable)
                                obj.attributes |= JSObjectAttributesInternal.ReadOnly;
                        }
                        else if (get.valueType != JSObjectType.NotExistInObject
                              || set.valueType != JSObjectType.NotExistInObject)
                        {
                            if (get.valueType > JSObjectType.Undefined
                                && get.valueType != JSObjectType.Function)
                                throw new JSException(TypeProxy.Proxy(new TypeError("Getter mast be a function.")));
                            if (set.valueType > JSObjectType.Undefined
                                && set.valueType != JSObjectType.Function)
                                throw new JSException(TypeProxy.Proxy(new TypeError("Setter mast be a function.")));
                            obj.valueType = JSObjectType.Property;
                            obj.oValue = new Function[]
                            {
                                set.valueType > JSObjectType.Undefined ? set.oValue as Function : null,
                                get.valueType > JSObjectType.Undefined ? get.oValue as Function : null
                            };
                        }
                        else if (!(bool)writable) // �� ��� ������, ����� � ����������� �� ������� �� ��������, �� ������/������
                        {
                            obj.attributes |= JSObjectAttributesInternal.ReadOnly;
                            obj.valueType = JSObjectType.Undefined;
                        }
                    }
                    else if (obj.valueType != JSObjectType.Property && value.valueType > JSObjectType.Undefined)
                        obj.Assign(value);
                }
            }
            return res;
        }

        [ParametersCount(3)]
        [DoNotEnumerate]
        public static JSObject defineProperty(JSObject args)
        {
            var source = args.GetMember("0");
            source = source.oValue as JSObject ?? source;
            if (source.valueType <= JSObjectType.Undefined)
                return undefined;
            var desc = args["2"];
            var value = desc["value"];
            var configurable = desc["configurable"];
            var enumerable = desc["enumerable"];
            var writable = desc["writable"];
            var get = desc["get"];
            var set = desc["set"];
            if (value.valueType != JSObjectType.NotExistInObject
                && (get.valueType != JSObjectType.NotExistInObject
                || set.valueType != JSObjectType.NotExistInObject))
                throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and default value.")));
            if (writable.valueType != JSObjectType.NotExistInObject
                && (get.valueType != JSObjectType.NotExistInObject
                || set.valueType != JSObjectType.NotExistInObject))
                throw new JSException(TypeProxy.Proxy(new TypeError("Property can not have getter or setter and writable attribute.")));
            string name = args.GetMember("1").ToString();
            JSObject obj = null;
            if (source.fields == null)
                source.fields = new Dictionary<string, JSObject>();
            if (source is TypeProxy)
                obj = source.DefineMember(name);
            else
                if (!source.fields.TryGetValue(name, out obj) || (obj.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                {
                    if ((source.attributes & JSObjectAttributesInternal.Immutable) != 0)
                        throw new JSException(TypeProxy.Proxy(new TypeError("Can not define property \"" + name + "\". Object is immutable.")));
                    source.fields[name] = obj = new JSObject() { valueType = JSObjectType.NotExist };
                }
            var newProp = obj.valueType < JSObjectType.Undefined;
            var config = (obj.attributes & JSObjectAttributesInternal.NotConfigurable) == 0 || newProp;
            if (config)
            {
                if (newProp)
                    obj.valueType = JSObjectType.Undefined;
                if (enumerable.valueType >= JSObjectType.Undefined || newProp)
                {
                    if ((bool)enumerable)
                        obj.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                    else
                        obj.attributes |= JSObjectAttributesInternal.DoNotEnum;
                }

                if (configurable.valueType >= JSObjectType.Undefined || newProp)
                {
                    if ((bool)configurable)
                        obj.attributes &= ~(JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete);
                    else
                        obj.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
                }

                if (value.valueType > JSObjectType.Undefined)
                {
                    obj.valueType = JSObjectType.Undefined; // ��� ����� ���� Property, �� ������� �� ��������
                    obj.Assign(value);
                    if (!(bool)writable)
                        obj.attributes |= JSObjectAttributesInternal.ReadOnly;
                }
                else if (get.valueType != JSObjectType.NotExistInObject
                      || set.valueType != JSObjectType.NotExistInObject)
                {
                    if (get.valueType > JSObjectType.Undefined
                        && get.valueType != JSObjectType.Function)
                        throw new JSException(TypeProxy.Proxy(new TypeError("Getter mast be a function.")));
                    if (set.valueType > JSObjectType.Undefined
                        && set.valueType != JSObjectType.Function)
                        throw new JSException(TypeProxy.Proxy(new TypeError("Setter mast be a function.")));
                    obj.valueType = JSObjectType.Property;
                    obj.oValue = new Function[]
                    {
                        set.valueType > JSObjectType.Undefined ? set.oValue as Function : null,
                        get.valueType > JSObjectType.Undefined ? get.oValue as Function : null
                    };
                }
                else if (!(bool)writable) // �� ��� ������, ����� � ����������� �� ������� �� ��������, �� ������/������
                    obj.attributes |= JSObjectAttributesInternal.ReadOnly;
            }
            else if (obj.valueType != JSObjectType.Property && value.valueType > JSObjectType.Undefined)
                obj.Assign(value);
            return true;
        }

        [DoNotEnumerate]
        public static JSObject freeze(JSObject args)
        {
            var obj = args["0"];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.freeze called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.freeze called on null."));
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            obj = obj.oValue as JSObject ?? obj;
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            if (obj is BaseTypes.Array)
            {
                var arr = obj as BaseTypes.Array;
                for (var i = arr.data.Count; i-- > 0; )
                    if (arr.data[i] != null)
                        arr.data[i].attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum;
            }
            else if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    f.Value.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum;
                }
            return obj;
        }

        /// <summary>
        /// ���������� ���� ������� � ��������� ������.
        /// </summary>
        /// <param name="name">��� �����.</param>
        /// <returns>������, �������������� ����������� ����.</returns>
        [Hidden]
        public JSObject GetMember(string name)
        {
            return GetMember(name, false, false);
        }

        /// <summary>
        /// ���������� ���� ������� � ��������� ������.
        /// </summary>
        /// <param name="name">��� �����.</param>
        /// <param name="own">���������, ������� �� ���������� ��������� ������� ��� ������ �����</param>
        /// <returns>������, �������������� ����������� ����.</returns>
        [Hidden]
        public JSObject GetMember(string name, bool own)
        {
            return GetMember(name, false, own);
        }

        /// <summary>
        /// ���������� ���� ������� � ��������� ������.
        /// </summary>
        /// <param name="name">��� �����.</param>
        /// <returns>������, �������������� ����������� ����.</returns>
        [Hidden]
        public JSObject DefineMember(string name)
        {
            return GetMember(name, true, true);
        }

        [Hidden]
        internal protected virtual JSObject GetMember(string name, bool createMember, bool own)
        {
            switch (valueType)
            {
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new ReferenceError("Variable not defined.")));
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"undefined\".")));
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        createMember = false;
                        if (__proto__ == null)
                            __proto__ = TypeProxy.GetPrototype(typeof(BaseTypes.Number));
#if DEBUG
                        else if (__proto__.oValue != TypeProxy.GetPrototype(typeof(BaseTypes.Number)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                    }
                case JSObjectType.String:
                    {
                        createMember = false;
                        int index = 0;
                        double dindex = 0.0;
                        if (Tools.ParseNumber(name, index, out dindex, Tools.ParseNumberOptions.Default))
                        {
                            if (dindex > 0.0 && ((index = (int)dindex) == dindex) && oValue.ToString().Length > index)
                                return oValue.ToString()[index];
                            return undefined;
                        }
                        if (__proto__ == null)
                            __proto__ = TypeProxy.GetPrototype(typeof(BaseTypes.String));
#if DEBUG
                        else if (__proto__.oValue != TypeProxy.GetPrototype(typeof(BaseTypes.String)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                    }
                case JSObjectType.Bool:
                    {
                        createMember = false;
                        if (__proto__ == null)
                            __proto__ = TypeProxy.GetPrototype(typeof(BaseTypes.Boolean));
#if DEBUG
                        else if (__proto__.oValue != TypeProxy.GetPrototype(typeof(BaseTypes.Boolean)).oValue)
                            System.Diagnostics.Debugger.Break();
#endif
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        if (oValue == this)
                            break;
                        if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0)
                            return __proto__.GetMember(name, createMember, own);
                        if (oValue != this && (oValue is JSObject))
                            return (oValue as JSObject).GetMember(name, createMember, own);
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"null\"")));
                        break;
                    }
                case JSObjectType.Function:
                    {
#if DEBUG
                        if (oValue == this)
                            System.Diagnostics.Debugger.Break();
#endif
                        if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0)
                            return __proto__.GetMember(name, createMember, own);
                        if (oValue == null)
                            throw new JSException(TypeProxy.Proxy(new TypeError("Can't get property \"" + name + "\" of \"null\"")));
                        if (oValue == this)
                            break;
                        return (oValue as JSObject).GetMember(name, createMember, own);
                    }
                case JSObjectType.Property:
                    throw new InvalidOperationException("Try to get member of property");
                default:
                    throw new NotImplementedException();
            }
            return DefaultFieldGetter(name, createMember, own);
        }

        [Hidden]
        protected JSObject DefaultFieldGetter(string name, bool forWrite, bool own)
        {
            switch (name)
            {
                case "__proto__":
                    {
                        forWrite &= (attributes & JSObjectAttributesInternal.Immutable) == 0;
                        if (this == GlobalPrototype)
                        {
                            if (forWrite)
                            {
                                if (__proto__ == null || (__proto__.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                                    return __proto__ = new JSObject();
                                else
                                    return __proto__ ?? Null;
                            }
                            else
                                return __proto__ ?? Null;
                        }
                        else
                        {
                            if (forWrite)
                            {
                                if (__proto__ == null || (__proto__.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                                    return __proto__ = new JSObject();
                                else
                                    return __proto__ ?? GlobalPrototype ?? Null;
                            }
                            else
                                return __proto__ ?? GlobalPrototype ?? Null;
                        }
                    }
                default:
                    {
                        JSObject res = null;
                        var proto = __proto__ ?? GlobalPrototype ?? Null;
                        bool fromProto =
                            (fields == null || !fields.TryGetValue(name, out res) || res.valueType < JSObjectType.Undefined)
                            && (proto != null)
                            && (proto != this)
                            && (!own || (proto.oValue is TypeProxy && proto.oValue != GlobalPrototype.oValue));
                        if (fromProto)
                        {
                            res = proto.GetMember(name, false, own);
                            if (own && (attributes & JSObjectAttributesInternal.ProxyPrototype) == 0 && res.valueType != JSObjectType.Property)
                                res = null;
                            else if (res.valueType < JSObjectType.Undefined)
                                res = null;
                        }
                        if (res == null)
                        {
                            if (!forWrite || (attributes & JSObjectAttributesInternal.Immutable) != 0)
                            {
                                notExist.valueType = JSObjectType.NotExistInObject;
                                return notExist;
                            }
                            res = new JSObject()
                            {
                                lastRequestedName = name,
                                valueType = JSObjectType.NotExistInObject
                            };
                            if (fields == null)
                                fields = new Dictionary<string, JSObject>();
                            fields[name] = res;
                        }
                        else if (forWrite && ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0 || fromProto))
                        {
                            if ((res.attributes & JSObjectAttributesInternal.ReadOnly) == 0 && res.valueType != JSObjectType.Property)
                            {
                                var t = res.Clone() as JSObject;
                                t.lastRequestedName = name;
                                if (fields == null)
                                    fields = new Dictionary<string, JSObject>();
                                fields[name] = t;
                                res = t;
                            }
                        }
                        else
                            res.lastRequestedName = name;
                        if (res.valueType == JSObjectType.NotExist)
                            res.valueType = JSObjectType.NotExistInObject;
                        return res;
                    }
            }
        }

        [Hidden]
        internal JSObject ToPrimitiveValue_Value_String()
        {
            if (valueType >= JSObjectType.Object && oValue != null)
            {
                if (oValue == null)
                    return nullString;
                var tpvs = GetMember("valueOf");
                JSObject res = null;
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                tpvs = GetMember("toString");
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
            }
            return this;
        }

        [Hidden]
        internal JSObject ToPrimitiveValue_String_Value()
        {
            if (valueType >= JSObjectType.Object && oValue != null)
            {
                if (oValue == null)
                    return nullString;
                var tpvs = GetMember("toString");
                JSObject res = null;
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                tpvs = GetMember("valueOf");
                if (tpvs.valueType == JSObjectType.Function)
                {
                    res = (tpvs.oValue as NiL.JS.Core.BaseTypes.Function).Invoke(this, null);
                    if (res.valueType == JSObjectType.Object)
                    {
                        if (res.oValue is BaseTypes.String)
                            res = res.oValue as BaseTypes.String;
                    }
                    if (res.valueType < JSObjectType.Object)
                        return res;
                }
                throw new JSException(TypeProxy.Proxy(new TypeError("Can't convert object to primitive value.")));
            }
            return this;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public virtual void Assign(JSObject value)
        {
            if (this.assignCallback != null)
                this.assignCallback(this);
            if (valueType == JSObjectType.Property)
                throw new InvalidOperationException("Try to assign to property.");
            if ((attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.SystemObject)) != 0)
                return;
            if (value == this)
                return;
            if (value != null)
            {
                this.valueType = (value.valueType & ~(JSObjectType.NotExistInObject | JSObjectType.NotExist)) | JSObjectType.Undefined;
                this.iValue = value.iValue;
                if (valueType < JSObjectType.String)
                {
                    this.fields = null;
                    this.oValue = null;
                }
                else
                {
                    this.oValue = value.oValue;
                    if (valueType < JSObjectType.Object)
                        this.fields = null;
                    else
                        this.fields = value.fields;
                }
                this.dValue = value.dValue;
                this.__proto__ = value.__proto__;
                this.attributes =
                    (this.attributes & ~JSObjectAttributesInternal.PrivateAttributes)
                    | (value.attributes & JSObjectAttributesInternal.PrivateAttributes);
                return;
            }
            this.fields = null;
            this.__proto__ = null;
            this.valueType = JSObjectType.Undefined;
            this.oValue = null;
            this.__proto__ = null;
        }

        [Hidden]
        public virtual object Clone()
        {
            var res = new JSObject();
            res.Assign(this);
            res.attributes = this.attributes & ~JSObjectAttributesInternal.SystemObject;
            return res;
        }

        [Hidden]
        public override string ToString()
        {
            if (valueType <= JSObjectType.Undefined)
                return "undefined";
            var res = ToPrimitiveValue_String_Value();
            switch (res.valueType)
            {
                case JSObjectType.Bool:
                    return res.iValue != 0 ? "true" : "false";
                case JSObjectType.Int:
                    return res.iValue >= 0 && res.iValue < 16 ? Tools.NumString[res.iValue] : res.iValue.ToString(CultureInfo.InvariantCulture);
                case JSObjectType.Double:
                    return Tools.DoubleToString(res.dValue);
                case JSObjectType.String:
                    return res.oValue as string;
                default:
                    return (res.oValue ?? "null").ToString();
            }
        }

        [Hidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        [Hidden]
        public IEnumerator<string> GetEnumerator()
        {
            if (this is JSObject && valueType >= JSObjectType.Object)
            {
                if (oValue != this && oValue is JSObject)
                    return (oValue as JSObject).GetEnumeratorImpl(true);
            }
            return GetEnumeratorImpl(true);
        }

        protected internal virtual IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.valueType >= JSObjectType.Undefined && (!hideNonEnum || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [Modules.ParametersCount(0)]
        public JSObject toString(JSObject args)
        {
            switch (this.valueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        return "[object Number]";
                    }
                case JSObjectType.Undefined:
                    {
                        return "[object Undefined]";
                    }
                case JSObjectType.String:
                    {
                        return "[object String]";
                    }
                case JSObjectType.Bool:
                    {
                        return "[object Boolean]";
                    }
                case JSObjectType.Function:
                    {
                        return "[object Function]";
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                    {
                        if (this.oValue is ThisBind)
                            return this.oValue.ToString();
                        if (this.oValue is TypeProxy)
                        {
                            var ht = (this.oValue as TypeProxy).hostedType;
                            if (ht == typeof(RegExp))
                                return "[object Object]";
                            return "[object " + (ht == typeof(JSObject) ? typeof(System.Object) : ht).Name + "]";
                        }
                        if (this.oValue != null)
                            return "[object " + (this.oValue.GetType() == typeof(JSObject) ? typeof(System.Object) : this.oValue.GetType()).Name + "]";
                        else
                            return "[object Null]";
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        [DoNotEnumerate]
        public virtual JSObject toLocaleString()
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("toLocaleString calling on undefined value.")));
            return toString(null);
        }

        [DoNotEnumerate]
        public virtual JSObject valueOf()
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("valueOf calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("valueOf calling on undefined value.")));
            if (valueType >= JSObjectType.Object && oValue is JSObject && oValue != this)
                return (oValue as JSObject).valueOf();
            else
                return this;
        }

        [DoNotEnumerate]
        public virtual JSObject isPrototypeOf(JSObject args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("isPrototypeOf calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("isPrototypeOf calling on undefined value.")));
            if (args.GetMember("length").iValue == 0)
                return false;
            var a = args.GetMember("0");
            if (this.valueType >= JSObjectType.Object && this.oValue != null)
            {
                while (a.valueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == this.oValue)
                        return true;
                    var pi = (a.oValue is TypeProxy) ? (a.oValue as TypeProxy).prototypeInstance : null;
                    if (pi != null && (this == pi || this == pi.oValue))
                        return true;
                    a = a.GetMember("__proto__");
                }
            }
            return false;
        }

        [DoNotEnumerate]
        public virtual JSObject hasOwnProperty(JSObject args)
        {
            JSObject name = args.GetMember("0");
            string n = "";
            switch (name.valueType)
            {
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        n = "undefined";
                        break;
                    }
                case JSObjectType.Int:
                    {
                        n = name.iValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    }
                case JSObjectType.Double:
                    {
                        n = Tools.DoubleToString(name.dValue);
                        break;
                    }
                case JSObjectType.String:
                    {
                        n = name.oValue as string;
                        break;
                    }
                case JSObjectType.Object:
                    {
                        args = name.ToPrimitiveValue_Value_String();
                        if (args.valueType == JSObjectType.String)
                            n = name.oValue as string;
                        if (args.valueType == JSObjectType.Int)
                            n = name.iValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (args.valueType == JSObjectType.Double)
                            n = Tools.DoubleToString(name.dValue);
                        break;
                    }
                case JSObjectType.NotExist:
                    throw new InvalidOperationException("Variable not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = GetMember(n, true);
            res = (res.valueType >= JSObjectType.Undefined);
            return res;
        }

        [DoNotEnumerate]
        public static JSObject preventExtensions(JSObject args)
        {
            var obj = args["0"];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Prevent the expansion can only for objects")));
            if (obj.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Can not prevent extensions for null")));
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            var res = (obj.oValue as JSObject);
            if (res != null)
                res.attributes |= JSObjectAttributesInternal.Immutable;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject isExtensible(JSObject args)
        {
            var obj = args["0"];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isExtensible called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isExtensible called on null."));
            return ((obj.oValue as JSObject ?? obj).attributes & JSObjectAttributesInternal.Immutable) == 0;
        }

        [DoNotEnumerate]
        public static JSObject isSealed(JSObject args)
        {
            var obj = args["0"];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isSealed called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isSealed called on null."));
            if (((obj = obj.oValue as JSObject ?? obj).attributes & JSObjectAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            if (obj is BaseTypes.Array)
            {
                var arr = obj as BaseTypes.Array;
                for (var i = arr.data.Count; i-- > 0; )
                    if (arr.data[i] != null
                        && arr.data[i].valueType >= JSObjectType.Object && arr.data[i].oValue != null
                        && (arr.data[i].attributes & JSObjectAttributesInternal.NotConfigurable) == 0)
                        return false;
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if (f.Value.valueType >= JSObjectType.Object && f.Value.oValue != null && (f.Value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0)
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject seal(JSObject args)
        {
            var obj = args["0"];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.seal called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.seal called on null."));
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            (obj.oValue as JSObject ?? obj).attributes |= JSObjectAttributesInternal.Immutable;
            foreach (var f in obj.fields)
                f.Value.attributes |= JSObjectAttributesInternal.NotConfigurable;
            return obj;
        }

        [DoNotEnumerate]
        public static JSObject isFrozen(JSObject args)
        {
            var obj = args["0"];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isFrozen called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isFrozen called on null."));
            if (((obj = obj.oValue as JSObject ?? obj).attributes & JSObjectAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            if (obj is BaseTypes.Array)
            {
                var arr = obj as BaseTypes.Array;
                for (var i = arr.data.Count; i-- > 0; )
                    if (arr.data[i] != null
                        &&
                        ((arr.data[i].attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                                || (arr.data[i].valueType != JSObjectType.Property && (arr.data[i].attributes & JSObjectAttributesInternal.ReadOnly) == 0)))
                        return false;
            }
            else if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if ((f.Value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                            || (f.Value.valueType != JSObjectType.Property && (f.Value.attributes & JSObjectAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public virtual JSObject propertyIsEnumerable(JSObject args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("propertyIsEnumerable calling on null.")));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("propertyIsEnumerable calling on undefined value.")));
            JSObject name = args.GetMember("0");
            string n = name.ToString();
            var res = GetMember(n);
            res = (res.valueType >= JSObjectType.Undefined) && ((res.attributes & JSObjectAttributesInternal.DoNotEnum) == 0);
            return res;
        }

        [Hidden]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [DoNotEnumerate]
        public static JSObject getPrototypeOf(JSObject args)
        {
            if (args.GetMember("0").valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Parameter isn't an Object.")));
            var res = args.GetMember("0")["__proto__"];
            if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
                res = (res.oValue as TypeProxy).prototypeInstance;
            return res;
        }

        [ParametersCount(2)]
        [DoNotEnumerate]
        public static JSObject getOwnPropertyDescriptor(JSObject args)
        {
            var source = args.GetMember("0");
            if (source.valueType <= JSObjectType.Undefined)
                throw new JSException(TypeProxy.Proxy(new TypeError("Object.getOwnPropertyDescriptor called on undefined.")));
            if (source.valueType < JSObjectType.Object)
                throw new JSException(TypeProxy.Proxy(new TypeError("Object.getOwnPropertyDescriptor called on non-object.")));
            var obj = source.GetMember(args.GetMember("1").ToString(), true);
            if (obj.valueType < JSObjectType.Undefined)
                return undefined;
            var res = CreateObject();
            if (obj.valueType != JSObjectType.Property || (obj.attributes & JSObjectAttributesInternal.Field) != 0)
            {
                if (obj.valueType == JSObjectType.Property)
                    res["value"] = (obj.oValue as Function[])[1].Invoke(source, null);
                else
                    res["value"] = obj;
                res["writable"] = obj.valueType < JSObjectType.Undefined || (obj.attributes & JSObjectAttributesInternal.ReadOnly) == 0;
            }
            else
            {
                res["set"] = (obj.oValue as Function[])[0];
                res["get"] = (obj.oValue as Function[])[1];
            }
            res["configurable"] = (obj.attributes & JSObjectAttributesInternal.NotConfigurable) == 0 || (obj.attributes & JSObjectAttributesInternal.DoNotDelete) == 0;
            res["enumerable"] = (obj.attributes & JSObjectAttributesInternal.DoNotEnum) == 0;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject getOwnPropertyNames(JSObject args)
        {
            var obj = args.GetMember("0");
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyNames called on non-object value."));
            if (obj.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Cannot get property names of null")));
            return new BaseTypes.Array((obj.oValue as JSObject ?? obj).GetEnumeratorImpl(false));
        }

        [DoNotEnumerate]
        public static JSObject keys(JSObject args)
        {
            var obj = args.GetMember("0");
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.keys called on non-object value."));
            if (obj.oValue == null)
                throw new JSException(TypeProxy.Proxy(new TypeError("Cannot get property names of null")));
            return new BaseTypes.Array((obj.oValue as JSObject ?? obj).GetEnumeratorImpl(true));
        }

        [Hidden]
        public static implicit operator JSObject(char value)
        {
            return new BaseTypes.String(value.ToString());
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static implicit operator JSObject(bool value)
        {
            return (BaseTypes.Boolean)value;
        }

        [Hidden]
        public static implicit operator JSObject(int value)
        {
            return (BaseTypes.Number)value;
        }

        [Hidden]
        public static implicit operator JSObject(long value)
        {
            return (BaseTypes.Number)(double)value;
        }

        [Hidden]
        public static implicit operator JSObject(double value)
        {
            return (BaseTypes.Number)value;
        }

        [Hidden]
        public static implicit operator JSObject(string value)
        {
            return new BaseTypes.String(value);
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static explicit operator bool(JSObject obj)
        {
            var vt = obj.valueType;
            switch (vt)
            {
                case JSObjectType.Int:
                case JSObjectType.Bool:
                    return obj.iValue != 0;
                case JSObjectType.Double:
                    return obj.dValue != 0.0 && !double.IsNaN(obj.dValue);
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                    return obj.oValue != null;
                case JSObjectType.String:
                    return !string.IsNullOrEmpty(obj.oValue as string);
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    return false;
                case JSObjectType.NotExist:
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
