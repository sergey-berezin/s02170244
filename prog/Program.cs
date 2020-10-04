using System;
using classlib;
using System.IO;
using System.Linq;
using System.Threading;
namespace prog
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo myDirectory = new DirectoryInfo(args.FirstOrDefault() ?? "images");
            
            MyClass obj = new MyClass(myDirectory);
            var enterThread = new Thread(new ThreadStart(()=>
            { 
                while (Console.ReadKey().Key != ConsoleKey.Enter){ }
                MyClass.cancelTokenSource.Cancel();
            }));
            enterThread.Start();
            obj.GetResults();

            foreach (var item in obj.result)
            {
                Console.Write(item);
            }
            System.Environment.Exit(0);
        }
    }
}
