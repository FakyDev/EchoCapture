using System;

namespace Screenshoter.Exceptions.Data{
    
    /// <summary> Defining that failed to read data file for the application.</summary>
    public class ReadingDataFileException : InvalidDataFileException{

        public ReadingDataFileException() : base(InvalidDataFileType.Reading){}

        /// <param name="message"> The user-defined message to pass.</param>
        public ReadingDataFileException(string message) : base(InvalidDataFileType.Reading, message){}

        public ReadingDataFileException(Exception inner) : base(InvalidDataFileType.Reading, inner){}

        /// <param name="message"> The user-defined message to pass.</param>
        public ReadingDataFileException(string message, Exception inner) : base(InvalidDataFileType.Reading, message, inner){}
    }
}