using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 公共方法
    /// </summary>
    internal class Common
    {
        /// <summary>
        /// 将固定格式的string类型转为List<int>
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static List<int> StrToList(string str)
        {
            if (str == "")
                return new List<int>();
            string[] parts = str.Split(',');
            List<int> list = parts.Select(part => int.Parse(part.Trim())).ToList();
            return list;
        }

        /// <summary>
        /// 设置一个结构体某个字段的值
        /// </summary>
        /// <param name="obj">类/结构体</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="value">对应值</param>
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

        /// <summary>
        /// 取得某个结构体/类 某个字段的值
        /// </summary>
        /// <param name="obj">类/结构体</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
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
}
