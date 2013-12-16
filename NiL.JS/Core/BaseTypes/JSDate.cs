﻿using System;

namespace NiL.JS.Core.BaseTypes
{
    internal class JSDate : JSObject
    {
        private readonly static string[] month = new[] { "Jun", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public readonly static string Marker = new System.String(new char[] { 'D', 'a', 't', 'e' });
        public static JSObject Prototype;

        public static void RegisterTo(Context context)
        {
            JSObject proto = null;
            var func = context.Assign("Date", new CallableField((_this, args) =>
            {
                JSObject res;
                if (_this.ValueType == ObjectValueType.Object && _this.oValue == Marker as object)
                    res = _this;
                else
                    res = new JSObject();
                res.prototype = proto;
                res.ValueType = ObjectValueType.Date;
                res.oValue = new DateTime(DateTime.UtcNow.Ticks);
                if (args != null && args.Length > 0)
                    throw new NotImplementedException();
                return res;
            }));
            proto = func.GetField("prototype");
            proto.Assign(null);
            Prototype = proto;
            proto.ValueType = ObjectValueType.Object;
            proto.oValue = Marker;
            proto.GetField("valueOf").Assign(new CallableField((_th, args) =>
            {
                DateTime dt = (DateTime)_th.oValue;
                var res = dt.Ticks - new DateTime(1970, 1, 1).Ticks;
                return res / 10000;
                throw new NotImplementedException();
            }));
            proto.GetField("toString").Assign(new CallableField((_th, args) =>
            {
                DateTime dt = (DateTime)_th.oValue;
                var lt = dt.ToLongTimeString();
                var offset = TimeZone.CurrentTimeZone.GetUtcOffset(dt);
                var res = dt.DayOfWeek.ToString().Substring(0, 3) + " "
                    + month[dt.Month - 1] + " " + dt.Day + " " + dt.Year + " " + lt
                    + " GMT+" + offset.Hours.ToString("00") + offset.Minutes.ToString("00") + " (" + TimeZone.CurrentTimeZone.DaylightName + ")";
                return res;
            }));
            proto.prototype = BaseObject.Prototype;
        }
    }
}