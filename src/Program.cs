using System;
using System.Reflection;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

using EchoCapture.Command;
using EchoCapture.Data;

using EchoCapture.Networking;

namespace EchoCapture{
    public partial class Program{

        /// <summary> Determine if application can run.</summary>
        private static bool canRun = true;

        /// <summary> Hold the application name.</summary>
        private static string applicationName;

        /// <summary> Hold the application version.</summary>
        private static Version applicationVersion;

        /// <summary> Hold the current state of the application.</summarY>
        private static ApplicationState? currentState = null;

        /// <summary> (Get only) Determine if the application is in debug state.</summary>
        public static bool DebugState{
            get{
                //debug state
                if(Program.currentState == ApplicationState.Debug){
                    return true;
                }

                return false;
            }
        }

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


        public static void Main(string[] args){
            try{
                //update state
                if(args.Length == 2 && args[0] == "captureDebug"){
                    Program.currentState = ApplicationState.Debug;
                } else {
                    Program.currentState = ApplicationState.Normal;
                }
                
                //intialise application
                Program.Initialise();
                //perform operation
                if(Program.DebugState){
                    Program.PerformDebug(SocketListener.LocalIp, Int32.Parse(args[1]));
                } else {
                    Program.PerformDefault();
                }
            } catch (Exception){} finally{
                Program.OnExit(null, null);
            }
        }

        /// <summary> Initialise the application.</summary>
        private static void Initialise(){
            //for capture debug mode
            if(Program.currentState == ApplicationState.Debug){
                //disable input
                //Console.ReadKey(false);
                //Console.CursorVisible = false;
                return;
            }

            //add event
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(Program.OnExit);

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

        /// <summary> Perform application's task for normal state.</summary>        
        private static void PerformDefault(){
            //check if normal state
            if(Program.DebugState){
                return;
            }
            
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
    
        public static void Test(string msg){
            ((SocketClient)Program.socket).SendMessage(msg+"<EOF>");
        }
    }

    //this part of the class is for the capture debug state
    public partial class Program{

        /// <summary> Hold the process of the debug console for capture screen.</summary>
        private static Process debugProcess = null;

        /// <summary> Hold the server.</summary>
        private static SocketListener server = null;

        /// <summary> Hold the client.</summary>
        private static SocketClient client = null;

        /// <summary> (Get only) Return the process of the debug console for capture screen.</summary>
        public static Process DebugProcess{
            get{
                //already ended or null
                if(Program.debugProcess == null || Program.debugProcess.HasExited){
                    return null;
                }

                //return process
                return Program.debugProcess;
            }
        }

        /// <summary> (Get only) Return the client or the server, based on application state.</summary>
        public static INetworkable socket{
            get{
                //return server
                if(Program.server != null){
                    return Program.server;
                }

                //return clinet
                if(Program.client != null){
                    return Program.client;
                }

                return null;
            }
        }

        /// <summary> Perform application's task for capture debug state.</summary>        
        private static void PerformDebug(string ipAddress, int port){
            //check if debug state
            if(!Program.DebugState){
                return;
            }

            //create listener with max request of 5
            SocketListener listener = new SocketListener(ipAddress, port, 5);

            //update reference
            Program.server = listener;
            
            //start listening
            listener.StartListening();

            //accecpt request
            Socket handler = listener.AllowRequest();

            do{
                // Incoming data from the client.
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                Debug.Success(data.Replace("<EOF>", ""));
            } while (handler.Connected);
        }

        /// <summary> Start another process in a new window, to debug capture screen.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when there is already an instance of debug running.</exception>
        internal static void StartCaptureDebug(){
            //already have a capture screen debug console.
            if(Program.DebugProcess != null){
                throw new InvalidOperationException("Already have an instance of debug running.");
            }

            //get free port
            int port = SocketListener.FreePort;

            //create instance
            Process process = new Process();
            //set executable
            process.StartInfo.FileName = Program.ApplicationName + ".exe";
            //set argument
            process.StartInfo.Arguments = $"captureDebug {port}";
            //set hidden
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            //to create new window
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;

            //update reference
            Program.debugProcess = process;

            //start process
            if(process.Start()){
                //create client
                Program.client = new SocketClient(SocketListener.LocalIp, port);
                //connect client
                Program.ConnectClient();
            }
        }

        /// <summary> Connect client to server and send error (if there is).</summary>
        private static void ConnectClient(){
            //this method is for debug enabled only, so no need to check if debug is enabled
            try{
                //connect
                Program.client.Connect();
            } catch (SocketException e){
                //debug msg
                Debug.Error(e.Message);
            }
        }

        /// <summary> Called just before application is exited.</summary>
        private static void OnExit(object sender, EventArgs e){
            //kill process
            if(Program.DebugProcess != null){
                Program.DebugProcess.Kill();
            }

            //disable send and request and dispose
            if(Program.server != null){
                //try to convert
                try{
                    Program.server.StopListening();
                } catch (InvalidCastException){}
            }
        }
    }
    
    /// <summary> Enum defining state of application.</summary>
    enum ApplicationState{

        /// <summary> Application is running in normal state.</summary>
        Normal = 0,
        /// <summary> Application is running in debug for capture screen.</summary>
        Debug
    }
}
