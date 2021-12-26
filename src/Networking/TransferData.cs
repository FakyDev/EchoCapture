using System;
using System.Text;

namespace EchoCapture.Networking{
    /// <summary> Structure used to send data over the network and parse data received from the network.</summary>
    public struct TransferData{
        /// <summary> Hold the ASCII-encodable message.</summary>
        private string message;
        
        /// <summary> Hold the transfer type.</summary>
        private TransferType tType;

        /// <summary> The original bytes used to construct this instance.</summary>
        private byte[] originalBytes;

        /// <summary> (Get only) Return the ASCII-encodable message.</summary>
        public string Message{
            get{
                return this.message;
            }
        }

        /// <summary> (Get only) Return the transfer type.</summary>
        public TransferType TType{
            get{
                return this.tType;
            }
        }

        /// <summary> (Get only) Return the original bytes used to construct this instance.</summary>
        public byte[] OriginalBytes{
            get{
                return this.originalBytes;
            }
        }

        /// <summary> Construct instance from bytes.</summary>
        /// <exception cref="System.ArgumentException"> Thrown when <paramref name="data"/> is insufficinet to construct an instance.</exception>
        public TransferData(byte[] data){
            //update
            this.originalBytes = data;

            //check if data sent is valid
            if(data.Length < 4){
                throw new ArgumentException("Bytes passed is insufficient to construct an instance.");
            }

            //will hold the transfer type in byte
            byte[] tTypeByte = new byte[4];

            //get transfer type in byte
            Array.Copy(data, 0, tTypeByte, 0, 4);
            //get trasnfer type
            TransferType tType = (TransferType)BitConverter.ToInt32(tTypeByte);

            //will hold the msg
            string msg = null;
            if(data.Length > 4){
                //will hold the message in byte
                byte[] msgByte = new byte[data.Length-4];

                //get message in byte
                Array.Copy(data, 4, msgByte, 0, msgByte.Length);

                //amount of byte to remove
                int length = 0;
                //calculate
                for (int i = msgByte.GetUpperBound(0); i > 0; i--){
                    if(msgByte[i].Equals(byte.MinValue)){
                        length++;
                    }
                }

                //resize array for msg only
                Array.Resize<byte>(ref msgByte, msgByte.Length - length);

                //get msg
                try{
                    msg = Encoding.ASCII.GetString(msgByte);
                } catch (Exception){}
            }
            

            //update reference
            this.message = msg;
            this.tType = tType;
        }

        /// <summary> Construct instance from parameters.</summary>
        /// <param name="message"> The message to pass through the network.</param>
        /// <param name="tType"> The transfer type to use.</param>
        public TransferData(string message, TransferType tType){
            //update reference
            this.message = message;
            this.tType = tType;
            this.originalBytes = null;
        }

        /// <summary> Convert this instance to bytes to send through network.</summary>
        public byte[] ToBytes(){
            //get transfer type in byte
            byte[] data = BitConverter.GetBytes((int)this.tType);

            //check if msg is not null
            if(this.message != null){
                //convert msg to byte
                byte[] msgBytes = Encoding.ASCII.GetBytes(this.message);

                //get last index
                int beforeResizeLastIndex = data.GetUpperBound(0);
                //resize data array
                Array.Resize<byte>(ref data, data.Length + msgBytes.Length);
                //update array
                Array.Copy(msgBytes, 0, data, beforeResizeLastIndex+1, msgBytes.Length);
            }

            //return
            return data;
        }
    }
}