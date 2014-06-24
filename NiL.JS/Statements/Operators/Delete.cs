﻿using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Delete : Operator
    {
        public sealed class SafeMemberGetter : Statement
        {
            private GetMemberStatement proto;

            internal SafeMemberGetter(GetMemberStatement gms)
            {
                proto = gms;
                Position = gms.Position;
                Length = gms.Length;
            }

            protected override Statement[] getChildsImpl()
            {
                return new[] { proto };
            }

            internal override JSObject Invoke(Context context)
            {
                var source = proto.Source.Invoke(context);
                var memberName = proto.FieldName.Invoke(context).ToString();
                context.objectSource = source;
                var res = context.objectSource.GetMember(memberName, false, true);
                if ((res.attributes & JSObjectAttributes.SystemObject) != 0)
                    return context.objectSource.GetMember(memberName, true, true);
                return res;
            }

            public override string ToString()
            {
                return proto.ToString();
            }
        }

        public Delete(Statement first)
            : base(first, null, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var temp = first.Invoke(context);
                tempContainer.valueType = JSObjectType.Bool;
                if (temp.valueType < JSObjectType.Undefined)
                    tempContainer.iValue = 1;
                else if ((temp.attributes & JSObjectAttributes.Argument) != 0)
                {
                    if (first is SafeMemberGetter)
                    {
                        tempContainer.iValue = 1;
                        var args = context.objectSource;
                        foreach (var a in args.fields)
                        {
                            if (a.Value == temp)
                            {
                                args.fields.Remove(a.Key);
                                return tempContainer;
                            }
                        }
                    }
                    else
                    {
                        tempContainer.iValue = 0;
                        return tempContainer;
                    }
                }
                else if ((temp.attributes & JSObjectAttributes.DoNotDelete) == 0)
                {
                    tempContainer.iValue = 1;
                    if ((temp.attributes & JSObjectAttributes.SystemObject) == 0)
                    {
                        temp.valueType = JSObjectType.NotExist;
                        temp.oValue = null;
                    }
                }
                else if (context.strict)
                    throw new JSException(new TypeError("Can not delete property \"" + first + "\"."));
                else
                    tempContainer.iValue = 0;
                return tempContainer;
            }
        }

        internal override bool Optimize(ref Statement _this, int depth, int fdepth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            if (base.Optimize(ref _this, depth, fdepth, vars, strict))
                return true;
            if (first is GetVariableStatement)
            {
                if (strict)
                    throw new JSException(new SyntaxError("Can not evalute delete on variable in strict mode"));
                first = new SafeVariableGetter(first as GetVariableStatement);
            }
            if (first is GetMemberStatement)
                first = new SafeMemberGetter(first as GetMemberStatement);
            return false;
        }

        public override string ToString()
        {
            return "delete " + first;
        }
    }
}