﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class TypeOf : Expression
    {
        private static readonly JSObject numberString = "number";
        private static readonly JSObject undefinedString = "undefined";
        private static readonly JSObject stringString = "string";
        private static readonly JSObject booleanString = "boolean";
        private static readonly JSObject functionString = "function";
        private static readonly JSObject objectString = "object";

        public TypeOf(CodeNode first)
            : base(first, null, false)
        {
            if (second != null)
                throw new InvalidOperationException("Second operand not allowed for typeof operator/");
        }

        internal override JSObject Evaluate(Context context)
        {
            var val = first.Evaluate(context);
            if (val.valueType == JSObjectType.Property)
                return (val.oValue as NiL.JS.Core.BaseTypes.Function[])[1].Invoke(context.objectSource, null);
            switch (val.valueType)
            {
                case JSObjectType.Int:
                case JSObjectType.Double:
                    {
                        return numberString;
                    }
                case JSObjectType.NotExists:
                case JSObjectType.NotExistsInObject:
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
                        //if (val.oValue is TypeProxy
                        //    && (val.oValue as TypeProxy).hostedType == typeof(Function))
                        //    return functionString;
                        return objectString;
                    }
                default: throw new NotImplementedException();
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            base.Build(ref _this, depth, vars, strict);
            if (first is GetVariableStatement)
                first = new SafeVariableGetter(first as GetVariableStatement);
            return false;
        }

        public override string ToString()
        {
            return "typeof " + first;
        }
    }
}