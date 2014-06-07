﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.BaseTypes
{
    [Serializable]
    public sealed class ArrayBuffer : EmbeddedType
    {
        [Serializable]
        private sealed class Element : JSObject
        {
            private int index;
            private byte[] data;

            public Element(int index, byte[] data)
            {
                this.ValueType = JSObjectType.Int;
                this.index = index;
                this.iValue = data[index];
                this.data = data;
            }

            public override void Assign(JSObject value)
            {
                data[index] = (byte)Tools.JSObjectToInt(value);
            }
        }

        [Modules.Hidden]
        public byte[] Data
        {
            [Modules.Hidden]
            get;
            [Modules.Hidden]
            private set;
        }

        [Modules.DoNotEnumerateAttribute]
        public ArrayBuffer()
        {
            Data = new byte[0];
        }

        [Modules.DoNotEnumerateAttribute]
        public ArrayBuffer(int length)
        {
            Data = new byte[length];
        }

        public int byteLength
        {
            [Modules.Hidden]
            get
            {
                return Data.Length;
            }
        }

        [Modules.Hidden]
        public ArrayBuffer slice(int begin, int end)
        {
            if (end < begin || begin >= Data.Length || end >= Data.Length)
                throw new JSException(TypeProxy.Proxy(new RangeError("Invalid begin or end index")));
            var res = new ArrayBuffer(end - begin + 1);
            for (int i = 0, j = begin; j <= end; j++, i++)
                res.Data[i] = Data[j];
            return res;
        }

        [Modules.Hidden]
        public ArrayBuffer slice(int begin)
        {
            return slice(begin, Data.Length - 1);
        }

        public ArrayBuffer slice(JSObject args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            var l = Tools.JSObjectToInt(args.GetField("length", true, false));
            if (l == 0)
                return this;
            if (l == 1)
                return slice(Tools.JSObjectToInt(args.GetField("0", true, false)), Data.Length - 1);
            else
                return slice(Tools.JSObjectToInt(args.GetField("0", true, false)), Tools.JSObjectToInt(args.GetField("1", true, false)));
        }

        [Modules.Hidden]
        public byte this[int index]
        {
            [Modules.Hidden]
            get
            {
                return Data[index];
            }
            [Modules.Hidden]
            set
            {
                Data[index] = value;
            }
        }

        [Modules.Hidden]
        public override JSObject GetField(string name, bool fast, bool own)
        {
            int index = 0;
            double dindex = 0.0;
            if (name != "NaN" && name != "Infinity" && name != "-Infinity" &&
                Tools.ParseNumber(name, ref index, false, out dindex))
            {
                if (dindex > 0x7fffffff || dindex < 0)
                    throw new JSException(TypeProxy.Proxy(new RangeError("Invalid array index")));
                if (((index = (int)dindex) == dindex))
                {
                    if (index >= Data.Length)
                        return undefined;
                    return new Element(index, Data);
                }
            }
            return base.GetField(name, fast, own);
        }

        [Modules.Hidden]
        public override IEnumerator<string> GetEnumerator()
        {
            var be = base.GetEnumerator();
            while (be.MoveNext())
                yield return be.Current;
            for (var i = 0; i < Data.Length; i++)
                yield return i < 16 ? Tools.NumString[i] : i.ToString();
        }
    }
}
