using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
    public class JSObject : JSValue
    {
        internal static JSObject GlobalPrototype;

        internal IDictionary<string, JSValue> fields;
        internal JSObject __prototype;

        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        [CLSCompliant(false)]
        public sealed override JSObject __proto__
        {
            [Hidden]
            get
            {
                if (GlobalPrototype == this)
                    return Null;
                if (!this.IsDefinded || this.IsNull)
                    throw new JSException(new TypeError("Can not get prototype of null or undefined"));
                if (valueType >= JSValueType.Object
                    && oValue != this // вот такого теперь быть не должно
                    && (oValue as JSObject) != null)
                    return (oValue as JSObject).__proto__;
                if (__prototype != null)
                {
                    if (__prototype.valueType < JSValueType.Object)
                        __prototype = GetDefaultPrototype(); // такого тоже
                    else if (__prototype.oValue == null)
                        return Null;
                    return __prototype;
                }
                return __prototype = GetDefaultPrototype();
            }
            [Hidden]
            set
            {
                if ((attributes & JSValueAttributesInternal.Immutable) != 0)
                    return;
                if (valueType < JSValueType.Object)
                    return;
                if (value != null && value.valueType < JSValueType.Object)
                    return;
                if (oValue != this && (oValue as JSObject) != null)
                {
                    (oValue as JSObject).__proto__ = value;
                    __prototype = null;
                    return;
                }
                if (value == null || value.oValue == null)
                {
                    __prototype = Null;
                }
                else
                {
                    var c = value.oValue as JSObject ?? value;
                    while (c != null && c != Null && c.valueType > JSValueType.Undefined)
                    {
                        if (c == this || c.oValue == this)
                            throw new JSException(new Error("Try to set cyclic __proto__ value."));
                        c = c.__proto__;
                    }
                    __prototype = value.oValue as JSObject ?? value;
                }
            }
        }

        [Hidden]
        public JSObject()
        {
            //valueType = JSObjectType.Undefined;
        }

        [Hidden]
        public JSObject(bool createFields)
            : this()
        {
            if (createFields)
                fields = JSObject.createFields();
        }

        [Hidden]
        public JSObject(object content)
            : this(true)
        {
            oValue = content;
            valueType = JSValueType.Object;
        }

        [Hidden]
        public static JSObject CreateObject()
        {
            return CreateObject(true);
        }

        [Hidden]
        public static JSObject CreateObject(bool createFields)
        {
            var t = new JSObject(true)
            {
                valueType = JSValueType.Object,
                __prototype = GlobalPrototype
            };
            t.oValue = t;
            return t;
        }

        protected internal override JSValue GetMember(JSValue key, bool forWrite, bool own)
        {
#if DEBUG
            // Это ошибочная ситуация, но, по крайней мере, так положение будет исправлено
            if (oValue != this && oValue is JSValue)
                return base.GetMember(key, forWrite, own);
#endif

            string name = null;
            if (forWrite || fields != null)
                name = key.ToString();
            JSValue res = null;
            JSObject proto = null;
            bool fromProto = (fields == null || !fields.TryGetValue(name, out res) || res.valueType < JSValueType.Undefined) && ((proto = __proto__).oValue != null);
            if (fromProto)
            {
                res = proto.GetMember(key, false, own);
                if (((own && ((res.attributes & JSValueAttributesInternal.Field) == 0 || res.valueType != JSValueType.Property))) || res.valueType < JSValueType.Undefined)
                    res = null;
            }
            if (res == null)
            {
                if (!forWrite || (attributes & JSValueAttributesInternal.Immutable) != 0)
                {
                    if (!own && string.CompareOrdinal(name, "__proto__") == 0)
                        return proto;
                    return notExists;
                }
                res = new JSValue()
                {
                    valueType = JSValueType.NotExistsInObject
                };
                if (fields == null)
                    fields = createFields();
                fields[name] = res;
            }
            else if (forWrite)
            {
                if (((res.attributes & JSValueAttributesInternal.SystemObject) != 0 || fromProto))
                {
                    if ((res.attributes & JSValueAttributesInternal.ReadOnly) == 0
                        && (res.valueType != JSValueType.Property || own))
                    {
                        res = res.CloneImpl();
                        if (fields == null)
                            fields = createFields();
                        fields[name] = res;
                    }
                }
            }

            res.valueType |= JSValueType.NotExistsInObject;
            return res;
        }

        protected internal override void SetMember(JSValue name, JSValue value, bool strict)
        {
            JSValue field;
            if (valueType >= JSValueType.Object && oValue != this)
            {
                if (oValue == null)
                    throw new JSException(new TypeError("Can not get property \"" + name + "\" of \"null\""));
                field = oValue as JSObject;
                if (field != null)
                {
                    field.SetMember(name, value, strict);
                    return;
                }
            }
            field = GetMember(name, true, false);
            if (field.valueType == JSValueType.Property)
            {
                var setter = (field.oValue as PropertyPair).set;
                if (setter != null)
                    setter.Invoke(this, new Arguments { value });
                else if (strict)
                    throw new JSException(new TypeError("Can not assign to readonly property \"" + name + "\""));
                return;
            }
            else if ((field.attributes & JSValueAttributesInternal.ReadOnly) != 0)
            {
                if (strict)
                    throw new JSException(new TypeError("Can not assign to readonly property \"" + name + "\""));
            }
            else
                field.Assign(value);
        }

        protected internal override bool DeleteMember(JSValue name)
        {
            JSValue field;
            if (valueType >= JSValueType.Object && oValue != this)
            {
                if (oValue == null)
                    throw new JSException(new TypeError("Can't get property \"" + name + "\" of \"null\""));
                field = oValue as JSObject;
                if (field != null)
                    return field.DeleteMember(name);
            }
            string tname = null;
            if (fields != null
                && fields.TryGetValue(tname = name.ToString(), out field)
                && (!field.IsExist || (field.attributes & JSValueAttributesInternal.DoNotDelete) == 0))
            {
                if ((field.attributes & JSValueAttributesInternal.SystemObject) == 0)
                    field.valueType = JSValueType.NotExistsInObject;
                return fields.Remove(tname);
            }
            field = GetMember(name, false, true);
            if (!field.IsExist)
                return true;
            if ((field.attributes & JSValueAttributesInternal.SystemObject) != 0)
                field = GetMember(name, true, true);
            if ((field.attributes & JSValueAttributesInternal.DoNotDelete) == 0)
            {
                field.valueType = JSValueType.NotExistsInObject;
                field.oValue = null;
                return true;
            }
            return false;
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (f.Value.IsExist && (!hideNonEnum || (f.Value.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                        yield return f.Key;
                }
            }
        }

        [Hidden]
        public sealed override void Assign(JSValue value)
        {
            if ((attributes & JSValueAttributesInternal.ReadOnly) == 0)
            {
                if (this is GlobalObject)
                    throw new JSException(new NiL.JS.BaseLibrary.ReferenceError("Invalid left-hand side"));
                throw new InvalidOperationException("Try to assign to a non-primitive value");
            }
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static IDictionary<string, JSValue> createFields()
        {
            return createFields(0);
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static IDictionary<string, JSValue> createFields(int p)
        {
            //return new Dictionary<string, JSObject>(p, System.StringComparer.Ordinal);
            return new StringMap2<JSValue>();
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        public static JSValue create(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Prototype may be only Object or null."));
            var proto = args[0].oValue as JSObject ?? Null;
            var members = args[1].oValue as JSObject ?? Null;
            if (args[1].valueType >= JSValueType.Object && members.oValue == null)
                throw new JSException(new TypeError("Properties descriptor may be only Object."));
            var res = CreateObject();
            if (proto.valueType >= JSValueType.Object)
                res.__prototype = proto;
            if (members.valueType >= JSValueType.Object)
            {
                foreach (var member in members)
                {
                    var desc = members[member];
                    if (desc.valueType == JSValueType.Property)
                    {
                        var getter = (desc.oValue as PropertyPair).get;
                        if (getter == null || getter.oValue == null)
                            throw new JSException(new TypeError("Invalid property descriptor for property " + member + " ."));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    if (desc.valueType < JSValueType.Object || desc.oValue == null)
                        throw new JSException(new TypeError("Invalid property descriptor for property " + member + " ."));
                    var value = desc["value"];
                    if (value.valueType == JSValueType.Property)
                        value = Tools.invokeGetter(value, desc);

                    var configurable = desc["configurable"];
                    if (configurable.valueType == JSValueType.Property)
                        configurable = Tools.invokeGetter(configurable, desc);

                    var enumerable = desc["enumerable"];
                    if (enumerable.valueType == JSValueType.Property)
                        enumerable = Tools.invokeGetter(enumerable, desc);

                    var writable = desc["writable"];
                    if (writable.valueType == JSValueType.Property)
                        writable = Tools.invokeGetter(writable, desc);

                    var get = desc["get"];
                    if (get.valueType == JSValueType.Property)
                        get = Tools.invokeGetter(get, desc);

                    var set = desc["set"];
                    if (set.valueType == JSValueType.Property)
                        set = Tools.invokeGetter(set, desc);

                    if (value.IsExist && (get.IsExist || set.IsExist))
                        throw new JSException(new TypeError("Property can not have getter or setter and default value."));
                    if (writable.IsExist && (get.IsExist || set.IsExist))
                        throw new JSException(new TypeError("Property can not have getter or setter and writable attribute."));
                    if (get.IsDefinded && get.valueType != JSValueType.Function)
                        throw new JSException(new TypeError("Getter mast be a function."));
                    if (set.IsDefinded && set.valueType != JSValueType.Function)
                        throw new JSException(new TypeError("Setter mast be a function."));
                    var obj = new JSValue() { valueType = JSValueType.Undefined };
                    res.fields[member] = obj;
                    obj.attributes |=
                          JSValueAttributesInternal.DoNotEnum
                        | JSValueAttributesInternal.NotConfigurable
                        | JSValueAttributesInternal.DoNotDelete
                        | JSValueAttributesInternal.ReadOnly;
                    if ((bool)enumerable)
                        obj.attributes &= ~JSValueAttributesInternal.DoNotEnum;
                    if ((bool)configurable)
                        obj.attributes &= ~(JSValueAttributesInternal.NotConfigurable | JSValueAttributesInternal.DoNotDelete);
                    if (value.IsExist)
                    {
                        var atr = obj.attributes;
                        obj.attributes = 0;
                        obj.Assign(value);
                        obj.attributes = atr;
                        if ((bool)writable)
                            obj.attributes &= ~JSValueAttributesInternal.ReadOnly;
                    }
                    else if (get.IsExist || set.IsExist)
                    {
                        Function setter = null, getter = null;
                        if (obj.valueType == JSValueType.Property)
                        {
                            setter = (obj.oValue as PropertyPair).set;
                            getter = (obj.oValue as PropertyPair).get;
                        }
                        obj.valueType = JSValueType.Property;
                        obj.oValue = new PropertyPair
                        {
                            set = set.IsExist ? set.oValue as Function : setter,
                            get = get.IsExist ? get.oValue as Function : getter
                        };
                    }
                    else if ((bool)writable)
                        obj.attributes &= ~JSValueAttributesInternal.ReadOnly;
                }
            }
            return res;
        }

        [ArgumentsLength(2)]
        [DoNotEnumerate]
        public static JSValue defineProperties(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Property define may only for Objects."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Can not define properties of null."));
            var target = args[0].oValue as JSObject ?? Null;
            var members = args[1].oValue as JSObject ?? Null;
            if (!args[1].IsDefinded)
                throw new JSException(new TypeError("Properties descriptor can not be undefined."));
            if (args[1].valueType < JSValueType.Object)
                return target;
            if (members.oValue == null)
                throw new JSException(new TypeError("Properties descriptor can not be null."));
            if (target.valueType < JSValueType.Object || target.oValue == null)
                return target;
            if (members.valueType > JSValueType.Undefined)
            {
                foreach (var memberName in members)
                {
                    var desc = members[memberName];
                    if (desc.valueType == JSValueType.Property)
                    {
                        var getter = (desc.oValue as PropertyPair).get;
                        if (getter == null || getter.oValue == null)
                            throw new JSException(new TypeError("Invalid property descriptor for property " + memberName + " ."));
                        desc = (getter.oValue as Function).Invoke(members, null);
                    }
                    if (desc.valueType < JSValueType.Object || desc.oValue == null)
                        throw new JSException(new TypeError("Invalid property descriptor for property " + memberName + " ."));
                    definePropertyImpl(target, desc.oValue as JSObject, memberName);
                }
            }
            return target;
        }

        [DoNotEnumerate]
        [ArgumentsLength(3)]
        public static JSValue defineProperty(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object || args[0].oValue == null)
                throw new JSException(new TypeError("Object.defineProperty cannot apply to non-object."));
            var target = args[0].oValue as JSObject ?? Null;
            var desc = args[2].oValue as JSObject ?? Null;
            if (desc.valueType < JSValueType.Object || desc.oValue == null)
                throw new JSException(new TypeError("Invalid property descriptor."));
            if (target.valueType < JSValueType.Object || target.oValue == null)
                return target;
            if (target is TypeProxy)
                target = (target as TypeProxy).prototypeInstance ?? target;
            string memberName = args[1].ToString();
            return definePropertyImpl(target, desc, memberName);
        }

        private static JSObject definePropertyImpl(JSObject target, JSObject desc, string memberName)
        {
            var value = desc["value"];
            if (value.valueType == JSValueType.Property)
                value = Tools.invokeGetter(value, desc);

            var configurable = desc["configurable"];
            if (configurable.valueType == JSValueType.Property)
                configurable = Tools.invokeGetter(configurable, desc);

            var enumerable = desc["enumerable"];
            if (enumerable.valueType == JSValueType.Property)
                enumerable = Tools.invokeGetter(enumerable, desc);

            var writable = desc["writable"];
            if (writable.valueType == JSValueType.Property)
                writable = Tools.invokeGetter(writable, desc);

            var get = desc["get"];
            if (get.valueType == JSValueType.Property)
                get = Tools.invokeGetter(get, desc);

            var set = desc["set"];
            if (set.valueType == JSValueType.Property)
                set = Tools.invokeGetter(set, desc);
            if (value.IsExist && (get.IsExist || set.IsExist))
                throw new JSException(new TypeError("Property can not have getter or setter and default value."));
            if (writable.IsExist && (get.IsExist || set.IsExist))
                throw new JSException(new TypeError("Property can not have getter or setter and writable attribute."));
            if (get.IsDefinded && get.valueType != JSValueType.Function)
                throw new JSException(new TypeError("Getter mast be a function."));
            if (set.IsDefinded && set.valueType != JSValueType.Function)
                throw new JSException(new TypeError("Setter mast be a function."));
            JSValue obj = null;
            obj = target.DefineMember(memberName);
            if ((obj.attributes & JSValueAttributesInternal.Argument) != 0 && (set.IsExist || get.IsExist))
            {
                var ti = 0;
                if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                    (target as Arguments)[ti] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject);
                else
                    target.fields[memberName] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject);
                obj.attributes &= ~JSValueAttributesInternal.Argument;
            }
            if ((obj.attributes & JSValueAttributesInternal.SystemObject) != 0)
                throw new JSException(new TypeError("Can not define property \"" + memberName + "\". Object immutable."));

            if (target is NiL.JS.BaseLibrary.Array)
            {
                if (memberName == "length")
                {
                    try
                    {
                        if (value.IsExist)
                        {
                            var nlenD = Tools.JSObjectToDouble(value);
                            var nlen = (uint)nlenD;
                            if (double.IsNaN(nlenD) || double.IsInfinity(nlenD) || nlen != nlenD)
                                throw new JSException(new RangeError("Invalid array length"));
                            if ((obj.attributes & JSValueAttributesInternal.ReadOnly) != 0
                                && ((obj.valueType == JSValueType.Double && nlenD != obj.dValue)
                                    || (obj.valueType == JSValueType.Int && nlen != obj.iValue)))
                                throw new JSException(new TypeError("Cannot change length of fixed size array"));
                            if (!(target as NiL.JS.BaseLibrary.Array).setLength(nlen))
                                throw new JSException(new TypeError("Unable to reduce length because exists not configurable elements"));
                            value = notExists;
                        }
                    }
                    finally
                    {
                        if (writable.IsExist && !(bool)writable)
                            obj.attributes |= JSValueAttributesInternal.ReadOnly;
                    }
                }
            }

            var newProp = obj.valueType < JSValueType.Undefined;
            var config = (obj.attributes & JSValueAttributesInternal.NotConfigurable) == 0 || newProp;

            if (!config)
            {
                if (enumerable.IsExist && (obj.attributes & JSValueAttributesInternal.DoNotEnum) != 0 == (bool)enumerable)
                    throw new JSException(new TypeError("Cannot change enumerable attribute for non configurable property."));

                if (writable.IsExist && (obj.attributes & JSValueAttributesInternal.ReadOnly) != 0 && (bool)writable)
                    throw new JSException(new TypeError("Cannot change writable attribute for non configurable property."));

                if (configurable.IsExist && (bool)configurable)
                    throw new JSException(new TypeError("Cannot set configurate attribute to true."));

                if ((obj.valueType != JSValueType.Property || ((obj.attributes & JSValueAttributesInternal.Field) != 0)) && (set.IsExist || get.IsExist))
                    throw new JSException(new TypeError("Cannot redefine not configurable property from immediate value to accessor property"));
                if (obj.valueType == JSValueType.Property && (obj.attributes & JSValueAttributesInternal.Field) == 0 && value.IsExist)
                    throw new JSException(new TypeError("Cannot redefine not configurable property from accessor property to immediate value"));
                if (obj.valueType == JSValueType.Property && (obj.attributes & JSValueAttributesInternal.Field) == 0
                    && set.IsExist
                    && (((obj.oValue as PropertyPair).set != null && (obj.oValue as PropertyPair).set.oValue != set.oValue)
                        || ((obj.oValue as PropertyPair).set == null && set.IsDefinded)))
                    throw new JSException(new TypeError("Cannot redefine setter of not configurable property."));
                if (obj.valueType == JSValueType.Property && (obj.attributes & JSValueAttributesInternal.Field) == 0
                    && get.IsExist
                    && (((obj.oValue as PropertyPair).get != null && (obj.oValue as PropertyPair).get.oValue != get.oValue)
                        || ((obj.oValue as PropertyPair).get == null && get.IsDefinded)))
                    throw new JSException(new TypeError("Cannot redefine getter of not configurable property."));
            }

            if (value.IsExist)
            {
                if (!config
                    && (obj.attributes & JSValueAttributesInternal.ReadOnly) != 0
                    && !((StrictEqualOperator.Check(obj, value) && ((obj.valueType == JSValueType.Undefined && value.valueType == JSValueType.Undefined) || !obj.IsNumber || !value.IsNumber || (1.0 / Tools.JSObjectToDouble(obj) == 1.0 / Tools.JSObjectToDouble(value))))
                        || (obj.valueType == JSValueType.Double && value.valueType == JSValueType.Double && double.IsNaN(obj.dValue) && double.IsNaN(value.dValue))))
                    throw new JSException(new TypeError("Cannot change value of not configurable not writable peoperty."));
                //if ((obj.attributes & JSObjectAttributesInternal.ReadOnly) == 0 || obj.valueType == JSObjectType.Property)
                {
                    obj.valueType = JSValueType.Undefined;
                    var atrbts = obj.attributes;
                    obj.attributes = 0;
                    obj.Assign(value);
                    obj.attributes = atrbts;
                }
            }
            else if (get.IsExist || set.IsExist)
            {
                Function setter = null, getter = null;
                if (obj.valueType == JSValueType.Property)
                {
                    setter = (obj.oValue as PropertyPair).set;
                    getter = (obj.oValue as PropertyPair).get;
                }
                obj.valueType = JSValueType.Property;
                obj.oValue = new PropertyPair
                {
                    set = set.IsExist ? set.oValue as Function : setter,
                    get = get.IsExist ? get.oValue as Function : getter
                };
            }
            else if (newProp)
                obj.valueType = JSValueType.Undefined;
            if (newProp)
            {
                obj.attributes |=
                    JSValueAttributesInternal.DoNotEnum
                    | JSValueAttributesInternal.DoNotDelete
                    | JSValueAttributesInternal.NotConfigurable
                    | JSValueAttributesInternal.ReadOnly;
            }
            else
            {
                var atrbts = obj.attributes;
                if (configurable.IsExist && (config || !(bool)configurable))
                    obj.attributes |= JSValueAttributesInternal.NotConfigurable | JSValueAttributesInternal.DoNotDelete;
                if (enumerable.IsExist && (config || !(bool)enumerable))
                    obj.attributes |= JSValueAttributesInternal.DoNotEnum;
                if (writable.IsExist && (config || !(bool)writable))
                    obj.attributes |= JSValueAttributesInternal.ReadOnly;

                if (obj.attributes != atrbts && (obj.attributes & JSValueAttributesInternal.Argument) != 0)
                {
                    var ti = 0;
                    if (target is Arguments && int.TryParse(memberName, NumberStyles.Integer, CultureInfo.InvariantCulture, out ti) && ti >= 0 && ti < 16)
                        (target as Arguments)[ti] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.Argument);
                    else
                        target.fields[memberName] = obj = obj.CloneImpl(JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.Argument);
                }
            }

            if (config)
            {
                if ((bool)enumerable)
                    obj.attributes &= ~JSValueAttributesInternal.DoNotEnum;
                if ((bool)configurable)
                    obj.attributes &= ~(JSValueAttributesInternal.NotConfigurable | JSValueAttributesInternal.DoNotDelete);
                if ((bool)writable)
                    obj.attributes &= ~JSValueAttributesInternal.ReadOnly;
            }
            return target;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public void __defineGetter__(Arguments args)
        {
            if (args.length < 2)
                throw new JSException(new TypeError("Missed parameters"));
            if (args[1].valueType != JSValueType.Function)
                throw new JSException(new TypeError("Expecting function as second parameter"));
            var field = GetMember(args[0], true, true);
            if ((field.attributes & JSValueAttributesInternal.NotConfigurable) != 0)
                throw new JSException(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field.attributes & JSValueAttributesInternal.ReadOnly) != 0)
                throw new JSException(new TypeError("Cannot change value of readonly peoperty."));
            if (field.valueType == JSValueType.Property)
                (field.oValue as PropertyPair).get = args.a1.oValue as Function;
            else
            {
                field.valueType = JSValueType.Property;
                field.oValue = new PropertyPair
                {
                    get = args.a1.oValue as Function
                };
            }
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public void __defineSetter__(Arguments args)
        {
            if (args.length < 2)
                throw new JSException(new TypeError("Missed parameters"));
            if (args[1].valueType != JSValueType.Function)
                throw new JSException(new TypeError("Expecting function as second parameter"));
            var field = GetMember(args[0], true, true);
            if ((field.attributes & JSValueAttributesInternal.NotConfigurable) != 0)
                throw new JSException(new TypeError("Cannot change value of not configurable peoperty."));
            if ((field.attributes & JSValueAttributesInternal.ReadOnly) != 0)
                throw new JSException(new TypeError("Cannot change value of readonly peoperty."));
            if (field.valueType == JSValueType.Property)
                (field.oValue as PropertyPair).set = args.a1.oValue as Function;
            else
            {
                field.valueType = JSValueType.Property;
                field.oValue = new PropertyPair
                {
                    set = args.a1.oValue as Function
                };
            }
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupGetter(Arguments args)
        {
            var field = GetMember(args[0], false, false);
            if (field.valueType == JSValueType.Property)
                return (field.oValue as PropertyPair).get;
            return null;
        }

        [DoNotEnumerate]
        [CLSCompliant(false)]
        public JSObject __lookupSetter(Arguments args)
        {
            var field = GetMember(args[0], false, false);
            if (field.valueType == JSValueType.Property)
                return (field.oValue as PropertyPair).get;
            return null;
        }

        [DoNotEnumerate]
        public static JSValue freeze(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.freeze called on non-object."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Object.freeze called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            obj.attributes |= JSValueAttributesInternal.Immutable;
            for (var e = obj.GetEnumeratorImpl(false); e.MoveNext(); )
            {
                var value = obj[e.Current];
                if ((value.attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    value.attributes |= JSValueAttributesInternal.NotConfigurable | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete;
                }
            }
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue preventExtensions(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Prevent the expansion can only for objects"));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Can not prevent extensions for null"));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            obj.attributes |= JSValueAttributesInternal.Immutable;
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue isExtensible(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.isExtensible called on non-object."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Object.isExtensible called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            return (obj.attributes & JSValueAttributesInternal.Immutable) == 0;
        }

        [DoNotEnumerate]
        public static JSValue isSealed(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.isSealed called on non-object."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Object.isSealed called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            if ((obj.attributes & JSValueAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr.data)
                {
                    if (node != null
                        && node.IsExist
                        && node.valueType >= JSValueType.Object && node.oValue != null
                        && (node.attributes & JSValueAttributesInternal.NotConfigurable) == 0)
                        return false;
                }
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if (f.Value.valueType >= JSValueType.Object && f.Value.oValue != null && (f.Value.attributes & JSValueAttributesInternal.NotConfigurable) == 0)
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject seal(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.seal called on non-object."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Object.seal called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            obj.attributes |= JSValueAttributesInternal.Immutable;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            for (var e = obj.GetEnumeratorImpl(false); e.MoveNext(); )
            {
                var value = obj[e.Current];
                if ((value.attributes & JSValueAttributesInternal.SystemObject) == 0)
                {
                    value.attributes |= JSValueAttributesInternal.NotConfigurable | JSValueAttributesInternal.DoNotDelete;
                }
            }
            return obj;
        }

        [DoNotEnumerate]
        public static JSValue isFrozen(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.isFrozen called on non-object."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Object.isFrozen called on null."));
            var obj = args[0].Value as JSObject ?? args[0].oValue as JSObject;
            if ((obj.attributes & JSValueAttributesInternal.Immutable) == 0)
                return false;
            if (obj is TypeProxy)
                return true;
            var arr = obj as NiL.JS.BaseLibrary.Array;
            if (arr != null)
            {
                foreach (var node in arr.data.DirectOrder)
                {
                    if (node.Value != null && node.Value.IsExist &&
                        ((node.Value.attributes & JSValueAttributesInternal.NotConfigurable) == 0
                        || (node.Value.valueType != JSValueType.Property && (node.Value.attributes & JSValueAttributesInternal.ReadOnly) == 0)))
                        return false;
                }
            }
            else if (obj is Arguments)
            {
                var arg = obj as Arguments;
                for (var i = 0; i < 16; i++)
                {
                    if ((arg[i].attributes & JSValueAttributesInternal.NotConfigurable) == 0
                            || (arg[i].valueType != JSValueType.Property && (arg[i].attributes & JSValueAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            }
            if (obj.fields != null)
                foreach (var f in obj.fields)
                {
                    if ((f.Value.attributes & JSValueAttributesInternal.NotConfigurable) == 0
                            || (f.Value.valueType != JSValueType.Property && (f.Value.attributes & JSValueAttributesInternal.ReadOnly) == 0))
                        return false;
                }
            return true;
        }

        [DoNotEnumerate]
        public static JSObject getPrototypeOf(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Parameter isn't an Object."));
            var res = args[0].__proto__;
            //if (res.oValue is TypeProxy && (res.oValue as TypeProxy).prototypeInstance != null)
            //    res = (res.oValue as TypeProxy).prototypeInstance;
            return res;
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        public static JSValue getOwnPropertyDescriptor(Arguments args)
        {
            if (args[0].valueType <= JSValueType.Undefined)
                throw new JSException(new TypeError("Object.getOwnPropertyDescriptor called on undefined."));
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyDescriptor called on non-object."));
            var source = args[0].oValue as JSObject ?? Null;
            var obj = source.GetMember(args[1], false, true);
            if (obj.valueType < JSValueType.Undefined)
                return undefined;
            if ((obj.attributes & JSValueAttributesInternal.SystemObject) != 0)
                obj = source.GetMember(args[1], true, true);
            var res = CreateObject();
            if (obj.valueType != JSValueType.Property || (obj.attributes & JSValueAttributesInternal.Field) != 0)
            {
                if (obj.valueType == JSValueType.Property)
                    res["value"] = (obj.oValue as PropertyPair).get.Invoke(source, null);
                else
                    res["value"] = obj;
                res["writable"] = obj.valueType < JSValueType.Undefined || (obj.attributes & JSValueAttributesInternal.ReadOnly) == 0;
            }
            else
            {
                res["set"] = (obj.oValue as PropertyPair).set;
                res["get"] = (obj.oValue as PropertyPair).get;
            }
            res["configurable"] = (obj.attributes & JSValueAttributesInternal.NotConfigurable) == 0 || (obj.attributes & JSValueAttributesInternal.DoNotDelete) == 0;
            res["enumerable"] = (obj.attributes & JSValueAttributesInternal.DoNotEnum) == 0;
            return res;
        }

        [DoNotEnumerate]
        public static JSObject getOwnPropertyNames(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.getOwnPropertyNames called on non-object value."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Cannot get property names of null"));
            var obj = args[0].oValue as JSObject;
            return new NiL.JS.BaseLibrary.Array(obj.GetEnumeratorImpl(false));
        }

        [DoNotEnumerate]
        public static JSObject keys(Arguments args)
        {
            if (args[0].valueType < JSValueType.Object)
                throw new JSException(new TypeError("Object.keys called on non-object value."));
            if (args[0].oValue == null)
                throw new JSException(new TypeError("Cannot get property names of null"));
            var obj = args[0].oValue as JSObject;
            return new NiL.JS.BaseLibrary.Array(obj.GetEnumeratorImpl(true));
        }
    }
}
