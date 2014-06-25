using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, JSObject> staticProxies = new Dictionary<Type, JSObject>();
        private static readonly Dictionary<Type, TypeProxy> dynamicProxies = new Dictionary<Type, TypeProxy>();

        internal Type hostedType;
        [NonSerialized]
        internal Dictionary<string, IList<MemberInfo>> members;
        private JSObject _prototypeInstance;
        internal JSObject prototypeInstance
        {
            get
            {
                if (_prototypeInstance == null)
                {
                    try
                    {
                        var ictor = hostedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, System.Type.EmptyTypes, null);
                        if (ictor != null)
                        {
                            _prototypeInstance = new JSObject()
                                                    {
                                                        oValue = ictor.Invoke(null),
                                                        __proto__ = this,
                                                        valueType = JSObjectType.Object,
                                                        attributes = attributes | JSObjectAttributes.ProxyPrototype,
                                                        fields = fields
                                                    };
                            if (_prototypeInstance.oValue is JSObject)
                                _prototypeInstance.valueType = (JSObjectType)System.Math.Max((int)_prototypeInstance.valueType, (int)(_prototypeInstance.oValue as JSObject).valueType);
                        }
                    }
                    catch (COMException)
                    {

                    }
                }
                return _prototypeInstance;
            }
        }
        internal BindingFlags bindFlags = BindingFlags.Public;

        /// <summary>
        /// ������ ������-��������� ���������� ������� ��� ������� � ����� ������� �� �������. 
        /// </summary>
        /// <param name="value">������, ������� ���������� �����������.</param>
        /// <returns>������-���������, �������������� ���������� ������.</returns>
        public static JSObject Proxy(object value)
        {
            if (value == null)
                return JSObject.Null;
            else if (value is JSObject)
                return value as JSObject;
            else if (value is sbyte)
                return (int)(sbyte)value;
            else if (value is byte)
                return (int)(byte)value;
            else if (value is short)
                return (int)(short)value;
            else if (value is ushort)
                return (int)(ushort)value;
            else if (value is int)
                return (int)value;
            else if (value is uint)
                return (double)(uint)value;
            else if (value is long)
                return (double)(long)value;
            else if (value is ulong)
                return (double)(ulong)value;
            else if (value is float)
                return (double)(float)value;
            else if (value is double)
                return (double)value;
            else if (value is string)
                return value.ToString();
            else if (value is char)
                return value.ToString();
            else if (value is bool)
                return (bool)value;
            else if (value is Delegate)
                return new MethodProxy(((Delegate)value).Method, ((Delegate)value).Target);
            else
            {
                var type = value.GetType();
                var res = new JSObject() { oValue = value, valueType = JSObjectType.Object, __proto__ = GetPrototype(type) };
                res.attributes |= res.__proto__.attributes & JSObjectAttributes.Immutable;
                return res;
            }
        }

        public static TypeProxy GetPrototype(Type type)
        {
            TypeProxy prot = null;
            if (!dynamicProxies.TryGetValue(type, out prot))
            {
                lock (dynamicProxies)
                {
                    new TypeProxy(type);
                    prot = dynamicProxies[type];
                }
            }
            return prot;
        }

        public static JSObject GetConstructor(Type type)
        {
            JSObject constructor = null;
            if (!staticProxies.TryGetValue(type, out constructor))
            {
                lock (staticProxies)
                {
                    new TypeProxy(type);
                    constructor = staticProxies[type];
                }
            }
            return constructor;
        }

        internal static void Clear()
        {
            BaseTypes.Boolean.True.__proto__ = null;
            BaseTypes.Boolean.False.__proto__ = null;
            JSObject.nullString.__proto__ = null;
            Number.NaN.__proto__ = null;
            Number.POSITIVE_INFINITY.__proto__ = null;
            Number.NEGATIVE_INFINITY.__proto__ = null;
            Number.MIN_VALUE.__proto__ = null;
            Number.MAX_VALUE.__proto__ = null;
            staticProxies.Clear();
            dynamicProxies.Clear();
        }

        private TypeProxy()
            : base(true)
        {
            valueType = JSObjectType.Object;
            oValue = this;
            attributes |= JSObjectAttributes.SystemObject;
        }

        private TypeProxy(Type type)
            : base(true)
        {
            if (dynamicProxies.ContainsKey(type))
                throw new InvalidOperationException("Type \"" + type + "\" already proxied.");
            else
            {
                hostedType = type;
                dynamicProxies[type] = this;
                if (hostedType == typeof(JSObject))
                {
                    _prototypeInstance = new JSObject()
                    {
                        valueType = JSObjectType.Object,
                        oValue = this, // �� �������!
                        attributes = JSObjectAttributes.ProxyPrototype | JSObjectAttributes.ReadOnly,
                        __proto__ = this
                    };
                }
                else
                {
                    if (typeof(JSObject).IsAssignableFrom(hostedType))
                    {
                        _prototypeInstance = prototypeInstance;
                    }
                }

                valueType = _prototypeInstance is JSObject ? (JSObjectType)System.Math.Max((int)(_prototypeInstance as JSObject).valueType, (int)JSObjectType.Object) : JSObjectType.Object;
                oValue = this;
                attributes |= JSObjectAttributes.DoNotEnum | JSObjectAttributes.SystemObject;
                if (hostedType.IsDefined(typeof(ImmutableAttribute), false))
                    attributes |= JSObjectAttributes.Immutable;
                var staticProxy = new TypeProxy() { hostedType = type, bindFlags = bindFlags | BindingFlags.Static };
                bindFlags |= BindingFlags.Instance;
                if (hostedType.IsAbstract)
                {
                    staticProxies[type] = staticProxy;
                }
                else
                {
                    var prot = staticProxy.DefaultFieldGetter("prototype", true, true);
                    prot.Assign(this);
                    ProxyConstructor ctor = null;
                    if (type == typeof(JSObject))
                    {
                        ctor = new ObjectConstructor(staticProxy);
                        prot.attributes = JSObjectAttributes.DoNotEnum | JSObjectAttributes.ReadOnly | JSObjectAttributes.DoNotDelete | JSObjectAttributes.NotConfigurable;
                    }
                    else
                    {
                        ctor = new ProxyConstructor(staticProxy);
                        prot.attributes = JSObjectAttributes.DoNotEnum;
                    }
                    ctor.attributes = attributes;
                    staticProxies[type] = ctor;
                    fields["constructor"] = ctor;
                }
                var pa = type.GetCustomAttributes(typeof(PrototypeAttribute), false);
                if (pa.Length != 0)
                    __proto__ = GetPrototype((pa[0] as PrototypeAttribute).PrototypeType).Clone() as JSObject;
            }
        }

        private void fillMembers()
        {
            lock (this)
            {
                lock (fields)
                {
                    if (members != null)
                        return;
                    var tempmemb = new Dictionary<string, IList<MemberInfo>>();
                    var mmbrs = hostedType.GetMembers(bindFlags);
                    string prewName = null;
                    IList<MemberInfo> temp = null;
                    for (int i = 0; i < mmbrs.Length; i++)
                    {
                        mmbrs[i].ToString();
                        if (mmbrs[i].IsDefined(typeof(HiddenAttribute), false))
                            continue;
                        var membername = mmbrs[i].Name;
                        if (mmbrs[i].MemberType == MemberTypes.Method
                            && ((mmbrs[i] as MethodBase).DeclaringType == typeof(object)))
                            continue;
                        membername = membername[0] == '.' ? membername : membername.Contains(".") ? membername.Substring(membername.LastIndexOf('.') + 1) : membername;
                        if (prewName != membername)
                        {
                            if (temp != null && temp.Count > 1)
                            {
                                var type = temp[0].DeclaringType;
                                for (var j = 1; j < temp.Count; j++)
                                {
                                    if (type != temp[j].DeclaringType && type.IsAssignableFrom(temp[j].DeclaringType))
                                    {
                                        type = temp[j].DeclaringType;
                                        j = 0;
                                        continue;
                                    }
                                    if (!type.IsAssignableFrom(temp[j].DeclaringType))
                                        temp.RemoveAt(j--);
                                }
                            }
                            if (!tempmemb.TryGetValue(membername, out temp))
                                tempmemb[membername] = temp = new List<MemberInfo>();
                            prewName = membername;
                        }
                        if (temp.Count == 1)
                            tempmemb.Add(membername + "$0", new[] { temp[0] });
                        temp.Add(mmbrs[i]);
                        if (temp.Count != 1)
                            tempmemb.Add(membername + "$" + (temp.Count - 1), new[] { mmbrs[i] });
                    }
                    members = tempmemb;
                }
            }
        }

        public override void Assign(NiL.JS.Core.JSObject value)
        {
            throw new JSException("Can not assign to __proto__ of immutable or special objects.");
        }

        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            JSObject r = null;
            if (fields.TryGetValue(name, out r))
            {
                if (r.valueType < JSObjectType.Undefined)
                    r.Assign(DefaultFieldGetter(name, false, own));
                if (create && (r.attributes & JSObjectAttributes.SystemObject) != 0)
                    fields[name] = r = r.Clone() as JSObject;
                return r;
            }
            IList<MemberInfo> m = null;
            if (members == null)
                fillMembers();
            members.TryGetValue(name, out m);
            if (m == null || m.Count == 0)
            {
                switch (name)
                {
                    default:
                        {
                            r = DefaultFieldGetter(name, create, own);
                            return r;
                        }
                }
            }
            if (m.Count > 1)
            {
                for (int i = 0; i < m.Count; i++)
                    if (!(m[i] is MethodBase))
                        throw new JSException(Proxy(new TypeError("Incompatible fields type.")));
                var cache = new MethodProxy[m.Count];
                for (int i = 0; i < m.Count; i++)
                    cache[i] = new MethodProxy(m[i] as MethodBase);
                r = new ExternalFunction((thisBind, args) =>
                {
                    int l = args == null ? 0 : args.GetMember("length").iValue;
                    for (int i = 0; i < m.Count; i++)
                    {
                        if (cache[i].Parameters.Length == l
                        || (cache[i].Parameters.Length == 1
                            && (cache[i].Parameters[0].ParameterType == typeof(JSObject)
                                || cache[i].Parameters[0].ParameterType == typeof(JSObject[])
                                || cache[i].Parameters[0].ParameterType == typeof(object[]))))
                        {
                            object[] cargs = null;
                            if (l != 0)
                            {
                                cargs = cache[i].ConvertArgs(args);
                                for (var j = cargs.Length; j-- > 0; )
                                {
                                    if (cargs[j] == null ? cache[i].Parameters[j].ParameterType.IsValueType : !cache[i].Parameters[j].ParameterType.IsAssignableFrom(cargs[j].GetType()))
                                    {
                                        j = 0;
                                        cargs = null;
                                    }
                                }
                                if (cargs == null)
                                    continue;
                            }
                            if (cargs != null && cargs.Length == 1 && cargs[0] is JSObject && (cargs[0] as JSObject).oValue == Arguments.Instance)
                                (cargs[0] as JSObject).fields["callee"] = cache[i];
                            return TypeProxy.Proxy(cache[i].InvokeImpl(thisBind, cargs, args));
                        }
                    }
                    throw new JSException(new TypeError("Invalid parameters for function " + m[0].Name));
                });
            }
            else
            {
                switch (m[0].MemberType)
                {
                    case MemberTypes.Constructor:
                        {
                            var method = (ConstructorInfo)m[0];
                            r = new MethodProxy(method);
                            r.attributes = JSObjectAttributes.SystemObject;
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            r = new MethodProxy(method);
                            r.attributes = JSObjectAttributes.SystemObject;
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            var field = (m[0] as FieldInfo);
                            var cva = field.GetCustomAttribute(typeof(ConvertValueAttribute)) as ConvertValueAttribute;
                            if (cva != null)
                            {
                                r = new JSObject()
                                {
                                    valueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                    {
                                        m[0].IsDefined(typeof(Modules.ReadOnlyAttribute), false) ? 
                                            new ExternalFunction((thisBind, a)=>
                                            {
                                                field.SetValue(field.IsStatic ? null : thisBind.Value, cva.To(a.GetMember("0").Value)); 
                                                return null; 
                                            }) : null,
                                        new ExternalFunction((thisBind, a)=>
                                        { 
                                            return Proxy(cva.From(field.GetValue(field.IsStatic ? null : thisBind.Value)));
                                        })
                                    }
                                };
                            }
                            else
                            {
                                r = new JSObject()
                                {
                                    valueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                    {
                                        !m[0].IsDefined(typeof(Modules.ReadOnlyAttribute), false) ? new ExternalFunction((thisBind, a)=>
                                        {
                                            field.SetValue(field.IsStatic ? null : thisBind.Value, a.GetMember("0").Value); 
                                            return null; 
                                        }) : null,
                                        new ExternalFunction((thisBind, a)=>
                                        { 
                                            return Proxy(field.GetValue(field.IsStatic ? null : thisBind.Value));
                                        })
                                    }
                                };
                            }
                            r.attributes = JSObjectAttributes.Immutable | JSObjectAttributes.SystemObject;
                            if ((r.oValue as Function[])[0] == null)
                                r.attributes |= JSObjectAttributes.ReadOnly;
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            var pinfo = (PropertyInfo)m[0];
                            var cva = pinfo.GetCustomAttribute(typeof(ConvertValueAttribute)) as ConvertValueAttribute;
                            if (cva != null)
                            {
                                r = new JSObject()
                                    {
                                        valueType = JSObjectType.Property,
                                        oValue = new Function[] 
                                        { 
                                            pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.GetSetMethod(false), cva, new[]{ cva }) : null,
                                            pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false), cva, null) : null 
                                        }
                                    };
                            }
                            else
                            {
                                r = new JSObject()
                                {
                                    valueType = JSObjectType.Property,
                                    oValue = new Function[] 
                                        { 
                                            pinfo.CanWrite && pinfo.GetSetMethod(false) != null && !pinfo.IsDefined(typeof(ReadOnlyAttribute), false) ? new MethodProxy(pinfo.GetSetMethod(false)) : null,
                                            pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false)) : null 
                                        }
                                };
                            }
                            r.attributes = JSObjectAttributes.Immutable;
                            if ((r.oValue as Function[])[0] == null)
                                r.attributes |= JSObjectAttributes.ReadOnly;
                            break;
                        }
                    case MemberTypes.Event:
                        {
                            var pinfo = (EventInfo)m[0];
                            r = new JSObject()
                            {
                                valueType = JSObjectType.Property,
                                oValue = new Function[] { 
                                    new MethodProxy(pinfo.GetAddMethod()),
                                    null
                                }
                            };
                            break;
                        }
                    case MemberTypes.NestedType:
                        {
                            r = GetConstructor(m[0] as Type);
                            break;
                        }
                    default:
                        throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
                }
            }
            //r.attributes |= JSObjectAttributes.NotConfigurable;
            r.attributes |= JSObjectAttributes.DoNotEnum;
            lock (fields)
                fields[name] = create && r.GetType() != typeof(JSObject) ? (r = r.Clone() as JSObject) : r;
            if (m[0].IsDefined(typeof(ReadOnlyAttribute), false))
                r.attributes |= JSObjectAttributes.ReadOnly;
            if (m[0].IsDefined(typeof(DoNotDeleteAttribute), false))
                r.attributes |= JSObjectAttributes.DoNotDelete;
            for (var i = m.Count; i-- > 0; )
            {
                if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                {
                    r.attributes &= ~JSObjectAttributes.DoNotEnum;
                    break;
                }
            }
            return r;
        }

        public override JSObject propertyIsEnumerable(JSObject args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var name = args.GetMember("0").ToString();
            JSObject temp;
            if (fields != null && fields.TryGetValue(name, out temp))
                return temp.valueType >= JSObjectType.Undefined && (temp.attributes & JSObjectAttributes.DoNotEnum) == 0;
            IList<MemberInfo> m = null;
            if (members.TryGetValue(name, out m))
            {
                for (var i = m.Count; i-- > 0; )
                    if (!m[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                        return false;
                return true;
            }
            return false;
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            if (members == null)
                fillMembers();
            foreach (var m in members)
            {
                for (var i = m.Value.Count; i-- > 0; )
                {
                    if (!pdef || !m.Value[i].IsDefined(typeof(DoNotEnumerateAttribute), false))
                    {
                        yield return m.Key;
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return ((bindFlags & BindingFlags.Static) != 0 ? "Proxy:Static (" : "Proxy:Dynamic (") + hostedType + ")";
        }
    }
}