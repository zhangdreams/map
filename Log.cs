using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMap
{
    public enum LogLevel
    {
        all = 0,    // 输出所有日志
        warning = 1,    // 输出警告和报错日志
        errorOnly = 2,  // 只输出报错
    }
    /// <summary>
    /// 日志输出
    /// </summary>
    internal class Log
    {
        private static readonly bool WriteLog = false;
        private static readonly BlockingCollection<(string fileType, string msg)> LogQueue = new ();
        private static readonly Thread LogThread = new (WhiteFile);
        private static readonly SemaphoreSlim Semaphore = new(5);
        private static bool RunWriteWhile = true;
        private static int OutLogLevel = (int)LogLevel.errorOnly;


        public Log()
        {
            if (WriteLog)
                LogThread.Start();

            string logFile = "../../../log/";
            if(!Directory.Exists(logFile))
                Directory.CreateDirectory(logFile);

            List<string> files = new()
            {
                logFile + $"error_{DateTime.Now:yyyy-MM-dd}.txt",
                logFile + $"log_{DateTime.Now:yyyy-MM-dd}.txt",
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
            msg = Show(msg);
            if (OutLogLevel == (int)LogLevel.all)
                ToFile("log", msg);
        }

        public static void R(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            msg = Show(msg);
            if (OutLogLevel == (int)LogLevel.all)
                ToFile("log", msg);
        }

        public static void W(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            msg = Show(msg);
            if (OutLogLevel <= (int)LogLevel.warning)
                ToFile("log", msg);
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
            //ToFile("log", msg);
            return msg;
        }

        private static void ToFile(string fileType, string msg)
        {
            if(WriteLog)
                LogQueue.Add((fileType, msg));
        }

        private static async void WhiteFile()
        {
            while (RunWriteWhile)
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

                    Thread.Sleep(500);
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
            RunWriteWhile = write;
        }
    }
}
