using System;
using System.IO;
using System.Collections.Generic;

using EchoCapture.Exceptions;
using EchoCapture.Data;
using EchoCapture.Data.File;

namespace EchoCapture.Command{

    /// <summary> Command used to modify application data file.</summary>
    public class SettingCommand : CommandBase{

        /// <summary> The argument one value, for updating folder.</summary>
        private const string FOLDER = "folder";

        /// <summary> The argument one value, for updating the time-interval.</summary>
        private const string INTERVAL = "interval";

        /// <summary> The argument one value, for updating the image format.</summary>
        private const string IMAGE_FORMAT = "format";

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
                dictionary.Add(0, new CommandArg("action", 1, $"The operation to perform. \"{SettingCommand.FOLDER}\", \"{SettingCommand.INTERVAL}\" or \"{SettingCommand.IMAGE_FORMAT}\" for updating data, and \"{SettingCommand.DISPLAY}\" for displaying settings.", typeof(string)));
                dictionary.Add(1, new CommandArg("data", 2, $"The new data for the setting based on argument 1. *{SettingCommand.FOLDER}: the new path to folder where capture screen will be saved. *{SettingCommand.INTERVAL}: the amount of miliseconds to wait between capture screen. *{SettingCommand.IMAGE_FORMAT}: the image format of capture screen.", new Type[2]{typeof(string), typeof(int)}));

                //return
                return dictionary;
            }
        }

        /// <inheritdoc/>
        public SettingCommand() : base("setting", "Allows you to show and update application's settings.", SettingCommand.commandArgs){}

        /// <inheritdoc/>
        /// <exception cref="EchoCapture.Exceptions.InsufficientLineArgumentException"></exception>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"></exception>
        public override void OnSendEvent(string[] args){
            //determine if caught exception for display
            bool excepForDisplay = false;

            //validate arguments for at least 1 arguments
            try{
                this.ValidateArguments(args, 1);
            } catch (InsufficientLineArgumentException){
                switch(args[0]){
                    //exception for folder
                    case SettingCommand.FOLDER:
                        throw new InsufficientLineArgumentException("The new path to folder, is not specified.");

                    //exception for time interval
                    case SettingCommand.INTERVAL:
                        throw new InsufficientLineArgumentException("The new time interval, is not specified.");

                    //exception for folder
                    case SettingCommand.IMAGE_FORMAT:
                        throw new InsufficientLineArgumentException("The new image format, is not specified.");

                    //exception for display
                    case SettingCommand.DISPLAY:
                        excepForDisplay = true;
                    break;
                }
            } finally {
                if(args[0] == SettingCommand.DISPLAY){
                    //contains additional args
                    if(!excepForDisplay){
                        throw new UnknownLineArgumentException(this, "It applies for the current operation you're trying to accomplish.");
                    }
                }
            }

            //hold argument one
            string action = args[0];

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
                    //update log
                    System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog("Cannot update folder path while performing task.");

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
                    //update log
                    System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog("Cannot update time interval while performing task.");

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

            if(action == SettingCommand.IMAGE_FORMAT){
                //performing
                if(SettingCommand.taskCommand.IsPerforming){
                    //notice user
                    Debug.Error("Cannot update image format while performing task.");
                    //update log
                    System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog("Cannot update image format while performing task.");

                    return;
                }

                //update interval
                this.UpdateImageFormat(args[1]);

                //inform user
                Debug.Success($"Image format has been set to '{args[1]}'.");

                return;
            }

            //get reference
            CommandArg commandArg = this.ArgsList[0];
            //throw exception
            throw new InvalidLineArgumentException(commandArg.ArgNumber, commandArg.ArgName, commandArg.ArgType[0]);
        }

        /// <summary> Get the application settings, and output to user.</summary>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        private void DisplayData(){
            //will hold the value
            string folderPath;
            int? interval;
            FileExtension? imageExtension;

            //get data
            ApplicationData.GetAllFileData(out folderPath, out interval, out imageExtension);
            
            //send to user
            Debug.Message($"folder path: {folderPath}\ntime-interval: {interval}\nimage format: {imageExtension}");
        }

        /// <summary> Update the folder path, along with validating the path.</summary>
        /// <remarks> No exception if successful.</remarks>
        /// <param name="path"> The path to saved.</param>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
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
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
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

        /// <summary> Update the image extension.</summary>
        /// <remarks> No exception if successful.</remarks>
        /// <param name="imageExtension"> The non-parsed string value, which will be parsed to FileExtension.</param>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        private void UpdateImageFormat(string imageExtension){
            //reference instance
            CommandArg arg = this.ArgsList[1];

            //the type of arg and value
            Type valueType = typeof(string);

            //check if not type
            if(!arg.IsType(valueType)){
                //throw exception
                throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[0]);
            }

            //will hold the parse one
            FileExtension parsedImageExtension;

            //update extension
            try{
                parsedImageExtension = (FileExtension)Enum.Parse(typeof(FileExtension), imageExtension);
            } catch (Exception){
                throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[0], "Image format can only be 'png' or 'jpg'.");
            }

            //invalid
            if(!EchoCapture.Data.File.Image.ImageFile.ValidateImageExtension(parsedImageExtension)){
                throw new InvalidLineArgumentException(arg.ArgNumber, arg.ArgName, arg.ArgType[0], "Image format can only be 'png' or 'jpg'.");
            }

            //update
            ApplicationData.UpdateFileData(new UpdateData(parsedImageExtension));
        }


        /// <summary> Validates the path passed.</summary>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
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
                SettingCommand.taskCommand = CommandManager.SearchCommand<TaskCommand>();
            }
        }
    }
}