using System;
using System.IO;
using System.Collections.Generic;

using Screenshoter.Exceptions;
using Screenshoter.Data;

namespace Screenshoter.Command{

    /// <summary> Command used to modify application data file.</summary>
    public class SettingCommand : CommandBase{

        /// <summary> The argument one value, for updating folder.</summary>
        private const string FOLDER = "folder";

        /// <summary> The argument one value, for updating the time-interval.</summary>
        private const string INTERVAL = "interval";

        /// <summary> The argument one value, for displaying settings.</summary>
        private const string DISPLAY = "show";

        /// <summary> Hold reference to the task command instance.</summary>
        private static TaskCommand taskCommand = null;

        
        /// <summary> (Get only) Reference to dictionary of command's arg dictionary.</summary>
        private static Dictionary<int, CommandArg> commandArgs{
            get{
                //create dictionary
                Dictionary<int, CommandArg> dictionary = new Dictionary<int, CommandArg>();

                //add to dictionary
                dictionary.Add(0, new CommandArg("action", 1, "The operation to perform. \"" + SettingCommand.FOLDER + "\" or \"" + SettingCommand.INTERVAL + "\" for updating data, and \"" + SettingCommand.DISPLAY + "\" for displaying settings.", typeof(string)));
                dictionary.Add(1, new CommandArg("data", 2, "The new data for the setting, based on argument 1. *" + SettingCommand.FOLDER + ": the new path to folder where screenshots will be saved. *" + SettingCommand.INTERVAL + ": the amount of miliseconds to wait between screenshots.", new Type[2]{typeof(string), typeof(int)}));

                //return
                return dictionary;
            }
        }

        /// <inheritdoc/>
        public SettingCommand() : base("setting", "Allows you to show and update application's settings.", SettingCommand.commandArgs){}

        /// <inheritdoc/>
        /// <exception cref="Screenshoter.Exceptions.InsufficientLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.Data.ReadingDataFileException"></exception>
        /// <exception cref="Screenshoter.Exceptions.Data.OverwritingDataFileException"></exception>
        public override void OnSendEvent(string[] args){
            //validate arguments for at least 1 arguments
            try{
                this.ValidateArguments(args, 1);
            } catch (InsufficientLineArgumentException e){
                //exception for folder
                if(args[0] == SettingCommand.FOLDER) {
                    throw new InsufficientLineArgumentException("The new path to folder, is not specified.");

                //exception for time interval
                } else if(args[0] == SettingCommand.INTERVAL) {
                    throw new InsufficientLineArgumentException("The new time interval, is not specified.");
                } else if(args[0] != SettingCommand.DISPLAY){
                    //rethrow
                    throw e;
                }
            }

            //will hold argument one
            string action;
            //get first argument
            try{
                action = args[0];
            } catch(IndexOutOfRangeException){
                throw new InsufficientLineArgumentException(this);
            }

            //for displaying data
            if(action == SettingCommand.DISPLAY){
                this.DisplayData();
                return;
            }

            //for updating folder
            if(action == SettingCommand.FOLDER){
                //performing
                if(SettingCommand.taskCommand.IsPerforming){
                    //notice user
                    Debug.Error("Cannot update folder path while performing task.");

                    return;
                }

                //update folder
                this.UpdateFolder(args[1]);

                //inform user
                Debug.Success("Folder path has been updated.");
                return;
            } 
            
            //for updating time interval
            if(action == SettingCommand.INTERVAL){
                //performing
                if(SettingCommand.taskCommand.IsPerforming){
                    //notice user
                    Debug.Error("Cannot update time interval while performing task.");

                    return;
                }

                //will hold the parse value
                int parsedValue;

                //update interval
                this.UpdateInterval(args[1], out parsedValue);

                //inform user
                Debug.Success($"Time interval has been set to {parsedValue}ms.");
                return;
            }

            //get reference
            CommandArg commandArg = this.ArgsList[0];
            //throw exception
            throw new InvalidLineArgumentException(commandArg.ArgNumber, commandArg.ArgName, commandArg.ArgType[0]);
        }

        /// <summary> Get the application settings, and output to user.</summary>
        /// <exception cref="Screenshoter.Exceptions.Data.ReadingDataFileException"></exception>
        private void DisplayData(){
            //will hold the value
            string folderPath;
            int? interval;

            //get data
            ApplicationData.GetFileData(out folderPath, out interval);
            
            //send to user
            Debug.Message($"folder path: {folderPath}\ntime-interval: {interval}");
        }

        /// <summary> Update the folder path, along with validating the path.</summary>
        /// <remarks> No exception if successful.</remarks>
        /// <param name="path"> The path to saved.</param>
        /// <exception cref="Screenshoter.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.Data.OverwritingDataFileException"></exception>
        /// <exception cref="Screenshoter.Exceptions.Data.ReadingDataFileException"></exception>
        private void UpdateFolder(string path){
            //reference instance
            CommandArg arg = this.ArgsList[1];

            //the type of arg and value
            Type valueType = typeof(string);

            //check if not type
            if(!arg.IsType(valueType)){
                //throw exception
                throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[0]);
            }

            //validate path, will throw exception otherwise
            SettingCommand.ValidatePath(path, arg, valueType);

            //update data file
            ApplicationData.UpdateFileData(new UpdateData(path));
        }

        /// <summary> Update the time interval, along with validating the value.</summary>
        /// <remarks> No exception if successful.</remarks>
        /// <param name="nonParsedvalue"> The non-parsed string value, which will be parsed to integer.</param>
        /// <param name="nonParsedvalue"> The parsed integer value.</param>
        /// <exception cref="Screenshoter.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="Screenshoter.Exceptions.Data.OverwritingDataFileException"></exception>
        /// <exception cref="Screenshoter.Exceptions.Data.ReadingDataFileException"></exception>
        private void UpdateInterval(string nonParsedvalue, out int parsedValue){
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
            //throw exception
            arg.Parse(nonParsedvalue, out parsedValue);

            //zero or negative number
            if(parsedValue < 1){
                throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[0], "Time interval cannot be zero or a negative number.");
            }

            //update data file
            ApplicationData.UpdateFileData(new UpdateData(parsedValue));
        }


        /// <summary> Validates the path passed.</summary>
        /// <exception cref="Screenshoter.Exceptions.InvalidLineArgumentException"></exception>
        private static void ValidatePath(string path, CommandArg arg, Type valueType){
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
                throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, valueType, "Path specified is not an absolute path or is invalid.");
            }
        }

        /// <inheritdoc/>
        public override void OnAfterAllCommandsInitialise(){
            //update instance
            if(SettingCommand.taskCommand == null){
                //loop through command list
                foreach (ICommand command in CommandManager.CommandList){
                    //same type
                    if(command.GetType() == typeof(TaskCommand)){
                        //update
                        SettingCommand.taskCommand = (TaskCommand)command;

                        return;
                    }
                }
            }
        }
    }
}