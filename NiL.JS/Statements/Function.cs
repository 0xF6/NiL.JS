﻿using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class Function : Statement, IOptimizable
    {
        private string[] arguments;
        private Statement body;
        public readonly string Name;

        private Function(string name)
        {
            this.Name = name;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "function", ref i))
                throw new ArgumentException("code (" + i + ")");
            if ((code[i] != '(') && (!char.IsWhiteSpace(code[i])))
                return new ParseResult() { IsParsed = false, Message = "Invalid char in function definition" };
            while (char.IsWhiteSpace(code[i])) i++;
            var arguments = new List<string>();
            string name = null;
            if (code[i] != '(')
            {
                int n = i;
                if (!Parser.ValidateName(code, ref i, true))
                    throw new ArgumentException("code (" + i + ")");
                name = code.Substring(n, i - n);
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '(')
                    throw new ArgumentException("Invalid char at " + i + ": '" + code[i] + "'");
            }
            do i++; while (char.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                throw new ArgumentException("code (" + i + ")");
            while (code[i] != ')')
            {
                if (code[i] == ',')
                    do i++; while (char.IsWhiteSpace(code[i]));
                int n = i;
                if (!Parser.ValidateName(code, ref i, true))
                    throw new ArgumentException("code (" + i + ")");
                arguments.Add(code.Substring(n, i - n));
                while (char.IsWhiteSpace(code[i])) i++;
            }
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            if (code[i] != '{')
                throw new ArgumentException("code (" + i + ")");
            Statement body = CodeBlock.Parse(state, ref i).Statement;
            index = i;
            Function res = new Function(name)
                {
                    arguments = arguments.ToArray(),
                    body = body
                };
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = res
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        private static IContextStatement[] defaultArgs = new ContextStatement[0];

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            Context internalContext = new Context(context);
            int i = 0;
            if (args == null)
                args = defaultArgs;
            int min = Math.Min(args.Length, arguments.Length);
            for (; i < min; i++)
                internalContext.Define(arguments[i]).Assign(args[i].Invoke());
            for (; i < arguments.Length; i++)
                internalContext.Define(arguments[i]).Assign(null);
            internalContext.thisBind = _this;
            body.Invoke(internalContext);
            return internalContext.abortInfo;
        }

        public override JSObject Invoke(Context context)
        {
            var res = new JSObject() { ValueType = ObjectValueType.Statement, oValue = this.Implement(context) };
            return res;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            (body as IOptimizable).Optimize(ref body, 0, varibles);
            return false;
        }
    }
}