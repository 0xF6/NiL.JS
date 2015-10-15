﻿using System;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
#if !PORTABLE
    [Serializable]
#endif
    [Prototype(typeof(Function))]
    internal class ProxyConstructor : Function
    {
        // На втором проходе будет выбираться первый метод, 
        // для которого получится сгенерировать параметры по-умолчанию.
        // Если нужен более строгий подбор, то количество проходов нужно
        // уменьшить до одного
        private const int passesCount = 2;

        private static readonly object[] _objectA = new object[0];
        internal readonly TypeProxy proxy;
        private MethodProxy[] constructors;

        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
                return proxy.hostedType.Name;
            }
        }

        [Hidden]
        public override FunctionType Type
        {
            [Hidden]
            get
            {
                return FunctionType.Function;
            }
        }

        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSValue prototype
        {
            [Hidden]
            get
            {
                return _prototype ?? (_prototype = TypeProxy.GetPrototype(proxy.hostedType));
            }
            [Hidden]
            set
            {
                _prototype = value;
            }
        }

        [Hidden]
        public ProxyConstructor(TypeProxy typeProxy)
        {
            fields = typeProxy.fields;
            proxy = typeProxy;
#if PORTABLE
            if (proxy.hostedType.GetTypeInfo().ContainsGenericParameters)
                ExceptionsHelper.Throw((new TypeError(proxy.hostedType.Name + " can't be created because it's generic type.")));
#else
            if (proxy.hostedType.ContainsGenericParameters)
                ExceptionsHelper.Throw((new TypeError(proxy.hostedType.Name + " can't be created because it's generic type.")));
#endif
            var ownew = typeProxy.hostedType.IsDefined(typeof(RequireNewKeywordAttribute), true);
            var owonew = typeProxy.hostedType.IsDefined(typeof(DisallowNewKeywordAttribute), true);
            if (ownew && owonew)
                throw new InvalidOperationException("Unacceptably use of " + typeof(RequireNewKeywordAttribute).Name + " and " + typeof(DisallowNewKeywordAttribute).Name + " for same type.");
            if (ownew)
                RequireNewKeywordLevel = RequireNewKeywordLevel.OnlyWithNew;
            if (owonew)
                RequireNewKeywordLevel = RequireNewKeywordLevel.OnlyWithoutNew;

            if (_length == null)
                _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };

#if PORTABLE
            var ctors = System.Linq.Enumerable.ToArray(typeProxy.hostedType.GetTypeInfo().DeclaredConstructors);
            List<MethodProxy> ctorsL = new List<MethodProxy>(ctors.Length + (typeProxy.hostedType.GetTypeInfo().IsValueType ? 1 : 0));
#else
            var ctors = typeProxy.hostedType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            List<MethodProxy> ctorsL = new List<MethodProxy>(ctors.Length + (typeProxy.hostedType.IsValueType ? 1 : 0));
