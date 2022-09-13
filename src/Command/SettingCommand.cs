using System;
using System.IO;
using System.Collections.Generic;

using EchoCapture.Exceptions;
using EchoCapture.Data;

namespace EchoCapture.Command{

    /// <summary> Command used to modify application data file.</summary>
    public class SettingCommand : CommandBase{

        /// <summary> The argument one value, for updating folder.</summary>
        private const string FOLDER = "folder";

        /// <summary> The argument one value, for updating the time-interval.</summary>
        private const string INTERVAL = "interval";

        /// <summary> Hold reference to the task command instance.</summary>
        private static TaskCommand taskCommand = null;
        
        /// <summary> (Get only) Reference to dictionary of command's arg dictionary.</summary>
        private static Dictionary<int, CommandArg> commandArgs{
            get{
                //create dictionary
                Dictionary<int, CommandArg> dictionary = new Dictionary<int, CommandArg>();

                //add to dictionary
                dictionary.Add(0, new CommandArg($"{FOLDER}|{INTERVAL}", 1, "Changes the folder to save capture screens in or changes the interval at which capture screen are taken.", typeof(string)));
                dictionary.Add(1, new CommandArg("value", 2, "The value to use for the selected options from argument one.", new Type[2]{typeof(string), typeof(int)}));

                //return
                return dictionary;
            }
        }

        /// <inheritdoc/>
        public SettingCommand() : base("setting", "Allows you to show and update application's settings.", SettingCommand.commandArgs){}

        /// <inheritdoc/>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        public override void OnSendEvent(string[] args){
            //performing
            if(SettingCommand.taskCommand.IsPerforming){
                //notice user
                Debug.Error("Cannot perform any changes to setting, while capture screens are being taken.");
                //update log
                System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog("Cannot perform any changes to setting, while capture screens are being taken.");

                return;
            }

            //validate args
            this.ValidateArguments(args);
            //define arg 1 as action
            string action = args[0];

            //for updating folder
            if(action == SettingCommand.FOLDER){
                //update folder
                this.UpdateFolder(args[1]);

                //inform user
                Debug.Success($"Capture screen are now saving in the directory \"${args[1]}\".");
                return;
            }

            //for updating time interval
            if(action == SettingCommand.INTERVAL){
                //will hold the parse value
                int parsedValue;

                //update interval
                this.UpdateInterval(args[1], out parsedValue);

                //inform user
                Debug.Success($"Capture screen will now be taken at {parsedValue}ms interval, equivalent to {parsedValue/1000f}s interval.");
                //warns user
                if(parsedValue < 1000){
                    Debug.Warning($"Interval of 1000ms or above is recommanded.");
                }
                return;
            }

            //get reference
            CommandArg commandArg = this.ArgsList[0];
            //throw exception
            throw new InvalidLineArgumentException(commandArg.ArgNumber, commandArg.ArgName, commandArg.ArgType[0], $"{action} does not perform any action. Please use '.help {this.CommandName}'.");
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