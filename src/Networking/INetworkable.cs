namespace EchoCapture.Networking{

    /// <summary> Interface for networkable objects.</summary>
    public interface INetworkable{

        /// <summary> (Get only) Return the port using.</summary>
        int? Port{
            get;
        }

        /// <summary> (Get only) Return the ip address.</summary>
        string IpAddress{
            get;
        }

        /// <summary> (Get only) Return if socket is connected.</summary>
        bool Connected{
            get;
        }
    }
}