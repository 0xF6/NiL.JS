﻿using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class CodeBlock : Statement
    {
        private FunctionStatement[] functions;
        private string[] varibles;
        private int linesCount;
        private string code;
        internal readonly Statement[] body;
        internal readonly bool strict;

        public FunctionStatement[] Functions { get { return functions; } }
        public string[] Varibles { get { return varibles; } }
        public Statement[] Body { get { return body; } }
        public bool Strict { get { return strict; } }
        public string Code
        {
            get
            {
                if (base.Length >= 0)
                {
                    lock (this)
                    {
                        if (base.Length >= 0)
                        {
                            if (Position != 0)
                                code = code.Substring(Position + 1, Length - 2);
                            Length = -base.Length;
                            return code;
                        }
                    }
                }
                return code;
            }
        }

        public override int Length
        {
            get
            {
                return base.Length < 0 ? -base.Length : base.Length;
            }
            internal set
            {
                base.Length = value;
            }
        }

        public CodeBlock(Statement[] body, bool strict)
        {
            code = "";
            this.body = body;
            linesCount = body.Length - 1;
            functions = null;
            varibles = null;
            this.strict = strict;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            bool sroot = i == 0;
            if (!sroot)
            {
                if (code[i] != '{')
                    throw new ArgumentException("code (" + i + ")");
                do
                    i++;
                while (char.IsWhiteSpace(code[i]));
            }
            var body = new List<Statement>();
            List<FunctionStatement> funcs = null;
            List<string> varibles = null;
            state.LabelCount = 0;
            bool strictSwitch = false;
            bool allowStrict = state.AllowStrict;
            bool root = state.AllowStrict;
            state.AllowStrict = false;
            while ((sroot && i < code.Length) || (!sroot && code[i] != '}'))
            {
                var t = Parser.Parse(state, ref i, 0);
                if (allowStrict)
                {
                    allowStrict = false;
                    if (t is ImmidateValueStatement)
                    {
                        var op = (t as ImmidateValueStatement).value.Value;
                        if ("use strict".Equals(op))
                        {
                            state.strict.Push(true);
                            strictSwitch = true;
                            continue;
                        }
                    }
                }
                if (t == null || t is EmptyStatement)
                    continue;
                if (t is FunctionStatement)
                {
                    if (state.strict.Peek())
                        if (!root)
                            throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    if (funcs == null)
                        funcs = new List<FunctionStatement>();
                    funcs.Add(t as FunctionStatement);
                }
                else if (t is VaribleDefineStatement)
                {
                    if (varibles == null)
                        varibles = new List<string>();
                    varibles.AddRange((t as VaribleDefineStatement).names);
                    body.Add(t);
                }
                else if (t is CodeBlock)
                {
                    var cb = t as CodeBlock;
                    if (cb.varibles != null && cb.varibles.Length > 0)
                    {
                        if (varibles == null)
                            varibles = new List<string>();
                        varibles.AddRange(cb.varibles);
                    }
                    if (cb.functions != null && cb.functions.Length > 0)
                    {
                        if (funcs == null)
                            funcs = new List<FunctionStatement>();
                        funcs.AddRange(cb.functions);
                    }
                    int s = body.Count;
                    body.AddRange(cb.body);
                    body.Reverse(s, cb.body.Length);
                }
                else body.Add(t);
            }
            if (!sroot)
                i++;
            int startPos = index;
            index = i;
            body.Reverse();
            if (funcs != null)
                funcs.Reverse();
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new CodeBlock(body.ToArray(), strictSwitch && state.strict.Pop())
                {
                    functions = funcs != null ? funcs.ToArray() : null,
                    varibles = varibles != null ? varibles.ToArray() : null,
                    Position = startPos,
                    code = state.SourceCode,
                    Length = i - startPos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.strict |= strict;
            for (int i = varibles == null ? 0 : varibles.Length; i-- > 0; )
                context.InitField(varibles[i]);
            for (int i = functions == null ? 0 : functions.Length; i-- > 0; )
            {
                if (string.IsNullOrEmpty((functions[i] as FunctionStatement).name))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Declarated function must have name.")));
                var f = context.InitField((functions[i] as FunctionStatement).name);
                f.Assign(functions[i].Invoke(context));
                f.assignCallback = null;
            }
            JSObject res = JSObject.undefined;
            for (int i = linesCount; i >= 0; i--)
            {
                res = body[i].Invoke(context) ?? res;
#if DEBUG
                if (JSObject.undefined.ValueType != JSObjectType.Undefined)
                    throw new ApplicationException("undefined was rewrite");
                if (Core.BaseTypes.String.EmptyString.oValue as string != "")
                    throw new ApplicationException("EmptyString was rewrite");
#endif
                if (context.abort != AbortType.None)
                    return context.abort == AbortType.Return ? context.abortInfo : res;
            }
            return res;
        }

        protected override Statement[] getChildsImpl()
        {
            var res = new List<Statement>();
            res.AddRange(body);
            res.AddRange(functions);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> varibles)
        {
            if (functions != null)
            {
                for (var i = functions.Length; i-- > 0; )
                {
                    Statement f = functions[i];
                    Parser.Optimize(ref f, 0, null);
                    functions[i] = f as FunctionStatement;
                }
            }
            if (this.varibles != null)
            {
                for (var i = this.varibles.Length; i-- > 0; )
                    if (!varibles.ContainsKey(this.varibles[i]))
                        varibles.Add(this.varibles[i], null);
            }
            for (int i = body.Length; i-- > 0; )
                Parser.Optimize(ref body[i], depth < 0 ? 2 : Math.Max(1, depth), varibles);

            if (depth > 0)
            {
                functions = null;
                varibles = null;
                if (body.Length == 1)
                    _this = body[0];
                if (body.Length == 0)
                    _this = EmptyStatement.Instance;
            }
            else
            {
                List<FunctionStatement> funcs = null;
                List<string> vars = null;
                foreach (var v in varibles)
                {
                    if (v.Value is FunctionStatement)
                    {
                        if (funcs == null)
                            funcs = new List<FunctionStatement>();
                        funcs.Add(v.Value as FunctionStatement);
                    }
                    else
                    {
                        if (v.Value is GetVaribleStatement && (v.Value as GetVaribleStatement).Descriptor.Defined)
                        {
                            if (vars == null)
                                vars = new List<string>();
                            vars.Add(v.Key);
                        }
                    }
                }
                if (funcs != null)
                    this.functions = funcs.ToArray();
                if (vars != null)
                    this.varibles = vars.ToArray();
            }
            return false;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool parsed)
        {
            if (parsed)
            {
                if (body == null || body.Length == 0)
                    return "{ }";
                string res = " {" + Environment.NewLine;
                var replp = Environment.NewLine;
                var replt = Environment.NewLine + "  ";
                for (var i = 0; i < functions.Length; i++)
                {
                    var func = functions[i].ToString().Replace(replp, replt);
                    res += "  " + func + Environment.NewLine;
                }
                for (int i = body.Length; i-- > 0; )
                {
                    string lc = body[i].ToString();
                    if (lc[0] == '(')
                        lc = lc.Substring(1, lc.Length - 2);
                    lc = lc.Replace(replp, replt);
                    res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
                }
                return res + "}";
            }
            else
                return '{' + Code + '}';
        }
    }
}