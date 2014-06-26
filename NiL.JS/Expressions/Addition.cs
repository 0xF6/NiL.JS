﻿using System;
using System.Globalization;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Addition : Expression
    {
        public Addition(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                JSObject temp = first.Invoke(context);
                switch (temp.valueType)
                {
                    case JSObjectType.Bool:
                    case JSObjectType.Int:
                        {
                            var type = temp.valueType;
                            int ir = temp.iValue;
                            temp = second.Invoke(context);
                            if (temp.valueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            if (temp.valueType == JSObjectType.Int || temp.valueType == JSObjectType.Bool)
                            {
                                if (((ir | temp.iValue) & (int)0x40000000) == 0)
                                {
                                    tempContainer.valueType = JSObjectType.Int;
                                    tempContainer.iValue = ir + temp.iValue;
                                    return tempContainer;
                                }
                                else
                                {
                                    tempContainer.valueType = JSObjectType.Double;
                                    tempContainer.dValue = (double)ir + temp.iValue;
                                    return tempContainer;
                                }
                            }
                            else if (temp.valueType == JSObjectType.Double)
                            {
                                tempContainer.valueType = JSObjectType.Double;
                                tempContainer.dValue = ir + temp.dValue;
                                return tempContainer;
                            }
                            else if (temp.valueType == JSObjectType.String)
                            {
                                tempContainer.oValue = (type == JSObjectType.Bool ? (ir != 0 ? "true" : "false") : ir.ToString(CultureInfo.InvariantCulture)) + (string)temp.oValue;
                                tempContainer.valueType = JSObjectType.String;
                                return tempContainer;
                            }
                            else if (temp.valueType == JSObjectType.Undefined || temp.valueType == JSObjectType.NotExistInObject)
                            {
                                tempContainer.dValue = double.NaN;
                                tempContainer.valueType = JSObjectType.Double;
                                return tempContainer;
                            }
                            else if (temp.valueType == JSObjectType.Object) // x+null
                            {
                                tempContainer.dValue = ir;
                                tempContainer.valueType = JSObjectType.Double;
                                return tempContainer;
                            }
                            else if (temp.valueType == JSObjectType.NotExist)
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            break;
                        }
                    case JSObjectType.Double:
                        {
                            double dr = temp.dValue;
                            temp = second.Invoke(context);
                            if (temp.valueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.valueType)
                            {
                                case JSObjectType.Int:
                                case JSObjectType.Bool:
                                    {
                                        dr += temp.iValue;
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = dr;
                                        return tempContainer;
                                    }
                                case JSObjectType.Double:
                                    {
                                        dr += temp.dValue;
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = dr;
                                        return tempContainer;
                                    }
                                case JSObjectType.String:
                                    {
                                        tempContainer.oValue = Tools.DoubleToString(dr) + (string)temp.oValue;
                                        tempContainer.valueType = JSObjectType.String;
                                        return tempContainer;
                                    }
                                case JSObjectType.Object: // null
                                    {
                                        tempContainer.dValue = dr;
                                        tempContainer.valueType = JSObjectType.Double;
                                        return tempContainer;
                                    }
                                case JSObjectType.NotExistInObject:
                                case JSObjectType.Undefined:
                                    {
                                        tempContainer.dValue = double.NaN;
                                        tempContainer.valueType = JSObjectType.Double;
                                        return tempContainer;
                                    }
                                case JSObjectType.NotExist:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            }
                            break;
                        }
                    case JSObjectType.String:
                        {
                            var val = temp.oValue as string;
                            temp = second.Invoke(context);
                            if (temp.valueType == JSObjectType.Date)
                                temp = temp.ToPrimitiveValue_String_Value();
                            else if (temp.valueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.valueType)
                            {
                                case JSObjectType.Int:
                                    {
                                        val += temp.iValue;
                                        break;
                                    }
                                case JSObjectType.Double:
                                    {
                                        val += Tools.DoubleToString(temp.dValue);
                                        break;
                                    }
                                case JSObjectType.Bool:
                                    {
                                        val += temp.iValue != 0 ? "true" : "false";
                                        break;
                                    }
                                case JSObjectType.String:
                                    {
                                        val += temp.oValue;
                                        break;
                                    }
                                case JSObjectType.Undefined:
                                case JSObjectType.NotExistInObject:
                                    {
                                        val += "undefined";
                                        break;
                                    }
                                case JSObjectType.Object:
                                case JSObjectType.Function:
                                case JSObjectType.Date:
                                    {
                                        val += "null";
                                        break;
                                    }
                                case JSObjectType.NotExist:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            }
                            tempContainer.oValue = val;
                            tempContainer.valueType = JSObjectType.String;
                            return tempContainer;
                        }
                    case JSObjectType.Date:
                        {
                            temp = temp.ToPrimitiveValue_String_Value();
                            goto case JSObjectType.String;
                        }
                    case JSObjectType.NotExistInObject:
                    case JSObjectType.Undefined:
                        {
                            var val = "undefined";
                            temp = second.Invoke(context);
                            if (temp.valueType >= JSObjectType.Object)
                                temp = temp.ToPrimitiveValue_Value_String();
                            switch (temp.valueType)
                            {
                                case JSObjectType.String:
                                    {
                                        tempContainer.valueType = JSObjectType.String;
                                        tempContainer.oValue = val as string + temp.oValue as string;
                                        return tempContainer;
                                    }
                                case JSObjectType.Double:
                                case JSObjectType.Bool:
                                case JSObjectType.Int:
                                    {
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = double.NaN;
                                        return tempContainer;
                                    }
                                case JSObjectType.Object: // undefined+null
                                case JSObjectType.NotExistInObject:
                                case JSObjectType.Undefined:
                                    {
                                        tempContainer.valueType = JSObjectType.Double;
                                        tempContainer.dValue = double.NaN;
                                        return tempContainer;
                                    }
                                case JSObjectType.NotExist:
                                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            }
                            break;
                        }
                    case JSObjectType.Function:
                    case JSObjectType.Object:
                        {
                            temp = temp.ToPrimitiveValue_Value_String();
                            if (temp.valueType == JSObjectType.Int || temp.valueType == JSObjectType.Bool)
                                goto case JSObjectType.Int;
                            else if (temp.valueType == JSObjectType.Object)
                            {
                                temp = second.Invoke(context);
                                if (temp.valueType >= JSObjectType.Object)
                                    temp = temp.ToPrimitiveValue_Value_String();
                                if (temp.valueType == JSObjectType.Int || temp.valueType == JSObjectType.Bool)
                                {
                                    tempContainer.valueType = JSObjectType.Int;
                                    tempContainer.iValue = temp.iValue;
                                    return tempContainer;
                                }
                                else if (temp.valueType == JSObjectType.Double)
                                {
                                    tempContainer.valueType = JSObjectType.Double;
                                    tempContainer.dValue = temp.dValue;
                                    return tempContainer;
                                }
                                else if (temp.valueType == JSObjectType.String)
                                {
                                    tempContainer.oValue = "null" + (string)temp.oValue;
                                    tempContainer.valueType = JSObjectType.String;
                                    return tempContainer;
                                }
                                else if (temp.valueType == JSObjectType.Undefined || temp.valueType == JSObjectType.NotExistInObject)
                                {
                                    tempContainer.dValue = double.NaN;
                                    tempContainer.valueType = JSObjectType.Double;
                                    return tempContainer;
                                }
                                else if (temp.valueType == JSObjectType.Object) // null+null
                                {
                                    tempContainer.iValue = 0;
                                    tempContainer.valueType = JSObjectType.Int;
                                    return tempContainer;
                                }
                            }
                            else if (temp.valueType == JSObjectType.Double)
                                goto case JSObjectType.Double;
                            else if (temp.valueType == JSObjectType.Int || temp.valueType == JSObjectType.Bool)
                                goto case JSObjectType.Int;
                            else if (temp.valueType == JSObjectType.String)
                                goto case JSObjectType.String;
                            else if (temp.valueType == JSObjectType.NotExist)
                                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                            break;
                        }
                    case JSObjectType.NotExist:
                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Variable not defined.")));
                }
                throw new NotImplementedException();
            }
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
