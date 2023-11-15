using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
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
                Console.WriteLine($"Set Field '{fieldName}' not found in type '{type.Name}'.");
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
                Console.WriteLine($"Get Field '{fieldName}' not found in type '{type.Name}'.");
                return null;
            }

        }
    }

}
