﻿using System.Collections.Generic;

namespace NiL.JS.Core
{
    internal sealed class ParsingState
    {
        public int AllowReturn;
        public readonly Stack<bool> AllowBreak;
        public readonly Stack<bool> AllowContinue;
        public readonly Stack<bool> AllowYield;
        public int functionsDepth;
        public int InExpression;
        public List<string> Labels;
        public readonly Stack<bool> strict;
        public int LabelCount;
        public string Code;
        public readonly string SourceCode;
        public bool AllowDirectives;
        public readonly Stack<bool> containsWith;
        public int breaksCount;
        public int continiesCount;

        public readonly CompilerMessageCallback message;

        public ParsingState(string code, string sourceCode, CompilerMessageCallback message)
        {
            Code = code;
            SourceCode = sourceCode;
            Labels = new List<string>();
            strict = new Stack<bool>();
            strict.Push(false);
            AllowDirectives = true;
            containsWith = new Stack<bool>();
            containsWith.Push(false);
            AllowBreak = new Stack<bool>();
            AllowBreak.Push(false);
            AllowContinue = new Stack<bool>();
            AllowContinue.Push(false);
            AllowYield = new Stack<bool>();
            AllowYield.Push(false);
            this.message = message;
        }
    }
}