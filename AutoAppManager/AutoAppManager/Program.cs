using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAppManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string title = "Auto App Manager";
            Console.Title = title;

            Console.WriteLine(title);

            //AppManagement appMangr = new AppManagement();
            //appMangr.CheckAppIsAlive();

            CheckAppIsAlive.Start();

            CommandListenner cmdListen = new CommandListenner();
            cmdListen.ListenForCommands();


            Console.ReadLine();

        }
    }
}
