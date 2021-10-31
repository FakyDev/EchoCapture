using System;
using System.Collections.Generic;
using Screenshoter.Exceptions;

namespace Screenshoter.Command{

    /// <summary> Command used to provide the user with help about commands.</summary>
    public class HelpCommand : CommandBase{

        public HelpCommand() : base("help", "Shows list of commands.", HelpCommand._dictionary){}
        
        /// <summary> (Get only) Reference to dictionary of help command's arg dictionary.</summary>
        private static Dictionary<int, CommandArg> _dictionary{
            get{
                //dictionary holding args for help command
                Dictionary<int, CommandArg> _dictionary = new Dictionary<int, CommandArg>();
                //for first arg
                _dictionary.Add(0, new CommandArg("commandName", 1, "The name of the command, to get its arguments description.", typeof(string)));

                //return
                return _dictionary;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="Screenshoter.Exceptions.UnknownLineArgumentException"></exception>
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
                        Console.WriteLine("Command '" + e2.CommandPrefix + e2.CommandName + "' was not found.");
                        Debug.SkipLine();

                        //stop
                        return;
                    } catch(ArgumentOutOfRangeException){}

                    //check if nested try catch failed
                    if(command == null){
                        //output msg
                        Console.WriteLine("Command '" + e.CommandName + "' was not found.");
                        Debug.SkipLine();

                        //stop
                        return;
                    }
                }

                //notice
                Console.Write("Searching '" + command.CommandName + "' for arguments...");

                //check if contain
                if(!command.HasArgs){
                    //notice
                    Console.Write(" None was found.\n\n");

                    //inform
                    Console.Write(command.CommandName + ": " + command.CommandDescription + " [description]");

                    //stop
                    return;
                } else {
                    //inform
                    Console.Write("\n\n" + command.CommandName + ": " + command.CommandDescription + " [description]");

                    Console.Write("\n\n" + command.CommandName + ":\n");
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

                    Console.WriteLine(line);
                }

                Debug.SkipLine();
                //stop for args
                return;
            }

            //notify user
            Console.Write("Searching for commands...");
            Console.Write(" " + CommandManager.CommandList.Count + " were found.\n\n");

            //get all commands
            foreach (ICommand command in CommandManager.CommandList){
                //output command
                Console.WriteLine(command.CommandPrefix + command.CommandName + " " + command.CommandDescription);
            }

            //inform user
            Console.WriteLine("\nUse '" + this.CommandPrefix + this.CommandName + " " + this.ArgsList[0].ArgName + "' for information about its arguments.\n");
        }
    }
}