using System;
using System.Linq;
using System.Collections.Generic;
using EchoCapture.Exceptions;

namespace EchoCapture.Command{

    /// <summary> Manages commands, allow functionality for commands.</summary>
    public static class CommandManager{
        
        /// <summary> Determine if has initialised.</summary>
        private static bool hasInitialise = false;

        /// <summary> Hold list of command.</summary>
        private static List<ICommand> commandList = new List<ICommand>();

        /// <summary> (Get only) Return the list of command reference.</summary>
        public static List<ICommand> CommandList{
            get{
                return CommandManager.commandList;
            }
        }

        /// <summary> Initialise commands.</summary>
        public static void Initialise(){
            //already initialise
            if(CommandManager.hasInitialise){
                return;
            }

            //intialise commands
            new HelpCommand();
            new ExitCommand();
            new SettingCommand();
            new TaskCommand();

            //loop through list
            foreach(ICommand command in CommandManager.CommandList){
                //call
                command.OnAfterAllCommandsInitialise();
            }

            //if debug is on
            if(Debug.IsDebug){
                Debug.SkipLine();
            }

            //update state
            CommandManager.hasInitialise = true;
        }


        /// <summary> Add command properly and to the list.</summary>
        /// <exception cref="System.ArgumentException"> Thrown when parameter one is already in the list.</exception>
        public static void AddToList(ICommand command){
            //check if not in list
            if(!CommandManager.commandList.Contains(command)){
                //determine if catch exception
                bool catched = false;

                try{
                    CommandManager.CommandExist(command.CommandPrefix, command.CommandName);
                } catch(UnknownLineCommandException){
                    catched = true;
                } finally{
                    if(!catched){
                        //throw exception
                        throw new ArgumentException("Another command, using the same prefix and name, is already in the list.");
                    }
                } 
                //add event
                command.AddEvent();
                //add to list
                CommandManager.commandList.Add(command);

                return;
            }

            //throw exception
            throw new ArgumentException("Parameter 1 passed, is already in the list of commands.");
        }

        /// <summary> Remove command properly and from the list.</summary>
        /// <exception cref="System.ArgumentException"> Thrown when parameter one is not in the list.</exception>
        public static void RemoveFromList(ICommand command){
            //check if in list
            if(CommandManager.commandList.Contains(command)){
                //remove event
                command.RemoveEvent();
                //remove from list
                CommandManager.commandList.Remove(command);

                return;
            }

            //throw exception
            throw new ArgumentException("Parameter 1 passed, is not in the list of commands.");
        }

        /// <summary> Remove all commands properly.</summary>
        public static void RemoveAll(){
            //loop through list
            foreach(ICommand command in CommandManager.commandList){
                //remove event
                command.RemoveEvent();
            }
            
            //remove all
            CommandManager.commandList.RemoveRange(0, CommandManager.commandList.Count);
        }
    

        /// <summary> Look for command which uses default prefix.</summary>
        /// <param name="commandName"> The command name to look for.</param>
        /// <exception cref="EchoCapture.Exceptions.UnknownLineCommandException"> Thrown when command doesn't exists.</exception>
        public static void CommandExist(string commandName){
            //loop through list
            foreach(ICommand command in CommandManager.CommandList){
                //check if same
                if(command.CommandName == commandName && command.CommandPrefix == CommandBase.DefaultCommandPrefix){
                    return;
                }
            }

            //throw exception
            throw new UnknownLineCommandException(CommandBase.DefaultCommandPrefix, commandName);
        }
        
        /// <summary> Look for command which uses default prefix, and return its reference.</summary>
        /// <param name="commandName"> The command name to look for.</param>
        /// <param name="outputCommand"> The command reference.</param>
        /// <exception cref="EchoCapture.Exceptions.UnknownLineCommandException"> Thrown when command doesn't exists.</exception>
        public static void CommandExist(string commandName, out ICommand outputCommand){
            //default value
            outputCommand = null;

            //loop through list
            foreach(ICommand command in CommandManager.CommandList){
                //check if same
                if(command.CommandName == commandName && command.CommandPrefix == CommandBase.DefaultCommandPrefix){
                    //update value
                    outputCommand = command;
                    
                    return;
                }
            }

            //throw exception
            throw new UnknownLineCommandException(CommandBase.DefaultCommandPrefix, commandName);
        }

