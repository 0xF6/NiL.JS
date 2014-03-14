using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NiL.JS.Core
{
    public sealed class TypeProxy : JSObject
    {
        private static readonly Dictionary<Type, JSObject> constructors = new Dictionary<Type, JSObject>();
        private static readonly Dictionary<Type, TypeProxy> prototypes = new Dictionary<Type, TypeProxy>();

        internal Type hostedType;
        [NonSerialized]
        internal Dictionary<string, List<MemberInfo>> members;
        internal object prototypeInstance;
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
            else
            {
                var type = value.GetType();
                var res = new JSObject() { oValue = value, ValueType = JSObjectType.Object, prototype = GetPrototype(type) };
                res.attributes |= res.prototype.attributes & ObjectAttributes.Immutable;
                return res;
            }
        }

        public static TypeProxy GetPrototype(Type type)
        {
            TypeProxy prot = null;
            if (!prototypes.TryGetValue(type, out prot))
            {
                lock (prototypes)
                {
                    new TypeProxy(type);
                    prot = prototypes[type];
                }
            }
            return prot;
        }

        public static JSObject GetConstructor(Type type)
        {
            JSObject constructor = null;
            if (!constructors.TryGetValue(type, out constructor))
            {
                lock (prototypes)
                {
                    new TypeProxy(type);
                    constructor = constructors[type];
                }
            }
            return constructor;
        }

        internal static void Clear()
        {
            constructors.Clear();
            prototypes.Clear();
        }

        private TypeProxy()
            : base(true)
        {
            ValueType = JSObjectType.Object;
            oValue = this;
        }

        private TypeProxy(Type type)
            : base(true)
        {
            if (prototypes.ContainsKey(type))
                throw new InvalidOperationException("Type \"" + type + "\" already proxied.");
            else
            {
                hostedType = type;
                prototypes[type] = this;
                if (type.IsValueType)
                    prototypeInstance = Activator.CreateInstance(type);
                else
                {
                    if (hostedType == typeof(JSObject))
                    {
                        prototypeInstance = new JSObject()
                        {
                            ValueType = JSObjectType.Object,
                            oValue = this // �� �������!
                        };
                    }
                    else
                    {
                        var ictor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
                        if (ictor != null)
                        {
                            try
                            {
                                prototypeInstance = ictor.Invoke(null);
                                if (prototypeInstance is JSObject)
                                {
                                    (prototypeInstance as JSObject).fields = this.fields;
                                    if ((prototypeInstance as JSObject).ValueType < JSObjectType.Object)
                                        (prototypeInstance as JSObject).ValueType = JSObjectType.Object;
                                }
                            }
                            catch (System.Runtime.InteropServices.COMException)
                            {

                            }
                        }
                    }
                }

                ValueType = prototypeInstance is JSObject ? (JSObjectType)System.Math.Max((int)(prototypeInstance as JSObject).ValueType, (int)JSObjectType.Object) : JSObjectType.Object;
                oValue = this;
                attributes |= ObjectAttributes.DontDelete | ObjectAttributes.DontEnum | ObjectAttributes.ReadOnly;
                if (hostedType.GetCustomAttributes(typeof(ImmutableAttribute), false).Length != 0)
                    attributes |= ObjectAttributes.Immutable;
                var ctorProxy = new TypeProxy() { hostedType = type, bindFlags = bindFlags | BindingFlags.Static };
                if (hostedType.IsAbstract)
                {
                    constructors[type] = ctorProxy;
                }
                else
                {
                    var prot = ctorProxy.DefaultFieldGetter("prototype", false, false);
                    prot.Assign(this);
                    prot.attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum | ObjectAttributes.ReadOnly;
                    var ctor = new TypeProxyConstructor(ctorProxy);
                    ctorProxy.DefaultFieldGetter("__proto__", false, false).Assign(GetPrototype(typeof(TypeProxyConstructor)));
                    ctor.attributes = attributes;
                    constructors[type] = ctor;
                    fields["constructor"] = ctor;
                }
                bindFlags |= BindingFlags.Instance;
                var pa = type.GetCustomAttributes(typeof(PrototypeAttribute), false);
                if (pa.Length != 0)
                    prototype = GetPrototype((pa[0] as PrototypeAttribute).PrototypeType).Clone() as JSObject;
                else
                    prototype = JSObject.GlobalPrototype ?? (typeof(JSObject) != type ? GetPrototype(typeof(JSObject)) : null);
            }
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            JSObject r = null;
            if (fields.TryGetValue(name, out r))
            {
                if (r.ValueType < JSObjectType.Undefined)
                    r.Assign(DefaultFieldGetter(name, fast, own));
                return r;
            }
            List<MemberInfo> m = null;
            if (members == null)
            {
                members = new Dictionary<string, List<MemberInfo>>();
                var mmbrs = hostedType.GetMembers(bindFlags);
                string prewName = null;
                List<MemberInfo> temp = null;
                for (int i = 0; i < mmbrs.Length; i++)
                {
                    if (mmbrs[i].GetCustomAttributes(typeof(HiddenAttribute), false).Length != 0)
                        continue;
                    if (prewName != mmbrs[i].Name && !members.TryGetValue(mmbrs[i].Name, out temp))
                    {
                        members[mmbrs[i].Name] = temp = new List<MemberInfo>();
                        prewName = mmbrs[i].Name;
                    }
                    temp.Add(mmbrs[i]);
                }
            }
            members.TryGetValue(name, out m);
            if (m == null || name == "GetType" || m.Count == 0)
            {
                switch (name)
                {
                    case "toString":
                        {
                            return GetField("ToString", true, true);
                        }
                    default:
                        {
                            r = DefaultFieldGetter(name, fast, own);
                            return r;
                        }
                }
            }
            if (m.Count > 1)
            {
                for (int i = 0; i < m.Count; i++)
                    if (!(m[i] is MethodInfo))
                        throw new JSException(Proxy(new TypeError("Incompatible fields type.")));
                var cache = new Function[m.Count];
                r = new ExternalFunction((context, args) =>
                {
                    context.ValidateThreadID();
                    int l = args.GetField("length", true, false).iValue;
                    for (int i = 0; i < m.Count; i++)
                    {
                        var mi = m[i] as MethodInfo;
                        if (mi.GetParameters().Length == l)
                            return (cache[i] ?? (cache[i] = new MethodProxy(m[i] as MethodInfo))).Invoke(context, args);
                    }
                    return null;
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
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            var method = (MethodInfo)m[0];
                            r = new MethodProxy(method);
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            var field = (m[0] as FieldInfo);
                            r = new JSObject()
                            {
                                ValueType = JSObjectType.Property,
                                oValue = new Function[] 
                                    {
                                        m[0].GetCustomAttributes(typeof(Modules.ProtectedAttribute), false).Length == 0 ? new ExternalFunction((c,a)=>{ field.SetValue(field.IsStatic ? null : (c.thisBind ?? c.GetField("this")).oValue, a.GetField("0", true, false).Value); return null; }) : null,
                                        new ExternalFunction((c,a)=>{ return Proxy(field.GetValue(field.IsStatic ? null : c.thisBind.oValue));})
                                    }
                            };
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            var pinfo = (PropertyInfo)m[0];
                            r = new JSObject()
                            {
                                ValueType = JSObjectType.Property,
                                oValue = new Function[] { 
                                    pinfo.CanWrite && pinfo.GetSetMethod(false) != null ? new MethodProxy(pinfo.GetSetMethod(false)) : null,
                                    pinfo.CanRead && pinfo.GetGetMethod(false) != null ? new MethodProxy(pinfo.GetGetMethod(false)) : null 
                                }
                            };
                            break;
                        }
                    case MemberTypes.Event:
                        {
                            var pinfo = (EventInfo)m[0];
                            r = new JSObject()
                            {
                                ValueType = JSObjectType.Property,
                                oValue = new Function[] { 
                                    new MethodProxy(pinfo.GetAddMethod()),
                                    null
                                }
                            };
                            break;
                        }
                    default: throw new NotImplementedException("Convertion from " + m[0].MemberType + " not implemented");
                }
                if (m[0].GetCustomAttributes(typeof(ProtectedAttribute), false).Length != 0)
                    r.Protect();
                if (m[0].GetCustomAttributes(typeof(DoNotDeleteAttribute), false).Length != 0)
                    r.attributes |= ObjectAttributes.DontDelete;
            }
            r.attributes |= ObjectAttributes.DontEnum;
            fields[name] = r;
            return r;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            if (fields == null)
                return JSObject.EmptyEnumerator;
            return fields.Keys.GetEnumerator();
        }

        public override string ToString()
        {
            return ((bindFlags & BindingFlags.Static) != 0 ? "Proxy:Static (" : "Proxy:Dynamic (") + hostedType + ")";
        }
    }
}