﻿using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    internal sealed class ReturnStatement : Statement
    {
        private Statement body;

        public Statement Body { get { return body; } }

        internal ReturnStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "return", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            if (state.AllowReturn == 0)
                throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid use of return statement.")));
            var body = Parser.Parse(state, ref i, 1, true);
            var pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ReturnStatement()
                {
                    body = body,
                    Position = pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.abortInfo = body != null ? Tools.RaiseIfNotExist(body.Invoke(context)) : JSObject.undefined;
            context.abort = AbortType.Return;
            return null;
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref body, 2, varibles);
            return false;
        }

        public override string ToString()
        {
            return "return" + (body != null ? " " + body : "");
        }
    }
}