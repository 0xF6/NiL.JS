using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS.Core
{
    [Serializable]
    public enum JSObjectType : int
    {
        NotExists = 0,
        NotExistsInObject = 1,
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
        /// ������� eval()
        /// </summary>
        Eval = 1 << 20,
        /// <summary>
        /// ��������� ������.
        /// </summary>
        Temporary = 1 << 21,
        /// <summary>
        /// ������ ��� ���������� ����� ��������� ��� �������� � ����� �������.
        /// </summary>
        Cloned = 1 << 22,
        /// <summary>
        /// ���������, ����������� ��� ������������ ��������.
        /// </summary>
        PrivateAttributes = Immutable | ProxyPrototype | Field,
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
    public class JSObject : IEnumerable<string>, IEnumerable, ICloneable, IComparable<JSObject>
    {
        [Hidden]
        internal static readonly AssignCallback ErrorAssignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Invalid left-hand side"))); };
        [Hidden]
        internal static readonly IEnumerator<string> EmptyEnumerator = ((IEnumerable<string>)(new string[0])).GetEnumerator();
        [Hidden]
        internal static readonly JSObject undefined = new JSObject() { valueType = JSObjectType.Undefined, attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject };
        [Hidden]
        internal static readonly JSObject notExists = new JSObject() { valueType = JSObjectType.NotExists, attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.SystemObject };
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
        public virtual JSObject this[string name]
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
                    case JSObjectType.NotExistsInObject:
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

        [Hidden]
        public JSObject(object content)
            : this(true)
        {
            oValue = content;
            valueType = JSObjectType.Object;
        }

        [DoNotEnumerate]
        [ParametersCount(2)]
        public static JSObject create(Arguments args)
        {
            var proto = args[0];
            if (proto.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Prototype may be only Object or null."));
            proto = proto.oValue as JSObject ?? proto;
            var members = args[1];
            if (members.valueType >= JSObjectType.Object && members.oValue == null)
                throw new JSException(new TypeError("Properties descriptor may be only Object."));
            var res = CreateObject();
            if (proto.valueType >= JSObjectType.Object && proto.oValue != null)
                res.__proto__ = proto;
            if (members.valueType >= JSObjectType.Object)
            {
                members = (members.oValue as JSObject ?? members);
                foreach (var member in members)
                {
                    var desc = members[member];
                    if (desc.valueType == JSObjectType.Property)
                    {
                        var getter = (desc.oValue as Function[])[1];
                        if (getter == null || getter.oValue == null)
                            throw new JSException(new TypeError("Invalid property descriptor for property " + member + " ."));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    if (desc.valueType < JSObjectType.Object || desc.oValue == null)
                        throw new JSException(new TypeError("Invalid property descriptor for property " + member + " ."));
                    var value = desc["value"];
                    if (value.valueType == JSObjectType.Property)
                    {
                        value = ((value.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                        if (value.valueType < JSObjectType.Undefined)
                            value = undefined;
                    }
                    var configurable = desc["configurable"];
                    if (configurable.valueType == JSObjectType.Property)
                    {
                        configurable = ((configurable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                        if (configurable.valueType < JSObjectType.Undefined)
                            configurable = undefined;
                    }
                    var enumerable = desc["enumerable"];
                    if (enumerable.valueType == JSObjectType.Property)
                    {
                        enumerable = ((enumerable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                        if (enumerable.valueType < JSObjectType.Undefined)
                            enumerable = undefined;
                    }
                    var writable = desc["writable"];
                    if (writable.valueType == JSObjectType.Property)
                    {
                        writable = ((writable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                        if (writable.valueType < JSObjectType.Undefined)
                            writable = undefined;
                    }
                    var get = desc["get"];
                    if (get.valueType == JSObjectType.Property)
                    {
                        get = ((get.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                        if (get.valueType < JSObjectType.Undefined)
                            get = undefined;
                    }
                    var set = desc["set"];
                    if (set.valueType == JSObjectType.Property)
                    {
                        set = ((set.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                        if (set.valueType < JSObjectType.Undefined)
                            set = undefined;
                    }
                    if (value.isExist && (get.isExist || set.isExist))
                        throw new JSException(new TypeError("Property can not have getter or setter and default value."));
                    if (writable.isExist && (get.isExist || set.isExist))
                        throw new JSException(new TypeError("Property can not have getter or setter and writable attribute."));
                    if (get.isDefinded && get.valueType != JSObjectType.Function)
                        throw new JSException(new TypeError("Getter mast be a function."));
                    if (set.isDefinded && set.valueType != JSObjectType.Function)
                        throw new JSException(new TypeError("Setter mast be a function."));
                    JSObject obj = new JSObject();
                    res.fields[member] = obj;
                    obj.attributes |=
                        JSObjectAttributesInternal.DoNotEnum
                        | JSObjectAttributesInternal.NotConfigurable
                        | JSObjectAttributesInternal.DoNotDelete
                        | JSObjectAttributesInternal.ReadOnly;
                    if ((bool)enumerable)
                        obj.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                    if ((bool)configurable)
                        obj.attributes &= ~(JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete);
                    if (value.isExist)
                    {
                        var atr = obj.attributes;
                        obj.attributes = 0;
                        obj.Assign(value);
                        obj.attributes = atr;
                        if ((bool)writable)
                            obj.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                    }
                    else if (get.isExist || set.isExist)
                    {
                        Function setter = null, getter = null;
                        if (obj.valueType == JSObjectType.Property)
                        {
                            setter = (obj.oValue as Function[])[0];
                            getter = (obj.oValue as Function[])[1];
                        }
                        obj.valueType = JSObjectType.Property;
                        obj.oValue = new Function[]
                        {
                            set.isExist ? set.oValue as Function : setter,
                            get.isExist ? get.oValue as Function : getter
                        };
                    }
                    else if ((bool)writable) // �� ��� ������, ����� � ����������� �� ������� �� ��������, �� ������/������
                        obj.attributes &= ~JSObjectAttributesInternal.ReadOnly;
                }
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

        [ParametersCount(2)]
        [DoNotEnumerate]
        public static JSObject defineProperties(Arguments args)
        {
            var target = args[0];
            if (target.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Property define may only for Objects."));
            if (target.oValue == null)
                throw new JSException(new TypeError("Can not define properties of null."));
            target = target.oValue as JSObject ?? target;
            var members = args[1];
            if (!members.isDefinded)
                throw new JSException(new TypeError("Properties descriptor can not be undefined."));
            if (members.valueType < JSObjectType.Object)
                return target;
            if (members.oValue == null)
                throw new JSException(new TypeError("Properties descriptor can not be null."));
            if (members.valueType > JSObjectType.Undefined)
            {
                if (members.valueType < JSObjectType.Object)
                    throw new JSException(new TypeError("Properties descriptor may be only Object."));
                foreach (var memberName in members)
                {
                    var desc = members[memberName];
                    if (desc.valueType == JSObjectType.Property)
                    {
                        var getter = (desc.oValue as Function[])[1];
                        if (getter == null || getter.oValue == null)
                            throw new JSException(new TypeError("Invalid property descriptor for property " + memberName + " ."));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    if (desc.valueType < JSObjectType.Object || desc.oValue == null)
                        throw new JSException(new TypeError("Invalid property descriptor for property " + memberName + " ."));
                    definePropertyImpl(target, desc, memberName);
                }
            }
            return target;
        }

        [DoNotEnumerate]
        [ParametersCount(3)]
        public static JSObject defineProperty(Arguments args)
        {
            var target = args[0];
            if (target.valueType < JSObjectType.Object || target.oValue == null)
                throw new JSException(new TypeError("Object.defineProperty cannot apply to non-object."));
            target = target.oValue as JSObject ?? target;
            if (target.valueType <= JSObjectType.Undefined)
                return undefined;
            var desc = args[2];
            if (desc.valueType < JSObjectType.Object || desc.oValue == null)
                throw new JSException(new TypeError("Invalid property descriptor."));
            string memberName = args[1].ToString();
            return definePropertyImpl(target, desc, memberName);
        }

        private static JSObject definePropertyImpl(JSObject target, JSObject desc, string memberName)
        {
            var value = desc["value"];
            if (value.valueType == JSObjectType.Property)
            {
                value = ((value.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                if (value.valueType < JSObjectType.Undefined)
                    value = undefined;
            }
            var configurable = desc["configurable"];
            if (configurable.valueType == JSObjectType.Property)
            {
                configurable = ((configurable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                if (configurable.valueType < JSObjectType.Undefined)
                    configurable = undefined;
            }
            var enumerable = desc["enumerable"];
            if (enumerable.valueType == JSObjectType.Property)
            {
                enumerable = ((enumerable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                if (enumerable.valueType < JSObjectType.Undefined)
                    enumerable = undefined;
            }
            var writable = desc["writable"];
            if (writable.valueType == JSObjectType.Property)
            {
                writable = ((writable.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                if (writable.valueType < JSObjectType.Undefined)
                    writable = undefined;
            }
            var get = desc["get"];
            if (get.valueType == JSObjectType.Property)
            {
                get = ((get.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                if (get.valueType < JSObjectType.Undefined)
                    get = undefined;
            }
            var set = desc["set"];
            if (set.valueType == JSObjectType.Property)
            {
                set = ((set.oValue as Function[])[1] ?? Function.emptyFunction).Invoke(desc, null);
                if (set.valueType < JSObjectType.Undefined)
                    set = undefined;
            }
            if (value.isExist && (get.isExist || set.isExist))
                throw new JSException(new TypeError("Property can not have getter or setter and default value."));
            if (writable.isExist && (get.isExist || set.isExist))
                throw new JSException(new TypeError("Property can not have getter or setter and writable attribute."));
            if (get.isDefinded && get.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("Getter mast be a function."));
            if (set.isDefinded && set.valueType != JSObjectType.Function)
                throw new JSException(new TypeError("Setter mast be a function."));
            JSObject obj = null;
            obj = target.DefineMember(memberName);
            if ((obj.attributes & JSObjectAttributesInternal.Argument) != 0 && (set.isExist || get.isExist))
            {
                var ti = 0;
                if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                    (target as Arguments)[ti] = obj = obj.CloneImpl();
                else
                    target.fields[memberName] = obj = obj.CloneImpl();
                obj.attributes &= ~JSObjectAttributesInternal.Argument;
            }
            if ((obj.attributes & JSObjectAttributesInternal.SystemObject) != 0)
                throw new JSException(new TypeError("Can not define property \"" + memberName + "\". Object is immutable."));

            if (target is BaseTypes.Array)
            {
                if (memberName == "length")
                {
                    try
                    {
                        if (value.isExist)
                        {
                            if ((obj.oValue as Function[])[0] != null)
                            {
                                var nlenD = Tools.JSObjectToDouble(value);
                                var nlen = (uint)nlenD;
                                if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                                    throw new JSException(new RangeError("Invalid array length"));
                                if (!(target as BaseTypes.Array).setLength(nlen))
                                    throw new JSException(new TypeError("Unable to reduce length because not configurable elements"));
                            }
                            else if (!StrictEqual.Check((obj.oValue as Function[])[1].Invoke(target, null), value, null))
                                throw new JSException(new TypeError("Cannot redefine property length."));
                            value = notExists; // ����� ������ ����������������, ������� ��� ���� ����� � �����,
                            // � ��� ������ ��������, �������, ��� �������� ����, ���� ���������� �� ����
                        }
                    }
                    finally
                    {
                        if (writable.isExist && !(bool)writable)
                        {
                            (obj.oValue as Function[])[0] = null;
                            obj.attributes |= JSObjectAttributesInternal.ReadOnly;
                        }
                    }
                }
            }

            var newProp = obj.valueType < JSObjectType.Undefined;
            var config = (obj.attributes & JSObjectAttributesInternal.NotConfigurable) == 0 || newProp;

            if (!config)
            {
                // enumerable ������ �����������
                if (enumerable.isExist && (obj.attributes & JSObjectAttributesInternal.DoNotEnum) != 0 == (bool)enumerable)
                    throw new JSException(new TypeError("Cannot change enumerable attribute for non configurable property."));

                // writable ������ ��������
                if (writable.isExist && (obj.attributes & JSObjectAttributesInternal.ReadOnly) != 0 && (bool)writable)
                    throw new JSException(new TypeError("Cannot change writable attribute for non configurable property."));

                if (configurable.isExist && (bool)configurable)
                    throw new JSException(new TypeError("Cannot set configurate attribute to true."));

                if ((obj.valueType != JSObjectType.Property || ((obj.attributes & JSObjectAttributesInternal.Field) != 0)) && (set.isExist || get.isExist))
                    throw new JSException(new TypeError("Cannot redefine not configurable property from immediate value to accessor property"));
                if (obj.valueType == JSObjectType.Property && (obj.attributes & JSObjectAttributesInternal.Field) == 0 && value.isExist)
                    throw new JSException(new TypeError("Cannot redefine not configurable property from accessor property to immediate value"));
                if (obj.valueType == JSObjectType.Property && (obj.attributes & JSObjectAttributesInternal.Field) == 0
                    && set.isExist
                    && (((obj.oValue as Function[])[0] != null && (obj.oValue as Function[])[0].oValue != set.oValue)
                        || ((obj.oValue as Function[])[0] == null && set.isDefinded)))
                    throw new JSException(new TypeError("Cannot redefine setter of not configurable property."));
                if (obj.valueType == JSObjectType.Property && (obj.attributes & JSObjectAttributesInternal.Field) == 0
                    && get.isExist
                    && (((obj.oValue as Function[])[1] != null && (obj.oValue as Function[])[1].oValue != get.oValue)
                        || ((obj.oValue as Function[])[1] == null && get.isDefinded)))
                    throw new JSException(new TypeError("Cannot redefine getter of not configurable property."));
            }

            if (value.isExist)
            {
                if (!config
                    && (obj.attributes & JSObjectAttributesInternal.ReadOnly) != 0
                    && !((StrictEqual.Check(obj, value, null) && ((obj.valueType == JSObjectType.Undefined && value.valueType == JSObjectType.Undefined) || !obj.isNumber || !value.isNumber || (1.0 / Tools.JSObjectToDouble(obj) == 1.0 / Tools.JSObjectToDouble(value))))
                        || (obj.valueType == JSObjectType.Double && value.valueType == JSObjectType.Double && double.IsNaN(obj.dValue) && double.IsNaN(value.dValue))))
                    throw new JSException(new TypeError("Cannot change value of not configurable not writable peoperty."));
                //if ((obj.attributes & JSObjectAttributesInternal.ReadOnly) == 0 || obj.valueType == JSObjectType.Property)
                {
                    obj.valueType = JSObjectType.Undefined; // ��� ����� ���� Property, �� ������� �� ��������
                    var atrbts = obj.attributes;
                    obj.attributes = 0;
                    obj.Assign(value);
                    obj.attributes = atrbts;
                }
            }
            else if (get.isExist || set.isExist)
            {
                Function setter = null, getter = null;
                if (obj.valueType == JSObjectType.Property)
                {
                    setter = (obj.oValue as Function[])[0];
                    getter = (obj.oValue as Function[])[1];
                }
                obj.valueType = JSObjectType.Property;
                obj.oValue = new Function[]
                {
                    set.isExist ? set.oValue as Function : setter,
                    get.isExist ? get.oValue as Function : getter
                };
            }
            else if (newProp)
                obj.valueType = JSObjectType.Undefined;
            if (newProp)
            {
                obj.attributes |=
                    JSObjectAttributesInternal.DoNotEnum
                    | JSObjectAttributesInternal.DoNotDelete
                    | JSObjectAttributesInternal.NotConfigurable
                    | JSObjectAttributesInternal.ReadOnly;
            }
            else
            {
                var atrbts = obj.attributes;
                if (configurable.isExist && (config || !(bool)configurable))
                    obj.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
                if (enumerable.isExist && (config || !(bool)enumerable))
                    obj.attributes |= JSObjectAttributesInternal.DoNotEnum;
                if (writable.isExist && (config || !(bool)writable))
                    obj.attributes |= JSObjectAttributesInternal.ReadOnly;

                if (obj.attributes != atrbts && (obj.attributes & JSObjectAttributesInternal.Argument) != 0)
                {
                    var ti = 0;
                    if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                        (target as Arguments)[ti] = obj = obj.CloneImpl();
                    else
                        target.fields[memberName] = obj = obj.CloneImpl();
                    obj.attributes &= ~JSObjectAttributesInternal.Argument;
                }
            }

            if (config)
            {
                if ((bool)enumerable)
                    obj.attributes &= ~JSObjectAttributesInternal.DoNotEnum;
                if ((bool)configurable)
                    obj.attributes &= ~(JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete);
                if ((bool)writable)
                    obj.attributes &= ~JSObjectAttributesInternal.ReadOnly;
            }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject freeze(Arguments args)
        {
            var obj = args[0];
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
                foreach (var element in arr.data)
                    element.Value.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete;
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                    arg[i].attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete;
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                    f.Value.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete;
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
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, false, false);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc != null ? cc.wrap(name) : (JSObject)name, false, false);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
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
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, false, own);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc != null ? cc.wrap(name) : (JSObject)name, false, own);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        /// <summary>
        /// ���������� ���� ������� � ��������� ������.
        /// </summary>
        /// <param name="name">��� �����.</param>
        /// <returns>������, �������������� ����������� ����.</returns>
        [Hidden]
        public JSObject DefineMember(string name)
        {
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, true, true);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc != null ? cc.wrap(name) : (JSObject)name, true, true);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        [Hidden]
        internal protected JSObject GetMember(string name, bool createMember, bool own)
        {
            var cc = Context.CurrentContext;
            if (cc == null)
                return GetMember((JSObject)name, createMember, own);
            var oi = cc.tempContainer.iValue;
            var od = cc.tempContainer.dValue;
            object oo = cc.tempContainer.oValue;
            var ovt = cc.tempContainer.valueType;
            try
            {
                return GetMember(cc != null ? cc.wrap(name) : (JSObject)name, createMember, own);
            }
            finally
            {
                cc.tempContainer.iValue = oi;
                cc.tempContainer.oValue = oo;
                cc.tempContainer.dValue = od;
                cc.tempContainer.valueType = ovt;
            }
        }

        [Hidden]
        internal protected virtual JSObject GetMember(JSObject name, bool createMember, bool own)
        {
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("Can't get property \"" + name + "\" of \"undefined\"."));
            switch (valueType)
            {
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

                        double dindex = 0.0;
                        int index = 0;
                        dindex = Tools.JSObjectToDouble(name as JSObject);

                        if (dindex > 0.0
                            && !double.IsNaN(dindex)
                            && !double.IsInfinity(dindex)
                            && ((index = (int)dindex) == dindex)
                            && oValue.ToString().Length > index)
                            return oValue.ToString()[index];

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
                        if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0 && !(oValue is BaseTypes.Array))
                            return __proto__.GetMember(name, createMember, own);
                        if (oValue != this && (oValue is JSObject))
                        {
                            var inObj = oValue as JSObject;
                            try
                            {
                                var res = inObj.GetMember(name, createMember, own);
                                if (inObj.valueType < JSObjectType.Object && res.valueType < JSObjectType.Undefined)
                                    break;
                                return res;
                            }
                            finally
                            {
                                if (fields == null)
                                    fields = inObj.fields;
                            }
                        }
                        if (oValue == null)
                            throw new JSException(new TypeError("Can't get property \"" + name + "\" of \"null\""));
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
                            throw new JSException(new TypeError("Can't get property \"" + name + "\" of \"null\""));
                        if (oValue == this)
                            break;
                        try
                        {
                            return (oValue as JSObject).GetMember(name, createMember, own);
                        }
                        finally
                        {
                            if (fields == null)
                                fields = (oValue as JSObject).fields;
                        }
                    }
                case JSObjectType.Property:
                    throw new InvalidOperationException("Try to get member of property");
                default:
                    throw new NotImplementedException();
            }
            return DefaultFieldGetter(name, createMember, own);
        }

        [Hidden]
        protected JSObject DefaultFieldGetter(JSObject nameObj, bool forWrite, bool own)
        {
            string name = nameObj.ToString();
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
                            res = proto.GetMember(nameObj, false, own);
                            if (own
                                && (res.valueType != JSObjectType.Property
                                    || (res.attributes & JSObjectAttributesInternal.Field) == 0)
                                && (attributes & JSObjectAttributesInternal.ProxyPrototype) == 0)
                                res = null;
                            else if (res.valueType < JSObjectType.Undefined)
                                res = null;
                        }
                        if (res == null)
                        {
                            if (!forWrite || (attributes & JSObjectAttributesInternal.Immutable) != 0)
                                return notExists;
                            res = new JSObject()
                            {
                                valueType = JSObjectType.NotExistsInObject
                            };
                            if (fields == null)
                                fields = new Dictionary<string, JSObject>();
                            fields[name] = res;
                        }
                        else if (forWrite && ((res.attributes & JSObjectAttributesInternal.SystemObject) != 0 || fromProto))
                        {
                            if ((res.attributes & JSObjectAttributesInternal.ReadOnly) == 0
                                && (res.valueType != JSObjectType.Property || own))
                            {
                                var t = res.CloneImpl();
                                if (fields == null)
                                    fields = new Dictionary<string, JSObject>();
                                fields[name] = t;
                                res = t;
                            }
                        }
                        if (res.valueType == JSObjectType.NotExists)
                            res.valueType = JSObjectType.NotExistsInObject;
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
                throw new JSException(new TypeError("Can't convert object to primitive value."));
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
                throw new JSException(new TypeError("Can't convert object to primitive value."));
            }
            return this;
        }

        [Hidden]
        public virtual void Assign(JSObject value)
        {
            if (assignCallback != null)
                assignCallback(this);
#if DEBUG
            if (valueType == JSObjectType.Property)
                throw new InvalidOperationException("Try to assign to property.");
#endif
            if ((attributes & (JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.SystemObject)) != 0)
                return;
            if (value == this)
                return;
            if (value != null)
            {
                this.valueType = (value.valueType & ~(JSObjectType.NotExistsInObject | JSObjectType.NotExists)) | JSObjectType.Undefined;
                if (valueType < JSObjectType.String)
                {
                    this.iValue = value.iValue;
                    this.dValue = value.dValue;
                    this.fields = null;
                    this.oValue = null;
                }
                else
                {
                    if (valueType < JSObjectType.Object)
                        fields = null;
                    else
                        fields = value.fields;
                    oValue = value.oValue;
                }
                __proto__ = value.__proto__;
                this.attributes =
                    (this.attributes & ~JSObjectAttributesInternal.PrivateAttributes)
                    | (value.attributes & JSObjectAttributesInternal.PrivateAttributes);
                return;
            }
            this.__proto__ = null;
            this.fields = null;
            this.oValue = null;
            this.valueType = JSObjectType.Undefined;
        }

        [Hidden]
        public virtual object Clone()
        {
            return CloneImpl();
        }

        internal JSObject CloneImpl()
        {
            var res = new JSObject();
            res.Assign(this);
            res.attributes = this.attributes & ~JSObjectAttributesInternal.SystemObject;
            return res;
        }

        [Hidden]
        public override string ToString()
        {
            if (valueType == JSObjectType.String)
                return oValue as string;
            if (valueType <= JSObjectType.Undefined)
                return "undefined";
            if (valueType == JSObjectType.Property)
            {
                var tempStr = "[";
                if ((oValue as Function[])[1] != null)
                    tempStr += "Getter";
                if ((oValue as Function[])[0] != null)
                    tempStr += (tempStr.Length != 1 ? "/Setter" : "Setter");
                if (tempStr.Length == 1)
                    return "[Invalid Property]";
                tempStr += "]";
                return tempStr;
            }
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
                    if (f.Value.isExist && (!hideNonEnum || (f.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [ParametersCount(0)]
        public virtual JSObject toString(Arguments args)
        {
            var self = this.oValue as JSObject ?? this;
            switch (self.valueType)
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
                        if (self.oValue is ThisBind)
                            return self.oValue.ToString();
                        if (self.oValue is TypeProxy)
                        {
                            var ht = (self.oValue as TypeProxy).hostedType;
                            if (ht == typeof(RegExp))
                                return "[object Object]";
                            return "[object " + (ht == typeof(JSObject) ? typeof(System.Object) : ht).Name + "]";
                        }
                        if (self.oValue != null)
                            return "[object " + (self.oValue.GetType() == typeof(JSObject) ? typeof(System.Object) : self.oValue.GetType()).Name + "]";
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
            var self = this.oValue as JSObject ?? this;
            if (self.valueType >= JSObjectType.Object && self.oValue == null)
                throw new JSException(new TypeError("toLocaleString calling on null."));
            if (self.valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("toLocaleString calling on undefined value."));
            return self.toString(null);
        }

        [DoNotEnumerate]
        public virtual JSObject valueOf()
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(new TypeError("valueOf calling on null."));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("valueOf calling on undefined value."));
            return valueType < JSObjectType.Object ? new JSObject() { valueType = JSObjectType.Object, oValue = this } : this;
        }

        [DoNotEnumerate]
        public virtual JSObject isPrototypeOf(Arguments args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(new TypeError("isPrototypeOf calling on null."));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("isPrototypeOf calling on undefined value."));
            if (args.GetMember("length").iValue == 0)
                return false;
            var a = args[0];
            a = a.GetMember("__proto__");
            if (this.valueType >= JSObjectType.Object)
            {
                if (this.oValue != null)
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
            }
            else
            {
                if (a.oValue == this.oValue)
                    return true;
                var pi = (a.oValue is TypeProxy) ? (a.oValue as TypeProxy).prototypeInstance : null;
                if (pi != null && (this == pi || this == pi.oValue))
                    return true;
            }
            return false;
        }

        [DoNotEnumerate]
        public virtual JSObject hasOwnProperty(Arguments args)
        {
            JSObject name = args[0];
            string n = "";
            switch (name.valueType)
            {
                case JSObjectType.Undefined:
                case JSObjectType.NotExistsInObject:
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
                        var pn = name.ToPrimitiveValue_Value_String();
                        if (pn.valueType == JSObjectType.String)
                            n = pn.oValue as string;
                        if (pn.valueType == JSObjectType.Int)
                            n = pn.iValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        if (pn.valueType == JSObjectType.Double)
                            n = Tools.DoubleToString(pn.dValue);
                        break;
                    }
                case JSObjectType.NotExists:
                    throw new InvalidOperationException("Variable not defined.");
                default:
                    throw new NotImplementedException("Object.hasOwnProperty. Invalid Value Type");
            }
            var res = GetMember(n, true);
            return res.isExist;
        }

        [DoNotEnumerate]
        public static JSObject preventExtensions(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Prevent the expansion can only for objects"));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Can not prevent extensions for null"));
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            var res = (obj.oValue as JSObject);
            if (res != null)
                res.attributes |= JSObjectAttributesInternal.Immutable;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject isExtensible(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isExtensible called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isExtensible called on null."));
            return ((obj.oValue as JSObject ?? obj).attributes & JSObjectAttributesInternal.Immutable) == 0;
        }

        [DoNotEnumerate]
        public static JSObject isSealed(Arguments args)
        {
            var obj = args[0];
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
                foreach (var node in arr.data.Nodes)
                {
                    if (node.value.valueType >= JSObjectType.Object && node.value.oValue != null
                        && (node.value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0)
                        return false;
                }
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
        public static JSObject seal(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.seal called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.seal called on null."));
            obj = obj.oValue as JSObject ?? obj;
            obj.attributes |= JSObjectAttributesInternal.Immutable;
            (obj.oValue as JSObject ?? obj).attributes |= JSObjectAttributesInternal.Immutable;
            if (obj is BaseTypes.Array)
            {
                var arr = obj as BaseTypes.Array;
                foreach (var element in arr.data)
                    element.Value.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                    arg[i].attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                    f.Value.attributes |= JSObjectAttributesInternal.NotConfigurable | JSObjectAttributesInternal.DoNotDelete;
            return obj;
        }

        [DoNotEnumerate]
        public static JSObject isFrozen(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.isFrozen called on non-object."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Object.isFrozen called on null."));
            if (obj.oValue is JSObject && (obj.oValue as JSObject).valueType >= JSObjectType.Object)
                obj = obj.oValue as JSObject;
            if ((obj.attributes & JSObjectAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            if (obj is BaseTypes.Array)
            {
                var arr = obj as BaseTypes.Array;
                foreach (var node in arr.data.Nodes)
                {
                    if (((node.value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                        || (node.value.valueType != JSObjectType.Property && (node.value.attributes & JSObjectAttributesInternal.ReadOnly) == 0)))
                        return false;
                }
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                {
                    if ((arg[i].attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                            || (arg[i].valueType != JSObjectType.Property && (arg[i].attributes & JSObjectAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if ((f.Value.attributes & JSObjectAttributesInternal.NotConfigurable) == 0
                            || (f.Value.valueType != JSObjectType.Property && (f.Value.attributes & JSObjectAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public virtual JSObject propertyIsEnumerable(Arguments args)
        {
            if (valueType >= JSObjectType.Object && oValue == null)
                throw new JSException(new TypeError("propertyIsEnumerable calling on null."));
            if (valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("propertyIsEnumerable calling on undefined value."));
            JSObject name = args[0];
            string n = name.ToString();
            var res = GetMember(n, true);
            res = (res.isExist) && ((res.attributes & JSObjectAttributesInternal.DoNotEnum) == 0);
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
        public static JSObject getPrototypeOf(Arguments args)
        {
            if (args[0].valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Parameter isn't an Object."));
            var res = args[0]["__proto__"];
            if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
                res = (res.oValue as TypeProxy).prototypeInstance;
            return res;
        }

        [ParametersCount(2)]
        [DoNotEnumerate]
        public static JSObject getOwnPropertyDescriptor(Arguments args)
        {
            var source = args[0];
            if (source.valueType <= JSObjectType.Undefined)
                throw new JSException(new TypeError("Object.getOwnPropertyDescriptor called on undefined."));
            if (source.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyDescriptor called on non-object."));
            var obj = source.GetMember(args[1], false, true);
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
        public static JSObject getOwnPropertyNames(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyNames called on non-object value."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Cannot get property names of null"));
            return new BaseTypes.Array((obj.oValue as JSObject ?? obj).GetEnumeratorImpl(false));
        }

        [DoNotEnumerate]
        public static JSObject keys(Arguments args)
        {
            var obj = args[0];
            if (obj.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Object.keys called on non-object value."));
            if (obj.oValue == null)
                throw new JSException(new TypeError("Cannot get property names of null"));
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
            //return new JSObject() { valueType = JSObjectType.Bool, iValue = value ? 1 : 0 };
        }

        [Hidden]
        public static implicit operator JSObject(int value)
        {
            return new JSObject() { valueType = JSObjectType.Int, iValue = value };
        }

        [Hidden]
        public static implicit operator JSObject(long value)
        {
            return new JSObject() { valueType = JSObjectType.Double, dValue = value };
        }

        [Hidden]
        public static implicit operator JSObject(double value)
        {
            return new JSObject() { valueType = JSObjectType.Double, dValue = value };
        }

        [Hidden]
        public static implicit operator JSObject(string value)
        {
            return new JSObject() { valueType = JSObjectType.String, oValue = value };
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [Hidden]
        public static explicit operator bool(JSObject obj)
        {
            switch (obj.valueType)
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
                case JSObjectType.NotExists:
                case JSObjectType.NotExistsInObject:
                case JSObjectType.Undefined:
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }

        [Hidden]
        public bool isExist
        {
            [Hidden]
#if INLINE
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get { return valueType >= JSObjectType.Undefined; }
        }

        [Hidden]
        public bool isDefinded
        {
            [Hidden]
#if INLINE
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get { return valueType > JSObjectType.Undefined; }
        }

        [Hidden]
        public bool isNumber
        {
            [Hidden]
#if INLINE
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            get { return valueType == JSObjectType.Int || valueType == JSObjectType.Double; }
        }

        #region ����� IComparable<JSObject>

        int IComparable<JSObject>.CompareTo(JSObject other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
