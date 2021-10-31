using System;

namespace Screenshoter.Exceptions{

    /// <summary> Line exception, used when the error is unknown.</summary>
    public class UnknownLineException : InvalidLineException{
        
        public UnknownLineException() : base(InvalidLineType.unknown){}

        /// <param name="message"> The message to pass.</param>
        public UnknownLineException(string message) : base(InvalidLineType.unknown, new string[1]{message}){}

        /// <param name="message"> The message to pass.</param>
        public UnknownLineException(string message, Exception inner) : base(InvalidLineType.unknown, new string[1]{message}, inner){}
    }
}