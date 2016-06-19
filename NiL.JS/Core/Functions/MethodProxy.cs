﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using System.Collections.Generic;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Core.Functions
{
    [Flags]
    internal enum ConvertArgsOptions
    {
        None = 0,
        ThrowOnError = 1,
        StrictConversion = 2,
        DummyValues = 4
    }

    internal sealed class MethodProxy : Function
    {
        private static readonly Dictionary<MethodBase, Func<object, object[], Arguments, object>> _wrapperCache = new Dictionary<MethodBase, Func<object, object[], Arguments, object>>();
        private Func<object, object[], Arguments, object> wrapper;
        private bool forceInstance;
        private bool strictConversion;

        private object hardTarget;
        private MethodBase method;
        private ConvertValueAttribute returnConverter;
        private ConvertValueAttribute[] paramsConverters;

        internal ParameterInfo[] parameters;
        internal bool raw;

        [Hidden]
        public ParameterInfo[] Parameters
        {
            [Hidden]
            get
            {
                return parameters;
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override string name
        {
            [Hidden]
            get
            {
                return method.Name;
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSValue prototype
        {
            [Hidden]
            get
            {
                return null;
            }
            [Hidden]
            set
            {

            }
        }

        private MethodProxy()
        {
            parameters = new ParameterInfo[0];
            wrapper = delegate { return null; };
            RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
        }

        public MethodProxy(MethodBase methodBase, object hardTarget)
        {
            this.method = methodBase;
            this.hardTarget = hardTarget;
            this.parameters = methodBase.GetParameters();
            this.strictConversion = methodBase.IsDefined(typeof(StrictConversionAttribute), true);

            if (_length == null)
                _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject };

            var pc = methodBase.GetCustomAttributes(typeof(ArgumentsLengthAttribute), false).ToArray();
            if (pc.Length != 0)
                _length.iValue = (pc[0] as ArgumentsLengthAttribute).Count;
            else
                _length.iValue = parameters.Length;

            for (int i = 0; i < parameters.Length; i++)
            {
                var t = parameters[i].GetCustomAttributes(typeof(ConvertValueAttribute), false).ToArray();
                if (t != null && t.Length != 0)
                {
                    if (paramsConverters == null)
                        paramsConverters = new ConvertValueAttribute[parameters.Length];
                    paramsConverters[i] = t[0] as ConvertValueAttribute;
                }
            }

            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
            {
                returnConverter = methodInfo.ReturnParameter.GetCustomAttribute(typeof(ConvertValueAttribute), false) as ConvertValueAttribute;

                forceInstance = methodBase.IsDefined(typeof(InstanceMemberAttribute), false);

                if (forceInstance)
                {
                    if (!methodInfo.IsStatic
                        || (parameters.Length == 0)
                        || (parameters.Length > 2)
                        || (parameters[0].ParameterType != typeof(JSValue))
                        || (parameters.Length > 1 && parameters[1].ParameterType != typeof(Arguments)))
                        throw new ArgumentException("Force-instance method \"" + methodBase + "\" have invalid signature");
                    raw = true;
                }

                if (!_wrapperCache.TryGetValue(method, out wrapper))
                {
#if PORTABLE
                    wrapper = makeMethodOverExpression(methodInfo);
#else
                    wrapper = makeMethodOverEmit(methodInfo, parameters, forceInstance);
#endif
                    _wrapperCache[method] = wrapper;
                }
                else
                {
                    raw |= parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments));
                }

                RequireNewKeywordLevel = BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly;
            }
            else if (methodBase is ConstructorInfo)
            {
                if (!_wrapperCache.TryGetValue(method, out wrapper))
                {
                    wrapper = makeConstructorOverExpression(methodBase as ConstructorInfo, parameters);
                    _wrapperCache[method] = wrapper;
                }
                else
                {
                    raw |= parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments));
                }
            }
            else
                throw new NotImplementedException();
        }
#if !PORTABLE
        private Func<object, object[], Arguments, object> makeMethodOverEmit(MethodInfo methodInfo, ParameterInfo[] parameters, bool forceInstance)
        {
            var impl = new DynamicMethod(
                "<nil.js@wrapper>" + methodInfo.Name,
                typeof(object),
                new[]
                {
                    typeof(object), // target
                    typeof(object[]), // argsArray
                    typeof(Arguments) // argsSource
                },
                typeof(MethodProxy),
                true);

            var generator = impl.GetILGenerator();

            if (!methodInfo.IsStatic)
            {
                generator.Emit(OpCodes.Ldarg_0);
                if (methodInfo.DeclaringType.IsValueType)
                {
                    generator.Emit(OpCodes.Ldc_I4, IntPtr.Size);
                    generator.Emit(OpCodes.Add);
                }
            }

            if (forceInstance)
            {
                for (;;)
                {
                    if (methodInfo.IsStatic && parameters[0].ParameterType == typeof(JSValue))
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        if (parameters.Length == 1)
                        {
                            break;
                        }
                        else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Arguments))
                        {
                            generator.Emit(OpCodes.Ldarg_2);
                            break;
                        }
                    }
                    throw new ArgumentException("Invalid method signature");
                }
                raw = true;
            }
            else if (parameters.Length == 0)
            {
                raw = true;
            }
            else
            {
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                {
                    raw = true;
                    generator.Emit(OpCodes.Ldarg_2);
                }
                else
                {
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        switch (i)
                        {
                            case 0:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_0);
                                    break;
                                }
                            case 1:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_1);
                                    break;
                                }
                            case 2:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_2);
                                    break;
                                }
                            case 3:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_3);
                                    break;
                                }
                            case 4:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_4);
                                    break;
                                }
                            case 5:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_5);
                                    break;
                                }
                            case 6:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_6);
                                    break;
                                }
                            case 7:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_7);
                                    break;
                                }
                            case 8:
                                {
                                    generator.Emit(OpCodes.Ldc_I4_8);
                                    break;
                                }
                            default:
                                {
                                    generator.Emit(OpCodes.Ldc_I4, i);
                                    break;
                                }
                        }
                        generator.Emit(OpCodes.Ldelem_Ref);
                        if (parameters[i].ParameterType.IsValueType)
                            generator.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
                    }
                }
            }
            if (methodInfo.IsStatic || methodInfo.DeclaringType.IsValueType)
            {
                generator.Emit(OpCodes.Call, methodInfo);
            }
            else
            {
                generator.Emit(OpCodes.Callvirt, methodInfo);
            }

            if (methodInfo.ReturnType == typeof(void))
            {
                generator.Emit(OpCodes.Ldnull);
            }
            else if (methodInfo.ReturnType.IsValueType)
            {
                generator.Emit(OpCodes.Box, methodInfo.ReturnType);
            }

            generator.Emit(OpCodes.Ret);
            return (Func<object, object[], Arguments, object>)impl.CreateDelegate(typeof(Func<object, object[], Arguments, object>));
        }
