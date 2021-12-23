using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoCapture.Networking{
    public class SocketClient : INetworkable{
        
        /// <summary> Hold the ip address.</summary>
        private IPAddress ipAddress;

        /// <summary> Hold the port to connect to.</summary>
        private int port;

        /// <summary> Hold the to connect(ed) client.</summary>
        private Socket client;

        /// <summary> (Get only) Return the port to connect to</summary>
        public int? Port{
            get{
                return this.Port;
            }
        }

        /// <summary> (Get only) Return the ip address of the server to connect to.</summary>
        public string IpAddress{
            get{
                return this.ipAddress.ToString();
            }
        }

        /// <summary> (Get only) Return the client socket.</summary>
        public Socket _Socket{
            get{
                return this.client;
            }
        }


        /// <summary> Create client which will be connecting to the ip and port specified.</summary>
        /// <param name="ipAddress"> The ip address of the server.</param>
        /// <param name="port"> The port of the server.</param>
        public SocketClient(string ipAddress, int port){
            //update reference
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
        }

        /// <summary> Connect to the server.</summary>
        public void Connect(){
            //no socket
            if(this.client != null){
                throw new InvalidOperationException("There is already a connected client.");
            }

            //create client for tcp
            Socket client = new Socket(this.ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            //create endpoint
            IPEndPoint endPoint = new IPEndPoint(this.ipAddress, this.port);

            //connect to server
            client.Connect(endPoint);

            //update refence
            this.client = client;

            //debug msg
            if(Debug.IsDebug){
                Debug.Warning($"Connected to {endPoint}");
            }
        }

        public void SendMessage(string msg){
            //convert to byte
            byte[] convertedMsg = Encoding.ASCII.GetBytes(msg);

            int sent = this.client.Send(convertedMsg);

            Debug.Warning($"{sent} byte(s) sent.");
        }
    }
}