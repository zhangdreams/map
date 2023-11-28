using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    internal class Log
    {
        public static void P()
        {
            Console.WriteLine();
            //Show("");
        }
        public static void P(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
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
            Console.WriteLine($"{DateTimeOffset.Now} " + msg);
        }
    }
}
