using System;
using System.Net;
using System.Net.Sockets;

namespace EchoCapture.Networking{

    /// <summary> Allows you to connect to a server and send request, using TCP protocol.</summary>
    public class SocketClient : Networkable, INetworkable{
        
        /// <summary> Create client which will be connecting to the ip and port specified.</summary>
        /// <param name="ipAddress"> The ip address of the server.</param>
        /// <param name="port"> The port of the server.</param>
        public SocketClient(string ipAddress, int port){
            //update reference
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.port = port;
        }

        /// <summary> Connect to the server.</summary>
        /// <exception cref="System.InvalidOperationException"> If client is already connected.</exception>
        public void Connect(){
            //no socket
            if(this.socket != null){
                throw new InvalidOperationException("There is already a connected client.");
            }

            //create client for tcp
            Socket client = new Socket(this.ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            //create endpoint
            IPEndPoint endPoint = new IPEndPoint(this.ipAddress, (int)this.port);

            //connect to server
            client.Connect(endPoint);

            //update refence
            this.socket = client;

            //debug msg
            if(Debug.IsDebug){
                Debug.Warning($"Connected to {endPoint}");
            }
        }

        /// <summary> Disconnects client from the server.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when client is not connected.</exception>
        public void Disconnect(){
            //no socket
            if(this.socket == null){
                throw new InvalidOperationException("Client is not connected");
            }

            //get local end point before disposing
            string info = this.socket.LocalEndPoint.ToString();

            //shutdown and close
            try{
                this.socket.Shutdown(SocketShutdown.Both);
            } catch(Exception) {} finally {
                this.socket.Close();
            }

            //update reference
            this.socket = null;
            this.port = null;

            //send msg
            if(Debug.IsDebug){
                Debug.Warning($"Disconnected from {info}.");
            }
        }


        /// <summary> Send a message with default preset to the server.</summary>
        /// <param name="msg"> The message to send. Should be in the charset of ASCII.</param>
        /// <exception cref="System.InvalidOperationException"> If client is not connected.</exception>
        /// <returns> Amount of byte sent.</returns>
        public int SendMessage(string msg){
            //check if connected
            if(this.socket == null || !this.socket.Connected){
                throw new InvalidOperationException("Client is not connect to any server.");
            }

            //create instance
            TransferData data = new TransferData(msg, TransferType.DefaultMessage);

            //send and return amount of bytes send
            return this.socket.Send(data.ToBytes());
        }

        /// <summary> Send a message to the server, letting you to choose the transfer type.</summary>
        /// <param name="msg"> The message to send. Should be in the charset of ASCII.</param>
        /// <param name="tType"> The transfer type, determing what kind of message.</param>
        /// <exception cref="System.InvalidOperationException"> If client is not connected or <paramref name="tType"/> is invalid.</exception>
        /// <returns> Amount of byte sent.</returns>
        public int SendMessage(string msg, TransferType tType){
            //check if connected
            if(this.socket == null || !this.socket.Connected){
                throw new InvalidOperationException("Client is not connect to any server.");
            }

            if(tType == TransferType.Drop){
                throw new ArgumentException("TransferType passed as argument, isn't a valid one for this case.");
            }

            //create instance
            TransferData data = new TransferData(msg, tType);

            //send and return amount of bytes send
            return this.socket.Send(data.ToBytes());
        }

        /// <summary> Send a request to the server, which ask the server to stop.</summary>
        /// <exception cref="System.InvalidOperationException"> If client is not connected.</exception>
        /// <returns> Amount of byte sent.</returns>
        public int DropRequest(){
            //check if connected
            if(this.socket == null || !this.socket.Connected){
                throw new InvalidOperationException("Client is not connect to any server.");
            }

            //create instance
            TransferData data = new TransferData(null, TransferType.Drop);

            //send and return amount of bytes send
            return this.socket.Send(data.ToBytes());
        }
    }
}