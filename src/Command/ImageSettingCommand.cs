using System;
using System.Collections.Generic;
using EchoCapture.Data;
using EchoCapture.Exceptions;


namespace EchoCapture.Command{
    /// <summary> Command used to modify the capture screen quality setting.</summary>
    public class ImageSettingCommand : CommandBase, ICommand{
        
        /// <summary> The constant value, specifying image format is selected.</summary>
        private const string FORMAT = "format";

        /// <summary> The constant value, specifying how the capture screen is being saved.</summary>
        private const string IMAGETYPE = "imageType";

        /// <summary> The constant value, specifying image rescaling is being enable or disabled.</summary>
        private const string RESCALING = "rescaling";

        /// <summary> The constant value, specifying preset selection is being selected.</summary>
        private const string PRESET = "preset";

        /// <summary> The constant value, specifying rescaling resolution is being set up.</summary>
        private const string RESCALINGRESOLUTION = "rescalingResolution";

        /// <summary> The constant value, specifying jpeg quality is being changed.</summary>
        private const string JPEGQUALITY = "jpgQuality";

        /// <summary> Holds the reference to the instace of task command.</summary>
        private static TaskCommand taskCommand = null;

        /// <summary> (Get only) Reference to dictionary of command's arg dictionary.</summary>
        private static Dictionary<int, CommandArg> commandArgs{
            get{
                //create dictionary
                Dictionary<int, CommandArg> dictionary = new Dictionary<int, CommandArg>();

                //add to dictionary
                dictionary.Add(0, new CommandArg($"{IMAGETYPE}|{FORMAT}|{RESCALING}|{PRESET}|{RESCALINGRESOLUTION}|{JPEGQUALITY}", 1, "Changes the capture image file extension, changes image pixel format, enables/disables image rescaling, change chosen preset, change rescaling resolution or set quality for jpg image.", typeof(string)));
                dictionary.Add(1, new CommandArg("value", 2, "The value to use for the selected options from argument one.", new Type[3]{typeof(string), typeof(bool), typeof(int)}));
                dictionary.Add(2, new CommandArg("value2", 3, $"The second value, only possible use case if for {RESCALINGRESOLUTION} is selected.", typeof(int)));

                //return
                return dictionary;
            }
        }
        
        /// <inheritdoc/>
        public ImageSettingCommand() : base("imageSetting", "Shows and allows you to select image configurations.", commandArgs){}

