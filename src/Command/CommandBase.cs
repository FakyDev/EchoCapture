using System;
using System.Collections.Generic;

using Screenshoter.Exceptions;

namespace Screenshoter.Command{
    public delegate void CommandSend(string[] args);

    /// <summary> Base class for command.</summary>
    public abstract class CommandBase : ICommand{

        /// <summary> When command is send to console, event.</summary>
        private CommandSend Send;
        
        /// <summary> Default prefix used to show using a command.</summary>
        private const char defaultCommandPrefix = '.';

        /// <summary> (Get only) Return the default prefix for command.</summary>
        public static char DefaultCommandPrefix{
            get{
                return defaultCommandPrefix;
            }
        }

        /// <inheritdoc/>
        public char CommandPrefix{
            get{
                //check if default
                if(useDefaultPrefix){
                    return CommandBase.DefaultCommandPrefix;
                }

                return this.customPrefix;
            }
        }

        /// <inheritdoc/>
        public bool UseDefaultPrefix{
            get{
                return this.useDefaultPrefix;
            }
        }


        /// <summary> Determine if using default prefix.</summary>
        private bool useDefaultPrefix = true;

        /// <summary> Hold the custom prefix.</summary>
        private char customPrefix;

        /// <summary> Hold the command name.</summary>
        private string commandName;

        /// <summary> Hold the command description.</summary>
        private string commandDescription;

        /// <summary> Dictionary holding args data.</summary>
        private Dictionary<int, CommandArg> argsList = null;

        /// <inheritdoc/>
        public string CommandName{
            get{
                return this.commandName;
            }
        }

        /// <inheritdoc/>
        public string CommandDescription{
            get{
                return this.commandDescription;
            }
        }

        /// <inheritdoc/>
        public Dictionary<int, CommandArg> ArgsList{
            get{
                return this.argsList;
            }
        }

        /// <inheritdoc/>
        public bool HasArgs{
            get{
                //check if empty
                if(ArgsList == null){
                    return false;

                //check if only contains no args key
                } else if((ArgsList.ContainsKey(-1) && ArgsList.Count == 1)){
                    return false;
                }

                return true;
            }
        }

        /// <summary> Determine if has subscribed to event.</summary>
        private bool hasSubscribed = false;

        /// <summary> Create command using default prefix.</summary>
        /// <param name="commandName"> The command name.</param>
        /// <param name="commandDescription"> The description of the command.</param>
        public CommandBase(string commandName, string commandDescription){
            //update reference
            this.commandName = commandName;
            this.commandDescription = commandDescription;

            //update list
            CommandManager.AddToList(this);
        }

        /// <summary> Create command using default prefix.</summary>
        /// <param name="commandName"> The command name.</param>
        /// <param name="commandDescription"> The description of the command.</param>
        /// <param name="argsList"> Dictionary holding args data for this command.</param>
        public CommandBase(string commandName, string commandDescription, Dictionary<int, CommandArg> argsList){
            //update reference
            this.commandName = commandName;
            this.commandDescription = commandDescription;
            this.argsList = argsList;

            //update list
            CommandManager.AddToList(this);
        }

        /// <summary> Create command using custom prefix.</summary>
        /// <param name="customPrefix"> The prefix used to activate the command.</param>
        /// <param name="commandName"> The command name.</param>
        /// <param name="commandDescription"> The description of the command.</param>
        public CommandBase(char customPrefix, string commandName, string commandDescription){
            //update reference
            this.useDefaultPrefix = false;
            this.customPrefix = customPrefix;
            this.commandName = commandName;
            this.commandDescription = commandDescription;

            //update list
            CommandManager.AddToList(this);
        }

        /// <summary> Create command using custom prefix.</summary>
        /// <param name="customPrefix"> The prefix used to activate the command.</param>
        /// <param name="commandName"> The command name.</param>
        /// <param name="commandDescription"> The description of the command.</param>
        /// <param name="argsList"> Dictionary holding args data for this command.</param>
        public CommandBase(char customPrefix, string commandName, string commandDescription, Dictionary<int, CommandArg> argsList = null){
            //update reference
            this.useDefaultPrefix = false;
            this.customPrefix = customPrefix;
            this.commandName = commandName;
            this.commandDescription = commandDescription;
            this.argsList = argsList;

            //update list
            CommandManager.AddToList(this);
        }
        

