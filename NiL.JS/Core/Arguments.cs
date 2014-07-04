﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    [Serializable]
    public sealed class Arguments : JSObject
    {
        private JSObject a0;
        private JSObject a1;
        private JSObject a2;
        private JSObject a3;
        private JSObject a4;
        private JSObject a5;
        private JSObject a6;
        private JSObject a7;
        private JSObject a8;
        private JSObject a9;
        private JSObject a10;
        private JSObject a11;
        private JSObject a12;
        private JSObject a13;
        private JSObject a14;
        private JSObject a15;
        internal JSObject callee;
        internal JSObject caller;
        private JSObject _length;
        internal int length;

        public override JSObject this[string name]
        {
            get
            {
                return base[name];
            }
            set
            {
                switch (name)
                {
                    case "callee":
                        callee = value;
                        return;
                    case "caller":
                        caller = value;
                        return;
                }
                base[name] = value;
            }
        }

        public JSObject this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return a0;
                    case 1:
                        return a1;
                    case 2:
                        return a2;
                    case 3:
                        return a3;
                    case 4:
                        return a4;
                    case 5:
                        return a5;
                    case 6:
                        return a6;
                    case 7:
                        return a7;
                    case 8:
                        return a8;
                    case 9:
                        return a9;
                    case 10:
                        return a10;
                    case 11:
                        return a11;
                    case 12:
                        return a12;
                    case 13:
                        return a13;
                    case 14:
                        return a14;
                    case 15:
                        return a15;
                }
                return base[index.ToString()];
            }
            set
            {
                switch (index)
                {
                    case 0:
                        a0 = value;
                        break;
                    case 1:
                        a1 = value;
                        break;
                    case 2:
                        a2 = value;
                        break;
                    case 3:
                        a3 = value;
                        break;
                    case 4:
                        a4 = value;
                        break;
                    case 5:
                        a5 = value;
                        break;
                    case 6:
                        a6 = value;
                        break;
                    case 7:
                        a7 = value;
                        break;
                    case 8:
                        a8 = value;
                        break;
                    case 9:
                        a9 = value;
                        break;
                    case 10:
                        a10 = value;
                        break;
                    case 11:
                        a11 = value;
                        break;
                    case 12:
                        a12 = value;
                        break;
                    case 13:
                        a13 = value;
                        break;
                    case 14:
                        a14 = value;
                        break;
                    case 15:
                        a15 = value;
                        break;
                    default:
                        base[index.ToString()] = value;
                        break;
                }

            }
        }

        public Arguments()
            : base(false)
        {
            valueType = JSObjectType.Object;
            oValue = this;
            attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum;
        }

        protected internal override JSObject GetMember(JSObject name, bool createMember, bool own)
        {
            switch (name.ToString())
            {
                case "0":
                    return (a0 ?? (!createMember ? notExists : (a0 = new JSObject())));
                case "1":
                    return (a1 ?? (!createMember ? notExists : (a1 = new JSObject())));
                case "2":
                    return (a2 ?? (!createMember ? notExists : (a2 = new JSObject())));
                case "3":
                    return (a3 ?? (!createMember ? notExists : (a3 = new JSObject())));
                case "4":
                    return (a4 ?? (!createMember ? notExists : (a4 = new JSObject())));
                case "5":
                    return (a5 ?? (!createMember ? notExists : (a5 = new JSObject())));
                case "6":
                    return (a6 ?? (!createMember ? notExists : (a6 = new JSObject())));
                case "7":
                    return (a7 ?? (!createMember ? notExists : (a7 = new JSObject())));
                case "8":
                    return (a8 ?? (!createMember ? notExists : (a8 = new JSObject())));
                case "9":
                    return (a9 ?? (!createMember ? notExists : (a9 = new JSObject())));
                case "10":
                    return (a10 ?? (!createMember ? notExists : (a10 = new JSObject())));
                case "11":
                    return (a11 ?? (!createMember ? notExists : (a11 = new JSObject())));
                case "12":
                    return (a12 ?? (!createMember ? notExists : (a12 = new JSObject())));
                case "13":
                    return (a13 ?? (!createMember ? notExists : (a13 = new JSObject())));
                case "14":
                    return (a14 ?? (!createMember ? notExists : (a14 = new JSObject())));
                case "15":
                    return (a15 ?? (!createMember ? notExists : (a15 = new JSObject())));
                case "length":
                    {
                        if (_length == null)
                            _length = new JSObject() { valueType = JSObjectType.Int, iValue = length, attributes = JSObjectAttributesInternal.DoNotEnum };
                        return _length;
                    }
                case "callee":
                    return (callee ?? (!createMember ? notExists : (callee = new JSObject())));
                case "caller":
                    return (caller ?? (!createMember ? notExists : (caller = new JSObject())));
            }
            return base.GetMember(name, createMember, own);
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            if (a0 != null && a0.isExist && (!hideNonEnum || (a0.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "0";
            if (a1 != null && a1.isExist && (!hideNonEnum || (a1.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "1";
            if (a2 != null && a2.isExist && (!hideNonEnum || (a2.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "2";
            if (a3 != null && a3.isExist && (!hideNonEnum || (a3.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "3";
            if (a4 != null && a4.isExist && (!hideNonEnum || (a4.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "4";
            if (a5 != null && a5.isExist && (!hideNonEnum || (a5.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "5";
            if (a6 != null && a6.isExist && (!hideNonEnum || (a6.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "6";
            if (a7 != null && a7.isExist && (!hideNonEnum || (a7.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "7";
            if (a8 != null && a8.isExist && (!hideNonEnum || (a8.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "8";
            if (a9 != null && a9.isExist && (!hideNonEnum || (a9.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "9";
            if (a10 != null && a10.isExist && (!hideNonEnum || (a10.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "10";
            if (a11 != null && a11.isExist && (!hideNonEnum || (a11.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "11";
            if (a12 != null && a12.isExist && (!hideNonEnum || (a12.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "12";
            if (a13 != null && a13.isExist && (!hideNonEnum || (a13.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "13";
            if (a14 != null && a14.isExist && (!hideNonEnum || (a14.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "14";
            if (a15 != null && a15.isExist && (!hideNonEnum || (a15.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "15";
            if (callee != null && callee.isExist && (!hideNonEnum || (callee.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "callee";
            if (caller != null && callee.isExist && (!hideNonEnum || (caller.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "caller";
            if (_length != null && _length.isExist && (!hideNonEnum || (_length.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                yield return "length";
            var be = base.GetEnumeratorImpl(hideNonEnum);
            while (be.MoveNext())
                yield return be.Current;
        }
    }
}
