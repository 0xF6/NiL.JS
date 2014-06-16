﻿using System;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    [Immutable]
    [Serializable]
    public sealed class RangeError : Error
    {
        public override JSObject message
        {
            get
            {
                return base.message;
            }
        }
        public override JSObject name { get { return "RangeError"; } }

        public RangeError()
        {

        }

        public RangeError(JSObject args)
            : base(args.GetMember("0").ToString())
        {

        }

        public RangeError(string message)
            : base(message)
        {

        }
    }
}
