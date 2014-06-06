using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System.Collections.Generic;
using System.Collections;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetVaribleStatement : VaribleReference
    {
        private string varibleName;
        private VaribleDescriptor descriptor;
        public override VaribleDescriptor Descriptor { get { return descriptor; } internal set { descriptor = value; } }

        public override string Name { get { return varibleName; } }

        internal GetVaribleStatement(string name)
        {
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, ref i, false, true, true, false))
                throw new ArgumentException("Invalid varible name");
            this.varibleName = name;
        }

        internal override JSObject InvokeForAssing(Context context)
        {
            if (context.strict)
                return Tools.RaiseIfNotExist(descriptor.Get(context));
            return descriptor.Get(context);
        }

        internal override JSObject Invoke(Context context)
        {
            var res = Tools.RaiseIfNotExist(descriptor.Get(context));
            if (context.GetType() == typeof(WithContext))
            {
                if (res.ValueType == JSObjectType.Property)
                    return (res.oValue as NiL.JS.Core.BaseTypes.Function[])[1].Invoke(context, context.objectSource, null);
                return res;
            }
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return varibleName;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> varibles)
        {
            VaribleDescriptor desc = null;
            if (!varibles.TryGetValue(varibleName, out desc) || desc == null)
            {
                this.descriptor = new VaribleDescriptor(this, desc != null);
                varibles[varibleName] = this.descriptor;
            }
            else
                desc.Add(this);
            return false;
        }
    }
}