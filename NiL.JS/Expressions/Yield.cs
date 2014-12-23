﻿using System;
using System.Threading;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Yield : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public Yield(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                tempContainer.Assign(first.Evaluate(context));
                context.abortInfo = tempContainer;
                context.abort = AbortType.Yield;
                context.Deactivate();
                while (context.abort == AbortType.Yield)
#if !NET35
                    Thread.Yield();
#else
                    Thread.Sleep(0);
#endif
                if (context.abort == AbortType.Exception)
                    throw new JSException(new Error("Execution aborted"));
                context.abort = AbortType.None;
                context.Activate();
                tempContainer.Assign(context.abortInfo ?? JSObject.notExists);
                return tempContainer;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "yield " + first;
        }
    }
}