using System;
using System.Collections.Generic;
using EchoCapture.Data;

namespace EchoCapture.Command{
    /// <summary> Command used to display information about application and its setting onto the console.</summary>
    public class InformationCommand : CommandBase{

        /// <inheritdoc/>
        public InformationCommand() : base("information", "Display all setting and information for the application."){}

        /// <inheritdoc/>
        /// <exception cref="EchoCapture.Exceptions.UnknownLineArgumentException"></exception>
        public override void OnSendEvent(string[] args){
            if(args.Length > 0){
                throw new Exceptions.UnknownLineArgumentException(this, "This command requires no argument.");
            }

            //holds the directory and capture interval
            string directory;
            int? interval;
            //retrives the directory and capture interval
            ApplicationData.GetAllFileData(out directory, out interval);

            //holds the image preset object
            ApplicationData.ImageQualityPresetSetting imageQualityPresetSetting = ApplicationData.GetImageQualityData();
            //retrives available information from object
            string presetChose = ApplicationData.GetCurrentSelectedPreset();
            Data.File.FileExtension imageType = imageQualityPresetSetting.ImageType;
            bool imageRescaling = imageQualityPresetSetting.EnabledRescaling;
            int[] imageRescalingResolution = imageQualityPresetSetting.RescalingResolution;

            //get presets data
            Dictionary<string, ApplicationData.ImageQualityPresetSetting> presets = ApplicationData.GetPresetsData();

            //get string array of valid pixel format
            string[] validPixelFormats = ApplicationData.ImageQualityPresetSetting.ValidPixelFormats;

            //sends to console
            Debug.SkipLine();
            Console.WriteLine("EchoCapture v1.0.0.0, for capturing your screen at a time-interval.");
            Console.WriteLine("Copyright (C) 2021  FakyDev");

            Debug.SkipLine();
            Debug.Warning("General Setting:");
            Debug.Message($"\tDirectory: {directory}");
            Console.WriteLine("\t The path capture screens are being saved at.");
            Debug.Message($"\tTime-interval: {interval}");
            Console.WriteLine("\t The period of time waiting before another capture screen is taken.");

            Debug.SkipLine();
            Debug.Warning("Image Setting:");
            Debug.Message($"\tSelected Preset: {presetChose}");
            Debug.SkipLine();
            Debug.Message($"\tImage-type: {imageType}");
            Console.WriteLine("\t The file type in which the capture screen is being saved.");
            Debug.Message(imageRescaling ? "\tImage-rescaling: Enable" : "\tImage-rescaling: Disable");
            Console.WriteLine("\t Determines if rescales the capture screen to a specific resolution.");
            Debug.Message($"\tImage-rescaling resolution: {imageRescalingResolution[0]}x{imageRescalingResolution[1]}");
            Console.WriteLine("\t The specific resolution to rescale to, if enabled.");

            Debug.SkipLine();
            Debug.Warning("Presets:");
            //loops through preset
            foreach(KeyValuePair<string, ApplicationData.ImageQualityPresetSetting> preset in presets){
                Debug.Warning($"\t{preset.Key}:");
                Debug.Message($"\t Pixel format: {preset.Value._PixelFormat}");
                Console.WriteLine("\t  The way the pixels are being represented.");
                Debug.Message($"\t Jpeg image quality: {preset.Value.JpegImageQuality}");
                Console.WriteLine("\t  The quality level of only jpeg image-type.");
                Debug.SkipLine();
            }

            Debug.Warning("Available pixel formats:");
            foreach(string pf in validPixelFormats){
                Debug.Message($"\t{pf}");
            }
        }
    }
}