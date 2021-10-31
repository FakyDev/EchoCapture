using System;
using Screenshoter.Command;

namespace Screenshoter.Exceptions{

    /// <summary> Line exception, which determine there is not enough arguments.</summary>
    public class InsufficientLineArgumentException : InvalidLineException{

        public InsufficientLineArgumentException() : base(InvalidLineType.insufficientArgument){}

        /// <param name="message"> The additional message to pass.</param>
        public InsufficientLineArgumentException(string message) : base(InvalidLineType.insufficientArgument, new string[1]{message}){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        public InsufficientLineArgumentException(ICommand command) : base(InvalidLineType.insufficientArgument, new string[1]{InsufficientLineArgumentException.CreateHelpMessage(command)}){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        /// <param name="message"> The additional message to pass.</param>
        public InsufficientLineArgumentException(ICommand command, string message) : base(InvalidLineType.insufficientArgument, new string[1]{InsufficientLineArgumentException.CreateHelpMessage(command)+" "+message}){}

        public InsufficientLineArgumentException(Exception inner) : base(InvalidLineType.insufficientArgument, inner){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        public InsufficientLineArgumentException(ICommand command, Exception inner) : base(InvalidLineType.insufficientArgument, new string[1]{InsufficientLineArgumentException.CreateHelpMessage(command)}, inner){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        /// <param name="message"> The additional message to pass.</param>
        public InsufficientLineArgumentException(ICommand command, string message, Exception inner) : base(InvalidLineType.insufficientArgument, new string[1]{InsufficientLineArgumentException.CreateHelpMessage(command)+" "+message}, inner){}

        /// <param name="message"> The additional message to pass.</param>
        public InsufficientLineArgumentException(string message, Exception inner) : base(InvalidLineType.insufficientArgument, new string[1]{message}, inner){}

        /// <summary> Create help message for the command passed.</summary>
        /// <param name="command"> Command to create message from.</command>
        private static string CreateHelpMessage(ICommand command){
            //create initial part
            string msg = "Try using '" + CommandBase.DefaultCommandPrefix + "help ";// + prefix +  this.CommandName + "', for help with the command."

            //add prefix part (custom only)
            if(!command.UseDefaultPrefix){
                msg += command.CommandPrefix;
            }

            //add last part
            msg += command.CommandName + "', for help with the command.";

            return msg;
        }
    }
}