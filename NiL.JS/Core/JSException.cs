﻿using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core
{
    /// <summary>
    /// Представляет ошибки, возникшие во время выполнения скрипта.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public sealed class JSException : Exception
    {
        public JSValue Avatar { get; private set; }

        public JSException(Error avatar)
        {
            Avatar = TypeProxy.Marshal(avatar);
        }

        public JSException(JSValue avatar)
        {
            Avatar = avatar;
        }

        public JSException(JSValue avatar, Exception innerException)
            : base("", innerException)
        {
            Avatar = avatar;
        }

        public JSException(Error avatar, Exception innerException)
            : base("", innerException)
        {
            Avatar = TypeProxy.Marshal(avatar);
        }

        public override string Message
        {
            get
            {
                if (Avatar.oValue is Error)
                {
                    var n = Avatar.GetProperty("name");
                    if (n.valueType == JSValueType.Property)
                        n = (n.oValue as GsPropertyPair).get.Call(Avatar, null).ToString();

                    var m = Avatar.GetProperty("message");
                    if (m.valueType == JSValueType.Property)
                        return n + ": " + (m.oValue as GsPropertyPair).get.Call(Avatar, null);
                    else
                        return n + ": " + m;
                }
                else return Avatar.ToString();
            }
        }
    }
}
