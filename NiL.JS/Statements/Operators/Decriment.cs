﻿using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Decriment : Operator
    {
        public Decriment(Statement first, Statement second)
            : base(first, second)
        {
            tempResult.assignCallback = null;
        }

        public override JSObject Invoke(Context context)
        {
            var val = Tools.RaiseIfNotExist((first ?? second).Invoke(context));
            if (val.assignCallback != null)
                val.assignCallback();
            if ((val.attributes & ObjectAttributes.ReadOnly) != 0)
                return double.NaN;
            switch (val.ValueType)
            {
                case JSObjectType.Object:
                case JSObjectType.Date:
                case JSObjectType.Function:
                    {
                        val.Assign(val.ToPrimitiveValue_Value_String());
                        break;
                    }
            }
            switch (val.ValueType)
            {
                case JSObjectType.Bool:
                    {
                        val.ValueType = JSObjectType.Int;
                        break;
                    }
                case JSObjectType.String:
                    {
                        double resd;
                        int i = 0;
                        if (!Tools.ParseNumber(val.oValue as string, ref i, false, out resd))
                            resd = double.NaN;
                        val.ValueType = JSObjectType.Double;
                        val.dValue = resd;
                        break;
                    }
                case JSObjectType.Date:
                case JSObjectType.Function:
                case JSObjectType.Object: // null
                    {
                        val.iValue = 0;
                        val.ValueType = JSObjectType.Int;
                        break;
                    }
            }
            JSObject o = null;
            if ((second != null) && (val.ValueType != JSObjectType.Undefined) && (val.ValueType != JSObjectType.NotExistInObject))
            {
                o = tempResult;
                o.Assign(val);
            }
            else
                o = val;
            switch (val.ValueType)
            {
                case JSObjectType.Int:
                    {
                        val.iValue--;
                        break;
                    }
                case JSObjectType.Double:
                    {
                        val.dValue--;
                        break;
                    }
                case JSObjectType.Undefined:
                case JSObjectType.NotExistInObject:
                    {
                        val.ValueType = JSObjectType.Double;
                        val.dValue = double.NaN;
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
            return o;
        }
    }
}