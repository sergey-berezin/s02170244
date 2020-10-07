using System;
using classlib;
using System.IO;
using System.Linq;
using System.Threading;
namespace prog
{
    public class WriteToConsole: IConsoleView
    {
        public void ReturnRes(ImageInfo info)
        {
            Console.WriteLine("Predicting contents of image: ");
            Console.Write(info);
            Console.WriteLine();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo myDirectory = new DirectoryInfo(args.FirstOrDefault() ?? "images");
            
            ImageRecognizer obj = new ImageRecognizer(myDirectory, new WriteToConsole());

            var enterThread = new Thread(new ThreadStart(()=>
            { 
                while (Console.ReadKey().Key != ConsoleKey.Enter){ }
                ImageRecognizer.cancelTokenSource.Cancel();
            }));
            enterThread.Start();
            obj.GetResults();

            // foreach (var item in obj.result)
            // {
            //     Console.WriteLine("Predicting contents of image...");
            //     Console.Write(item);
            //     Console.WriteLine();
            // }
            System.Environment.Exit(0);
        }
    }
}
