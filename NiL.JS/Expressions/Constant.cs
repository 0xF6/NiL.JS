﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Constant : Expression
    {
        internal JSObject value;

        public JSObject Value { get { return value; } }

        protected internal override PredictedType ResultType
        {
            get
            {
                switch (value.valueType)
                {
                    case JSObjectType.Undefined:
                    case JSObjectType.NotExists:
                    case JSObjectType.NotExistsInObject:
                        return PredictedType.Undefined;
                    case JSObjectType.Bool:
                        return PredictedType.Bool;
                    case JSObjectType.Int:
                        return PredictedType.Int;
                    case JSObjectType.Double:
                        return PredictedType.Double;
                    case JSObjectType.String:
                        return PredictedType.String;
                    default:
                        return PredictedType.Object;
                }
            }
        }

        public Constant(JSObject value)
            : base(null, null, false)
        {
            this.value = value;
        }
        
        internal override JSObject Evaluate(Context context)
        {
            return value;
        }

        internal override NiL.JS.Core.JSObject EvaluateForAssing(NiL.JS.Core.Context context)
        {
            if (value == JSObject.undefined)
                return value;
            return base.EvaluateForAssing(context);
        }

        protected override CodeNode[] getChildsImpl()
        {
            if (value.Value is CodeNode[])
                return value.Value as CodeNode[];
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message, FunctionStatistic statistic)
        {
            var vss = value.oValue as CodeNode[];
            if (vss != null)
                throw new InvalidOperationException("It behaviour is deprecated");
            if (depth <= 1)
            {
                _this = null;
                if (message != null && (value.valueType != JSObjectType.String || value.oValue.ToString() != "use strict"))
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Unused constant was removed. Maybe, something missing.");
            }
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            if (value.valueType == JSObjectType.String)
                return "\"" + value.oValue + "\"";
            if (value.oValue is CodeNode[])
            {
                string res = "";
                for (var i = (value.oValue as CodeNode[]).Length; i-- > 0; )
                    res = (i != 0 ? ", " : "") + (value.oValue as CodeNode[])[i] + res;
                return res;
            }
            return value.ToString();
        }
    }
}