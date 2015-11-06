﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class UnsignedShiftRightOperator : Expression
    {
        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        public UnsignedShiftRightOperator(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var left = (uint)Tools.JSObjectToInt32(first.Evaluate(context));
            var t = left >> Tools.JSObjectToInt32(second.Evaluate(context));
            if (t <= int.MaxValue)
            {
                tempContainer.iValue = (int)t;
                tempContainer.valueType = JSValueType.Int;
            }
            else
            {
                tempContainer.dValue = (double)t;
                tempContainer.valueType = JSValueType.Double;
            }
            return tempContainer;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
            if (!res && _this == this)
            {
                try
                {
                    if (first.ContextIndependent && Tools.JSObjectToInt32((first).Evaluate(null)) == 0)
                        _this = new ConstantDefinition(0);
                    else if (second.ContextIndependent && Tools.JSObjectToInt32((second).Evaluate(null)) == 0)
                        _this = new ToUnsignedIntegerExpression(first);
                }
                catch
                {

                }
            }
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " >>> " + second + ")";
        }
    }
}