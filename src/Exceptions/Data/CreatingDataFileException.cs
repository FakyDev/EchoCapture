using System;

namespace Screenshoter.Exceptions.Data{
    
    /// <summary> Defining that failed to create data file for the application.</summary>
    public class CreatingDataFileException : InvalidDataFileException{

        public CreatingDataFileException() : base(InvalidDataFileType.Create){}

        /// <param name="message"> The user-defined message to pass.</param>
        public CreatingDataFileException(string message) : base(InvalidDataFileType.Create, message){}

        public CreatingDataFileException(Exception inner) : base(InvalidDataFileType.Create, inner){}

        /// <param name="message"> The user-defined message to pass.</param>
        public CreatingDataFileException(string message, Exception inner) : base(InvalidDataFileType.Create, message, inner){}
    }
}