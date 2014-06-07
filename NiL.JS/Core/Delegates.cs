﻿using NiL.JS.Core.BaseTypes;
using System;

namespace NiL.JS.Core
{
    internal delegate ParseResult ParseDelegate(ParsingState state, ref int position);
    internal delegate bool ValidateDelegate(string code, ref int position);
    public delegate JSObject ExternalFunctionDelegate(Context context, JSObject args);
    public sealed class DebuggerCallbackEventArgs : EventArgs
    {
        public Statement Statement { get; internal set; }
    }
    public delegate void DebuggerCallback(Context sender, DebuggerCallbackEventArgs e);
}
