﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Call : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        private CodeNode[] arguments;
        public CodeNode[] Arguments { get { return arguments; } }
        public bool allowTCO;
        public bool AllowTCO { get { return allowTCO; } }

        internal Call(CodeNode first, CodeNode[] arguments)
            : base(first, null, false)
        {
            this.arguments = arguments;
        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject newThisBind = null;
            Function func = null;
            var temp = first.Evaluate(context);
            newThisBind = context.objectSource;

            bool tail = false;
            func = temp.valueType == JSObjectType.Function ? temp.oValue as Function ?? (temp.oValue as TypeProxy).prototypeInstance as Function : null; // будем надеяться, что только в одном случае в oValue не будет лежать функция
            if (allowTCO
                && func == context.caller.oValue
                && context.caller.oValue != Script.pseudoCaller)
            {
                context.abort = AbortType.TailRecursion;
                tail = true;
            }
            Arguments arguments = new Arguments();
            arguments.length = this.arguments.Length;
            for (int i = 0; i < arguments.length; i++)
            {
                context.objectSource = null;
                var a = this.arguments[i].Evaluate(context);
                if ((a.attributes & JSObjectAttributesInternal.Temporary) != 0)
                {
                    a = a.CloneImpl();
                    a.attributes |= JSObjectAttributesInternal.Cloned;
                }
#if DEBUG
                if (a == null)
                    System.Diagnostics.Debugger.Break();
#endif
                arguments[i] = a;
            }
            context.objectSource = null;
            if (tail)
            {
                for (var i = func.creator.body.localVariables.Length; i-- > 0; )
                {
                    if (func.creator.body.localVariables[i].Inititalizator == null)
                        func.creator.body.localVariables[i].cacheRes.Assign(JSObject.undefined);
                }
                func._arguments = arguments;
                return JSObject.undefined;
            }
            // Аргументы должны быть вычислены даже если функция не существует.
            if (func == null)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(first + " is not callable")));
            func.attributes = (func.attributes & ~JSObjectAttributesInternal.Eval) | (temp.attributes & JSObjectAttributesInternal.Eval);

            return func.Invoke(newThisBind, arguments);
        }

        private static bool isSimple(CodeNode expression)
        {
            if (expression == null
                || expression is ImmidateValueStatement
                || expression is GetVariableStatement)
                return true;
            if (expression is Call)
            {
                if ((expression as Call).first is VariableReference
                    && (expression as Call).first.ToString() == "eval")
                    return false;
                var args = (expression as Call).arguments;
                for (var i = 0; i < args.Length; i++)
                {
                    if (!isSimple(args[i]))
                        return false;
                }
                return true;
            }
            if (expression is Expression)
                return isSimple((expression as Expression).FirstOperand) && isSimple((expression as Expression).SecondOperand);
            return false;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            for (var i = 0; i < arguments.Length; i++)
                Parser.Optimize(ref arguments[i], depth + 1, vars, strict);
            base.Build(ref _this, depth, vars, strict);
            if (first is GetVariableStatement)
            {
                var name = first.ToString();
                VariableDescriptor f = null;
                if (vars.TryGetValue(name, out f))
                {
                    if (f.Inititalizator != null) // Defined function
                    {
                        var func = f.Inititalizator as FunctionStatement;
                        if (func != null)
                        {
                            if (func.body == null || func.body.body == null || func.body.body.Length == 0)
                            {
                                if (arguments.Length == 0)
                                    _this = new EmptyStatement();
                                else
                                {
                                    System.Array.Reverse(arguments, 0, arguments.Length);
                                    _this = new CodeBlock(arguments, strict);
                                }
                            }
                            /* // TODO
                            else if (func.body.body.Length == 1 && func.body.body[0] is ReturnStatement)
                            {
                                var ret = func.body.body[0] as ReturnStatement;
                                if (isSimple(ret.Body))
                                {
                                    var prms = func.Parameters;
                                    for (var i = 0; i < prms.Length; i++)
                                    {
                                    }
                                }
                            }
                            */
                        }
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            string res = first + "(";
            for (int i = 0; i < arguments.Length; i++)
            {
                res += arguments[i];
                if (i + 1 < arguments.Length)
                    res += ", ";
            }
            return res + ")";
        }
    }
}