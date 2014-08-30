﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace NiL.JS.Core.JIT
{
    internal sealed class TreeBuildingState
    {
        public readonly Stack<LabelTarget> BreakLabels;
        public readonly Stack<LabelTarget> ContinueLabels;
        public readonly Dictionary<string, LabelTarget> NamedBreakLabels;
        public readonly Dictionary<string, LabelTarget> NamedContinueLabels;
        public readonly bool AllowReturn;

        public TreeBuildingState(bool allowReturn)
        {
            BreakLabels = new Stack<LabelTarget>();
            ContinueLabels = new Stack<LabelTarget>();
            AllowReturn = allowReturn;
            NamedBreakLabels = new Dictionary<string, LabelTarget>();
            NamedContinueLabels = new Dictionary<string, LabelTarget>();
        }
    }
}
