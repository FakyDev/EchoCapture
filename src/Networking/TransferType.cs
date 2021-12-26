namespace EchoCapture.Networking{

    /// <summary> Constants determining how data send over network, will be used.</summary>
    public enum TransferType{

        /// <summary> Determine data sent is for displaying a message.</summary>
        DefaultMessage = 0,

        /// <summary> Determine data sent is for displaying an error message.</summary>
        ErrorMessage,

        /// <summary> Determine data sent is for stopping a process/action.</summary>
        Drop
    }
}