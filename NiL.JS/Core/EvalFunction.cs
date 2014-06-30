﻿using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    public sealed class EvalFunction : Function
    {
        [Hidden]
        public override string name
        {
            [Hidden]
            get
            {
                return "eval";
            }
        }

        [Hidden]
        public override FunctionType Type
        {
            [Hidden]
            get
            {
                return FunctionType.Function;
            }
        }

        [DoNotDelete]
        [DoNotEnumerate]
        public override JSObject length
        {
            [Hidden]
            get
            {
                return 1;
            }
        }

        [Hidden]
        public EvalFunction()
        {

        }

        [Hidden]
        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisBind, NiL.JS.Core.JSObject args)
        {
            var arg = args["0"];
            if (arg.valueType != JSObjectType.String)
                return arg;
            if (this.lastRequestedName == "eval")
                return Context.CurrentContext.Eval(arg.ToString());
            Stack<Context> stack = new Stack<Context>();
            try
            {
                var ccontext = Context.CurrentContext;
                var root = ccontext.Root;
                while (ccontext != root && ccontext != null)
                {
                    stack.Push(ccontext);
                    ccontext = ccontext.Deactivate();
                }
                if (ccontext == null)
                {
                    root.Activate();
                    try
                    {
                        return root.Eval(args["0"].ToString());
                    }
                    finally
                    {
                        root.Deactivate();
                    }
                }
                else
                    return ccontext.Eval(args["0"].ToString());
            }
            finally
            {
                while (stack.Count != 0) stack.Pop().Activate();
            }
        }

        protected internal override JSObject GetMember(JSObject name, bool create, bool own)
        {
            if (name.ToString() == "prototype")
                return undefined;
            return base.GetMember(name, create, own);
        }
    }
}