        /// <summary> Look for command which uses custom prefix.</summary>
        /// <param name="commandPrefix"> The command prefix to look for.</param>
        /// <param name="commandName"> The command name to look for.</param>
        /// <exception cref="EchoCapture.Exceptions.UnknownLineCommandException"> Thrown when command doesn't exists.</exception>
        public static void CommandExist(char commandPrefix, string commandName){
            //loop through list
            foreach(ICommand command in CommandManager.CommandList){
                //check if same
                if(command.CommandPrefix == commandPrefix && command.CommandName == commandName){
                    return;
                }
            }

            //throw exception
            throw new UnknownLineCommandException(commandPrefix, commandName);
        }

        /// <summary> Look for command which uses custom prefix, and return its reference.</summary>
        /// <param name="commandPrefix"> The command prefix to look for.</param>
        /// <param name="commandName"> The command name to look for.</param>
        /// <param name="outputCommand"> The command reference.</param>
        /// <exception cref="EchoCapture.Exceptions.UnknownLineCommandException"> Thrown when command doesn't exists.</exception>
        public static void CommandExist(char commandPrefix, string commandName, out ICommand outputCommand){
            //default value
            outputCommand = null;

            //loop through list
            foreach(ICommand command in CommandManager.CommandList){
                //check if same
                if(command.CommandPrefix == commandPrefix && command.CommandName == commandName){
                    //update value
                    outputCommand = command;

                    return;
                }
            }

            //throw exception
            throw new UnknownLineCommandException(commandPrefix, commandName);
        }


        /// <summary> Read line, search for the command and run it along with its arguments.</summary>
        /// <param name="line"> Raw line, inputted directly from user.</param>
        public static void ReadCommand(string line){
            //remove prefix
            string currentLine = line.Substring(1);

            //get command name from line
            string lineCommandName = currentLine.Split(' ')[0];

            //will hold the command reference
            ICommand command;

            try{
                //get command
                CommandManager.CommandExist(line[0], lineCommandName, out command);

                //hold args only
                string argsOnly;
                try{
                    //try to get the args only which is after the start after the command
                    argsOnly = currentLine.Substring(lineCommandName.Length+1);
                } catch (ArgumentOutOfRangeException){
                    //exception, if there is no args
                    argsOnly = "";
                }

                //send command
                command.OnSendEvent(CommandManager.ParseArguments(argsOnly));
            } catch(InvalidLineException e){
                //output error
                Debug.Error(e.Message);
            }
        }

