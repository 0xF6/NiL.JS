﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    internal class MethodProxy : BaseTypes.Function
    {
        internal static readonly new NiL.JS.Core.BaseTypes.String @string = new BaseTypes.String();
        internal static readonly new NiL.JS.Core.BaseTypes.Number number = new Number();
        internal static readonly new NiL.JS.Core.BaseTypes.Boolean boolean = new BaseTypes.Boolean();

        private MethodBase info;
        private Func<JSObject[], object> @delegate = null;

        public MethodProxy(MethodBase methodinfo)
        {
            info = methodinfo;
            if (info is MethodInfo && methodinfo.IsStatic)
            {
                var mi = info as MethodInfo;
                if (mi.ReturnType.IsSubclassOf(typeof(object))
                    && info.GetParameters().Length == 1
                    && info.GetParameters()[0].ParameterType == typeof(JSObject[]))
                    this.@delegate = (Func<JSObject[], object>)Delegate.CreateDelegate(typeof(Func<JSObject[], object>), mi);
            }
        }

        private static object[] convertArray(NiL.JS.Core.BaseTypes.Array array)
        {
            var arg = new object[array.length];
            for (var j = 0; j < arg.Length; j++)
            {
                var temp = array[j].Value;
                arg[j] = temp is NiL.JS.Core.BaseTypes.Array ? convertArray(temp as NiL.JS.Core.BaseTypes.Array) : temp;
            }
            return arg;
        }

        private static JSObject[] argumentsToArray(JSObject source)
        {
            var len = source.GetField("length", true, false).iValue;
            var res = new JSObject[len];
            for (int i = 0; i < len; i++)
                res[i] = source.GetField(i.ToString(), true, true);
            return res;
        }

        private static object[] convertArgs(JSObject source, ParameterInfo[] targetTypes)
        {
            if (targetTypes.Length == 0)
                return null;
            object[] res;
            var len = source.GetField("length", true, false).iValue;
            if (targetTypes.Length == 1)
            {
                if (targetTypes[0].ParameterType == typeof(JSObject))
                    return new object[] { source };
                if (targetTypes[0].ParameterType == typeof(JSObject[]))
                {
                    res = new JSObject[len];
                    for (int i = 0; i < len; i++)
                        res[i] = source.GetField(i.ToString(), true, true);
                    return new object[] { res };
                }
            }
            int targetCount = targetTypes.Length;
            targetCount = System.Math.Min(targetCount, len);
            res = new object[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                var obj = source.GetField(i.ToString(), true, true);
                if (source == null || obj == null)
                    continue;
                res[i] = embeddedTypeConvert(obj, targetTypes[i].ParameterType);
                if (res[i] != null)
                    continue;
                if (targetTypes[i].ParameterType == typeof(JSObject))
                    res[i] = obj;
                else
                {
                    var v = obj.ValueType == JSObjectType.Object && obj.oValue != null && obj.oValue.GetType() == typeof(object) ? obj : obj.Value;
                    if (v is Core.BaseTypes.Array)
                        res[i] = convertArray(v as Core.BaseTypes.Array);
                    else if (v is TypeProxy)
                    {
                        var tp = v as TypeProxy;
                        res[i] = (tp.bindFlags & BindingFlags.Static) != 0 ? tp.hostedType : tp.prototypeInstance;
                    }
                    else if (v is TypeProxyConstructor)
                        res[i] = (v as TypeProxyConstructor).proxy.hostedType;
                    else if (v is Function && targetTypes[i].ParameterType.IsSubclassOf(typeof(Delegate)))
                    {
                        res[i] = (v as Function).MakeDelegate(targetTypes[i].ParameterType);
                    }
                    else
                        res[i] = v;
                }
            }
            return res;
        }

        private static JSObject embeddedTypeConvert(JSObject source, Type targetType)
        {
            if (source.GetType() == targetType)
                return source;
            
            switch (source.ValueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        if (typeof(BaseTypes.Number) != targetType && !typeof(BaseTypes.Number).IsSubclassOf(targetType))
                            return null;
                        number.iValue = source.iValue;
                        number.dValue = source.dValue;
                        number.ValueType = source.ValueType;
                        return number;
                    }
                case JSObjectType.String:
                    {
                        if (typeof(BaseTypes.String) != targetType && !typeof(BaseTypes.String).IsSubclassOf(targetType))
                            return null;
                        @string.oValue = source.oValue;
                        return @string;
                    }
                case JSObjectType.Bool:
                    {
                        if (typeof(BaseTypes.Boolean) != targetType && !typeof(BaseTypes.Boolean).IsSubclassOf(targetType))
                            return null;
                        boolean.iValue = source.iValue;
                        return boolean;
                    }
            }
            return null;
        }

        private object getTargetObject(JSObject _this, Type targetType)
        {
            if (info.IsStatic)
                return null;
            object obj = _this;
            if (obj == null)
                return JSObject.undefined;//throw new JSException(TypeProxy.Proxy(new TypeError("Try to call method for incompatible receiver.")));
            if (obj is EmbeddedType)
                return obj;
            var objasjso = obj as JSObject;
            if (obj is JSObject && objasjso.ValueType >= JSObjectType.Object && objasjso.oValue is JSObject)
            {
                obj = objasjso.oValue ?? obj;
                objasjso = obj as JSObject;
            }
            obj = embeddedTypeConvert(objasjso, targetType) ?? objasjso.oValue;
            if (obj is TypeProxy)
                return (obj as TypeProxy).prototypeInstance;
            return obj;
        }

        public override JSObject length
        {
            get
            {
                _length.iValue = info.GetParameters().Length;
                return _length;
            }
        }

        public override JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            try
            {
                object res = null;
                if (@delegate != null)
                    res = @delegate(argumentsToArray(args));
                else
                    res = info.Invoke(getTargetObject(thisOverride ?? context.thisBind, info.DeclaringType), convertArgs(args, info.GetParameters()));
                return TypeProxy.Proxy(res);
            }
            catch (TargetException e)
            {
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(e.Message)));
            }
            catch (Exception e)
            {
                throw e.InnerException ?? e;
            }
        }

        public override JSObject Invoke(Context contextOverride, JSObject args)
        {
            var oldContext = context;
            context = contextOverride;
            try
            {
                return Invoke(null as JSObject, args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject Invoke(Context contextOverride, JSObject thisOverride, JSObject args)
        {
            var oldContext = context;
            if (contextOverride == null || oldContext == contextOverride)
                return Invoke(thisOverride, args);
            context = contextOverride;
            try
            {
                return Invoke(thisOverride, args);
            }
            finally
            {
                context = oldContext;
            }
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            return DefaultFieldGetter(name, fast, own);
        }

        public override string ToString()
        {
            return "function " + info.Name + "(){ [native code] }";
        }

        public override JSObject call(JSObject args)
        {
            var newThis = args.GetField("0", true, false);
            var prmlen = --args.GetField("length", true, false).iValue;
            for (int i = 0; i < prmlen; i++)
                args.fields[i.ToString()] = args.GetField((i + 1).ToString(), true, false);
            args.fields.Remove(prmlen.ToString());
            if (newThis.ValueType < JSObjectType.Object || newThis.oValue != null || (info.DeclaringType == typeof(JSObject)))
                return Invoke(newThis, args);
            else
                return Invoke(Context.currentRootContext.thisBind ?? Context.currentRootContext.GetOwnField("this"), args);
        }
    }
}
