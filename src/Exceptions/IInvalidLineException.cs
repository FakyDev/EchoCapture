namespace Screenshoter.Exceptions{

    /// <summary> Interface for line exception.</summary>
    public interface IInvalidLineException{
        
        /// <summary> (Get only) Return the line type.</summary>
        InvalidLineType LineType{
            get;
        }

        /// <summary> The exception message.</summary>
        string Message{
            get;
        }
    }
}