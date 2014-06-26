﻿using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Serializable]
    internal sealed class ThisBind : JSObject
    {
        private static JSObject thisProto;

        internal static JSObject refreshThisBindProto()
        {
            thisProto = CreateObject();
            thisProto.oValue = thisProto;
            thisProto.attributes |= JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete;
            return thisProto;
        }

        private Context context;

        public ThisBind(Context context)
            : base(false)
        {
            attributes = JSObjectAttributesInternal.SystemObject;
            this.context = context;
            fields = context.fields;
            valueType = JSObjectType.Object;
            oValue = this;
            __proto__ = thisProto ?? refreshThisBindProto();
            assignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new ReferenceError("Invalid left-hand side in assignment"))); };
        }

        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            if (name == "__proto__")
            {
                if (__proto__ == null)
                    __proto__ = thisProto;
                return __proto__;
            }
            var res = context.GetVariable(name, create);
            if (res.valueType == JSObjectType.NotExist)
                res.valueType = JSObjectType.NotExistInObject;
            return res;
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            return context.fields.Keys.GetEnumerator();
        }

        public override string ToString()
        {
            return "[object global]";
        }
    }
}
