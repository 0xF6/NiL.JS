﻿using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.Functions
{
    /// <summary>
    /// Представляет функцию платформы с фиксированной сигнатурой.
    /// </summary>
    [Prototype(typeof(Function))]
    [Serializable]
    public sealed class ExternalFunction : Function
    {
        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
                return del.Method.Name;
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
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSObject prototype
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

        private readonly ExternalFunctionDelegate del;

        [Hidden]
        public ExternalFunctionDelegate Delegate { get { return del; } }

        public ExternalFunction(ExternalFunctionDelegate del)
        {
            if (_length == null)
                _length = new Number(0) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum };
            var paramCountAttrbt = del.Method.GetCustomAttributes(typeof(ParametersCountAttribute), false);
            _length.iValue = paramCountAttrbt != null && paramCountAttrbt.Length > 0 ? ((ParametersCountAttribute)paramCountAttrbt[0]).Count : 1;
            _prototype = undefined;
            if (del == null)
                throw new ArgumentNullException();
            this.del = del;
       }

        [Hidden]
        public override JSObject Invoke(JSObject thisBind, Arguments args)
        {
            var res = del(thisBind, args);
            if (res == null)
                return JSObject.Null;
            return res;
        }
    }
}