#else
        private Func<object, object[], Arguments, object> makeMethodOverExpression(MethodInfo methodInfo)
        {
            Expression[] prms = null;
            ParameterExpression target = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsArray = Expression.Parameter(typeof(object[]), "argsArray");
            ParameterExpression argsSource = Expression.Parameter(typeof(Arguments), "arguments");

            Expression tree = null;

            if (forceInstance)
            {
                for (; ; )
                {
                    if (methodInfo.IsStatic && parameters[0].ParameterType == typeof(JSValue))
                    {
                        if (parameters.Length == 1)
                        {
                            tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSValue)));
                            break;
                        }
                        else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Arguments))
                        {
                            tree = Expression.Call(methodInfo, Expression.Convert(target, typeof(JSValue)), argsSource);
                            break;
                        }
                    }
                    throw new ArgumentException("Invalid method signature");
                }
            }
            else if (parameters.Length == 0)
            {
                raw = true;
                tree = methodInfo.IsStatic ?
                    Expression.Call(methodInfo)
                    :
                    Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo);
            }
            else
            {
                prms = new Expression[parameters.Length];
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                {
                    raw = true;
                    tree = methodInfo.IsStatic ?
                        Expression.Call(methodInfo, argsSource)
                        :
                        Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, argsSource);
                }
                else
                {
                    for (var i = 0; i < prms.Length; i++)
                        prms[i] = Expression.Convert(Expression.ArrayAccess(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
                    tree = methodInfo.IsStatic ?
                        Expression.Call(methodInfo, prms)
                        :
                        Expression.Call(Expression.Convert(target, methodInfo.DeclaringType), methodInfo, prms);
                }
            }
            if (methodInfo.ReturnType == typeof(void))
                tree = Expression.Block(tree, Expression.Constant(null));
            try
            {
                return Expression.Lambda<Func<object, object[], Arguments, object>>(Expression.Convert(tree, typeof(object)), target, argsArray, argsSource).Compile();
            }
            catch
            {
                throw;
            }
        }
