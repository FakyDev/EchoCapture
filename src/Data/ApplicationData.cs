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
                return new Setting(ApplicationData.DataFolder, 10000, FileExtension.png);
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

        /// <summary> (Get only) Return path to the image quality config folder.</summary>
        internal static string ImageConfigFolder{
            get{
                return ApplicationData.DataFolder + System.IO.Path.DirectorySeparatorChar + "imageConfig";
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
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"Storage location has been updated to \"{data.Path}\" in the application data folder.");
            }
            if(data.UpdateType == UpdateData.DataType.timeout){
                updatedContent.TimeoutMS = data.Timeout;

                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"Time-interval has been updated to {data.Timeout}ms in the application data folder.");
            }
            if(data.UpdateType == UpdateData.DataType.imageFileExtension){
                updatedContent.SetImageExtension((FileExtension)data.ImageExtension);

                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"Image format has been updated to {data.ImageExtension.ToString().ToUpper()} in the application data folder.");
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

        /// <summary> Retrieve the image extension from the data file.</summary>
        /// <param name="interval"> Will hold the time interval value.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        public static void GetFileData(out FileExtension? imageExtension){
            //default value
            imageExtension = null;

            //will hold the data of the file
            List<Setting> content;
            //try to read file
            if(!ApplicationData.dataFile.ReadFile(out content)){
                throw new ReadingDataFileException();
            }

            //update data
            imageExtension = content[0].GetImageExtension();
        }

        /// <summary> Retrieve all the data from application data file.</summary>
        /// <param name="path"> Will hold the retrieved path.</param>
        /// <param name="interval"> Will hold the time interval value.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        public static void GetAllFileData(out string path, out int? interval, out FileExtension? imageExtension){
            //default value
            path = null;
            interval = null;
            imageExtension = null;

            //will hold the data of the file
            List<Setting> content;
            //try to read file
            if(!ApplicationData.dataFile.ReadFile(out content)){
                throw new ReadingDataFileException();
            }

            //update data
            path = content[0].SavedPath;
            interval = content[0].TimeoutMS;
            imageExtension = content[0].GetImageExtension();
        }
        
        /// <summary> Validate the file(s) holding data for the application.</summary>
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
            ApplicationData.imagePresetConfigFile = new IniFile("imageQuality", ApplicationData.ImageConfigFolder);

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
                    System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog("Image quality preset config has been created.");
                    //if debug is on
                    if(Debug.IsDebug){
                        Console.WriteLine("Image quality preset config has been created.");
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
            //add image rescaling
            iniFile.Parsed_ini.AddEmptyLine(12);
            iniFile.Parsed_ini.AddLineComment("Image rescaling, scales the screenshot taken to the following values if enabled.", 13);
            iniFile.Parsed_ini.AddValueAtEnd<bool>("rescaleImage", ImageQualityPresetSetting.DefaultRescaling, "True or False (case-sensitive)");
            iniFile.Parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0]);
            iniFile.Parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1]);
            iniFile.Parsed_ini.AddEmptyLineAtEnd();

            //defines presets
            string[] subsecions = new string[3]{"high", "standard", "low"};

            //create subsection for presets
            for (int i = 0; i < subsecions.Length; i++){
                //create subsection and add values
                iniFile.Parsed_ini.CreateSubsection(subsecions[i]);
                iniFile.Parsed_ini.AddValueInSubsectionAtEnd<string>(subsecions[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i]);
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
            //add image rescaling
            iniFile.Parsed_ini.AddEmptyLine(12);
            iniFile.Parsed_ini.AddLineComment("Image rescaling, scales the screenshot taken to the following values if enabled.", 13);
            iniFile.Parsed_ini.AddValueAtEnd<bool>("rescaleImage", ImageQualityPresetSetting.DefaultRescaling, "#True or False (case-sensitive)");
            iniFile.Parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0]);
            iniFile.Parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1]);
            iniFile.Parsed_ini.AddEmptyLineAtEnd();

            //defines presets
            string[] subsecions = new string[3]{"high", "standard", "low"};

            //create subsection for presets
            for (int i = 0; i < subsecions.Length; i++){
                //create subsection and add values
                iniFile.Parsed_ini.CreateSubsection(subsecions[i]);
                iniFile.Parsed_ini.AddValueInSubsectionAtEnd<string>(subsecions[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i]);
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
                    parsed_ini.AddValue<string>("selectedPreset", ImageQualityPresetSetting.DefaultChoosenPreset, 11);
                    //update state
                    hasUpdatedSomething = true;
                }

            //value type is invalid
            } catch(IniLineDataParsingException){
                parsed_ini.SetValueIgnoringType<string>("selectedPreset", ImageQualityPresetSetting.DefaultChoosenPreset);
                //update state
                hasUpdatedSomething = true;
            }

            //hold the image rescaling state
            bool imageRescaling;
            try{
                //check if doesn't exists
                if(!parsed_ini.SearchValue<bool>("rescaleImage", out imageRescaling)){
                    //add value
                    parsed_ini.AddValue<bool>("rescaleImage", ImageQualityPresetSetting.DefaultRescaling, 14, "#True or False (case-sensitive)");
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
                        parsed_ini.SetValue<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0]);
                        parsed_ini.SetValue<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1]);
                        //update state
                        hasUpdatedSomething = true;
                    }

                //failed to retrieve both
                } else if(!retrivedHeight && !retrivedWidth){
                    //update
                    parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0]);
                    parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1]);
                    //update state
                    hasUpdatedSomething = true;
                } else {
                    //update
                    if(retrivedWidth){
                        parsed_ini.SetValue<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0]);
                    } else {
                        parsed_ini.AddValueAtEnd<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0]);
                    }
                    //update
                    if(retrivedHeight){
                        parsed_ini.SetValue<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1]);
                    } else {
                        parsed_ini.AddValueAtEnd<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1]);
                    }
                    //update state
                    hasUpdatedSomething = true;
                }
            } catch(IniLineDataParsingException){
                parsed_ini.SetValueIgnoringType<int>("newWidthDimension", ImageQualityPresetSetting.DefaultRescalingResolution[0]);
                parsed_ini.SetValueIgnoringType<int>("newHeightDimension", ImageQualityPresetSetting.DefaultRescalingResolution[1]);
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
                parsed_ini.CreateSubsection(subsections[i], new string[1]{$"pixelFormat = {ImageQualityPresetSetting.DefaultPixelFormats[i]}"});
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
                        if(!ImageQualityPresetSetting.ValidDefaultPixelFormat.Contains(pixelFormatValue)){
                            parsed_ini.SetValueIgnoringType<string>(subsections[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i], false);
                            //update state
                            hasUpdatedSomething = true;
                        }

                    //non-existant
                    } else {
                        parsed_ini.AddValueInSubsectionAtEnd<string>(subsections[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i]);
                        //update state
                        hasUpdatedSomething = true;
                    }
                } catch (IniLineDataParsingException){
                    parsed_ini.SetValueIgnoringType<string>(subsections[i], "pixelFormat", ImageQualityPresetSetting.DefaultPixelFormats[i], false);
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

        /// <summary> Reads the ini file containing jpeg quality config, and return it in an object representing jpeg quality setting.</summary>
        public static ImageQualityPresetSetting GetImageQualityData() => new ImageQualityPresetSetting(ApplicationData.imagePresetConfigFile);


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

            /// <summary> Hold the file extension of the capture screen.</summary>
            private FileExtension? imageExtension;

            public Setting(string savedPath, int? timeoutMS, FileExtension imageExtension){
                this.savedPath = savedPath;
                this.timeoutMS = timeoutMS;
                this.imageExtension = imageExtension;
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

            /// <summary> (Get and partially set) The image file extension using. It also defines the image format.</summary>
            public string ImageExtension{
                get{
                    return this.imageExtension.ToString().ToLower();
                }
                set{
                    //try to update
                    try{
                        this.imageExtension = (FileExtension)Enum.Parse(typeof(FileExtension), value);
                    } catch (Exception){}
                }
            }

            /// <summary> Update the image file extension.</summary>
            /// <exception cref="System.ArgumentException"> Thrown when file extension is invalid.</exception>
            public void SetImageExtension(FileExtension imageExtension){
                //check if invalid
                if(imageExtension != FileExtension.png && imageExtension != FileExtension.jpg){
                    throw new ArgumentException("File extension passed is invalid.");
                }

                //update
                this.imageExtension = imageExtension;
            }

            /// <summary> Return the image file extension.</summary>
            public FileExtension? GetImageExtension(){
                return this.imageExtension;
            }


            /// <summary> Check if the instance, is corrupted.</summary>
            public static bool IsCorrupted(Setting instance){
                //check if one is null
                if(instance.savedPath == null || instance.timeoutMS == null || instance.imageExtension == null){
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
            public readonly static string[] DefaultPixelFormats = new string[3]{"Format48bppRgb", "Format32bppRgb", "Format24bppRgb"};

            /// <summary> The default image rescaling value in an image quality preset config.</summary>
            public const bool DefaultRescaling = false;

            /// <summary> The default rescaling resolution of a screenshot.</summary>
            public readonly static int[] DefaultRescalingResolution = new int[2]{1920, 1080};

            /// <summary> The default preset that the application will use.</summary>
            public const string DefaultChoosenPreset = "standard";

            /// <summary> String array holding valid pixel formats.</summary>
            private static string[] validDefaultPixelFormat = null;

            /// <summary> (Get only) Return string array holding valid pixel formats.</summary>
            public static string[] ValidDefaultPixelFormat{
                get{
                    //update
                    if(validDefaultPixelFormat == null){
                        ImageQualityPresetSetting.validDefaultPixelFormat = new string[9]{"Format16bppRgb555", "Format16bppRgb565", "Format24bppRgb", "Format32bppArgb",
                        "Format32bppPArgb", "Format32bppRgb", "Format48bppRgb", "Format64bppArgb", "Format64bppPArgb"};
                    }

                    return ImageQualityPresetSetting.validDefaultPixelFormat;
                }
            }
            

            /// <summary> The pixel format that the preset is set for.</summary>
            private System.Drawing.Imaging.PixelFormat pixelFormat;

            /// <summary> Determine if image rescaling is enable.</summary>
            private bool enabledRescaling;

            /// <summary> The resolution to rescale the image with.</summary>
            private int[] rescalingResolution;

            /// <summary> (Get only) Returns the pixel format that the preset if set for.</summary>
            public System.Drawing.Imaging.PixelFormat _PixelFormat{
                get{
                    return this.pixelFormat;
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

            /// <summary> Creates instance with image rescaling enable.</summary>
            /// <param name="pixelFormat"> The name of the in use pixel format, case-sensitive.</param>
            /// <param name="newWidth"> The new width of the image dimension.</param>
            /// <param name="newHeight"> The new height of the image dimension.</param>
            /// <exception cref="System.ArgumentException"> Thrown when pixelFormat is not from list of valid pixel formats or failed to parse pixelFormat into enum.</exception>
            /// <exception cref="System.ArgumentOutOfRangeException"> The value of qualityLevel was out of range of the valid values for quality.</exception>
            private ImageQualityPresetSetting(string pixelFormat, int newWidth, int newHeight){
                //check for list of pixel format
                if(!ImageQualityPresetSetting.ValidDefaultPixelFormat.Contains(pixelFormat)){
                    throw new ArgumentException("Argument 1 passed is not from the list of valid pixel formats.", "pixelFormat");
                }

                //check for range of new dimension
                if(newWidth <= 0){
                    throw new ArgumentOutOfRangeException("Argument 3 passed is invalid. Zero and negative values isn't valid.", "newWidth");
                }

                //check for range of new dimension
                if(newHeight <= 0){
                    throw new ArgumentOutOfRangeException("Argument 3 passed is invalid. Zero and negative values isn't valid.", "newHeight");
                }

                //parse value to enum and update field
                if(!Enum.TryParse<System.Drawing.Imaging.PixelFormat>(pixelFormat, false, out this.pixelFormat)){
                    throw new ArgumentException("Argument 1 passed has failed to be parsed into an enum.", "pixelFormat");
                }

                //update values
                this.enabledRescaling = true;
                this.rescalingResolution = new int[2]{newWidth, newHeight};
            }

            /// <summary> Creates instance with image rescaling disable.</summary>
            /// <param name="pixelFormat"> The name of the in use pixel format, case-sensitive.</param>
            /// <exception cref="System.ArgumentException"> Thrown when pixelFormat is not from list of valid pixel formats or failed to parse pixelFormat into enum.</exception>
            private ImageQualityPresetSetting(string pixelFormat){
                //check for list of pixel format
                if(!ImageQualityPresetSetting.ValidDefaultPixelFormat.Contains(pixelFormat)){
                    throw new ArgumentException("Argument 1 passed is not from the list of valid pixel formats.", "pixelFormat");
                }

                //parse value to enum and update field
                if(!Enum.TryParse<System.Drawing.Imaging.PixelFormat>(pixelFormat, false, out this.pixelFormat)){
                    throw new ArgumentException("Argument 1 passed has failed to be parsed into an enum.", "pixelFormat");
                }

                //update values
                this.enabledRescaling = true;
                this.rescalingResolution = null;
            }

            /// <summary> Creates instance from an ini file.</summary>
            /// <param name="iniFile"> The ini file to search values from.</param>
            /// <exception cref="System.ArgumentException"> Thrown when pixelFormat is not from list of valid pixel formats or failed to parse pixelFormat into enum.</exception>
            /// <exception cref="System.ArgumentOutOfRangeException"> The new image dimension was out of range of the valid dimension.</exception>
            public ImageQualityPresetSetting(IniFile iniFile){
                //get parsed ini
                IniFile.ParsedIni parsed_ini = ApplicationData.imagePresetConfigFile.Parsed_ini;

                //hold retrived values from file
                string presetChose;
                bool enabledRescaling;
                int[] newResolution = new int[2];
                string pixelFormatValue;

                //search values
                parsed_ini.SearchValue<string>("selectedPreset", out presetChose);
                parsed_ini.SearchValue<bool>("rescaleImage", out enabledRescaling);
                parsed_ini.SearchValue<int>("newWidthDimension", out newResolution[0]);
                parsed_ini.SearchValue<int>("newHeightDimension", out newResolution[1]);
                parsed_ini.SearchValue<string>(presetChose, "pixelFormat", out pixelFormatValue);

                //update states
                this.enabledRescaling = enabledRescaling;
                this.rescalingResolution = null;

                //parse value to enum and update field
                if(!Enum.TryParse<System.Drawing.Imaging.PixelFormat>(pixelFormatValue, false, out this.pixelFormat)){
                    throw new ArgumentException("Failed to parse pixel format into an enum, pixel format read from the instance of argument 1.", "iniFile");
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