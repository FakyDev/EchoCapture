using System;

namespace Screenshoter.Exceptions{

    /// <summary> Base exception, for line-related exception.</summary>
    public class InvalidLineException : Exception, IInvalidLineException{
        
        /// <summary> Determine the invalid line type.</summary>
        private InvalidLineType lineType;

        /// <inheritdoc/>
        public InvalidLineType LineType{
            get{
                return this.lineType;
            }
        }

        /// <param name="lineType"> The invalid line type.</param>
        public InvalidLineException(InvalidLineType lineType) : base(InvalidLineException.PresetMessage(lineType)){
            //update reference
            this.lineType = lineType;
        }

        /// <param name="lineType"> The invalid line type.</param>
        /// <param name="messages"> Array of string, used for the message. All depends on <paramref name="lineType"/></param>
        public InvalidLineException(InvalidLineType lineType, string[] messages) : base(InvalidLineException.FinalMessage(lineType, messages)){
            //update reference
            this.lineType = lineType;
        }

        /// <param name="lineType"> The invalid line type.</param>
        public InvalidLineException(InvalidLineType lineType, Exception inner): base(InvalidLineException.PresetMessage(lineType), inner){
            //update reference
            this.lineType = lineType;
        }

        /// <param name="lineType"> The invalid line type.</param>
        /// <param name="messages"> Array of string, used for the message. All depends on <paramref name="lineType"/></param>
        public InvalidLineException(InvalidLineType lineType, string[] messages, Exception inner): base(InvalidLineException.FinalMessage(lineType, messages), inner){
            //update reference
            this.lineType = lineType;
        }


        /// <summary> Return pre-defined message, based on line type.</summary>
        /// <param name="lineType"> The invalid line type, to base message from.</param>
        private static string PresetMessage(InvalidLineType lineType){
            //will hold the message
            string message = null;

            //update message
            switch(lineType){
                case InvalidLineType.unknown:
                    message = "An unknown error has occurred.";
                break;

                case InvalidLineType.nonExistingCommand:
                    message = "Unknown command '%commandName%', try using '.help' for a list of commands.";
                break;

                case InvalidLineType.invalidFormat:
                    message = "Invalid format, there is a non-enclosed argument. Use '\\' to escape the double quote.";
                break;

                case InvalidLineType.invalidArgument:
                    message = "Argument[%argNumber%, name:%argName%, type:%argType%] is invalid.";
                break;

                case InvalidLineType.insufficientArgument:
                    message = "Insufficient arguments, for the action you are trying to execute.";
                break;

                case InvalidLineType.nonExistingArguments:
                    message = "Line consists of non-existing argument.";
                break;
            }

            return message;
        }

        /// <summary> Return the final message.</summary>
        /// <param name="messages"> The invalid line type, determining what base message to get.</param>
        private static string FinalMessage(InvalidLineType lineType, string[] messages){
            //get pre-defined message
            string preDef = InvalidLineException.PresetMessage(lineType);

            //update msg
            if(lineType == InvalidLineType.nonExistingCommand){
                //replace value
                preDef = preDef.Replace("%commandName%", messages[0]);

                //try to add custom msg to end of message
                try{
                    preDef += " " + messages[1];
                } catch (IndexOutOfRangeException){}
            } else if(lineType == InvalidLineType.invalidArgument){
                //defined tag in array
                string[] tag = new string[3]{"%argNumber%", "%argName%", "%argType%"};

                //loop and replace value
                for(int i = 0; i < 3; i++){
                    preDef = preDef.Replace(tag[i], messages[i]);
                }

                //try to add custom msg to end of message
                try{
                    preDef += " " + messages[3];
                } catch (IndexOutOfRangeException){}
            } else {
                //try to add custom msg to end of message
                try{
                    preDef += " " + messages[0];
                } catch (IndexOutOfRangeException){}
            }

            //return final message
            return preDef;
        }
    }

    /// <summary> Enum that defines types of invalid line.</summary>
    public enum InvalidLineType{
        
        /// <summary> Unknown invalid line.</summary>
        unknown = 0,

        /// <summary> Command does not exists.</summary>
        nonExistingCommand,

        /// <summary> Failed to parse line.</summary>
        invalidFormat,

        /// <summary> Argument(s) used is not valid.</summary>
        invalidArgument,

        /// <summary> Not enough arguments, for the command or the functionaly of it.</summary>
        insufficientArgument,

        /// <summary> There is non-existing arguments.</summary>
        nonExistingArguments
    }
}