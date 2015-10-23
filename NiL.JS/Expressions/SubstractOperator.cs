﻿
#define TYPE_SAFE

using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class SubstractOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public SubstractOperator(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            //lock (this)
            {
#if TYPE_SAFE
                double da = 0.0;
                JSValue f = first.Evaluate(context);
                JSValue s = null;
                long l = 0;
                int a;
                if (f.valueType == JSValueType.Int
                    || f.valueType == JSValueType.Bool)
                {
                    a = f.iValue;
                    s = second.Evaluate(context);
                    if (s.valueType == JSValueType.Int
                    || s.valueType == JSValueType.Bool)
                    {
                        l = (long)a - s.iValue;
                        //if (l > 2147483647L
                        //    || l < -2147483648L)
                        if (l != (int)l)
                        {
                            tempContainer.dValue = l;
                            tempContainer.valueType = JSValueType.Double;
                        }
                        else
                        {
                            tempContainer.iValue = (int)l;
                            tempContainer.valueType = JSValueType.Int;
                        }
                        return tempContainer;
                    }
                    else
                        da = a;
                }
                else
                {
                    da = Tools.JSObjectToDouble(f);
                    s = second.Evaluate(context);
                }
                tempContainer.dValue = da - Tools.JSObjectToDouble(s);
                tempContainer.valueType = JSValueType.Double;
                return tempContainer;
#else
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) - Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
#endif
            }
        }

        internal protected override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth, variables, state, message, statistic, opts);
            if (!res)
            {
                if (first is ConstantNotation && Tools.JSObjectToDouble(first.Evaluate(null)) == 0.0)
                {
                    _this = new NegationOperator(second);
                    return true;
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
            return "(" + first + " - " + second + ")";
        }
    }
}