#endif
        private Func<object, object[], Arguments, object> makeConstructorOverExpression(ConstructorInfo constructorInfo, ParameterInfo[] parameters)
        {
            Expression[] prms = null;
            ParameterExpression target = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsArray = Expression.Parameter(typeof(object[]), "argsArray");
            ParameterExpression argsSource = Expression.Parameter(typeof(Arguments), "arguments");

            Expression tree = null;

            if (parameters.Length == 0)
            {
                raw = true;
                tree = Expression.New(constructorInfo.DeclaringType);
            }
            else
            {
                prms = new Expression[parameters.Length];
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Arguments))
                {
                    raw = true;
                    tree = Expression.New(constructorInfo, argsSource);
                }
                else
                {
                    for (var i = 0; i < prms.Length; i++)
#if NET35
                        prms[i] = Expression.Convert(Expression.ArrayIndex(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
#else
                        prms[i] = Expression.Convert(Expression.ArrayAccess(argsArray, Expression.Constant(i)), parameters[i].ParameterType);
#endif
                    tree = Expression.New(constructorInfo, prms);
                }
            }
            try
            {
                return Expression.Lambda<Func<object, object[], Arguments, object>>(Expression.Convert(tree, typeof(object)), target, argsArray, argsSource).Compile();
            }
            catch
            {
                throw;
            }
        }

        internal override JSValue InternalInvoke(JSValue targetObject, Expressions.Expression[] arguments, Context initiator, bool withSpread, bool withNew)
        {
            if (withNew)
            {
                if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.WithoutNewOnly)
                {
                    ExceptionsHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithNew, name));
                }
            }
            else
            {
                if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.WithNewOnly)
                {
                    ExceptionsHelper.ThrowTypeError(string.Format(Strings.InvalidTryToCreateWithoutNew, name));
                }
            }
            if (parameters.Length == 0 || (forceInstance && parameters.Length == 1))
                return Invoke(withNew, correctTargetObject(targetObject, creator.body._strict), null);

            if (raw || withSpread)
            {
                return base.InternalInvoke(targetObject, arguments, initiator, true, withNew);
            }
            else
            {
                object[] args = null;
                int targetCount = parameters.Length;
                args = new object[targetCount];
                for (int i = 0; i < targetCount; i++)
                {
                    var obj = arguments.Length > i ? Tools.PrepareArg(initiator, arguments[i]) : notExists;

                    args[i] = convertArg(i, obj, ConvertArgsOptions.ThrowOnError | ConvertArgsOptions.DummyValues);
                }

                return TypeProxy.Proxy(InvokeImpl(targetObject, args, null));
            }
        }

        [Hidden]
        internal object InvokeImpl(JSValue thisBind, object[] args, Arguments argsSource)
        {
            object target = hardTarget;
            if (target == null)
            {
                if (forceInstance)
                {
                    if (thisBind != null && thisBind.valueType >= JSValueType.Object)
                    {
                        // Объект нужно развернуть до основного значения. Даже если это обёртка над примитивным значением
                        target = thisBind.Value;
                        if (target is TypeProxy)
                            target = (target as TypeProxy).prototypeInstance ?? thisBind.Value;
                        // ForceInstance работает только если первый аргумент типа JSValue
                        if (!(target is JSValue))
                            target = thisBind;
                    }
                    else
                        target = thisBind ?? undefined;
                }
                else if (!method.IsStatic && !method.IsConstructor)
                {
                    target = getTargetObject(thisBind ?? undefined, method.DeclaringType);
                    if (target == null
#if !PORTABLE
                        || !method.DeclaringType.IsAssignableFrom(target.GetType())
#endif
                        )
                    {
                        // Исключительная ситуация. Я не знаю почему Function.length обобщённое свойство, а не константа. Array.length работает по-другому.
                        if (method.Name == "get_length" && typeof(Function).IsAssignableFrom(method.DeclaringType))
                            return 0;

                        ExceptionsHelper.Throw(new TypeError("Can not call function \"" + name + "\" for object of another type."));
                    }
                }
            }

            try
            {
                object res = wrapper(
                    target,
                    raw ? null : (args ?? ConvertArgs(argsSource, ConvertArgsOptions.ThrowOnError | ConvertArgsOptions.DummyValues)),
                    argsSource);

                if (returnConverter != null)
                    res = returnConverter.From(res);
                return res;
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                    e = e.InnerException;
                if (e is JSException)
                    throw e;
                ExceptionsHelper.Throw(new TypeError(e.Message), e);
            }
            return null;
        }

        private object getDummy()
        {
            if (typeof(JSValue).IsAssignableFrom(method.DeclaringType))
                if (typeof(Function).IsAssignableFrom(method.DeclaringType))
                    return this;
                else if (typeof(TypedArray).IsAssignableFrom(method.DeclaringType))
                    return new Int8Array();
                else
                    return new JSValue();
            if (typeof(Error).IsAssignableFrom(method.DeclaringType))
                return new Error();
            return null;
        }

        public MethodProxy(MethodBase methodBase)
            : this(methodBase, null)
        {
        }

        private static object getTargetObject(JSValue _this, Type targetType)
        {
            if (_this == null)
                return null;
            _this = _this.oValue as JSValue ?? _this; // это может быть лишь ссылка на какой-то другой контейнер
            var res = Tools.convertJStoObj(_this, targetType, false);
            return res;
        }

        internal object[] ConvertArgs(Arguments source, ConvertArgsOptions options)
        {
            if (parameters.Length == 0)
                return null;

            if (forceInstance)
                ExceptionsHelper.Throw(new InvalidOperationException());

            object[] res = null;
            int targetCount = parameters.Length;
            for (int i = targetCount; i-- > 0;)
            {
                var obj = source?[i] ?? undefined;

                var trueNull = obj.valueType >= JSValueType.Object && obj.oValue == null;

                var t = convertArg(i, obj, options);

                if (t == null && !trueNull)
                    return null;

                if (res == null)
                    res = new object[targetCount];

                res[i] = t;
            }

            return res;
        }

        private object convertArg(int i, JSValue obj, ConvertArgsOptions options)
        {
            object result = null;
            var strictConv = strictConversion || (options & ConvertArgsOptions.StrictConversion) != 0;

            if (paramsConverters != null && paramsConverters[i] != null)
            {
                return paramsConverters[i].To(obj);
            }
            else
            {
                var trueNull = obj.valueType >= JSValueType.Object && obj.oValue == null;

                if (!trueNull)
                    result = Tools.convertJStoObj(obj, parameters[i].ParameterType, !strictConv);

                if (strictConv && (trueNull ? parameters[i].ParameterType.GetTypeInfo().IsValueType : result == null))
                {
                    if ((options & ConvertArgsOptions.ThrowOnError) != 0)
                        ExceptionsHelper.ThrowTypeError("Cannot convert " + obj + " to type " + parameters[i].ParameterType);
                    return null;
                }

                if (trueNull && parameters[i].ParameterType.GetTypeInfo().IsClass)
                    return null;
            }

            if (result == null)
            {
                result = parameters[i].DefaultValue;
#if PORTABLE
                if (result != null && result.GetType().FullName == "System.DBNull")
                {
                    if (parameters[i].ParameterType.GetTypeInfo().IsValueType)
#else
                if (result is DBNull)
                {
#endif
                    if (strictConv)
                    {
                        if ((options & ConvertArgsOptions.ThrowOnError) != 0)
                            ExceptionsHelper.ThrowTypeError("Cannot convert " + obj + " to type " + parameters[i].ParameterType);
                        return null;
                    }

                    if ((options & ConvertArgsOptions.DummyValues) != 0 && parameters[i].ParameterType.GetTypeInfo().IsValueType)
                        result = Activator.CreateInstance(parameters[i].ParameterType);
                    else
                        result = null;
                }
            }

            return result;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            return TypeProxy.Proxy(InvokeImpl(targetObject, null, arguments));
        }

        internal static object[] argumentsToArray(Arguments source)
        {
            var len = source.length;
            var res = new object[len];
            for (int i = 0; i < len; i++)
                res[i] = source[i] as object;
            return res;
        }

#if DEVELOPBRANCH || VERSION21
        public sealed override JSValue bind(Arguments args)
        {
            if (hardTarget != null || args.Length == 0)
                return this;

            return new MethodProxy()
            {
                hardTarget = getTargetObject(args[0], method.DeclaringType) ?? args[0].Value as JSObject ?? args[0],
                method = method,
                parameters = parameters,
                wrapper = wrapper,
                forceInstance = forceInstance,
                raw = raw
            };
        }
#if !NET40
        public override Delegate MakeDelegate(Type delegateType)
        {
            try
            {
                var methodInfo = method as MethodInfo;
                return methodInfo.CreateDelegate(delegateType, hardTarget);
            }
            catch
            {
            }

            return base.MakeDelegate(delegateType);
        }
#endif
#endif

        [Hidden]
        public override string ToString(bool headerOnly)
        {
            var result = "function " + name + "()";

            if (!headerOnly)
            {
                result += " { [native code] }";
            }

            return result;
        }
    }
}
