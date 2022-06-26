using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

using EchoCapture.Data;
using EchoCapture.Data.File;
using EchoCapture.Data.File.Image;
using EchoCapture.Exceptions;
using EchoCapture.Exceptions.Data;
using EchoCapture.Networking;

namespace EchoCapture.Command{

    /// <summary> Command used to start or stop perform the operation of taking screenshot.</summary>
    public class TaskCommand : CommandBase{
        
        /// <summary> The argument one value, which is used to start performing the action.</summary>
        private const string START = "start";

        /// <summary> The argument one value, which is used to stop the action.</summary>
        private const string STOP = "stop";

        /// <summary> The saved file name format, which will be changed into the time the screen capture was taken.</summary>
        private const string FILENAME_FORMAT = "MM-dd-yyyy HH-mm-ss-fff";

        /// <summary> Format when using debug window.</summary>
        private const string DEBUG_FORMAT = "[HH:mm:ss.fff]";


        /// <summary> (Get only) Reference to dictionary of help command's arg dictionary.</summary>
        private static Dictionary<int, CommandArg> commandArgs{
            get{
                //dictionary holding args for help command
                Dictionary<int, CommandArg> _dictionary = new Dictionary<int, CommandArg>();
                //for first arg
                _dictionary.Add(0, new CommandArg("state", 1, $"Either \"{TaskCommand.START}\" or \"{TaskCommand.STOP}\". *{TaskCommand.START} is used to start performing and *{TaskCommand.STOP} is used to stop the action.", typeof(string)));

                //return
                return _dictionary;
            }
        }


        /// <summary> Determine if can perform async task, which is take screenshot.</summary>
        private bool isRunning = false;

        /// <summary> Hold the cancellation token of the current asycn task.</summary>
        private CancellationTokenSource cancelTokenSource = null;

        /// <summary> Hold the asycn task.</summary>
        private Task work = null;
        
        /// <summary> Hold the client, to send data.</summary>
        private static SocketClient client = null;


        /// <summary> (Get only) Determing if performing the task.</summary>
        public bool IsPerforming{
            get{
                return this.isRunning;
            }
        }

        public TaskCommand() : base("task", "Start or stop taking capture of the screen.", TaskCommand.commandArgs){}

        /// <inheritdoc/>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"></exception>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        public override void OnSendEvent(string[] args){
            //validate arguments
            this.ValidateArguments(args);

            //get param 1
            string state = args[0];

            //for starting
            if(state == TaskCommand.START){
                //will hold the delay value
                int? delay = null;

                try{
                    //start operation
                    this.StartWork(out delay);
                } catch (ReadingDataFileException e){
                    //send error to user
                    Debug.Error(e.Message);
                    //update log
                    System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog(e.Message);
                    return;
                } catch (InvalidOperationException e){
                    //send error to user
                    Debug.Error(e.Message);
                    //update log
                    System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog(e.Message);
                    return;
                }

                //notice user
                Debug.Success($"The operation of capturing screen each {delay}ms, has successfully started.");
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog($"The operation of capturing screen each {delay}ms, has successfully started.");
                return;
            }

            //for ending
            if(state == TaskCommand.STOP){
                try{
                    this.StopWork();
                } catch(InvalidOperationException e){
                    if(Program.IsRunning){
                        //send error to user
                        Debug.Error(e.Message);
                    }
                    //update log
                    System.Threading.Tasks.Task _updateLog = ApplicationData.UpdateLog(e.Message);
                    return;
                }

                //notice user
                Debug.Success("Operation was successfully ended.");
                //update log
                System.Threading.Tasks.Task updateLog = ApplicationData.UpdateLog("Operation was successfully ended.");
                return;
            }

            //get reference
            CommandArg commandArg = this.ArgsList[0];
            //throw exception
            throw new InvalidLineArgumentException(commandArg.ArgNumber, commandArg.ArgName, commandArg.ArgType[0]);
        }

        /// <summary> Starts the operation of capturing screen.</summary>
        /// <exception cref="EchoCapture.Exceptions.Data.ReadingDataFileException"> Failed to read file.</exception>
        /// <exception cref="System.InvalidOperationException"> Already ongoing operation.</exception>
        private void StartWork(out int? delay){
            //default values
            delay = null;
            FileExtension? imageExtension = null;

            //refresh png quality setting
            ImageFile.QueueForImageQualitySettingRefresh();

            //check if already running
            if(!this.isRunning){
                //try get data
                try{
                    imageExtension = ImageFile._ImageQualitySetting.ImageType;
                    ApplicationData.GetFileData(out delay);
                    //ApplicationData.GetFileData(out imageExtension);
                    //move image format in the image quality preset from json
                } catch (ReadingDataFileException){
                    //throw new exception
                    throw new ReadingDataFileException("Failed to start the operation of capturing screen.");
                }

                //if debug start debug process
                if(Debug.IsDebug){
                    Program.StartCaptureDebug();
                    //update reference
                    TaskCommand.client = (SocketClient) Program._Socket;
                }

                //update state
                this.isRunning = true;
                //update token source
                this.cancelTokenSource = new CancellationTokenSource();
                //start work
                this.work = this.ExecuteWork(this.cancelTokenSource.Token, (int)delay, (FileExtension)imageExtension);

                return;
            }

            //already running
            throw new InvalidOperationException("Failed to start operation! End the ongoing operation to start a new one.");
        }

