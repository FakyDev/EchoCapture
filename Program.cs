using System;
using Screenshoter.Command;

namespace Screenshoter{

    class Program{

        private static bool canRun = true;

        static void Main(string[] args){
            CommandManager.Initialise();
            Console.WriteLine("Hello World!");

            //CommandManager.ReadCommand(".help a b \"just test\" \" \"\\\"another experiment\\\"\"");
            //return;
            do{
                string input = Console.ReadLine();
                if(input.Length == 0){
                    continue;
                }
                CommandManager.ReadCommand(input);
            } while (Program.canRun);
        }

        public static void Exit(){
            CommandManager.RemoveAll();
            Program.canRun = false;
        }
    }
}
