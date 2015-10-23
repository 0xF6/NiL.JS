﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public class MoreOperator : Expression
    {
        private bool trueMore;

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public MoreOperator(Expression first, Expression second)
            : base(first, second, true)
        {
            trueMore = this.GetType() == typeof(MoreOperator);
        }

        internal static bool Check(JSValue first, JSValue second, bool lessOrEqual)
        {
            switch (first.valueType)
            {
                case JSValueType.Bool:
                case JSValueType.Int:
                    {
                        switch (second.valueType)
                        {
                            case JSValueType.Bool:
                            case JSValueType.Int:
                                {
                                    return first.iValue > second.iValue;
                                }
                            case JSValueType.Double:
                                {
                                    if (double.IsNaN(second.dValue))
                                        return lessOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                    else
                                        return first.iValue > second.dValue;
                                }
                            case JSValueType.String:
                                {
                                    var index = 0;
                                    double td = 0;
                                    if (Tools.ParseNumber(second.oValue.ToString(), ref index, out td) && (index == (second.oValue.ToString()).Length))
                                        return first.iValue > td;
                                    else
                                        return lessOrEqual;
                                }
                            case JSValueType.Date:
                            case JSValueType.Object:
                                {
                                    second = second.ToPrimitiveValue_Value_String();
                                    if (second.valueType == JSValueType.Int)
                                        goto case JSValueType.Int;
                                    if (second.valueType == JSValueType.Bool)
                                        goto case JSValueType.Int;
                                    if (second.valueType == JSValueType.Double)
                                        goto case JSValueType.Double;
                                    if (second.valueType == JSValueType.String)
                                        goto case JSValueType.String;
                                    if (second.valueType >= JSValueType.Object) // null
                                        return first.iValue > 0;
                                    throw new NotImplementedException();
                                }
                            default:
                                return lessOrEqual;
                        }
                    }
                case JSValueType.Double:
                    {
                        if (double.IsNaN(first.dValue))
                            return lessOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                        else
                            switch (second.valueType)
                            {
                                case JSValueType.Bool:
                                case JSValueType.Int:
                                    {
                                        return first.dValue > second.iValue;
                                    }
                                case JSValueType.Double:
                                    {
                                        if (double.IsNaN(first.dValue) || double.IsNaN(second.dValue))
                                            return lessOrEqual; // Костыль. Для его устранения нужно делать полноценную реализацию оператора MoreOrEqual.
                                        else
                                            return first.dValue > second.dValue;
                                    }
                                case JSValueType.String:
                                    {
                                        var index = 0;
                                        double td = 0;
                                        if (Tools.ParseNumber(second.oValue.ToString(), ref index, out td) && (index == (second.oValue.ToString()).Length))
                                            return first.dValue > td;
                                        else
                                            return lessOrEqual;
                                    }
                                case JSValueType.Undefined:
                                case JSValueType.NotExistsInObject:
                                    {
                                        return lessOrEqual;
                                    }
                                case JSValueType.Date:
                                case JSValueType.Object:
                                    {
                                        second = second.ToPrimitiveValue_Value_String();
                                        if (second.valueType == JSValueType.Int)
                                            goto case JSValueType.Int;
                                        if (second.valueType == JSValueType.Bool)
                                            goto case JSValueType.Int;
                                        if (second.valueType == JSValueType.Double)
                                            goto case JSValueType.Double;
                                        if (second.valueType == JSValueType.String)
                                            goto case JSValueType.String;
                                        if (second.valueType >= JSValueType.Object) // null
                                            return first.dValue > 0;
                                        throw new NotImplementedException();
                                    }
                                default:
                                    return lessOrEqual;
                            }
                    }
                case JSValueType.String:
                    {
                        string left = first.oValue.ToString();
                        switch (second.valueType)
                        {
                            case JSValueType.Bool:
                            case JSValueType.Int:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, out d) && (i == left.Length))
                                        return d > second.iValue;
                                    else
                                        return lessOrEqual;
                                }
                            case JSValueType.Double:
                                {
                                    double d = 0;
                                    int i = 0;
                                    if (Tools.ParseNumber(left, ref i, out d) && (i == left.Length))
                                        return d > second.dValue;
                                    else
                                        return lessOrEqual;
                                }
                            case JSValueType.String:
                                {
                                    return string.CompareOrdinal(left, second.oValue.ToString()) > 0;
                                }
                            case JSValueType.Function:
                            case JSValueType.Object:
                                {
                                    second = second.ToPrimitiveValue_Value_String();
                                    switch (second.valueType)
                                    {
                                        case JSValueType.Int:
                                        case JSValueType.Bool:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t > second.iValue;
                                                else
                                                    goto case JSValueType.String;
                                            }
                                        case JSValueType.Double:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t > second.dValue;
                                                else
                                                    goto case JSValueType.String;
                                            }
                                        case JSValueType.String:
                                            {
                                                return string.CompareOrdinal(left, second.oValue.ToString()) > 0;
                                            }
                                        case JSValueType.Object:
                                            {
                                                double t = 0.0;
                                                int i = 0;
                                                if (Tools.ParseNumber(left, ref i, out t) && (i == left.Length))
                                                    return t > 0;
                                                else
                                                    return lessOrEqual;
                                            }
                                        default: throw new NotImplementedException();
                                    }
                                }
                            default:
                                return lessOrEqual;
                        }
                    }
                case JSValueType.Function:
                case JSValueType.Date:
                case JSValueType.Object:
                    {
                        first = first.ToPrimitiveValue_Value_String();
                        if (first.valueType == JSValueType.Int)
                            goto case JSValueType.Int;
                        if (first.valueType == JSValueType.Bool)
                            goto case JSValueType.Int;
                        if (first.valueType == JSValueType.Double)
                            goto case JSValueType.Double;
                        if (first.valueType == JSValueType.String)
                            goto case JSValueType.String;
                        if (first.valueType >= JSValueType.Object) // null
                        {
                            first.iValue = 0; // такое делать можно, поскольку тип не меняется
                            goto case JSValueType.Int;
                        }
                        throw new NotImplementedException();
                    }
                default:
                    return lessOrEqual;
            }
        }

        public override JSValue Evaluate(Context context)
        {
            var f = first.Evaluate(context);
            var temp = tempContainer;
            tempContainer = null;
            if (temp == null)
                temp = new JSValue { attributes = JSValueAttributesInternal.Temporary };
            temp.valueType = f.valueType;
            temp.iValue = f.iValue;
            temp.dValue = f.dValue;
            temp.oValue = f.oValue;
            var s = second.Evaluate(context);
            tempContainer = temp;
            if (tempContainer.valueType == JSValueType.Int && s.valueType == JSValueType.Int)
            {
                tempContainer.valueType = JSValueType.Bool;
                tempContainer.iValue = tempContainer.iValue > s.iValue ? 1 : 0;
                return tempContainer;
            }
            if (tempContainer.valueType == JSValueType.Double && s.valueType == JSValueType.Double)
            {
                temp.valueType = JSValueType.Bool;
                if (double.IsNaN(temp.dValue) || double.IsNaN(s.dValue))
                    temp.iValue = trueMore ? 0 : 1;
                else
                    temp.iValue = temp.dValue > s.dValue ? 1 : 0;
                return tempContainer;
            }
            return Check(tempContainer, s, !trueMore);
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            baseOptimize(ref _this, owner, message, opts, statistic);
            if (_this == this)
                if (first.ResultType == PredictedType.Number && second.ResultType == PredictedType.Number)
                {
                    _this = new NumberMoreOpeartor(first, second);
                    return;
                }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " > " + second + ")";
        }
    }
}