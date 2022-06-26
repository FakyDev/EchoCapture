using System;
using System.Collections.Generic;
using EchoCapture.Exceptions;

namespace EchoCapture.Command{

    /// <summary> Command used to provide the user with help about commands.</summary>
    public class HelpCommand : CommandBase{
        
        /// <summary> (Get only) Reference to dictionary of help command's arg dictionary.</summary>
        private static Dictionary<int, CommandArg> commandArgs{
            get{
                //dictionary holding args for help command
                Dictionary<int, CommandArg> _dictionary = new Dictionary<int, CommandArg>();
                //for first arg
                _dictionary.Add(0, new CommandArg("commandName", 1, "The name of the command you need help with.", typeof(string)));

                //return
                return _dictionary;
            }
        }

        public HelpCommand() : base("help", "Shows list of commands and provide help.", HelpCommand.commandArgs){}

        /// <inheritdoc/>
        /// <exception cref="EchoCapture.Exceptions.UnknownLineArgumentException"></exception>
        public override void OnSendEvent(string[] args){
            //validate arguments
            this.ValidateArguments(args, 1);

            //with args
            if(args.Length > 0){
                //will hold command
                ICommand command = null;

                try{
                    //get command
                    CommandManager.CommandExist(args[0], out command);
                } catch (UnknownLineCommandException e){
                    //try with custom prefix
                    try{
                        //get command
                        CommandManager.CommandExist(args[0][0], args[0].Substring(1), out command);
                    } catch(UnknownLineCommandException e2){
                        //output msg
                        Debug.Warning("Command '" + e2.CommandPrefix + e2.CommandName + "' was not found.");
                        Debug.SkipLine();

                        //stop
                        return;
                    } catch(ArgumentOutOfRangeException){}

                    //check if nested try catch failed
                    if(command == null){
                        //output msg
                        Debug.Warning("Command '" + e.CommandName + "' was not found.");
                        Debug.SkipLine();

                        //stop
                        return;
                    }
                }

                //check if contain
                if(!command.HasArgs){
                    //inform
                    Debug.Message(command.CommandName + ": " + command.CommandDescription + " [description]");
                    Debug.Warning("No arguments found, for this command.");

                    //stop
                    return;
                } else {
                    //inform
                    Debug.Message(command.CommandName + ": " + command.CommandDescription + " [description]");
                    //inform
                    Debug.Warning(this.ArgsList.Count > 1 ? "Arugments:" : "Argument:");
                }

                //loop through dictionary
                foreach(KeyValuePair<int, CommandArg> single in command.ArgsList){
                    //check if empty arg
                    if(single.Key == -1){
                        continue;
                    }

                    //create line
                    string line = "\t[" + (single.Key+1) + "]" + single.Value.ArgName + ": " + single.Value.ArgDescription + " ";

                    foreach (Type t in single.Value.ArgType){
                        line += "[" + t.ToString() + "]";
                    }

                    Debug.Warning(line);
                }
                
                //stop for args
                return;
            }

            //notify user
            Debug.Warning(CommandManager.CommandList.Count + (CommandManager.CommandList.Count > 1 ? " commands were found." : " command was found.") + "\n");

            //get all commands
            foreach (ICommand command in CommandManager.CommandList){
                //output command
                Debug.Message(command.CommandPrefix + command.CommandName + " : " + command.CommandDescription);
            }

            //inform user
            Debug.Warning("\nUse '" + this.CommandPrefix + this.CommandName + " " + this.ArgsList[0].ArgName + "' for information about its arguments.");
        }
    }
}