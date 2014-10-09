﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class TryCatchStatement : CodeNode
    {
        private CodeNode body;
        private CodeNode catchBody;
        private CodeNode finallyBody;
        private VariableDescriptor catchVariableDesc;

        public CodeNode Body { get { return body; } }
        public CodeNode CatchBody { get { return catchBody; } }
        public CodeNode FinalBody { get { return finallyBody; } }
        public string ExceptionVariableName { get { return catchVariableDesc.name; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "try", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i])) i++;
            if (i >= state.Code.Length)
                throw new JSException(new SyntaxError("Unexpected end of line."));
            if (state.Code[i] != '{')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid try statement definition at " + Tools.PositionToTextcord(state.Code, i))));
            var b = CodeBlock.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            CodeNode cb = null;
            string exptn = null;
            if (Parser.Validate(state.Code, "catch (", ref i) || Parser.Validate(state.Code, "catch(", ref i))
            {
                int s = i;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Catch block must contain variable name " + Tools.PositionToTextcord(state.Code, i))));
                exptn = Tools.Unescape(state.Code.Substring(s, i - s), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (exptn == "arguments" || exptn == "eval")
                        throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Varible name may not be \"arguments\" or \"eval\" in strict mode at " + Tools.PositionToTextcord(state.Code, s))));
                }
                while (char.IsWhiteSpace(state.Code[i])) i++;
                if (!Parser.Validate(state.Code, ")", ref i))
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at + " + Tools.PositionToTextcord(state.Code, i))));
                while (char.IsWhiteSpace(state.Code[i])) i++;
                if (state.Code[i] != '{')
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid catch block statement definition at " + Tools.PositionToTextcord(state.Code, i))));
                state.functionsDepth++;
                try
                {
                    cb = CodeBlock.Parse(state, ref i).Statement;
                }
                finally
                {
                    state.functionsDepth--;
                }
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i])) i++;
            }
            CodeNode f = null;
            if (Parser.Validate(state.Code, "finally", i) && Parser.isIdentificatorTerminator(state.Code[i + 7]))
            {
                i += 7;
                while (char.IsWhiteSpace(state.Code[i])) i++;
                if (state.Code[i] != '{')
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Invalid finally block statement definition at " + Tools.PositionToTextcord(state.Code, i))));
                f = CodeBlock.Parse(state, ref i).Statement;
            }
            if (cb == null && f == null)
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("try block must contain 'catch' or/and 'finally' block")));
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new TryCatchStatement()
                {
                    body = b,
                    catchBody = cb,
                    finallyBody = f,
                    catchVariableDesc = new VariableDescriptor(exptn, state.functionsDepth + 1),
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            Exception except = null;
            try
            {
                body.Evaluate(context);
            }
            catch (Exception e)
            {
                if (catchBody != null)
                {
#if DEV
                    if (context.debugging)
                        context.raiseDebugger(catchBody);
#endif
                    var catchContext = new Context(context, true, context.caller) { strict = context.strict, variables = context.variables };
                    var cvar = catchContext.DefineVariable(catchVariableDesc.name);
#if DEBUG
                    if (!(e is JSException))
                        System.Diagnostics.Debugger.Break();
#endif
                    cvar.Assign(e is JSException ? (e as JSException).Avatar : TypeProxy.Proxy(e));
                    try
                    {
                        catchContext.Activate();
                        catchBody.Evaluate(catchContext);
                    }
                    finally
                    {
                        catchContext.Deactivate();
                    }
                    context.abort = catchContext.abort;
                    context.abortInfo = catchContext.abortInfo;
                }
                else except = e;
            }
            finally
            {
                if (finallyBody != null)
                {
#if DEV
                    if (context.debugging)
                        context.raiseDebugger(finallyBody);
#endif
                    var abort = context.abort;
                    var ainfo = context.abortInfo;
                    context.abort = AbortType.None;
                    context.abortInfo = JSObject.undefined;
                    try
                    {
                        finallyBody.Evaluate(context);
                    }
                    finally
                    {
                        if (context.abort == AbortType.None)
                        {
                            context.abort = abort;
                            context.abortInfo = ainfo;
                        }
                        else
                            except = null;
                    }
                }
            }
            if (except != null)
                throw except;
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            Parser.Optimize(ref body, 1, variables, strict);
            if (catchBody != null)
            {
                catchVariableDesc.owner = this;
                VariableDescriptor oldVarDesc = null;
                variables.TryGetValue(catchVariableDesc.name, out oldVarDesc);
                variables[catchVariableDesc.name] = catchVariableDesc;
                Parser.Optimize(ref catchBody, 1 + 1, variables, strict);
                if (oldVarDesc != null)
                    variables[catchVariableDesc.name] = oldVarDesc;
                else
                    variables.Remove(catchVariableDesc.name);
                foreach (var v in variables)
                    v.Value.captured = true;
            }
            Parser.Optimize(ref finallyBody, 1, variables, strict);
            return false;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                catchBody,
                finallyBody
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        public override string ToString()
        {
            var sbody = body.ToString();
            var fbody = finallyBody == null ? "" : finallyBody.ToString();
            var cbody = catchBody == null ? "" : catchBody.ToString();
            return "try" + (body is CodeBlock ? sbody : " {" + Environment.NewLine + "  " + sbody + Environment.NewLine + "}") +
                (catchBody != null ?
                Environment.NewLine + "catch (" + catchVariableDesc + ")" +
                (catchBody is CodeBlock ? cbody : "{ " + cbody + " }") : "") +
                (finallyBody != null ?
                Environment.NewLine + "finally" +
                (finallyBody is CodeBlock ? fbody : " { " + fbody + " }") : "");
        }
    }
}