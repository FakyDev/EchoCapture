using System;
using System.Collections.Generic;

using EchoCapture.Data.File;
using EchoCapture.Exceptions.Data;

namespace EchoCapture.Data{

    /// <summary> Manages the application related data.</summary>
    public static class ApplicationData{

        /// <summary> Determine if has intialise application data manager.</summary>
        private static bool hasInitialise = false;

        /// <summary> Hold the instance of the application data file.</summary>
        private static JsonFile<Setting> dataFile;

        /// <summary> (Get only) Return setting with default values.</summary>
        private static Setting DefaultConfig{
            get{
                return new Setting(ApplicationData.DataFolder, 10000);
            }
        }

        /// <summary> (Get only) Return path to the data folder.</summary>
        public static string DataFolder{
            get{
                //get the path
                if(Debug.IsDebug){
                    return System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location) + @"\appData";
                }

                //return
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Program.ApplicationName);
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
                //content[0].SavedPath = data.Path;
                updatedContent.SavedPath = data.Path;
            }
            if(data.UpdateType == UpdateData.DataType.timeout){
                //content[0].TimeoutMS = data.Timeout;
                updatedContent.TimeoutMS = data.Timeout;
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

        /// <summary> Retrieve the folder path and time interval value from the data file.</summary>
        /// <param name="path"> Will hold the retrieved path.</param>
        /// <param name="interval"> Will hold the time interval value.</param>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        public static void GetFileData(out string path, out int? interval){
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
                    //overwrite
                    ApplicationData.dataFile.OverwriteFile(fs, content);

                    //notify user that data folder is created
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
                        //send error to user that data folder was created and default value was restored.
                        Debug.Error("Data config was found to be corrupted. It has been fixed and default values have been restored. Please reconfigure your settings.");
                        Debug.SkipLine();
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

        /// <summary> Object used to determine application data contents format.</summary>
        private struct Setting{
            /// <summary> Hold the value of the path.</summary>
            private string savedPath;
            /// <summary> Hold the value of the time interval.</summary>
            private int? timeoutMS;

            public Setting(string savedPath, int? timeoutMS){
                this.savedPath = savedPath;
                this.timeoutMS = timeoutMS;
            }

            /// <summary> The value of the path.</summary>
            public string SavedPath{
                get{
                    return this.savedPath;
                }
                set{
                    this.savedPath = value;
                }
            }
            
            /// <summary> The value of the time interval in ms.</summary>
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
    }
}