        /// <summary> Verify if arguments sent, is valid.</summary>
        /// <param name="args"> Array of string passed as arguments.</param>
        /// <exception cref="Screenshoter.Exceptions.UnknownLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.InsufficientLineArgumentException"></exception>
        protected void ValidateArguments(string[] args){
            //check if empty
            if(this.ArgsList == null || this.ArgsList.Count == 0){
                //passed non-existing arg
                if(args.Length > 0){
                    //throw exception
                    throw new UnknownLineArgumentException(this);
                }

                return;
            }
            
            //passed non-existing arg or missing argument
            if(args.Length != this.ArgsList.Count){
                //throw exception
                throw new InsufficientLineArgumentException(this);
            }

            //loop through arg list
            foreach(KeyValuePair<int, CommandArg> item in this.ArgsList){
                //get type
                Type[] argTypes = item.Value.ArgType;
                //get arg
                CommandArg arg = item.Value;
                //will hold the index
                int? index = null;
                
                //determine if at least parsed one value
                bool parsed = false;
                //the exception catched
                InvalidLineArgumentException except = null;
                try{
                    for (int i = 0; i < argTypes.Length; i++){
                        //update
                        index = i;
                        //get value
                        if(argTypes[i] == typeof(bool)){
                            bool argValue;
                            arg.Parse(args[item.Key], out argValue);
                            parsed = true;
                        } else if(argTypes[i] == typeof(int)){
                            int argValue;
                            arg.Parse(args[item.Key], out argValue);
                            parsed = true;
                        } else if(argTypes[i] == typeof(string)){
                            string argValue;
                            argValue = args[item.Key];
                            parsed = true;
                        }
                    }
                } catch(InvalidOperationException){
                    //throw exception
                    throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[(int)index]);
                } catch(InvalidLineArgumentException e){
                    //update
                    except = e;
                }

                if(!parsed){
                    //throw exception
                    throw except;
                }
            }
        }

        /// <summary> Verify if arguments sent, is valid. This overload requires args length of at least <paramref name="minArg"/> to start validating argument.</summary>
        /// <param name="args"> Array of string passed as arguments.</param>
        /// <param name="minArg"> The minimum amount of args require to start validating.</param>
        /// <exception cref="Screenshoter.Exceptions.UnknownLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.InsufficientLineArgumentException"></exception>
        protected void ValidateArguments(string[] args, int minArg){
            //stop
            if(args.Length < minArg){
                return;
            }

            //check if empty
            if(this.ArgsList == null || this.ArgsList.Count == 0){
                //passed non-existing arg
                if(args.Length > 0){
                    //throw exception
                    throw new UnknownLineArgumentException(this);
                }

                return;
            }
            
            //passed non-existing arg or missing argument
            if(args.Length != this.ArgsList.Count){
                //throw exception
                throw new InsufficientLineArgumentException(this);
            }

            //loop through arg list
            foreach(KeyValuePair<int, CommandArg> item in this.ArgsList){
                //get type
                Type[] argTypes = item.Value.ArgType;
                //get arg
                CommandArg arg = item.Value;
                //will hold the index
                int? index = null;
                
                //determine if at least parsed one value
                bool parsed = false;
                //the exception catched
                InvalidLineArgumentException except = null;
                try{
                    for (int i = 0; i < argTypes.Length; i++){
                        //update
                        index = i;
                        //get value
                        if(argTypes[i] == typeof(bool)){
                            bool argValue;
                            arg.Parse(args[item.Key], out argValue);
                            parsed = true;
                        } else if(argTypes[i] == typeof(int)){
                            int argValue;
                            arg.Parse(args[item.Key], out argValue);
                            parsed = true;
                        } else if(argTypes[i] == typeof(string)){
                            string argValue;
                            argValue = args[item.Key];
                            parsed = true;
                        }
                    }
                } catch(InvalidOperationException){
                    //throw exception
                    throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[(int)index]);
                } catch(InvalidLineArgumentException e){
                    //update
                    except = e;
                }

                if(!parsed){
                    //throw exception
                    throw except;
                }
            }
        }

        /// <inheritdoc/>
        public abstract void OnSendEvent(string[] args);

        /// <inheritdoc/>
        public virtual void OnAfterAllCommandsInitialise(){}


        /// <inheritdoc/>
        public virtual void AddEvent(){
            if(!this.hasSubscribed){
                //update state
                this.Send += this.OnSendEvent;
                this.hasSubscribed = true;

                //notice if debug on
                if(Debug.IsDebug){
                    Debug.Warning("(Command) " + this.CommandName + " has been loaded.");
                }
            }
        }

        /// <inheritdoc/>
        public virtual void RemoveEvent(){
            if(this.hasSubscribed){
                //update state
                this.Send -= this.OnSendEvent;
                this.hasSubscribed = false;

                //notice if debug on
                if(Debug.IsDebug){
                    Debug.Warning("(Command) " + this.CommandName + " has been unloaded.");
                }
            }
        }
    }
}