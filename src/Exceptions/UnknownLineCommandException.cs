using System;

namespace EchoCapture.Exceptions{

    /// <summary> Line exception, that is used when undefined command is used.</summary>
    public class UnknownLineCommandException : InvalidLineException{
        
        /// <summary> Hold the command prefix.</summary>
        private char commandPrefix;

        /// <summary> Hold the command name.</summary>
        private string commandName;

        /// <summary> (Get only) Return the command prefix.</summary>
        public char CommandPrefix{
            get{
                return this.commandPrefix;
            }
        }

        /// <summary> (Get only) Return the command name.</summary>
        public string CommandName{
            get{
                return this.commandName;
            }
        }

        /// <param name="commandPrefix"> The prefix of the command.</param>
        /// <param name="commandName"> The name of the command.</param>
        public UnknownLineCommandException(char commandPrefix, string commandName) : base(InvalidLineType.nonExistingCommand, new string[1]{commandPrefix+commandName}){
            this.commandPrefix = commandPrefix;
            this.commandName = commandName;
        }

        /// <param name="commandPrefix"> The prefix of the command.</param>
        /// <param name="commandName"> The name of the command.</param>
        /// <param name="additionalInfo"> Addtional info to add to the message.</param>
        public UnknownLineCommandException(char commandPrefix, string commandName, string additionalInfo) : base(InvalidLineType.nonExistingCommand, new string[2]{commandPrefix+commandName, additionalInfo}){
            this.commandPrefix = commandPrefix;
            this.commandName = commandName;
        }

        /// <param name="commandPrefix"> The prefix of the command.</param>
        /// <param name="commandName"> The name of the command.</param>
        public UnknownLineCommandException(char commandPrefix, string commandName, Exception inner): base(InvalidLineType.nonExistingCommand, new string[1]{commandPrefix+commandName}, inner){
            this.commandPrefix = commandPrefix;
            this.commandName = commandName;
        }

        /// <param name="commandPrefix"> The prefix of the command.</param>
        /// <param name="commandName"> The name of the command.</param>
        /// <param name="additionalInfo"> Addtional info to add to the message.</param>
        public UnknownLineCommandException(char commandPrefix, string commandName, string additionalInfo, Exception inner): base(InvalidLineType.nonExistingCommand, new string[2]{commandPrefix+commandName, additionalInfo}, inner){
            this.commandPrefix = commandPrefix;
            this.commandName = commandName;
        }
    }
}