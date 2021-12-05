using System;
using System.Reflection;

using Screenshoter.Command;
using Screenshoter.Data;

namespace Screenshoter{
    class Program{

        /// <summary> Determine if application can run.</summary>
        private static bool canRun = true;

        /// <summary> Will hold the application name.</summary>
        private static string applicationName;

        /// <summary> Will hold the application version.</summary>
        private static Version applicationVersion;

        /// <summary> (Get only) Return the application name.</summary>
        public static string ApplicationName{
            get{
                //update
                if(Program.applicationName == null){
                    Program.applicationName = AppDomain.CurrentDomain.FriendlyName;
                }

                //return value
                return Program.applicationName;
            }
        }

        /// <summary> (Get only) Return the application version object.</summary>
        public static Version ApplicationVersion{
            get{
                //check and get value
                if(Program.applicationVersion == null){
                    Program.applicationVersion = Assembly.GetExecutingAssembly().GetName().Version;
                }

                //return value
                return Program.applicationVersion;
            }
        }

        static void Main(string[] args){
            //intialise application
            Program.Initialise();

            //loop
            do{
                //read line
                string input = Console.ReadLine();
                //check if empty
                if(input.Length == 0){
                    continue;
                }
                //execute command
                CommandManager.ReadCommand(input);
            } while (Program.canRun);
        }

        /// <summary> Allow the application to close.</summary>
        public static void Exit(){
            //properly remove all commands
            CommandManager.RemoveAll();
            //disable from running
            Program.canRun = false;
        }

        /// <summary> Initialise the application.</summary>
        private static void Initialise(){
            //display message
            string msg = Program.ApplicationName + " v" + Program.ApplicationVersion + ", for capturing your screen at a time-interval.\n" +
            "Copyright (C) 2021  FakyDev\n\n" +

            "This program is free software: you can redistribute it and/or modify\n" +
            "it under the terms of the GNU Affero General Public License as published\n" +
            "by the Free Software Foundation, either version 3 of the License, or\n" +
            "(at your option) any later version.\n\n" +

            "This program is distributed in the hope that it will be useful,\n" +
            "but WITHOUT ANY WARRANTY; without even the implied warranty of\n" +
            "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n" +
            "GNU Affero General Public License for more details.\n\n" +

            "You should have received a copy of the GNU Affero General Public License\n" +
            "along with this program.  If not, see <https://www.gnu.org/licenses/>.\n\n" +
            
            "Github: https://github.com/FakyDev/Screenshoter";

            //message
            Debug.Message(msg);
            Debug.SkipLine();

            //initialise command manager and application data
            CommandManager.Initialise();
            ApplicationData.Initalise();
        }
    }
}
