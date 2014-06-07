﻿using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет функцию платформы с фиксированной сигнатурой.
    /// </summary>
    [Modules.Prototype(typeof(Function))]
    [Serializable]
    public sealed class ExternalFunction : Function
    {
        private readonly ExternalFunctionDelegate del;

        public ExternalFunction(ExternalFunctionDelegate del)
            : base(Context.globalContext, null, null, del.Method.Name)
        {
            this.del = del;
        }

        public override JSObject Invoke(Context contextOverride, JSObject args)
        {
            var oldContext = context;
            context = contextOverride;
            try
            {
                return Invoke(args);
            }
            finally
            {
                if (context != oldContext)
                {
                    context = oldContext;
                    oldContext.ValidateThreadID();
                }
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
                if (context != oldContext)
                {
                    context = oldContext;
                    oldContext.ValidateThreadID();
                }
            }
        }

        public override JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            if (thisOverride == null)
                return Invoke(args);
            var oldThisBind = context.thisBind;
            try
            {
                context.thisBind = thisOverride;
                return Invoke(args);
            }
            finally
            {
                context.thisBind = oldThisBind;
            }
        }

        [Modules.DoNotDelete]
        public override JSObject length
        {
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DontDelete | JSObjectAttributes.DontEnum };
                _length.iValue = 1;
                return _length;
            }
        }

        public override JSObject Invoke(JSObject args)
        {
            context.ValidateThreadID();
            var res = del(context, args);
            if (res == null)
                return JSObject.Null;
            return res;
        }
    }
}