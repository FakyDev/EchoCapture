using System;
using System.IO;
using System.Collections.Generic;

using Screenshoter.Exceptions;

namespace Screenshoter.Command{
    public class SettingCommand : CommandBase{
        
        /// <summary> (Get only) Reference to dictionary of command's arg dictionary.</summary>
        private static Dictionary<int, CommandArg> _dictionary{
            get{
                //create dictionary
                Dictionary<int, CommandArg> dictionary = new Dictionary<int, CommandArg>();

                //add to dictionary
                dictionary.Add(0, new CommandArg("action", 1, "Determine what action to perform. Either \"dir\" or \"timeout\". Dir is for the folder to save and timeout is the amount of ms to wait.", typeof(string)));
                dictionary.Add(1, new CommandArg("value", 2, "The value, based on action.", new Type[2]{typeof(string), typeof(int)}));

                //return
                return dictionary;
            }
        }

        /// <inheritdoc/>
        public SettingCommand() : base("setting", "Edit and view settings.", SettingCommand._dictionary){}

        /// <inheritdoc/>
        /// <exception cref="Screenshoter.Exceptions.InsufficientLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.InvalidLineArgumentException"></exception>
        public override void OnSendEvent(string[] args){
            //validate arguments
            this.ValidateArguments(args);

            //will hold arg 1
            string action;
            try{
                action = args[0];
            } catch(IndexOutOfRangeException){
                throw new InsufficientLineArgumentException("Argument 1, is not passed.");
            }

            //check if changing directory for saved file
            if(action == "dir"){
                //hold the value
                string path;

                try{
                    //update reference path
                    path = args[1];
                } catch(IndexOutOfRangeException){
                    throw new InsufficientLineArgumentException("Argument 2, is not passed.");
                }

                //reference instance
                CommandArg arg = this.ArgsList[1];

                //the type of arg and value
                Type valueType = typeof(string);

                //check if not type
                if(!arg.IsType(valueType)){
                    //throw exception
                    throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[0]);
                }

                //check if not rooted
                if(!Path.IsPathRooted(path)){
                    //throw exception
                    throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, valueType, "Path specified must be from root.");
                }

                //check if not directory
                if(Path.HasExtension(path)){
                    //throw exception
                    throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, valueType, "Path specified is not a directory.");
                }

                //check if valid
                if(!Path.IsPathFullyQualified(path)){
                    //throw exception
                    throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, valueType, "Path specified is invalid.");
                }

                //to-do: save value somewhere
                return;
            } 
            
            if(action == "timeout"){
                //hold the value
                string value;

                try{
                    //update reference path
                    value = args[1];
                } catch(IndexOutOfRangeException){
                    throw new InsufficientLineArgumentException("Argument 2, is not passed.");
                }
                //hold parsed value
                int parsedValue;

                //reference instance
                CommandArg arg = this.ArgsList[1];

                //the type of arg and value
                Type valueType = typeof(int);

                //check if not type
                if(!arg.IsType(valueType)){
                    //throw exception
                    throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[0]);
                }

                //parse value
                try{
                    arg.Parse(value, out parsedValue);
                } catch(InvalidLineArgumentException e){
                    //rethrow
                    throw e;
                }

                //to-do: save value somewhere
                return;
            }
        }
    }
}