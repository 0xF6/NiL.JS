﻿using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class TypeOf : Operator
    {
        private static readonly JSObject numberString = "number";
        private static readonly JSObject undefinedString = "undefined";
        private static readonly JSObject stringString = "string";
        private static readonly JSObject booleanString = "boolean";
        private static readonly JSObject functionString = "function";
        private static readonly JSObject objectString = "object";

        public TypeOf(Statement first)
            : base(first, null)
        {
            if (second != null)
                throw new InvalidOperationException("Second operand not allowed for typeof operator/");
        }

        internal override JSObject Invoke(Context context)
        {
            var val = first.InvokeForAssing(context);
            if ((val.attributes & JSObjectAttributes.GetValue) != 0)
                val = (val.oValue as NiL.JS.Core.BaseTypes.Function[])[1].Invoke(context, context.objectSource, null);
            var vt = val.ValueType;
            switch (vt)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        return numberString;
                    }
                case JSObjectType.NotExist:
                case JSObjectType.NotExistInObject:
                case JSObjectType.Undefined:
                    {
                        return undefinedString;
                    }
                case JSObjectType.String:
                    {
                        return stringString;
                    }
                case JSObjectType.Bool:
                    {
                        return booleanString;
                    }
                case JSObjectType.Function:
                    {
                        return functionString;
                    }
                case JSObjectType.Date:
                case JSObjectType.Object:
                case JSObjectType.Property:
                    {
                        return objectString;
                    }
                default: throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return "typeof " + first;
        }
    }
}