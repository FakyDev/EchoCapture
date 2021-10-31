using System;

namespace Screenshoter.Exceptions{

    /// <summary> Line exception, used when there is arguments used for command is invalid.</summary>
    public class InvalidLineArgumentException : InvalidLineException{

        /// <summary> Hold the arg number.</summary>
        private int argNumber;

        /// <summary> Hold the arg name.</summary>
        private string argName;

        /// <summary> (Get only) Return the arg number.</summary>
        public int ArgNumber{
            get{
                return this.argNumber;
            }
        }

        /// <summary> (Get only) Return the arg name.</summary>
        public string ArgName{
            get{
                return this.argName;
            }
        }

        /// <param name="argNumber"> The number of the argument, starting from 1.</param>
        /// <param name="argName"> The name of the argument.</param>
        /// <param name="argType"> The type of the argument.</param>
        public InvalidLineArgumentException(int argNumber, string argName, Type argType) : base(InvalidLineType.invalidArgument, new string[3]{argNumber.ToString(), argName, argType.ToString()}){
            this.argNumber = argNumber;
            this.argName = argName;
        }

        /// <param name="argNumber"> The number of the argument, starting from 1.</param>
        /// <param name="argName"> The name of the argument.</param>
        /// <param name="argType"> The type of the argument.</param>
        /// <param name="additionalInfo"> Additional info to add to the message.</param>
        public InvalidLineArgumentException(int argNumber, string argName, Type argType, string addtionalInfo) : base(InvalidLineType.invalidArgument, new string[4]{argNumber.ToString(), argName, argType.ToString(), addtionalInfo}){
            this.argNumber = argNumber;
            this.argName = argName;
        }
        
        /// <param name="argNumber"> The number of the argument, starting from 1.</param>
        /// <param name="argName"> The name of the argument.</param>
        /// <param name="argType"> The type of the argument.</param>
        public InvalidLineArgumentException(int argNumber, string argName, Type argType, Exception inner) : base(InvalidLineType.invalidArgument, new string[3]{argNumber.ToString(), argName, argType.ToString()}, inner){
            this.argNumber = argNumber;
            this.argName = argName;
        }

        /// <param name="argNumber"> The number of the argument, starting from 1.</param>
        /// <param name="argName"> The name of the argument.</param>
        /// <param name="argType"> The type of the argument.</param>
        /// <param name="additionalInfo"> Additional info to add to the message.</param>
        public InvalidLineArgumentException(int argNumber, string argName, Type argType, string addtionalInfo, Exception inner) : base(InvalidLineType.invalidArgument, new string[4]{argNumber.ToString(), argName, argType.ToString(), addtionalInfo}, inner){
            this.argNumber = argNumber;
            this.argName = argName;
        }
    }
}