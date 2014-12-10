﻿using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class InstanceOf : Expression
    {
        private static readonly JSObject prototype = "prototype";

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public InstanceOf(Expression first, Expression second)
            : base(first, second, true)
        {
        }

        internal override JSObject Evaluate(Context context)
        {
            var a = tempContainer ?? new JSObject { attributes = JSObjectAttributesInternal.Temporary };
            a.Assign(first.Evaluate(context));
            tempContainer = null;
            var c = second.Evaluate(context);
            tempContainer = a;
            if (c.valueType != JSObjectType.Function)
                throw new JSException(new NiL.JS.Core.BaseTypes.TypeError("Right-hand value of instanceof is not function."));
            var p = (c.oValue as Function).prototype;
            if (p.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Property \"prototype\" of function not represent object."));
            if (p.oValue != null)
            {
                while (a != null && a.valueType >= JSObjectType.Object && a.oValue != null)
                {
                    if (a.oValue == p.oValue)
                        return true;
                    a = a.__proto__;
                }
            }
            return false;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message)
        {
            var res = base.Build(ref _this, depth, vars, strict, message);
            if (!res)
            {
                if (first is Constant)
                {

                }
            }
            return res;
        }

        public override string ToString()
        {
            return "(" + first + " instanceof " + second + ")";
        }
    }
}