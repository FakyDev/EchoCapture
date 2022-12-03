using System;
using System.Collections.Generic;
using System.Linq;

using EchoCapture.Data.File;
using EchoCapture.Data.File.Text;
using EchoCapture.Exceptions.Data;
using EchoCapture.Exceptions.Data.IniFile;

namespace EchoCapture.Data{

    /// <summary> Manages the application related data.</summary>
    public static class ApplicationData{

        /// <summary> Determine if has intialise application data manager.</summary>
        private static bool hasInitialise = false;

        /// <summary> Hold the instance of the application data file.</summary>
        private static JsonFile<Setting> dataFile;

        /// <summary> Hold the file, containing the image quality presets.</summary>
        private static IniFile imagePresetConfigFile = null;

        /// <summary> (Get only) Return setting with default values.</summary>
        private static Setting DefaultConfig{
            get{
                return new Setting(ApplicationData.CaptureScreenFolder, 10000, FileExtension.png);
            }
        }


        /// <summary> Hold the instance of the application log file.</summary>
        private static LogFile logFile = null;

        /// <summary> The date and time when had gotten the instance of the log file.</summary>
        private static DateTime? fileAcquiredDate = null;

        /// <summary> (Get only) Return log file for the current day.</summary>
        private static LogFile CurrentLogFile{
            get{
                //update if isn't updated
                //update if day doesn't match
                if(ApplicationData.logFile == null || (DateTime.Now.Day != ApplicationData.fileAcquiredDate.Value.Day)){
                    //update file log
                    ApplicationData.UpdateLogFile();
                }

                //return file instance
                return ApplicationData.logFile;
            }
        }

        /// <summary> The format for log file name.</summary>
        private const string logFileFormat = "MM-dd-yyyy";

        /// <summary> The date format for beginning of a line in log file.</summary>
        private const string lineDateFormat = "[MM/dd/yyyy HH:mm:ss]";


