using System;

namespace Screenshoter.Exceptions.Data{
    
    /// <summary> Defining that failed to overwrite data file for the application.</summary>
    public class OverwritingDataFileException : InvalidDataFileException{

        public OverwritingDataFileException() : base(InvalidDataFileType.Overwriting){}

        /// <param name="message"> The user-defined message to pass.</param>
        public OverwritingDataFileException(string message) : base(InvalidDataFileType.Overwriting, message){}

        public OverwritingDataFileException(Exception inner) : base(InvalidDataFileType.Overwriting, inner){}

        /// <param name="message"> The user-defined message to pass.</param>
        public OverwritingDataFileException(string message, Exception inner) : base(InvalidDataFileType.Overwriting, message, inner){}
    }
}