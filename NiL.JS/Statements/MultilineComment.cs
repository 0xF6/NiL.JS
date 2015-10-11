﻿using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Statements
{
    /// <summary>
    /// TODO
    /// </summary>
    internal sealed class MultilineComment : CodeNode
    {
        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "/*", ref i))
                return null;
            while (i + 1 < state.Code.Length && (state.Code[i] != '*' || state.Code[i + 1] != '/'))
                i++;
            if (i + 1 >= state.Code.Length)
                ExceptionsHelper.Throw((new SyntaxError("Non terminated multiline comment")));
            i += 2;
            try
            {
                return new MultilineComment(state.Code.Substring(index + 2, i - index - 4))
                    {
                        Length = i - index,
                        Position = index
                    };
            }
            finally
            {
                index = i;
            }
        }

        public string Text { get; private set; }

        public MultilineComment(string text)
        {
            Text = text;
        }

        public override JSValue Evaluate(Context context)
        {
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "/*" + Text + "*/";
        }
    }
}
