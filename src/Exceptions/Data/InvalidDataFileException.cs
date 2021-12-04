using System;

namespace Screenshoter.Exceptions.Data{

    /// <summary> Base class for defining error related to application data file.</summary>
    public class InvalidDataFileException : Exception{
        
        /// <param name="type"> The error type, for the invalid data file.</param>
        public InvalidDataFileException(InvalidDataFileType type) : base(InvalidDataFileException.PresetMessage(type, null)){}

        /// <param name="type"> The error type, for the invalid data file.</param>
        /// <param name="message"> The user-defined message to pass.</param>
        public InvalidDataFileException(InvalidDataFileType type, string message) : base(InvalidDataFileException.PresetMessage(type, message)){}

        /// <param name="type"> The error type, for the invalid data file.</param>
        public InvalidDataFileException(InvalidDataFileType type, Exception inner) : base(InvalidDataFileException.PresetMessage(type, null), inner){}

        /// <param name="type"> The error type, for the invalid data file.</param>
        /// <param name="message"> The user-defined message to pass.</param>
        public InvalidDataFileException(InvalidDataFileType type, string message, Exception inner) : base(InvalidDataFileException.PresetMessage(type, message), inner){}

        /// <summary> Provide message for the invalid type provided, along with additional message if stated.</summary>
        /// <param name="message"> The user-defined message to pass.</param>
        private static string PresetMessage(InvalidDataFileType type, string message){
            //will be updated
            string text = "";

            switch(type){
                case InvalidDataFileType.Reading:
                    text = "Failed to read data file.";
                break;

                case InvalidDataFileType.Create:
                    text = "Failed to create data file.";
                break;

                case InvalidDataFileType.Delete:
                    text = "Failed to delete data file.";
                break;

                case InvalidDataFileType.Overwriting:
                    text = "Failed to update data file.";
                break;
            }

            //check if additional message
            if(message != null){
                text += " " + message;
            }

            return text;
        }
    }

    /// <summary> Defining error types related to application data file.</summary>
    public enum InvalidDataFileType{
        
        /// <summary> Failed to read data file.</summary>
        Reading,

        /// <summary> Failed to create data file.</summary>
        Create,

        /// <summary> Failed to delete data file.</summary>
        Delete,

        /// <summary> Failed to overwrite data file.</summary>
        Overwriting
    }
}