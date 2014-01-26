using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class GetVaribleStatement : Statement, IOptimizable
    {
        private Context cacheContext;
        private JSObject cacheRes;
        private string varibleName;

        private static System.Collections.Generic.Dictionary<string, GetVaribleStatement> cache = new System.Collections.Generic.Dictionary<string, GetVaribleStatement>();

        public GetVaribleStatement(string name)
        {
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, ref i, false, true))
                throw new ArgumentException("Invalid varible name");
            this.varibleName = name;
            cache[name] = this;
        }

        public override JSObject Invoke(Context context)
        {
            if (context.updateThisBind)
                context.thisBind = null;
            if (context == cacheContext)
                if (cacheRes == null)
                    return (cacheRes = context.GetField(varibleName));
                else
                    return cacheRes;
            cacheContext = context;
            return cacheRes = context.GetField(varibleName);
        }

        public override string ToString()
        {
            return varibleName;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            _this = cache[varibleName];
            return false;
        }
    }
}