using System.Net.Sockets;
using System.Net;
using System;

namespace EchoCapture.Networking{

    /// <summary> Delegate for socket related event, passing socket as parameter.</summary>
    public delegate void SocketEvent(Socket socket);

    /// <summary> Allows you to listen for request and send request, using TCP protocol.</summary>
    public class SocketListener : INetworkable{
        
        /// <summary> Hold the ip address.</summary>
        private IPAddress ipAddress;

        /// <summary> Hold the port listening for.</summary>
        private int? port;

        /// <summary> The amount of request, the server can handle before sending busy response.</summary>
        private int maxRequest;

        /// <summary> Hold the listener.</summary>
        private Socket listener;


        /// <summary> Event called when socket starts listening.</summary>
        public event SocketEvent OnStart;

        /// <summary> Event called when socket stops listening.</summary>
        public event SocketEvent OnClose;

        /// <summary> (Get only) Return the port listening for.</summary>
        public int? Port{
            get{
                return this.Port;
            }
        }

        /// <summary> (Get only) Return the ip address of the server.</summary>
        public string IpAddress{
            get{
                return this.ipAddress.ToString();
            }
        }

        /// <summary> (Get only) Return the listening socket.</summary>
        public Socket _Socket{
            get{
                return this.listener;
            }
        }

        /// <summary> (Get only) Return the local address.</summary>    
        public static string LocalIp{
            get{
                //get ip address in string
                return Dns.GetHostEntry("localhost").AddressList[0].ToString();
            }
        }

        /// <summary> (Get only) Return a free port, but is not guaranteed.</summary>    
        public static int FreePort{
            get{
                //get ip address
                IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];

                //create socket
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //create endpoint
                IPEndPoint endPoint = new IPEndPoint(ipAddress, 0);

                //bing listener to end point
                socket.Bind(endPoint);

                Debug.Warning(endPoint.ToString());

                //start listening
                socket.Listen(1);

                //get port
                int port = ((IPEndPoint)socket.LocalEndPoint).Port;

                //shutdown and close
                try{
                    socket.Shutdown(SocketShutdown.Both);
                } catch(Exception) {} finally {
                    socket.Close();
                }

                //return port
                return port;
            }
        }


        /// <summary> Create listener which will be using a specified port.</summary>
        /// <exception cref="System.ArgumentException"> Thrown when <paramref name="maxRequest"/> is zero or negative.</exception>
        public SocketListener(string ipAddress, int port, int maxRequest){
            //invalid max request
            if(maxRequest <= 0){
                throw new ArgumentException("The maximum amount of request cannot be zero or a negative number.");
            }
            
            //update reference
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.maxRequest = maxRequest;
            this.port = port;
        }

        /// <summary> Create listener which will be a unspecified available port.</summary>
        /// <exception cref="System.ArgumentException"> Thrown when <paramref name="maxRequest"/> is zero or negative.</exception>
        /// <remarks> The port will be updated when has started listening.</remarks>
        public SocketListener(string ipAddress, int maxRequest){
            //invalid max request
            if(maxRequest <= 0){
                throw new ArgumentException("The maximum amount of request cannot be zero or a negative number.");
            }

            //update reference
            this.ipAddress = IPAddress.Parse(ipAddress);
            this.maxRequest = maxRequest;
            this.port = null;
        }

        /// <summary> Allow the socket to start listening.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when there is already an socket.</exception>
        public void StartListening(){
            //no socket
            if(this.listener != null){
                throw new InvalidOperationException("There is already an socket listening.");
            }

            //create socket
            Socket listener = new Socket(this.ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //create endpoint
            IPEndPoint endPoint = new IPEndPoint(this.ipAddress, (port == null ? 0 : (int)port));

            //bing listener to end point
            listener.Bind(endPoint);

            //start listening
            listener.Listen(this.maxRequest);
            //update reference
            this.listener = listener;

            //update port
            this.port = endPoint.Port;

            //send msg
            if(Debug.IsDebug){
                Debug.Warning($"Listening on {endPoint}.");
                Debug.SkipLine();
            }

            //call event
            this.OnStart?.Invoke(listener);
        }

        /// <summary> Accept and return request.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when there is no socket.</exception>
        public Socket AllowRequest(){
            //no listener
            if(this.listener == null){
                throw new InvalidOperationException("There is no socket listening.");
            }

            //accept request
            Socket handler = this.listener.Accept();

            //send msg
            if(Debug.IsDebug){
                Debug.Warning($"Client({handler.LocalEndPoint}) is connected.");
            }

            return handler;
        }

        /// <summary> Unlike <see cref="EchoCapture.Networking.SocketListener.StopListening"/>, it allows you to reuse this socket.</summary>
        public void Disconnect(){
            //disconnect
            this.listener.Disconnect(true);

            //send msg
            if(Debug.IsDebug){
                Debug.Warning($"Temporarily stopped listening on {this.listener.LocalEndPoint}.");
                Debug.SkipLine();
            }
        }

        /// <summary> Stop the socket from listening for requests.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when there is no socket.</exception>
        public void StopListening(){
            //no socket
            if(this.listener == null){
                throw new InvalidOperationException("There is no socket listening.");
            }

            //get local end point before disposing
            string info = this.listener.LocalEndPoint.ToString();

            //shutdown and close
            try{
                this.listener.Shutdown(SocketShutdown.Both);
            } catch(Exception) {} finally {
                this.listener.Close();
            }

            //update reference
            this.listener = null;
            this.port = null;

            //send msg
            if(Debug.IsDebug){
                Debug.Warning($"Stopped listening on {info}.");
            }

            //call event
            this.OnClose?.Invoke(null);
        }
    }
}