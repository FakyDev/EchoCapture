using System;
using System.Net;
using System.Net.Sockets;

namespace EchoCapture.Networking{

    /// <summary> Base class for class which will be using network.</summary>
    public abstract class Networkable : INetworkable{

        /// <summary> Hold the ip address.</summary>
        protected IPAddress ipAddress;

        /// <summary> Hold the port using.</summary>
        protected int? port;

        /// <summary> Hold the socket.</summary>
        protected Socket socket;


        /// <inheritdoc/>
        public int? Port{
            get{
                return this.Port;
            }
        }

        /// <inheritdoc/>
        public string IpAddress{
            get{
                return this.ipAddress.ToString();
            }
        }

        /// <inheritdoc/>
        public bool Connected{
            get{
                //not connected
                if(this.socket == null){
                    return false;
                }
                
                //return
                return this.socket.Connected;
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

        
        /// <summary> Get date received in socket, and parse them into <see cref="EchoCapture.Networking.TransferData"/>.</summary>
        /// <param name="handler"> The returned value from <see cref="System.Net.Sockets.Socket.Accept"/>.</param>
        public static TransferData ParseRequest(Socket handler){
            //will hold the bytes
            byte[] bytes = new byte[1024];
            
            //get data sent
            int received = handler.Receive(bytes);

            //return insatnce
            return new TransferData(bytes);
        }
    }
}