        /// <summary> Stops the ongoing operation.</summary>
        /// <exception cref="System.InvalidOperationException"> There is no ongoing operation.</exception>
        private void StopWork(){
            //check if already running
            if(this.isRunning){
                //update state
                this.isRunning = false;

                //check work has an instance
                if(this.work != null){
                    //check if still running
                    if(!this.work.IsCompleted){
                        //cancel work
                        this.cancelTokenSource.Cancel();
                    }
                }

                //request the debug to stop
                try{
                    TaskCommand.client.DropRequest();
                } catch (InvalidOperationException){}

                //update values
                this.work = null;
                this.cancelTokenSource = null;
                TaskCommand.client = null;

                return;
            }

            //already running
            throw new InvalidOperationException("There is no operation to end.");
        }

        /// <summary> Loops each <paramref name="delay"/> (ms) asynchronously, and capture the screen.</summary>
        /// <param name="token"> The cancellation token which allows to stop the work.</param>
        /// <param name="delay"> The amount of miliseconds to wait between work.</param>
        /// <param name="imageExtension"> The extension of the capture screen, to use.</param>
        private async Task ExecuteWork(CancellationToken token, int delay, FileExtension imageExtension){
            //were we already canceled?
            token.ThrowIfCancellationRequested();

            //loop if allowed
            while(this.isRunning){
                //check if requested
                if(token.IsCancellationRequested){
                    //cancel
                    token.ThrowIfCancellationRequested();
                }

                //wait
                await Task.Delay(delay);

                //check if requested again
                if(token.IsCancellationRequested){
                    //cancel
                    token.ThrowIfCancellationRequested();
                }

                //screenshot and save
                await TaskCommand.ScreenshotAndSave(imageExtension);
            }
        }

        /// <summary> Screenshot the display and save it, asynchronously.</summary>
        private async static Task ScreenshotAndSave(FileExtension imageExtension){
            //get the date and time of screenshot
            DateTime screenshotDateTime = DateTime.Now;

            //parse format to current time
            string date = screenshotDateTime.ToString(TaskCommand.FILENAME_FORMAT);
            string debugDate = screenshotDateTime.ToString(TaskCommand.DEBUG_FORMAT);

            //will hold the instance
            ImageFile file = null;

            //create instance
            if(imageExtension == FileExtension.png){
                file = new PngFile(date, ApplicationData.CaptureScreenFolder);
            } else if(imageExtension == FileExtension.jpeg){
                file = new JpgFile(date, ApplicationData.CaptureScreenFolder);
            }

            //will hold the stream
            if(!file.FileExists){
                //will hold the file stream
                System.IO.FileStream fs;

                //screenshot
                using(System.Drawing.Bitmap bmp = ImageFile.RescaleScreenshot()){
                    //create file
                    if(file.CreateFileAsync(out fs)){
                        try{
                            //save screenshot
                            await file.OverwriteFileAsync(fs, bmp);
                        } catch(System.IO.IOException){
                            //async debug
                            if(Debug.IsDebug && TaskCommand.client.Connected){
                                Task debugOpertion = Task.Run(() => {
                                    //create line
                                    string line = $"{debugDate} Failed to save capture screen: \"{date}.{file.Extension}\"";
                                    //send
                                    TaskCommand.client.SendMessage(line);
                                });
                            }

                            //log
                            Task _updateLog = ApplicationData.UpdateLog($"Failed to save capture screen: \"{date}.{file.Extension}\".");

                            return;
                        } finally {
                            //free resource
                            fs.Dispose();
                        }

                        //async debug
                        if(Debug.IsDebug && TaskCommand.client.Connected){
                            Task debugOpertion = Task.Run(() => {
                                //create line
                                string line = $"{debugDate} Created file and saved the capture screen: \"{date}.{file.Extension}\"";
                                //send
                                TaskCommand.client.SendMessage(line);
                            });
                        }

                        //log
                        Task updateLog = ApplicationData.UpdateLog($"Created file and saved the capture screen: \"{date}.{file.Extension}\".");
                    } else {
                        //async debug
                        if(Debug.IsDebug && TaskCommand.client.Connected){
                            Task debugOpertion = Task.Run(() => {
                                //create line
                                string line = $"{debugDate} Failed to create file storing capture screen: \"{date}.{file.Extension}\"";
                                //send
                                TaskCommand.client.SendMessage(line, TransferType.ErrorMessage);
                            });
                        }

                        //log
                        Task updateLog = ApplicationData.UpdateLog($"Failed to create file storing capture screen: \"{date}.{file.Extension}\".");
                    }
                }
            } else {
                //screenshot
                using(System.Drawing.Bitmap bmp = ImageFile.RescaleScreenshot()){
                    try{
                        //save screenshot
                        await file.OverwriteFileAsync(bmp);
                    } catch(System.IO.IOException){
                        //async debug
                        if(Debug.IsDebug && TaskCommand.client.Connected){
                            Task debugOpertion = Task.Run(() => {
                                //create line
                                string line = $"{debugDate} Failed to save capture screen: \"{date}.{file.Extension}\"";
                                //send
                                TaskCommand.client.SendMessage(line);
                            });
                        }

                        //log
                        Task _updateLog = ApplicationData.UpdateLog($"Failed to save capture screen: \"{date}.{file.Extension}\".");

                        return;
                    }

                    //async debug
                    if(Debug.IsDebug && TaskCommand.client.Connected){
                        Task debugOpertion = Task.Run(() => {
                            //create line
                            string line = $"{debugDate} Saved the capture screen: \"{date}.{file.Extension}\"";
                            //send
                            TaskCommand.client.SendMessage(line);
                        });
                    }

                    //log
                    Task updateLog = ApplicationData.UpdateLog($"Saved the capture screen: \"{date}.{file.Extension}\".");
                }
            }
        }

        /// <inheritdoc/>
        /// <remarks> Try to cancel asycn task as well.</remarks>
        public override void RemoveEvent(){
            //call base method
            base.RemoveEvent();

            //cancel task
            if(this.cancelTokenSource != null){
                this.cancelTokenSource.Cancel();
            }
        }
    }
}