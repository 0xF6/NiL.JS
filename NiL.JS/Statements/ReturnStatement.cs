﻿using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class ReturnStatement : Statement, IOptimizable
    {
        private Statement body;

        public ReturnStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "return", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            var body = Parser.Parse(state, ref i, 1, true);
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ReturnStatement()
                {
                    body = body,
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            context.abortInfo = Tools.RaiseIfNotExist(body.Invoke(context));
            context.abort = AbortType.Return;
            return null;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref body, 2, varibles);
            return false;
        }

        public override string ToString()
        {
            return "return " + body;
        }
    }
}