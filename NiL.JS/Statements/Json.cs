﻿using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal class Json : Statement
    {
        private string[] fields;
        private Statement[] values;

        public static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            if (code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");
            var flds = new List<string>();
            var vls = new List<Statement>();
            int i = index;
            while (code[i] != '}')
            {
                do i++; while (char.IsWhiteSpace(code[i]));
                int s = i;
                if (code[i] == '}')
                    break;
                if (Parser.ValidateName(code, ref i, true))
                    flds.Add(code.Substring(s, i - s));
                else if (Parser.ValidateValue(code, ref i, true))
                {
                    string value = code.Substring(s, i - s);
                    if ((value[0] == '\'') || (value[0] == '"'))
                        flds.Add(value.Substring(1, value.Length - 2));
                    else
                    {
                        int n = 0;
                        double d = 0.0;
                        if (int.TryParse(value, out n))
                            flds.Add(n.ToString());
                        else if (double.TryParse(value, out d))
                            flds.Add(d.ToString());
                        else
                            return new ParseResult();
                    }
                }
                else
                    return new ParseResult();
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != ':')
                    return new ParseResult();
                do i++; while (char.IsWhiteSpace(code[i]));
                vls.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                while (char.IsWhiteSpace(code[i])) i++;
                if ((code[i] != ',') && (code[i] != '}'))
                    return new ParseResult();
            }
            i++;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new Json()
                {
                    fields = flds.ToArray(),
                    values = vls.ToArray()
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            var res = new JSObject(false);
            res.ValueType = ObjectValueType.Object;
            res.oValue = new object();
            res.prototype = NiL.JS.Core.BaseTypes.BaseObject.Prototype;
            for (int i = 0; i < fields.Length; i++)
                res.GetField(fields[i]).Assign(values[i].Invoke(context));
            return res;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}