        /// <summary> Event called when the image setting command is called.</summary>
        /// <exception cref="EchoCapture.Exceptions.InsufficientLineArgumentException"></exception>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        /// <exception cref="EchoCapture.Exceptions.UnknownLineArgumentException"></exception>
        public override void OnSendEvent(string[] args){
            //currently capturing screen
            if(taskCommand.IsPerforming){
                Debug.Error("Cannot update image setting while capturing screen.");
                return;
            }

            //no args is passed
            if(args.Length < 2){
                throw new InsufficientLineArgumentException(this);
            }

            //hold arguments passed value
            string action;
            string value = null;
            string value2 = null;

            //determine if exception was thrown
            bool wasThrown = false;

            //retrieves argument
            //and check if correct amount of args is sent based on arg 1
            try{
                //validates args
                this.ValidateArguments(args, 2);
            } catch (InsufficientLineArgumentException){
                //update value
                wasThrown = true;

                //update action chose
                action = args[0];

                //rescaling requires two args
                if(action == RESCALINGRESOLUTION){
                    //args 2, height, is not passed
                    throw new InsufficientLineArgumentException(this, "The height for image-rescaling is missing.");
                }
            } finally {
                //update action chose
                action = args[0];

                if(!wasThrown){
                    if(action == RESCALINGRESOLUTION){
                        value2 = args[2];
                    } else {
                        //non-existing args is passed
                        throw new UnknownLineArgumentException(this);
                    }
                }
            }

            //update arg one
            value = args[1];

            //all depends on parameter 1
            switch(action){
                case FORMAT:
                    try{
                        //update image format
                        if(!ApplicationData.UpdateImageQualityPixelFormat(new ApplicationData.ImageQualityPresetSetting(value, 0, "png"))){
                            Debug.Error($"Failed to update the pixel format for the preset of \"{ApplicationData.GetCurrentSelectedPreset()}\".");
                            return;
                        }
                    } catch (ArgumentException){
                        //output error to console
                        Debug.Error($"\"{value}\" is not from the list of valid pixel formats.");
                        return;
                    }

                    //output result to console
                    Debug.Success($"Screen captures will now be saved using pixel format of \"{value}\".");
                    Debug.Warning($"\"{value}\" is set only in the current using preset \"{ApplicationData.GetCurrentSelectedPreset()}\".");
                break;

                case JPEGQUALITY:
                    //hold the jpg quality
                    int? jpgQuality;

                    //parse arg to int
                    CommandArg.ParseInt(value, out jpgQuality);

                    //failed to parse
                    if(jpgQuality == null){
                        throw new InvalidLineArgumentException(2, "value", typeof(int), "The jpeg quality requires to be an integer ranging from 0 to 100.");
                    }
                    try{
                        //update jpeg image quality
                        if(!ApplicationData.UpdateImageQualityJpgQuality(new ApplicationData.ImageQualityPresetSetting(ApplicationData.ImageQualityPresetSetting.DefaultPixelFormats[0], (int)jpgQuality, "png"))){
                            Debug.Error($"Failed to update the jpeg image quality for the preset of \"{ApplicationData.GetCurrentSelectedPreset()}\"");
                            return;
                        }
                    } catch(ArgumentException){
                        //integer is not from valid range
                        throw new InvalidLineArgumentException(2, "value", typeof(int), "The jpeg quality requires to be from the range of 0-100.");
                    }
                    
                    //output result to console
                    Debug.Success($"Screen capture using JPEG to save, is now saving at {jpgQuality}% quality.");
                    Debug.Warning($"{jpgQuality}% JPEG image quality is set only in the current using preset \"{ApplicationData.GetCurrentSelectedPreset()}\".");
                    Debug.Warning("Only screen capture using JPEG is affected by image quality.");
                break;

                case RESCALING:
                    //hold the rescaling value
                    bool? rescalingState;
                    //parse value
                    CommandArg.ParseBool(value, out rescalingState);

                    //second argument is invalid
                    if(rescalingState == null){
                        throw new InvalidLineArgumentException(2, "value", typeof(bool), "Image-rescaling state, can only be changed with either \"True\" or \"False\".");
                    }

                    //update state
                    if(!ApplicationData.UpdateImageQualityRescaling((bool)rescalingState)){
                        string context = (bool)rescalingState ? "enable" : "disable";
                        Debug.Error($"Failed to {context} the image-rescaling.");
                        return;
                    }
                    //send result to console
                    Debug.Success((bool)rescalingState ? "Image-rescaling is now enable." : "Image-rescaling is now disable.");
                break;

                case PRESET:
                    try{
                        if(!ApplicationData.UpdateImageQualityPreset(value)){
                            Debug.Error($"Failed to change the current using preset to {value}.");
                            return;
                        }
                    } catch (ArgumentException){
                        //send error to console
                        Debug.Error("The preset selected is not an existing preset. Only \"high\", \"standard\" or \"low\" is available.");
                        return;
                    }

                    Debug.Success($"\"{value}\" is now selected as preset.");
                break;

                case IMAGETYPE:
                    try{
                        if(!ApplicationData.UpdateImageQualityFileType(value)){
                            Debug.Error($"Failed to set the image type to {value}.");
                            return;
                        }
                    } catch (ArgumentException){
                        //send error to console
                        Debug.Error("The image type selected is not a valid one. Only \"png\" or \"jpeg\" is available.");
                        return;
                    }

                    Debug.Success($"Capture screens are now saving in .{value} file.");
                    Debug.Warning("Note: only \".jpeg\" file can be saved at various quality level (compression).");
                break;

                case RESCALINGRESOLUTION:
                    //hold the width and height of rescaling resolution
                    int? width;
                    int? height;

                    //parse the argument to int
                    CommandArg.ParseInt(value, out width);
                    CommandArg.ParseInt(value2, out height);

                    //failed to parse
                    if(width == null){
                        throw new InvalidLineArgumentException(2, "value", typeof(int), "The new width requires to be a non-zero and positive integer.");
                    }
                    if(height == null){
                        throw new InvalidLineArgumentException(3, "value2", typeof(int), "The new height requires to be a non-zero and positive integer.");
                    }

                    try{
                        //update rescaling resolution
                        if(!ApplicationData.UpdateImageQualityRescalingResolution(new ApplicationData.ImageQualityPresetSetting(ApplicationData.ImageQualityPresetSetting.DefaultPixelFormats[0], 0, "png", (int)width, (int)height))){
                            Debug.Error($"Failed to set the image-rescaling resolution to {width}x{height}.");
                            return;
                        }
                    } catch(ArgumentOutOfRangeException e){
                        //sets param for exception
                        int argNum = e.ParamName == "newWidth" ? 2 : 3;
                        string argName = e.ParamName == "newWidth" ? "value" : "value2";
                        string msgVar = e.ParamName == "newWidth" ? "width" : "height";

                        throw new InvalidLineArgumentException(argNum, argName, typeof(int), $"The new {msgVar} requires to be a non-zero and positive integer.");
                    }

                    Debug.Success($"Image-rescaling resolution is set to {width}x{height}.");
                break;

                default:
                    //invalid action
                    throw new InvalidLineArgumentException(1, $"{IMAGETYPE}|{FORMAT}|{RESCALING}|{PRESET}|{RESCALINGRESOLUTION}|{JPEGQUALITY}", typeof(string), $"\"{action}\" does not perform any action. Please use '.help {this.CommandName}'.");
            }
        }

        /// <inheritdoc/>
        public override void OnAfterAllCommandsInitialise(){
            //update instance
            if(ImageSettingCommand.taskCommand == null){
                ImageSettingCommand.taskCommand = CommandManager.SearchCommand<TaskCommand>();
            }
        }
    }
}