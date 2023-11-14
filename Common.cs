using System;
using System.Collections.Generic;
using System.Linq;
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
    }

}
