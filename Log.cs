using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    /// <summary>
    /// 日志输出
    /// </summary>
    internal class Log
    {
        private static readonly bool WriteLog = true;
        private static readonly BlockingCollection<(string fileType, string msg)> LogQueue = new ();
        private static readonly Thread LogThread = new (WhiteFile);
        private static readonly SemaphoreSlim Semaphore = new(5);
        private static bool RunWrite = true;


        public Log()
        {
            if (WriteLog)
                LogThread.Start();
            List<string> files = new()
            {
                $"../../../log/error_{DateTime.Now:yyyy-MM-dd}.txt",
                $"../../../log/log_{DateTime.Now:yyyy-MM-dd}.txt",
            };
            foreach (string file in files)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

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
            msg = Show(msg);
            ToFile("error", msg);
        }

        public static string Show(string msg)
        {
            msg = $"{DateTimeOffset.Now} " + msg;
            Console.WriteLine(msg);
            ToFile("log", msg);
            return msg;
        }

        private static void ToFile(string fileType, string msg)
        {
            if(WriteLog)
                LogQueue.Add((fileType, msg));
        }

        private static async void WhiteFile()
        {
            while (RunWrite)
            {
                if (LogQueue.TryTake(out var message))
                {
                    string path;
                    if (message.fileType == "error")
                        path = $"../../../log/error_{DateTime.Now:yyyy-MM-dd}.txt";
                    else
                        path = $"../../../log/log_{DateTime.Now:yyyy-MM-dd}.txt";
                    // File.AppendAllText(path, message.msg + "\n");
                    await Semaphore.WaitAsync();
                    try
                    {
                        await WriteFileAsync(path, message.msg + "\n");
                    }
                    finally
                    {
                        Semaphore.Release();
                    }
                    
                }
            }
        }

        private static async Task WriteFileAsync(string path , string msg)
        {
            using var writer = new StreamWriter(path, true, Encoding.UTF8, bufferSize: 8192);
            await writer.WriteAsync(msg);
        }

        public static void SetWrite(bool write)
        {
            RunWrite = write;
        }
    }
}
