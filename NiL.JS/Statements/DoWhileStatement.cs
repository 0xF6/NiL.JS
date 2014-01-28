﻿using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal sealed class DoWhileStatement : Statement, IOptimizable
    {
        private Statement condition;
        private Statement body;
        private List<string> labels;

        private DoWhileStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "do", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            while (char.IsWhiteSpace(code[i])) i++;
            state.AllowBreak++;
            state.AllowContinue++;
            var body = Parser.Parse(state, ref i, 4);
            state.AllowBreak--;
            state.AllowContinue--;
            if (!(body is CodeBlock) && code[i] == ';')
                i++;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "while", ref i))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != '(')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            var condition = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            i++;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new DoWhileStatement()
                {
                    body = body,
                    condition = condition,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount)
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            JSObject res = null;
            do
            {
                res = body.Invoke(context);
                if (context.abort != AbortType.None)
                {
                    bool _break = (context.abort > AbortType.Continue) || ((context.abortInfo != null) && (labels.IndexOf(context.abortInfo.oValue as string) == -1));
                    if (context.abort < AbortType.Return && ((context.abortInfo == null) || (labels.IndexOf(context.abortInfo.oValue as string) != -1)))
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = null;
                    }
                    if (_break)
                        return res;
                }
            }
            while ((bool)condition.Invoke(context));
            return res;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            depth = Math.Max(1, depth);
            Parser.Optimize(ref body, depth, varibles);
            Parser.Optimize(ref condition, depth, varibles);
            return false;
        }
    }
}