        /// <summary> (Get only) Return path to the data folder.</summary>
        internal static string DataFolder{
            get{
                //get the path
                if(Debug.IsDebug){
                    return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "appData");
                }

                //return
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Program.ApplicationName, "appData");
            }
        }

        /// <summary> (Get only) Return path to the log folder.</summary>
        internal static string LogFolder{
            get{
                return ApplicationData.DataFolder + System.IO.Path.DirectorySeparatorChar + "logs";
            }
        }

        /// <summary> (Get only) Return path to the capture screen folder..</summary>
        internal static string CaptureScreenFolder{
            get{
                //hold the path
                string path;
                try{
                    //retrive the path
                    ApplicationData.GetFileData(out path);
                } catch(Exception){
                    //return default path
                    return ApplicationData.DataFolder + System.IO.Path.DirectorySeparatorChar + "captureScreen";
                }
                //return path
                return path;
            }
        }

        /// <summary> (Get only) Return path to the application folder.</summary>
        internal static string AppLocation{
            get{
                return System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);
            }
        }
        

        /// <inheritdoc cref="EchoCapture.Data.ApplicationData.ValidateDataFile"/>
        /// <summary> Initialise the application data manager.</summary>
        public static void Initalise(){
            //has already initialised
            if(ApplicationData.hasInitialise){
                return;
            }

            //validate application data file
            ApplicationData.ValidateDataFile();
            //validate jpeg quality setting
            ApplicationData.ValidateImageQualityConfig();
            //update log file
            ApplicationData.UpdateLogFile();

            //update value
            ApplicationData.hasInitialise = true;
        }

        /// <summary> Update application data file.</summary>
        /// <param name="data"> Struct defining the update in the file.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"></exception>
        public static void UpdateFileData(UpdateData data){
            //will hold the data of the file
            List<Setting> content;
            //try to read file
            if(!ApplicationData.dataFile.ReadFile(out content)){
                throw new ReadingDataFileException();
            }

            //will hold the updated content
            Setting updatedContent = content[0];

            //update data
            if(data.UpdateType == UpdateData.DataType.path){
                updatedContent.SavedPath = data.Path;

                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"Screen captures saving directory has been updated to \"{data.Path}\" in the application setting.");
            }
            if(data.UpdateType == UpdateData.DataType.timeout){
                updatedContent.TimeoutMS = data.Timeout;

                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"The time-interval capturing screen has been updated to {data.Timeout}ms in the application setting.");
            }

            //update variable
            content[0] = updatedContent;

            //try to update file
            if(!ApplicationData.dataFile.OverwriteFile(content)){
                throw new OverwritingDataFileException();
            }
        }

        /// <summary> Retrieve the folder path from the data file.</summary>
        /// <param name="path"> Will hold the retrieved path.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        public static void GetFileData(out string path){
            //default value
            path = null;

            //will hold the data of the file
            List<Setting> content;
            //try to read file
            if(!ApplicationData.dataFile.ReadFile(out content)){
                throw new ReadingDataFileException();
            }

            //update data
            path = content[0].SavedPath;
        }

        /// <summary> Retrieve the time interval value from the data file.</summary>
        /// <param name="interval"> Will hold the time interval value.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        public static void GetFileData(out int? interval){
            //default value
            interval = null;

            //will hold the data of the file
            List<Setting> content;
            //try to read file
            if(!ApplicationData.dataFile.ReadFile(out content)){
                throw new ReadingDataFileException();
            }

            //update data
            interval = content[0].TimeoutMS;
        }

        /// <summary> Retrieve all the data from application data file.</summary>
        /// <param name="path"> Will hold the retrieved path.</param>
        /// <param name="interval"> Will hold the time interval value.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        public static void GetAllFileData(out string path, out int? interval){
            //default value
            path = null;
            interval = null;

            //will hold the data of the file
            List<Setting> content;
            //try to read file
            if(!ApplicationData.dataFile.ReadFile(out content)){
                throw new ReadingDataFileException();
            }

            //update data
            path = content[0].SavedPath;
            interval = content[0].TimeoutMS;
        }
        
        /// <summary> Validate the file holding data for the application.</summary>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"></exception>
        /// <exception cref="EchoCapture.Exceptions.Data.CreatingDataFileException"></exception>
        private static void ValidateDataFile(){
            //create list
            List<Setting> content = new List<Setting>();
            //update list
            content.Add(ApplicationData.DefaultConfig);

            //create instance
            ApplicationData.dataFile = new JsonFile<Setting>("settings", ApplicationData.DataFolder);

            //will hold the file stream
            System.IO.FileStream fs = null;
            
            try{
                //create file if dont exists
                if(ApplicationData.dataFile.CreateFile(out fs)){
                    //failed to overwrite
                    if(!ApplicationData.dataFile.OverwriteFile(fs, content)){
                            //if debug is off
                            if(!Debug.IsDebug){
                                //log
                                System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog($"Failed to set data config, after file creation, at \"{ApplicationData.DataFolder}\".");
                                //notify user failed to create
                                Debug.Error($"Failed to set data config, after file creation, at \"{ApplicationData.DataFolder}\".");
                                Debug.SkipLine();

                                return;
                            }

                            //log and notify user
                            Debug.DebugError($"Failed to set data config, after file creation, at \"{ApplicationData.DataFolder}\".", true);
                        return;
                    }

                    //notify user that data file is created
                    System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"New data config was created at \"{ApplicationData.DataFolder}\".");
                    //if debug is on
                    if(Debug.IsDebug){
                        Console.WriteLine($"New data config was created at \"{ApplicationData.DataFolder}\".");
                        Debug.SkipLine();
                    }
                } else if(ApplicationData.dataFile.FileExists){
                    //hold the output
                    List<Setting> output;

                    //try to read file or if failed to parse object after reading
                    if(!ApplicationData.dataFile.ReadFile(out output) || Setting.IsCorrupted(output[0])){
                        //overwrite
                        if(!ApplicationData.dataFile.OverwriteFile(content)){
                            //throw exception
                            throw new OverwritingDataFileException("Failure of fixing corrupted data file.");
                        }
                        //send error to user that data file was created and default value was restored.
                        Debug.Error("Data config was found to be corrupted. It has been fixed and default values have been restored. Please reconfigure your settings.");
                        Debug.SkipLine();

                        //log msg
                        System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog("Data config was found to be corrupted. It has been fixed and default values have been restored. Please reconfigure your settings.");
                    }
                } else {
                    //throw exception
                    throw new CreatingDataFileException();
                }
            } finally{
                if(fs != null){
                    //free resource
                    fs.Dispose();
                }
            }
        }

        
        /// <summary> Validates the image quality presets and try to repair them.</summary>
        private static void ValidateImageQualityConfig(){
            //create instance
            ApplicationData.imagePresetConfigFile = new IniFile("imageQuality", ApplicationData.DataFolder);

            //will hold the file stream
            System.IO.FileStream fs = null;

            //determine if was performing correction on file
            bool performingCorrection = false;
            //determine if finished performing correction on file
            bool finishedPerformingCorrection = false;
            try{
                //success if file wasn't existant
                if(ApplicationData.imagePresetConfigFile.CreateFile(out fs)){
                    //try to set to a default config
                    try{
                        ApplicationData.ResetImageQualityConfig(ApplicationData.imagePresetConfigFile, fs);
                    } catch (OverwritingDataFileException e){
                        //notify even though debug mode is disable
                        if(!Debug.IsDebug){
                            Debug.Error("Failure of setting up image quality preset config, after file creation.");
                        }
                        //log and error messaged it if debug is enable
                        Debug.DebugError("Failure of setting up image quality preset config, after file creation.");

                        throw new OverwritingDataFileException("Failure of setting up image quality preset config, after file creation.", e);
                    }

                    //notify user that image quality config file is created
                    System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"New image quality preset config has been created at \"{ApplicationData.DataFolder}\".");
                    //if debug is on
                    if(Debug.IsDebug){
                        Console.WriteLine($"New image quality preset config has been created at \"{ApplicationData.DataFolder}\".");
                        Debug.SkipLine();
                    }

                //exsting file
                //check if data is correct
                } else if(ApplicationData.imagePresetConfigFile.FileExists){
                    //on failure to load parsed ini
                    if(!ApplicationData.imagePresetConfigFile.Load()){
                        //try to set to a default config
                        try{
                            ApplicationData.ResetImageQualityConfig(ApplicationData.imagePresetConfigFile, fs);
                        } catch (OverwritingDataFileException e){
                            //notify even though debug mode is disable
                            if(!Debug.IsDebug){
                                Debug.Error("Failure of fixing image quality preset config.");
                            }
                            //log and error messaged it if debug is enable
                            Debug.DebugError("Failure of fixing image quality preset config.");

                            throw new OverwritingDataFileException("Failure of fixing image quality preset config.", e);
                        }
                    }
                    //update state
                    performingCorrection = true;
                    //correct invalid values
                    ApplicationData.CorrectImageQualityConfig();
                    //update state
                    finishedPerformingCorrection = true;
                } else {
                    //throw exception
                    throw new CreatingDataFileException();
                }
            } finally {
                if(fs != null){
                    //free resource
                    fs.Dispose();
                }

                //debug and log for Correction of values in file
                if(performingCorrection && !finishedPerformingCorrection){
                    //notify even though debug mode is disable
                    if(!Debug.IsDebug){
                        Debug.Error("Failure of saving image quality preset config, on fixing invalid values.");
                    }
                    //log and error messaged it if debug is enable
                    Debug.DebugError("Failure of saving image quality preset config, on fixing invalid values.");
                }
            }
        }

        /// <summary> Set the contents of an ini file to a default image quality preset config.</summary>
        /// <remarks> Free resources after use.</remarks>
        /// <param name="iniFile"> The ini file instance to use.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"> Thrown on failure to save default image quality preset config.</exception>
        private static void ResetImageQualityConfig(IniFile iniFile){
            //default to empty ini
            iniFile.Emptify();

            //add help/guidelines
            iniFile.Parsed_ini.AddLineComment("possible pixel formats:", 0);
            iniFile.Parsed_ini.AddLineComment("Format16bppRgb555, Format16bppRgb565, Format24bppRgb, Format32bppArgb, Format32bppPArgb, Format32bppRgb, Format48bppRgb,", 1);
            iniFile.Parsed_ini.AddLineComment("Format64bppArgb, Format64bppPArgb (on this line, images might be transparent instead of the actual screen).", 2);
            iniFile.Parsed_ini.AddEmptyLine(3);
            iniFile.Parsed_ini.AddLineComment("More information on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.pixelformat.", 4);
            iniFile.Parsed_ini.AddEmptyLine(5);
            iniFile.Parsed_ini.AddLineComment("imageQuality ranges from 0 to 100, lower value means more compression.", 6);
            iniFile.Parsed_ini.AddEmptyLine(7);
            iniFile.Parsed_ini.AddLineComment("You're free to modify values.", 8);
            iniFile.Parsed_ini.AddEmptyLine(9);
            //add preset using
            iniFile.Parsed_ini.AddLineComment("The preset to use.", 10);
            iniFile.Parsed_ini.AddValue<string>("selectedPreset", ImageQualityPresetSetting.DefaultChoosenPreset, 11);
            //add image type using
            iniFile.Parsed_ini.AddEmptyLine(12);
            iniFile.Parsed_ini.AddLineComment("The image type to use to save the capture screen", 13);
            iniFile.Parsed_ini.AddValue<string>("imageType", ImageQualityPresetSetting.DefaultImageType, 14, "#Either png or jpeg");
            //add image rescaling
            iniFile.Parsed_ini.AddEmptyLine(15);
            iniFile.Parsed_ini.AddLineComment("Image rescaling, scales the screenshot taken to the following values if enabled.", 16);
            iniFile.Parsed_ini.AddValueAtEnd<bool>("rescaleImage", ImageQualityPresetSetting.DefaultRescaling, "#True or False (case-sensitive)");
            iniFile.Parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0], "#Value must be greater than zero");
            iniFile.Parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1], "#Value must be greater than zero");
            iniFile.Parsed_ini.AddEmptyLineAtEnd();

            //defines presets
            string[] subsecions = new string[3]{"high", "standard", "low"};

            //create subsection for presets
            for (int i = 0; i < subsecions.Length; i++){
                //create subsection and add values
                iniFile.Parsed_ini.CreateSubsection(subsecions[i]);
                iniFile.Parsed_ini.AddValueInSubsectionAtEnd<string>(subsecions[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i], "#Choose from the list above");
                iniFile.Parsed_ini.AddValueInSubsectionAtEnd<int>(subsecions[i], "imageQuality", ImageQualityPresetSetting.DefaultJpegImageQuality[i], "#Only works if jpeg is set to image type, ranges from 0 to 100.");
                iniFile.Parsed_ini.AddEmptyLineAtEnd(subsecions[i]);
            }

            if(!imagePresetConfigFile.Save()){
                //throw exception
                throw new OverwritingDataFileException("Failure of saving default image quality preset config.");
            }
        }

        /// <summary> Set the contents of an ini file to a default image quality preset config.</summary>
        /// <remarks> Free resources after use.</remarks>
        /// <param name="iniFile"> The ini file instance to use.</param>
        /// <param name="fs"> The filestream to use to update the file.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"> Thrown on failure to save default image quality preset config.</exception>
        private static void ResetImageQualityConfig(IniFile iniFile, System.IO.FileStream fs){
            //default to empty ini
            iniFile.Emptify();

            //add help/guidelines
            iniFile.Parsed_ini.AddLineComment("possible pixel formats:", 0);
            iniFile.Parsed_ini.AddLineComment("Format16bppRgb555, Format16bppRgb565, Format24bppRgb, Format32bppArgb, Format32bppPArgb, Format32bppRgb, Format48bppRgb,", 1);
            iniFile.Parsed_ini.AddLineComment("Format64bppArgb, Format64bppPArgb (on this line, images might be transparent instead of the actual screen).", 2);
            iniFile.Parsed_ini.AddEmptyLine(3);
            iniFile.Parsed_ini.AddLineComment("More information on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.pixelformat.", 4);
            iniFile.Parsed_ini.AddEmptyLine(5);
            iniFile.Parsed_ini.AddLineComment("imageQuality ranges from 0 to 100, lower value means more compression.", 6);
            iniFile.Parsed_ini.AddEmptyLine(7);
            iniFile.Parsed_ini.AddLineComment("You're free to modify values.", 8);
            iniFile.Parsed_ini.AddEmptyLine(9);
            //add preset using
            iniFile.Parsed_ini.AddLineComment("The preset to use.", 10);
            iniFile.Parsed_ini.AddValue<string>("selectedPreset", ImageQualityPresetSetting.DefaultChoosenPreset, 11);
            //add image type using
            iniFile.Parsed_ini.AddEmptyLine(12);
            iniFile.Parsed_ini.AddLineComment("The image type to use to save the capture screen", 13);
            iniFile.Parsed_ini.AddValue<string>("imageType", ImageQualityPresetSetting.DefaultImageType, 14, "#Either png or jpeg");
            //add image rescaling
            iniFile.Parsed_ini.AddEmptyLine(15);
            iniFile.Parsed_ini.AddLineComment("Image rescaling, scales the screenshot taken to the following values if enabled.", 16);
            iniFile.Parsed_ini.AddValueAtEnd<bool>("rescaleImage", ImageQualityPresetSetting.DefaultRescaling, "#True or False (case-sensitive)");
            iniFile.Parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0], "#Value must be greater than zero");
            iniFile.Parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1], "#Value must be greater than zero");
            iniFile.Parsed_ini.AddEmptyLineAtEnd();

            //defines presets
            string[] subsecions = new string[3]{"high", "standard", "low"};

            //create subsection for presets
            for (int i = 0; i < subsecions.Length; i++){
                //create subsection and add values
                iniFile.Parsed_ini.CreateSubsection(subsecions[i]);
                iniFile.Parsed_ini.AddValueInSubsectionAtEnd<string>(subsecions[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i], "#Choose from the list above");
                iniFile.Parsed_ini.AddValueInSubsectionAtEnd<int>(subsecions[i], "imageQuality", ImageQualityPresetSetting.DefaultJpegImageQuality[i], "#Only works if jpeg is set to image type, ranges from 0 to 100.");
                iniFile.Parsed_ini.AddEmptyLineAtEnd(subsecions[i]);
            }

            if(!imagePresetConfigFile.Save(fs)){
                //throw exception
                throw new OverwritingDataFileException("Failure of saving default image quality preset config.");
            }
        }

        /// <summary> Correct invalid value(s) in an ini file representing jpeg quality config to default value.</summary>
        /// <remarks> Should use <see cref="EchoCapture.Data.File.Text.IniFile.Load"/> beforehand. Free resources after use.</remarks>
        /// <param name="iniFile"> The ini file instance to use.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.OverwritingDataFileException"> Thrown on failure to save jpeg quality config, on fixing incorrect values</exception>
        private static void CorrectImageQualityConfig(){
            //note: assuming already loaded

            //determine if file was updated
            bool hasUpdatedSomething = false;
            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

            //valid subsections, also presets
            string[] subsections = new string[3]{"high", "standard", "low"};

            //hold the using preset
            string presetValue;
            try{
                //retrive the preset value
                if(parsed_ini.SearchValue<string>("selectedPreset", out presetValue)){
                    //determine if chose preset is valid
                    bool validPresetValue = false;

                    //check if chose preset is valid
                    for (int i = 0; i < subsections.Length; i++){
                        if(presetValue == subsections[i]){
                            validPresetValue = true;
                            break;
                        }
                    }

                    //defaults the value of choosen preset
                    if(!validPresetValue){
                        parsed_ini.SetValue<string>("selectedPreset", ImageQualityPresetSetting.DefaultChoosenPreset);
                        //update state
                        hasUpdatedSomething = true;
                    }
                //value dont exists
                } else {
                    //add value
                    parsed_ini.AddValue<string>("selectedPreset", ImageQualityPresetSetting.DefaultChoosenPreset, 14);
                    //update state
                    hasUpdatedSomething = true;
                }

            //value type is invalid
            } catch(IniLineDataParsingException){
                parsed_ini.SetValueIgnoringType<string>("selectedPreset", ImageQualityPresetSetting.DefaultChoosenPreset);
                //update state
                hasUpdatedSomething = true;
            }

            //hold the image type
            string imageTypeValue;
            try{
                //retrive the image type
                if(parsed_ini.SearchValue<string>("imageType", out imageTypeValue)){
                    //determine if image type is valid
                    bool validImageType = false;

                    //check if chose preset is valid
                    for (int i = 0; i < ImageQualityPresetSetting.ValidImageTypes.Length; i++){
                        if(imageTypeValue == ImageQualityPresetSetting.ValidImageTypes[i]){
                            validImageType = true;
                            break;
                        }
                    }

                    //defaults the image type
                    if(!validImageType){
                        parsed_ini.SetValue<string>("imageType", ImageQualityPresetSetting.DefaultImageType, "#Either png or jpeg");
                        //update state
                        hasUpdatedSomething = true;
                    }
                //value dont exists
                } else {
                    //add value
                    parsed_ini.AddValue<string>("imageType", ImageQualityPresetSetting.DefaultImageType, 14, "#Either png or jpeg");
                    //update state
                    hasUpdatedSomething = true;
                }

            //value type is invalid
            } catch(IniLineDataParsingException){
                parsed_ini.SetValueIgnoringType<string>("imageType", ImageQualityPresetSetting.DefaultImageType, "#Either png or jpeg");
                //update state
                hasUpdatedSomething = true;
            }

            //hold the image rescaling state
            bool imageRescaling;
            try{
                //check if doesn't exists
                if(!parsed_ini.SearchValue<bool>("rescaleImage", out imageRescaling)){
                    //add value
                    parsed_ini.AddValue<bool>("rescaleImage", ImageQualityPresetSetting.DefaultRescaling, 17, "#True or False (case-sensitive)");
                    //update state
                    hasUpdatedSomething = true;
                }

            //value type is invalid
            } catch(IniLineDataParsingException){
                parsed_ini.SetValueIgnoringType<bool>("rescaleImage", ImageQualityPresetSetting.DefaultRescaling, "#True or False (case-sensitive)");
                //update state
                hasUpdatedSomething = true;
            }

            //hold the image rescaling value
            int[] newImageResolution = new int[2];
            try{
                //determine if retrived or not
                bool retrivedWidth;
                bool retrivedHeight;
                
                //retrive the new width dimension and get result
                retrivedWidth = parsed_ini.SearchValue<int>("newWidthDimension", out newImageResolution[0]);

                //retrive the new width dimension and get result
                retrivedHeight = parsed_ini.SearchValue<int>("newHeightDimension", out newImageResolution[1]);

                //retrieved both
                if(retrivedHeight && retrivedWidth){
                    //invalid range
                    if(newImageResolution[0] <= 0f || newImageResolution[1] <= 0f){
                        //update
                        parsed_ini.SetValue<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0], "#Value must be greater than zero");
                        parsed_ini.SetValue<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1], "#Value must be greater than zero");
                        //update state
                        hasUpdatedSomething = true;
                    }

                //failed to retrieve both
                } else if(!retrivedHeight && !retrivedWidth){
                    //update
                    parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0], "#Value must be greater than zero");
                    parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1], "#Value must be greater than zero");
                    //update state
                    hasUpdatedSomething = true;
                } else {
                    //update
                    if(retrivedWidth){
                        parsed_ini.SetValue<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0], "#Value must be greater than zero");
                    } else {
                        parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0], "#Value must be greater than zero");
                    }
                    //update
                    if(retrivedHeight){
                        parsed_ini.SetValue<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1], "#Value must be greater than zero");
                    } else {
                        parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1], "#Value must be greater than zero");
                    }
                    //update state
                    hasUpdatedSomething = true;
                }
            } catch(IniLineDataParsingException){
                parsed_ini.SetValueIgnoringType<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0], "#Value must be greater than zero");
                parsed_ini.SetValueIgnoringType<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1], "#Value must be greater than zero");
                //update state
                hasUpdatedSomething = true;
            }
            
            //check if all subsections exists
            //and fix it
            for (int i = 0; i < subsections.Length; i++){
                //if exists, next subsection
                if(parsed_ini.SubsectionExists(subsections[i])){
                    continue;
                }

                //create subsection along with values
                parsed_ini.CreateSubsection(subsections[i], new string[1]{$"pixelFormat = {ImageQualityPresetSetting.DefaultPixelFormats[i]} #Choose from the list above"});
                //update state
                hasUpdatedSomething = true;
            }

            //loop through subsection to check for correct data type of pixel format
            string pixelFormatValue;
            for (int i = 0; i < subsections.Length; i++){
                //exception on invalid value type
                try{
                    //exists, correct value type but might be invalid value
                    if(parsed_ini.SearchValue<string>(subsections[i], "pixelFormat", out pixelFormatValue)){
                        //invalid pixel format
                        if(!ImageQualityPresetSetting.ValidPixelFormats.Contains(pixelFormatValue)){
                            parsed_ini.SetValueIgnoringType<string>(subsections[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i], "#Choose from the list above");
                            //update state
                            hasUpdatedSomething = true;
                        }

                    //non-existant
                    } else {
                        parsed_ini.AddValueInSubsectionAtEnd<string>(subsections[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i], "#Choose from the list above");
                        //update state
                        hasUpdatedSomething = true;
                    }
                } catch (IniLineDataParsingException){
                    parsed_ini.SetValueIgnoringType<string>(subsections[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i], "#Choose from the list above");
                    //update state
                    hasUpdatedSomething = true;
                }
            }

            //loop through subsection to check for correct data type of image quality
            int imageQuality;
            for (int i = 0; i < subsections.Length; i++){
                //exception on invalid value type
                try{
                    //exists, correct value type but might be invalid value
                    if(parsed_ini.SearchValue<int>(subsections[i], "imageQuality", out imageQuality)){
                        //invalid pixel format
                        if(!ImageQualityPresetSetting.ValidJpegImageQuality.Contains(imageQuality)){
                            parsed_ini.SetValueIgnoringType<int>(subsections[i], "imageQuality", ImageQualityPresetSetting.DefaultJpegImageQuality[i], "#Only works if jpeg is set to image type, ranges from 0 to 100.");
                            //update state
                            hasUpdatedSomething = true;
                        }

                    //non-existant
                    } else {
                        parsed_ini.AddValueInSubsectionAtEnd<int>(subsections[i], "imageQuality", ImageQualityPresetSetting.DefaultJpegImageQuality[i], "#Only works if jpeg is set to image type, ranges from 0 to 100.");
                        //update state
                        hasUpdatedSomething = true;
                    }
                } catch (IniLineDataParsingException){
                    parsed_ini.SetValueIgnoringType<int>(subsections[i], "imageQuality", ImageQualityPresetSetting.DefaultJpegImageQuality[i], "#Only works if jpeg is set to image type, ranges from 0 to 100.");
                    //update state
                    hasUpdatedSomething = true;
                }
            }

            if(!ApplicationData.imagePresetConfigFile.Save()){
                //throw exception
                throw new OverwritingDataFileException("Failure of saving image quality preset config, on fixing invalid values.");
            }

            //log
            if(hasUpdatedSomething){
                Debug.DebugWarning("Some value(s) of the Image Quality Preset Config, has been set to its default value as previous values was invalid.");
            }
        }

        /// <summary> Reloads the object representing the ini file.</summary>
        public static void RefreshImageQualityConfig(){
            //reload
            //on failed to reload
            if(!ApplicationData.imagePresetConfigFile.Load()){
                //try to set to a default config
                try{
                    ApplicationData.ResetImageQualityConfig(ApplicationData.imagePresetConfigFile);
                } catch (OverwritingDataFileException e){
                    //notify even though debug mode is disable
                    if(!Debug.IsDebug){
                        Debug.Error("Failure of fixing image quality preset config.");
                    }
                    //log and error messaged it if debug is enable
                    Debug.DebugError("Failure of fixing image quality preset config.");

                    throw new OverwritingDataFileException("Failure of fixing image quality preset config.", e);
                }
            }
            
            try{
                //correct invalid values
                ApplicationData.CorrectImageQualityConfig();
            } catch (OverwritingDataFileException){
                //notify even though debug mode is disable
                if(!Debug.IsDebug){
                    Debug.Error("Failure of saving image quality preset config, on fixing invalid values during refreshing.");
                }
                //log and error messaged it if debug is enable
                Debug.DebugError("Failure of saving image quality preset config, on fixing invalid values during refreshing.");
            }
        }

        /// <summary> Reads the ini file containing image quality preset config, and return it in an object representing image quality preset config.</summary>
        public static ImageQualityPresetSetting GetImageQualityData() => new ImageQualityPresetSetting(ApplicationData.imagePresetConfigFile);

        /// <summary> Return the current using preset.</summary>
        public static string GetCurrentSelectedPreset(){
            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

            //will hold the preset name
            string presetName;

            //retrives the current preset
            parsed_ini.SearchValue<string>("selectedPreset", out presetName);

            //return
            return presetName;
        }

        /// <summary> Get data in all presets and return them in dictionary and key as the preset name.</summary>
        public static Dictionary<string, ImageQualityPresetSetting> GetPresetsData(){
            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;
            //list of presets data
            Dictionary<string, ImageQualityPresetSetting> presets = new Dictionary<string, ImageQualityPresetSetting>();

            //valid subsections, also presets
            string[] subsections = new string[3]{"high", "standard", "low"};
            //loop through them
            foreach(string preset in subsections){
                //holds the pixel for and jpeg image quality
                string pixelFormat;
                int jpegImageQuality;

                //retrieves data for the preset
                parsed_ini.SearchValue<string>(preset, "pixelFormat", out pixelFormat);
                parsed_ini.SearchValue<int>(preset, "imageQuality", out jpegImageQuality);

                //add to list
                presets.Add(preset, new ImageQualityPresetSetting(pixelFormat, jpegImageQuality, "png"));
            }

            //return presets
            return presets;
        }


        /// <summary> Update the rescaling resolution in the image quality preset.</summary>
        /// <param name="qualitySetting"> The object representing the new setting to be updated with.</param>
        public static bool UpdateImageQualityRescalingResolution(ImageQualityPresetSetting qualitySetting){
            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

            //update rescaling resolution
            parsed_ini.SetValue<int>("newWidthDimension", qualitySetting.RescalingResolution[0]);
            parsed_ini.SetValue<int>("newHeightDimension", qualitySetting.RescalingResolution[1]);

            //save value
            if(ApplicationData.imagePresetConfigFile.Save()){
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"The image-rescaling resolution has been set to {qualitySetting.RescalingResolution[0]}x{qualitySetting.RescalingResolution[1]} in the image quality setting.");
                return true;
            }

            //update log
            System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog($"Failed to save the image-rescaling resolution in the image quality setting.");
            return false;
        }

        /// <summary> Update the pixel format in the current chosen preset.</summary>
        /// <param name="qualitySetting"> The structure containing the new pixel format.</param>
        public static bool UpdateImageQualityPixelFormat(ImageQualityPresetSetting qualitySetting){
            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;
            //hold the selected preset value
            string selectedPreset;
            
            //retrieve the value
            parsed_ini.SearchValue<string>("selectedPreset", out selectedPreset);

            //update pixel format
            parsed_ini.SetValue<string>(selectedPreset, "pixelFormat", qualitySetting._PixelFormat.ToString(), false);

            //save value
            if(ApplicationData.imagePresetConfigFile.Save()){
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"The pixel format has been set to {qualitySetting._PixelFormat.ToString()} for the \"{selectedPreset}\" preset in the image quality setting.");
                return true;
            }

            //update log
            System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog($"Failed to save the pixel format for the \"{selectedPreset}\" preset in the image quality setting.");
            return false;
        }

        /// <summary> Update the jpeg image quality in current chosen preset.</summary>
        /// <param name="qualitySetting"> The structure containing the new jpeg quality.</param>
        public static bool UpdateImageQualityJpgQuality(ImageQualityPresetSetting qualitySetting){
            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;
            //hold the selected preset value
            string selectedPreset;
            
            //retrieve the value
            parsed_ini.SearchValue<string>("selectedPreset", out selectedPreset);

            //update jpg quality
            parsed_ini.SetValue<int>(selectedPreset, "imageQuality", qualitySetting.JpegImageQuality, false);

            //save value
            if(ApplicationData.imagePresetConfigFile.Save()){
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"The jpeg image quality has been set to {qualitySetting.JpegImageQuality}% for the \"{selectedPreset}\" preset in the image quality setting.");
                return true;
            }

            //update log
            System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog($"Failed to save the jpeg image quality for the \"{selectedPreset}\" preset in the image quality setting.");
            return false;
        }

        /// <summary> Update the preset chosen in the image quality file.</summary>
        /// <remarks> <paramref name="presetName"/> can only be "high", "standard" or "low".</remarks>
        /// <param name="presetName"> The name of the preset.</param>
        /// <exception cref="System.ArgumentException"> Thrown when <paramref name="presetName"/> is not valid.</exception>
        public static bool UpdateImageQualityPreset(string presetName){
            //valid subsections, also presets
            string[] subsections = new string[3]{"high", "standard", "low"};

            //invaid preset
            if(!subsections.Contains(presetName)){
                throw new ArgumentException("Preset passed is not from the valid list of presets.", "presetName");
            }

            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

            //update preset chose
            parsed_ini.SetValue<string>("selectedPreset", presetName);

            //save value
            if(ApplicationData.imagePresetConfigFile.Save()){
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"\"{presetName}\" has been selected as the current using preset in the image quality setting.");
                return true;
            }

            //update log
            System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog($"Failed to save \"{presetName}\" as the current using preset in the image quality setting.");
            return false;
        }

        /// <summary> Update the image type in the image quality preset.</summary>
        /// <remarks> <paramref name="imageType"/> can only be "png" or "jpeg".</remarks>
        /// <param name="imageType"> The image type to use.</param>
        /// <exception cref="System.ArgumentException"> Thrown when <paramref name="imageType"/> is not valid.</exception>
        public static bool UpdateImageQualityFileType(string imageType){
            //valid extension
            string[] subsections = new string[2]{"png", "jpeg"};

            //invaid preset
            if(!subsections.Contains(imageType)){
                throw new ArgumentException("Image type passed is not from the valid list of image types.", "imageType");
            }

            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

            //update preset chose
            parsed_ini.SetValue<string>("imageType", imageType);

            //save value
            if(ApplicationData.imagePresetConfigFile.Save()){
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"The image type to save capture screen has been set to \"{imageType}\" in the image quality setting.");
                return true;
            }

            //update log
            System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog($"Failed to save the image type of \"{imageType}\" in the image quality setting.");
            return false;
        }
        
        /// <summary> Update the image rescaling state in the image quality preset.</summary>
        public static bool UpdateImageQualityRescaling(bool state){
            //reference to the parsed ini
            IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

            //update rescale state
            parsed_ini.SetValue<bool>("rescaleImage", state);

            //save value
            if(ApplicationData.imagePresetConfigFile.Save()){
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"Image rescaling has been set to {state.ToString().ToLower()} in the image quality setting.");
                return true;
            }
            //update log
            System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog($"Failed to set image rescaling to {state.ToString().ToLower()} in the image quality setting.");
            return false;
        }


        /// <summary> Update the log with <paramref name="text"/>.</summary>
        public static async System.Threading.Tasks.Task UpdateLog(string text){
            //get current log file
            LogFile file = ApplicationData.CurrentLogFile;

            //update text
            text = $"{DateTime.Now.ToString(ApplicationData.lineDateFormat)} {text}\n";

            //update log
            await file.AddUpFileAsync(text);
        }

        /// <summary> Get log file, for the current date.</summary>
        private static void UpdateLogFile(){
            //get date time struct
            DateTime acquiredDate = DateTime.Now;
            //get the log file name
            string fileName = acquiredDate.ToString(ApplicationData.logFileFormat);

            //create instance
            LogFile file = new LogFile(fileName, ApplicationData.LogFolder);

            //check if exists
            if(!file.FileExists){
                //if failed to create
                if(!file.CreateFile()){
                    Debug.DebugError($"Failed to create log file for {fileName}.");
                }
            }

            //update reference
            ApplicationData.fileAcquiredDate = acquiredDate;
            ApplicationData.logFile = file;
        }

        /// <summary> Object used to determine application data contents format.</summary>
        private struct Setting{

            /// <summary> Hold the value of the path.</summary>
            private string savedPath;

            /// <summary> Hold the value of the time interval.</summary>
            private int? timeoutMS;

            public Setting(string savedPath, int? timeoutMS, FileExtension imageExtension){
                this.savedPath = savedPath;
                this.timeoutMS = timeoutMS;
            }

            /// <summary> The path where capture screen will be saved.</summary>
            public string SavedPath{
                get{
                    return this.savedPath;
                }
                set{
                    this.savedPath = value;
                }
            }

            /// <summary> The time-interval between screenshots in miliseconds.</summary>
            public int? TimeoutMS{
                get{
                    return this.timeoutMS;
                }
                set{
                    this.timeoutMS = value;
                }
            }

            /// <summary> Check if the instance, is corrupted.</summary>
            public static bool IsCorrupted(Setting instance){
                //check if one is null
                if(instance.savedPath == null || instance.timeoutMS == null){
                    return true;
                }

                //valid
                return false;
            }
        }

        /// <summary> Object representing an image quality preset setting.</summary>
        public struct ImageQualityPresetSetting{
            
            /// <summary> Default pixel formats set in an image quality preset config.</summary>
            /// <remarks> [0] is for high, [1] is for standard, [2] is for low.</remarks>
            public readonly static string[] DefaultPixelFormats = new string[3]{"Format32bppRgb", "Format32bppRgb", "Format24bppRgb"};

            /// <summary> Default image quality applicable only when jpeg format is set.</summary>
            /// <remarks> [0] is for high, [1] is for standard, [2] is for low.</remarks>
            public readonly static int[] DefaultJpegImageQuality = new int[3]{100, 90, 80};

            /// <summary> The default image type to use for saving image in an image quality preset config.</summary>
            public const string DefaultImageType = "png";

            /// <summary> The default image rescaling value in an image quality preset config.</summary>
            public const bool DefaultRescaling = false;

            /// <summary> The default rescaling resolution of a screenshot.</summary>
            public readonly static int[] DefaultRescalingResolution = new int[2]{1920, 1080};

            /// <summary> The default preset that the application will use.</summary>
            public const string DefaultChoosenPreset = "standard";

            
            /// <summary> String array holding valid pixel formats.</summary>
            private static string[] validPixelFormats = null;

            /// <summary> String array holding valid image type for saving image.</summary>
            private static string[] validImageTypes = null;

            /// <summary> (Get only) Return string array holding valid pixel formats.</summary>
            public static string[] ValidPixelFormats{
                get{
                    //update
                    if(validPixelFormats == null){
                        ImageQualityPresetSetting.validPixelFormats = new string[6]{"Format16bppRgb555", "Format16bppRgb565", "Format24bppRgb", "Format32bppArgb",
                        "Format32bppPArgb", "Format32bppRgb"};
                    }

                    return ImageQualityPresetSetting.validPixelFormats;
                }
            }
            
            /// <summary> (Get only) Return string array holding valid image type for saving image.</summary>
            public static string[] ValidImageTypes{
                get{
                    //update
                    if(validImageTypes == null){
                        ImageQualityPresetSetting.validImageTypes = new string[2]{"png", "jpeg"};
                    }

                    return ImageQualityPresetSetting.validImageTypes;
                }
            }

            /// <summary> The valid ranges for jpeg image quality.</summary>
            public readonly static IEnumerable<int> ValidJpegImageQuality = Enumerable.Range(0, 101);


            /// <summary> The pixel format that the preset is set for.</summary>
            private System.Drawing.Imaging.PixelFormat pixelFormat;

            /// <summary> Holds the the image type set in the image quality preset config.</summary>
            private FileExtension imageType;

            /// <summary> Determine if image rescaling is enable.</summary>
            private bool enabledRescaling;

            /// <summary> The resolution to rescale the image with.</summary>
            private int[] rescalingResolution;

            /// <summary> The value determine the image quality applicable only to jpeg.</summary>
            private int jpegImageQuality;


            /// <summary> (Get only) Returns the pixel format that the preset if set for.</summary>
            public System.Drawing.Imaging.PixelFormat _PixelFormat{
                get{
                    return this.pixelFormat;
                }
            }

            /// <summary> (Get only) Returns the image type set in the image quality preset config.</summary>
            public FileExtension ImageType{
                get{
                    return this.imageType;
                }
            }

            /// <summary> (Get only) Determine if image rescaling is enable.</summary>
            public bool EnabledRescaling{
                get{
                    return this.enabledRescaling;
                }
            }

            /// <summary> (Get only) Return the resolution to rescale the image with.</summary>
            public int[] RescalingResolution{
                get{
                    return this.rescalingResolution;
                }
            }

            /// <summary> (Get only) Return the value determine the image quality applicable only to jpeg.</summary>
            public int JpegImageQuality{
                get{
                    return this.jpegImageQuality;
                }
            }


            /// <summary> Creates instance with image rescaling enable.</summary>
            /// <param name="pixelFormat"> The name of the in use pixel format, case-sensitive.</param>
            /// <param name="jpegQuality"> The image quality applicable only to jpeg.</param>
            /// <param name="imageType"> The image type set.</param>
            /// <param name="newWidth"> The new width of the image dimension.</param>
            /// <param name="newHeight"> The new height of the image dimension.</param>
            /// <exception cref="System.ArgumentException"> Thrown when pixelFormat is not from list of valid pixel formats, failed
            /// to parse pixelFormat into enum, imageType is not from list of valid image types or failed to parse imageType into enum</exception>
            /// <exception cref="System.ArgumentOutOfRangeException"> The value of qualityLevel was out of range of the valid values for quality.</exception>
            public ImageQualityPresetSetting(string pixelFormat, int jpegQuality, string imageType, int newWidth, int newHeight){
                //check for list of pixel format
                if(!ImageQualityPresetSetting.ValidPixelFormats.Contains(pixelFormat)){
                    throw new ArgumentException("Argument 1 passed is not from the list of valid pixel formats.", "pixelFormat");
                }

                //check for list of image types
                if(!ImageQualityPresetSetting.ValidImageTypes.Contains(imageType)){
                    throw new ArgumentException("Argument 3 passed is not from the list of valid image types.", "imageType");
                }

                //check for jpeg image quality range
                if(!ImageQualityPresetSetting.ValidJpegImageQuality.Contains(jpegQuality)){
                    throw new ArgumentException("Argument 2 passed is not in the range of valid jpeg image quality values.", "jpegQuality");
                }

                //check for range of new dimension
                if(newWidth <= 0){
                    throw new ArgumentOutOfRangeException("newWidth", "Argument 4 passed is invalid. Zero and negative values isn't valid.");
                }

                //check for range of new dimension
                if(newHeight <= 0){
                    throw new ArgumentOutOfRangeException("newHeight", "Argument 5 passed is invalid. Zero and negative values isn't valid.");
                }

                //parse value to enum and update field
                if(!Enum.TryParse<System.Drawing.Imaging.PixelFormat>(pixelFormat, false, out this.pixelFormat)){
                    throw new ArgumentException("Argument 1 passed has failed to be parsed into an enum.", "pixelFormat");
                }

                //parse value to enum and update field
                if(!Enum.TryParse<FileExtension>(imageType, false, out this.imageType)){
                    throw new ArgumentException("Argument 2 passed has failed to be parsed into an enum.", "imageType");
                }

                //update values
                this.jpegImageQuality = jpegQuality;
                this.enabledRescaling = true;
                this.rescalingResolution = new int[2]{newWidth, newHeight};
            }

            /// <summary> Creates instance with image rescaling disable.</summary>
            /// <param name="pixelFormat"> The name of the in use pixel format, case-sensitive.</param>
            /// <param name="jpegQuality"> The image quality applicable only to jpeg.</param>
            /// <param name="imageType"> The image type set.</param>
            /// <exception cref="System.ArgumentException"> Thrown when pixelFormat is not from list of valid pixel formats, failed
            /// to parse pixelFormat into enum, imageType is not from list of valid image types or failed to parse imageType into enum</exception>
            public ImageQualityPresetSetting(string pixelFormat, int jpegQuality, string imageType){
                //check for list of pixel format
                if(!ImageQualityPresetSetting.ValidPixelFormats.Contains(pixelFormat)){
                    throw new ArgumentException("Argument 1 passed is not from the list of valid pixel formats.", "pixelFormat");
                }

                //check for jpeg image quality range
                if(!ImageQualityPresetSetting.ValidJpegImageQuality.Contains(jpegQuality)){
                    throw new ArgumentException("Argument 2 passed is not in the range of valid jpeg image quality values.", "jpegQuality");
                }

                //check for list of image types
                if(!ImageQualityPresetSetting.ValidImageTypes.Contains(imageType)){
                    throw new ArgumentException("Argument 3 passed is not from the list of valid image types.", "imageType");
                }

                //parse value to enum and update field
                if(!Enum.TryParse<System.Drawing.Imaging.PixelFormat>(pixelFormat, false, out this.pixelFormat)){
                    throw new ArgumentException("Argument 1 passed has failed to be parsed into an enum.", "pixelFormat");
                }

                //parse value to enum and update field
                if(!Enum.TryParse<FileExtension>(imageType, false, out this.imageType)){
                    throw new ArgumentException("Argument 3 passed has failed to be parsed into an enum.", "imageType");
                }

                //update values
                this.jpegImageQuality = jpegQuality;
                this.enabledRescaling = true;
                this.rescalingResolution = null;
            }

            /// <summary> Creates instance from an ini file.</summary>
            /// <param name="iniFile"> The ini file to search values from.</param>
            /// <exception cref="System.ArgumentException"> Thrown when pixelFormat is not from list of valid pixel formats, failed
            /// to parse pixelFormat into enum, imageType is not from list of valid image types or failed to parse imageType into enum</exception>
            /// <exception cref="System.ArgumentOutOfRangeException"> The new image dimension was out of range of the valid dimension.</exception>
            public ImageQualityPresetSetting(IniFile iniFile){
                //get parsed ini
                IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

                //hold retrived values from file
                string presetChose;
                string imageType;
                bool enabledRescaling;
                int[] newResolution = new int[2];
                string pixelFormatValue;
                int jpegQuality;

                //search values
                parsed_ini.SearchValue<string>("selectedPreset", out presetChose);
                parsed_ini.SearchValue<string>("imageType", out imageType);
                parsed_ini.SearchValue<bool>("rescaleImage", out enabledRescaling);
                parsed_ini.SearchValue<int>("newWidthDimension", out newResolution[0]);
                parsed_ini.SearchValue<int>("newHeightDimension", out newResolution[1]);
                parsed_ini.SearchValue<string>(presetChose, "pixelFormat", out pixelFormatValue);
                parsed_ini.SearchValue<int>(presetChose, "imageQuality", out jpegQuality);

                //update states
                this.enabledRescaling = enabledRescaling;
                this.rescalingResolution = newResolution;
                this.jpegImageQuality = jpegQuality;

                //parse value to enum and update field
                if(!Enum.TryParse<System.Drawing.Imaging.PixelFormat>(pixelFormatValue, false, out this.pixelFormat)){
                    throw new ArgumentException("Failed to parse pixel format into an enum, pixel format read from the instance of argument 1.", "iniFile");
                }

                //parse value to enum and update field
                if(!Enum.TryParse<FileExtension>(imageType, false, out this.imageType)){
                    throw new ArgumentException("Failed to parse image type into an enum, image type read from the instance of argument 1.", "iniFile");
                }

                //check for jpeg image quality range
                if(!ImageQualityPresetSetting.ValidJpegImageQuality.Contains(jpegQuality)){
                    throw new ArgumentException("Image quality for jpeg format, is invalid, read from the instance of argument 1.", "iniFile");
                }

                if(enabledRescaling){
                    //check for range of new dimension
                    if(newResolution[0] <= 0){
                        throw new ArgumentOutOfRangeException("Width dimension is invalid, read from the instance of argument 1. Zero and negative values isn't valid.", "iniFile");
                    }

                    //check for range of new dimension
                    if(newResolution[1] <= 0){
                        throw new ArgumentOutOfRangeException("Height dimension is invalid, read from the instance of argument 1. Zero and negative values isn't valid.", "iniFile");
                    }

                    this.rescalingResolution = newResolution;
                }
            }
        }
    }
}