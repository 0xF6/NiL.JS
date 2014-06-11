﻿using NiL.JS.Core;
using NiL.JS.Core.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestsDownloader
{
    class Program
    {
        private static readonly string[] TestsSource = new[]
        {
            @"http://test262.ecmascript.org/json/ch06.json",
            @"http://test262.ecmascript.org/json/ch07.json",
            @"http://test262.ecmascript.org/json/ch08.json",
            @"http://test262.ecmascript.org/json/ch09.json",
            @"http://test262.ecmascript.org/json/ch10.json",
            @"http://test262.ecmascript.org/json/ch11.json",
            @"http://test262.ecmascript.org/json/ch12.json",
            @"http://test262.ecmascript.org/json/ch13.json",
            @"http://test262.ecmascript.org/json/ch14.json",
            @"http://test262.ecmascript.org/json/ch15.json",
            @"http://test262.ecmascript.org/json/annexB.json",
        };

        private static void saveTest(string rootDir, JSObject testObj)
        {
            var code = Convert.FromBase64String(testObj["code"].ToString());
            var commentary = testObj["commentary"].ToString();
            var description = testObj["description"].ToString();
            var path = rootDir + testObj["path"];
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var file = new FileStream( path, FileMode.Create, FileAccess.Write))
                file.Write(code, 0, code.Length);
        }

        private static void download(string url, string rootDir)
        {
            WebRequest wr = HttpWebRequest.Create(url);
            using (var response = wr.GetResponse())
            {
                var data = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var tests = JSON.parse(data).GetMember("testsCollection").GetMember("tests");
                foreach (var i in tests)
                    saveTest(rootDir, tests.GetMember(i));
            }
        }

        static void Main(string[] args)
        {
            for (var i = 0; i < TestsSource.Length; i++)
            {
                Console.WriteLine("downloading: " + TestsSource[i]);
                download(TestsSource[i], "tests/");
            }
        }
    }
}
