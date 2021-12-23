namespace EchoCapture.Networking{

    /// <summary> Interface for networkable objects.</summary>
    public interface INetworkable{

        /// <summary> (Get only) Return the port of the socket.</summary>
        int? Port{
            get;
        }

        /// <summary> (Get only) Return the ip address of the socket.</summary>
        string IpAddress{
            get;
        }

        /// <summary> (Get only) Return the socket.</summary>
        System.Net.Sockets.Socket _Socket{
            get;
        }
    }
}