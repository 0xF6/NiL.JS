﻿using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal class ObjectConstructor : ProxyConstructor
    {
        public override string name
        {
            get
            {
                return "Object";
            }
        }

        public ObjectConstructor(Context context, StaticProxy staticProxy, PrototypeProxy dynamicProxy)
            : base(context, staticProxy, dynamicProxy)
        {
            _length = new Number(1);
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            JSValue nestedValue = targetObject;

            if (arguments != null && arguments.length > 0)
                nestedValue = arguments[0];

            if (nestedValue == null)
                return ConstructObject();

            if (nestedValue._valueType >= JSValueType.Object)
            {
                if (nestedValue._oValue == null)
                    return ConstructObject();

                return nestedValue;
            }

            if (nestedValue._valueType <= JSValueType.Undefined)
                return ConstructObject();

            return nestedValue.ToObject();
        }

        protected internal override JSValue ConstructObject()
        {
            return JSObject.CreateObject();
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumerationMode)
        {
            var pe = _staticProxy.GetEnumerator(hideNonEnum, enumerationMode);
            while (pe.MoveNext())
                yield return pe.Current;
            pe = __proto__.GetEnumerator(hideNonEnum, enumerationMode);
            while (pe.MoveNext())
                yield return pe.Current;
        }
    }
}