        /// <summary> Parse line, returing array of args.</summary>
        /// <remarks> Line should not container the prefix, and args which has space char is required
        /// to have double quote around it.</remarks>
        /// <param name="line"> The line to parse.</param>
        /// <returns> Array of string or empty array.</returns>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineFormatException"></exception>
        private static string[] ParseArguments(string line){
            //check if empty
            if(line.Length == 0){
                //remove empty string array
                return new string[]{};
            }
            
            //link list holding the found char index
            LinkedList<int> charIndexList = new LinkedList<int>();
            //hold the last char hold
            int lastFoundCharIndex = -1;

            do{
                //get index
                lastFoundCharIndex = line.IndexOf('"', lastFoundCharIndex + 1);

                //check if found, then update
                if(lastFoundCharIndex != -1){
                    //update list
                    charIndexList.AddLast(lastFoundCharIndex);
                }
            } while(lastFoundCharIndex != -1);

            //list which will hold the index to remove
            List<int> toRemoveIndex = new List<int>();

            //loop through list
            foreach(int charIndex in charIndexList){
                //check if index exists, and there is not space near it
                if(charIndex - 1 >= 0){
                    //check if escaped
                    if(line[charIndex-1] == '\\'){
                        try{
                            if(line[charIndex-2] != '\\'){
                                //store index
                                toRemoveIndex.Add(charIndex);
                            }
                        } catch(IndexOutOfRangeException){
                            //store index
                            toRemoveIndex.Add(charIndex);
                        }

                        continue;
                    }
                }
            }

            //loop through array to remove index
            foreach(int currentRemoveIndex in toRemoveIndex){
                //remove from linked list
                charIndexList.Remove(charIndexList.Find(currentRemoveIndex));
            }

            //not even, in other words non-enclosed args
            if(charIndexList.Count % 2 != 0){
                //throw exception
                throw new InvalidLineFormatException();
            }

            //array of parts
            string[] parts;
            
            //check if contains the char '"'
            if(charIndexList.Count > 0){
                parts = new string[3];
                //initial part
                parts[0] = line.Substring(0, charIndexList.First.Value);
                //middle part
                parts[1] = line.Substring(charIndexList.First.Value, (charIndexList.Last.Value - charIndexList.First.Value)+1);
                //last part
                parts[2] = line.Substring(charIndexList.Last.Value+1);
            } else {
                //the whole as doesn't contain the char
                parts = new string[1]{line};
            }

            //final args
            string[] finalArgs = new string[line.Split(' ').Length];

            //check if contains all three parts
            if(parts.Length == 3){
                //get args for first part
                finalArgs = parts[0].Split(' ');

                //second part--
                //value to remove from
                int charIndexOffset = parts[0].Length - 1;
                //array holding the args
                string[] middlePartArgs = new string[charIndexList.Count/2];
                //the index of array above
                int middlePartIndex = 0;

                //loop through char index list, to make param which is using double quote
                while(charIndexList.Count > 0){
                    //get first
                    int first = charIndexList.First.Value;
                    //remove it
                    charIndexList.RemoveFirst();

                    //get first
                    int second = charIndexList.First.Value;
                    //remove it
                    charIndexList.RemoveFirst();

                    //update array
                    middlePartArgs[middlePartIndex] = parts[1].Substring(first-charIndexOffset, (second-first)-1);

                    //check if there's more
                    if(charIndexList.Count != 0){
                        //check if there is no double quote args in the middle part, and add to list
                        //get the string
                        string partBetweenCurrentAndNewQuote = parts[1].Substring(first-charIndexOffset + (second-first), charIndexList.First.Value-charIndexOffset - (second-first+2));
                        
                        //old
                        //Debug.Dump(charIndexList.First.Value-first-charIndexOffset - (second-first+2));

                        //split string
                        string[] splittedBetween = partBetweenCurrentAndNewQuote.Split(' ');

                        //check if not empty
                        if(splittedBetween.Length > 0){
                            //loop through splitted
                            foreach(string sPart in splittedBetween){
                                //check if empty
                                if(sPart == String.Empty){
                                    continue;
                                }

                                //resize it (could have list would get better performance)
                                Array.Resize(ref middlePartArgs, middlePartArgs.Length + 1);

                                //increment
                                middlePartIndex++;
                                //update array
                                middlePartArgs[middlePartIndex] = sPart;
                            }
                        }
                    }

                    //increment
                    middlePartIndex++;
                }

                //loop through array to replace
                for (int i = 0; i < middlePartArgs.Length; i++){
                    //replace values
                    middlePartArgs[i] = middlePartArgs[i].Replace("\\\\\"", "\\\"");
                    middlePartArgs[i] = middlePartArgs[i].Replace("\\\"", "\"");

                    try{
                        //get last index
                        int lastIndex = middlePartArgs[i].Length - 1;
                        //check if '\\' is the two last character and remove it, leaving '\'
                        if(middlePartArgs[i][lastIndex] == '\\' && middlePartArgs[i][lastIndex-1] == '\\'){
                            middlePartArgs[i] = middlePartArgs[i].Substring(0, lastIndex);
                        }
                    } catch (IndexOutOfRangeException){}
                }

                //merge array
                finalArgs = finalArgs.Concat(middlePartArgs).ToArray();
                //end of second part--

                //split arguments
                string[] lastPartArgs = parts[2].Split(' ');
                //check if not empty
                if(lastPartArgs.Length > 0){
                    //merge to final
                    finalArgs = finalArgs.Concat(lastPartArgs).ToArray();
                }
            } else {
                //split arguments
                finalArgs = parts[0].Split(' ');
            }

            //filter and return args
            return finalArgs.Where((source, index) => source != string.Empty).ToArray();
        }
    }
}