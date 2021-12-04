using System;

namespace Screenshoter.Exceptions.Data{
    
    /// <summary> Defining that failed to delete data file for the application.</summary>
    public class DeletingDataFileException : InvalidDataFileException{

        public DeletingDataFileException() : base(InvalidDataFileType.Delete){}

        /// <param name="message"> The user-defined message to pass.</param>
        public DeletingDataFileException(string message) : base(InvalidDataFileType.Delete, message){}

        public DeletingDataFileException(Exception inner) : base(InvalidDataFileType.Delete, inner){}

        /// <param name="message"> The user-defined message to pass.</param>
        public DeletingDataFileException(string message, Exception inner) : base(InvalidDataFileType.Delete, message, inner){}
    }
}