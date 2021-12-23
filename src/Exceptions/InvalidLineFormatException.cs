using System;

namespace EchoCapture.Exceptions{

    /// <summary> Line exception, used when there is invalid format. To be precise, there is non-enclosed argument.</summary>
    public class InvalidLineFormatException : InvalidLineException{
        
        public InvalidLineFormatException() : base(InvalidLineType.invalidFormat){}

        /// <param name="addtionalInfo"> Addtional info to add to the message.</param>
        public InvalidLineFormatException(string addtionalInfo) : base(InvalidLineType.invalidFormat, new string[1]{addtionalInfo}){}

        /// <param name="addtionalInfo"> Addtional info to add to the message.</param>
        public InvalidLineFormatException(string addtionalInfo, Exception inner): base(InvalidLineType.invalidFormat, new string[1]{addtionalInfo}, inner){}
    }
}