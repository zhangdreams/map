using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class Time
    {
        // 返回当前时间戳 (秒）
        public static long Now()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long Seconds = now.ToUnixTimeSeconds();
            return Seconds;
        }

        // 返回当前时间戳 (毫秒）
        public static long Now2()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            long millSeconds = now.ToUnixTimeMilliseconds();
            return millSeconds;
        }
    }

    internal class Common
    {
        public static List<int> StrToList(string str)
        {
            if (str == "")
                return new List<int>();
            string[] parts = str.Split(',');
            List<int> list = parts.Select(part => int.Parse(part.Trim())).ToList();
            return list;
        }

        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            var fieldInfo = type.GetProperty(fieldName);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
            else
            {
                Log.E($"Set Field '{fieldName}' not found in type '{type.Name}'.");
            }
        }

        public static object? GetFieldValue(object obj, string fieldName)
        {
            Type type = obj.GetType();
            var fieldInfo = type.GetProperty(fieldName);
            if (fieldInfo != null)
                return fieldInfo.GetValue(obj);
            else
            {
                Log.E($"Get Field '{fieldName}' not found in type '{type.Name}'.");
                return null;
            }
        }
    }

    internal class Log
    {
        public static void P()
        {
            Show("");
        }
        public static void P(string msg) 
        { 
            Console.ResetColor();
            Show(msg);
        }

        public static void R(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Show(msg);
        }

        public static void W(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Show(msg);
        }

        public static void E(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Show(msg);
        }

        public static void Show(string msg)
        {
            if (msg == "")
                Console.WriteLine(msg);
            else
                Console.WriteLine($"{DateTimeOffset.Now} " + msg);
        }
    }
}
