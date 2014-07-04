﻿using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Decriment : Expression
    {
        public enum Type
        {
            Predecriment,
            Postdecriment
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public Decriment(CodeNode op, Type type)
            : base(type == Type.Predecriment ? op : null, type == Type.Postdecriment ? op : null, type == Type.Postdecriment)
        {
            if (type > Type.Postdecriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
            if (tempContainer != null)
                tempContainer.assignCallback = null;
        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                Function setter = null;
                var val = Tools.RaiseIfNotExist((first ?? second).InvokeForAssing(context), first ?? second);
                if (val.valueType == JSObjectType.Property)
                {
                    setter = (val.oValue as Function[])[0];
                    if (context.strict && setter == null)
                        throw new JSException(new TypeError("Can not increment property \"" + (first ?? second) + "\" without setter."));
                    val = (val.oValue as Function[])[1].Invoke(context.objectSource, null).Clone() as JSObject;
                    val.attributes = 0;
                }
                else if (context.strict && (val.attributes & JSObjectAttributesInternal.ReadOnly) != 0)
                    throw new JSException(new TypeError("Can not decriment readonly property \"" + (first ?? second) + "\""));
                switch (val.valueType)
                {
                    case JSObjectType.Object:
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                        {
                            val.Assign(val.ToPrimitiveValue_Value_String());
                            break;
                        }
                    default:
                        {
                            if (val.assignCallback != null)
                                val.assignCallback(val);
                            break;
                        }
                }
                switch (val.valueType)
                {
                    case JSObjectType.Bool:
                        {
                            val.valueType = JSObjectType.Int;
                            break;
                        }
                    case JSObjectType.String:
                        {
                            double resd;
                            int i = 0;
                            if (!Tools.ParseNumber(val.oValue as string, i, out resd, Tools.ParseNumberOptions.Default))
                                resd = double.NaN;
                            val.valueType = JSObjectType.Double;
                            val.dValue = resd;
                            break;
                        }
                    case JSObjectType.Date:
                    case JSObjectType.Function:
                    case JSObjectType.Object: // null
                        {
                            val.iValue = 0;
                            val.valueType = JSObjectType.Int;
                            break;
                        }
                }
                JSObject o = null;
                if (second != null && val.isDefinded)
                {
                    o = tempContainer;
                    o.Assign(val);
                }
                else
                    o = val;
                switch (val.valueType)
                {
                    case JSObjectType.Int:
                        {
                            if (val.iValue == -0x80000000)
                            {
                                val.dValue = val.iValue - 1.0;
                                val.valueType = JSObjectType.Double;
                            }
                            else
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
                            val.valueType = JSObjectType.Double;
                            val.dValue = double.NaN;
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
                if (setter != null)
                {
                    var args = new Arguments();
                    args.length = 1;
                    args["0"] = val;
                    setter.Invoke(context.objectSource, args);
                }
                return o;
            }
        }

        public override string ToString()
        {
            return first != null ? "--" + first : second + "--";
        }
    }
}