#endif
            for (int i = 0; i < ctors.Length; i++)
            {
                if (!ctors[i].IsDefined(typeof(HiddenAttribute), false) || ctors[i].IsDefined(typeof(ForceUseAttribute), true))
                {
                    ctorsL.Add(new MethodProxy(ctors[i]));
                    length.iValue = System.Math.Max(ctorsL[ctorsL.Count - 1]._length.iValue, _length.iValue);
                }
            }
            ctorsL.Sort((x, y) => x.Parameters.Length == 1 && x.Parameters[0].ParameterType == typeof(Arguments) ? 1 :
                y.Parameters.Length == 1 && y.Parameters[0].ParameterType == typeof(Arguments) ? -1 :
                x.Parameters.Length - y.Parameters.Length);
            constructors = ctorsL.ToArray();
        }

        [Hidden]
        internal protected override JSValue GetMember(JSValue key, bool forWrite, bool own)
        {
            if (key.valueType != JSValueType.Symbol)
            {
                if (key.ToString() == "prototype") // Все прокси-прототипы read-only и non-configurable. Это и оптимизация, и устранение необходимости навешивания атрибутов
                    return prototype;
                var res = proxy.GetMember(key, forWrite && own, own);
                if (res.IsExists || (own && forWrite))
                {
                    if (forWrite && res.isNeedClone)
                        res = proxy.GetMember(key, true, own);
                    return res;
                }
                res = __proto__.GetMember(key, forWrite, own);
                if (own && (res.valueType != JSValueType.Property || (res.attributes & JSValueAttributesInternal.Field) == 0))
                    return notExists; // если для записи, то первая ветка всё разрулит и сюда выполнение не придёт
                return res;
            }
            return base.GetMember(key, forWrite, own);
        }

        protected internal override bool DeleteMember(JSValue name)
        {
            return proxy.DeleteMember(name) && __proto__.DeleteMember(name);
        }

        [Hidden]
        public override JSValue Invoke(JSValue thisOverride, Arguments argsObj)
        {
            bool bynew = false;
            if (thisOverride != null)
                bynew = thisOverride.oValue == typeof(Expressions.NewOperator) as object;
            if (bynew)
            {
                if (RequireNewKeywordLevel == RequireNewKeywordLevel.OnlyWithoutNew)
                    ExceptionsHelper.Throw(new TypeError(string.Format(Strings.InvalidTryToCreateWithNew, proxy.hostedType.Name)));
            }
            else
            {
                if (RequireNewKeywordLevel == RequireNewKeywordLevel.OnlyWithNew)
                    ExceptionsHelper.Throw(new TypeError(string.Format(Strings.InvalidTryToCreateWithoutNew, proxy.hostedType.Name)));

                if (proxy.hostedType == typeof(Date))
                    return new Date().ToString();
            }
            try
            {
                object obj;
                if (proxy.hostedType == typeof(NiL.JS.BaseLibrary.Array))
                {
                    if (argsObj == null)
                        obj = new NiL.JS.BaseLibrary.Array();
                    else
                        switch (argsObj.length)
                        {
                            case 0:
                                obj = new NiL.JS.BaseLibrary.Array();
                                break;
                            case 1:
                                {
                                    switch (argsObj.a0.valueType)
                                    {
                                        case JSValueType.Int:
                                            obj = new NiL.JS.BaseLibrary.Array(argsObj.a0.iValue);
                                            break;
                                        case JSValueType.Double:
                                            obj = new NiL.JS.BaseLibrary.Array(argsObj.a0.dValue);
                                            break;
                                        default:
                                            obj = new NiL.JS.BaseLibrary.Array(argsObj);
                                            break;
                                    }
                                    break;
                                }
                            default:
                                obj = new NiL.JS.BaseLibrary.Array(argsObj);
                                break;
                        }
                }
                else
                {
                    if ((argsObj == null || argsObj.length == 0)
#if PORTABLE
 && proxy.hostedType.GetTypeInfo().IsValueType)
#else
 && proxy.hostedType.IsValueType)
#endif
                        obj = Activator.CreateInstance(proxy.hostedType);
                    else
                    {
                        object[] args = null;
                        MethodProxy constructor = findConstructor(argsObj, ref args);
                        if (constructor == null)
                            ExceptionsHelper.Throw((new TypeError(proxy.hostedType.Name + " can't be created.")));
                        obj = constructor.InvokeImpl(null, args, argsObj == null ? constructor.parameters.Length != 0 ? new Arguments() : null : argsObj);
                    }
                }
                JSValue res = obj as JSValue;

                // Здесь нельзя возвращать контейнер с ValueType < Object, иначе из New выйдет служебный экземпляр NewMarker
                if (bynew)
                {
                    if (res != null)
                    {
                        // Для Number, Boolean и String
                        if (res.valueType < JSValueType.Object)
                        {
                            res = new ObjectContainer(obj, res.__proto__);
                        }
                        else if (res.oValue is JSValue)
                        {
                            res.oValue = res;
                            // На той стороне понять, по new или нет вызван конструктор не удастся,
                            // поэтому по соглашению такие типы себя настраивают так, как будто они по new,
                            // а в oValue пишут экземпляр аргумента на тот случай, если вызван конструктор типа как функция
                            // с передачей в качестве аргумента существующего экземпляра
                        }
                    }
                    else
                    {
                        res = new ObjectContainer(obj, TypeProxy.GetPrototype(proxy.hostedType));
                        //if (res.fields == null)
                        //    res.fields = createFields();
                        // из-за того, что GetMember сам дотягивается до объекта, можно попробовать убрать создание филдов
                        res.attributes |= proxy.hostedType.IsDefined(typeof(ImmutableAttribute), false) ? JSValueAttributesInternal.Immutable : JSValueAttributesInternal.None;
                        if (obj is Date)
                            res.valueType = JSValueType.Date;
                    }
                }
                else
                {
                    if (proxy.hostedType == typeof(JSValue))
                    {
                        if ((res.oValue is JSValue) && (res.oValue as JSValue).valueType >= JSValueType.Object)
                            return res.oValue as JSValue;
                    }
                    res = res ?? new ObjectContainer(obj)
                    {
                        attributes = JSValueAttributesInternal.SystemObject | (proxy.hostedType.IsDefined(typeof(ImmutableAttribute), false) ? JSValueAttributesInternal.Immutable : JSValueAttributesInternal.None)
                    };
                }
                return res;
            }
            catch (TargetInvocationException e)
            {
#if !PORTABLE
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Log(10, "Exception", e.Message);
#endif
                throw e.InnerException;
            }
        }

        [Hidden]
        private MethodProxy findConstructor(Arguments argObj, ref object[] args)
        {
            args = null;
            var len = argObj == null ? 0 : argObj.length;
            for (var pass = 0; pass < passesCount; pass++)
                for (int i = 0; i < constructors.Length; i++)
                {
                    if (constructors[i].parameters.Length == 1 && (constructors[i].parameters[0].ParameterType == typeof(Arguments)))
                        return constructors[i];
                    if (pass == 1 || constructors[i].parameters.Length == len)
                    {
                        if (len == 0)
                            args = _objectA;
                        else
                        {
                            args = constructors[i].ConvertArgs(argObj, pass == 1);
                            for (var j = args.Length; j-- > 0; )
                            {
                                if (!constructors[i].parameters[j].ParameterType.IsAssignableFrom(args[j] != null ? args[j].GetType() : typeof(object)))
                                {
                                    j = 0;
                                    args = null;
                                }
                            }
                            if (args == null)
                                continue;
                        }
                        return constructors[i];
                    }
                }
            return null;
        }

        internal override JSObject GetDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }

        [Hidden]
        public override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode)
        {
            var e = __proto__.GetEnumerator(hideNonEnumerable, enumerationMode);
            while (e.MoveNext())
                yield return e.Current;
            e = proxy.GetEnumerator(hideNonEnumerable, enumerationMode);
            while (e.MoveNext())
                yield return e.Current;
        }

        [Hidden]
        public override string ToString(bool headerOnly)
        {
            return "function " + proxy.hostedType.Name + "() { [native code] }";
        }

        [Hidden]
        public override JSValue toString(Arguments args)
        {
            return base.toString(args);
        }
    }
}
