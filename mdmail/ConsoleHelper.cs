using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate
{
    public class ConsoleHelper
    {
        public string Title
        {
            get { return Console.Title;  }
            set { Console.Title = value;  }
        }

        
        public void Write(string msg)
        {
            Console.Write(msg);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }

        public void Clear()
        {
            Console.Clear();
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }



        public void WriteLineInColor(ConsoleColor foreground, string msg)
        {
            ConsoleColor b4 = Console.ForegroundColor;
            Console.ForegroundColor = foreground;
            Console.WriteLine(msg);
            Console.ForegroundColor = b4;
        }
        public void WriteInColor(ConsoleColor foreground, string msg)
        {
            ConsoleColor b4 = Console.ForegroundColor;
            Console.ForegroundColor = foreground;
            Console.Write(msg);
            Console.ForegroundColor = b4;
        }

        public string[] SplitArgs(string argString)
        {
            int countOfArgs = 0;
            var ptrResult = CommandLineToArgvW(argString, out countOfArgs);
            string[] array = new string[countOfArgs];

            if (ptrResult == IntPtr.Zero)
                throw new Win32Exception("Cannot parse {0}".Fmt(argString));

            try
            {
                for (int i = 0; i < countOfArgs; i++)
                {
                    string arg = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptrResult, i * IntPtr.Size));
                    array[i] = arg;
                }
            }
            finally
            {
                LocalFree(ptrResult);
            }

            return array;
        }

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr hMem);
    }
}
