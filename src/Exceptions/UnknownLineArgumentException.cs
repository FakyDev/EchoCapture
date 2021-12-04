using System;
using Screenshoter.Command;

namespace Screenshoter.Exceptions{

    /// <summary> Line exception, that is used when undefined argument is passed.</summary>
    public class UnknownLineArgumentException : InvalidLineException{

        public UnknownLineArgumentException() : base(InvalidLineType.nonExistingArguments){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        public UnknownLineArgumentException(ICommand command) : base(InvalidLineType.nonExistingArguments, new string[1]{UnknownLineArgumentException.CreateHelpMessage(command)}){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        /// <param name="additional"> The additional message to add.</param>
        public UnknownLineArgumentException(ICommand command, string additional) : base(InvalidLineType.nonExistingArguments, new string[1]{UnknownLineArgumentException.CreateHelpMessage(command)+" "+additional}){}

        /// <param name="additional"> The additional message to add.</param>
        public UnknownLineArgumentException(string additional) : base(InvalidLineType.nonExistingArguments, new string[1]{additional}){}

        public UnknownLineArgumentException(Exception inner) : base(InvalidLineType.nonExistingArguments, inner){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        public UnknownLineArgumentException(ICommand command, Exception inner) : base(InvalidLineType.nonExistingArguments, new string[1]{UnknownLineArgumentException.CreateHelpMessage(command)}, inner){}

        /// <param name="command"> The command instance, which causes the exception.</param>
        /// <param name="additional"> The additional message to add.</param>
        public UnknownLineArgumentException(ICommand command, string additional, Exception inner) : base(InvalidLineType.nonExistingArguments, new string[1]{UnknownLineArgumentException.CreateHelpMessage(command)+" "+additional}, inner){}

        /// <param name="additional"> The additional message to add.</param>
        public UnknownLineArgumentException(string additional, Exception inner) : base(InvalidLineType.nonExistingArguments, new string[1]{additional}, inner){}
    
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