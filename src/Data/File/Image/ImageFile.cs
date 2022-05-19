using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

using EchoCapture.Data.File.Async;

namespace EchoCapture.Data.File.Image{

    /// <summary> Base class for image files, which supports asynchronous and synchronous file operations.</summary>
    public class ImageFile : FileBase<Bitmap>, IAsyncFile<Bitmap>{
        
        /// <summary> The alternate extension of a jpeg image file.</summary>
        private const string ALTERNATE_JPG_EXT = "jpeg";

        /// <summary> Hold the image format of the bitmap.</summary>
        private ImageFormat imageFormat;

        /// <summary> Determine if need to reload to png quality config.</summary>
        private static bool pngQualitySettingRefresh = false;

        /// <summary> Holds the object representing the png quality setting.</summary>
        private static ApplicationData.PngQualitySetting _pngQualitySetting = default(ApplicationData.PngQualitySetting);

        /// <summary> (Get only) Returns the object representing the png quality setting.</summary>
        public static ApplicationData.PngQualitySetting _PngQualitySetting{
            get{
                //checks if default
                if(ImageFile.pngQualitySettingRefresh || ImageFile._pngQualitySetting.Equals(default(ApplicationData.PngQualitySetting))){
                    //refresh and update
                    ApplicationData.RefreshPngQualityConfig();
                    ImageFile._pngQualitySetting = ApplicationData.GetPngQualityData();

                    //update state
                    ImageFile.pngQualitySettingRefresh = false;
                }

                return ImageFile._pngQualitySetting;
            }
        }


        /// <param name="name"> Name of the image file.</param>
        /// <param name="path"> Directory of the image file is located in.</param>
        /// <param name="name"> Extension of the image file.</param>
        /// <exception cref="System.ArgumentException"> Thrown when the file extension is invalid.</exception>
        protected ImageFile(string name, string path, FileExtension extension) : base(name, extension, path){
            //update image type
            if(extension == FileExtension.png){
                this.imageFormat = ImageFormat.Png;
            } else if(extension == FileExtension.jpg){
                this.imageFormat = ImageFormat.Png;
            } else {
                throw new ArgumentException("Parameter 3 is not a valid image file extension.");
            }

            //correct file
            this.JPGFileFix();
        }

        /// <summary> Reads the image file, and return in bitmap.</summary>
        /// <remarks> You may want to clear resource after.</remarks>
        /// <returns> True if read the file and parse to bitmap successfully.</returns>
        public override bool ReadFile(out Bitmap value){
            //default value
            value = null;

            //check if exists
            if(!this.FileExists){
                return false;
            }

            //try to get bitmap
            try{
                value = new Bitmap(this.FullPath, true);
            } catch (ArgumentException){
                return false;
            }

            return true;
        }

        /// <summary> Overwrite the existing image file.</summary>
        /// <param name="value"> The new bitmap to overwrite with.</param>
        /// <returns> True if overwritten the file otherwise, either failed to save file or get filestream.</returns>
        public override bool OverwriteFile(Bitmap value){
            //does not exists
            if(!FileExists){
                return false;
            }

            try{
                //create file stream
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Write, FileShare.Read, 4096, false)){
                    //save
                    value.Save(fs, this.imageFormat);
                }
            } catch (Exception){
                //failed to save or create file stream
                return false;
            }
            
            //success
            return true;
        }

        /// <summary> Overwrite the existing image file.</summary>
        /// <remarks> Make sure to free resource after.</remarks>
        /// <param name="fs"> The file stream to use.</param>
        /// <param name="value"> The new bitmap to overwrite with.</param>
        /// <returns> Return true, if file is overwritten else file does not exist,
        /// failed to overwrite or filestream is invalid.</returns>
        public override bool OverwriteFile(FileStream fs, Bitmap value){
            //not same file or cannot write
            if(fs.Name != this.FullPath || !fs.CanWrite){
                return false;
            }

            try{
                //save
                value.Save(fs, this.imageFormat);
            } catch (Exception){
                //failed to save or create file stream
                return false;
            }
            
            //success
            return true;
        }


        /// <summary> Correct the extension of a jpeg image file.</summary>
        private void JPGFileFix(){
            //jpg extension only
            if(this.Extension != FileExtension.jpg){
                return;
            }

            //don't exists
            //thus either using alternate jpeg extension
            if(!this.FileExists){
                //get path with the alternate jpeg extension
                string toFixPath = Path.Combine(this.FilePath, $"{this.Name}.jpeg");

                //check if exists
                if(System.IO.File.Exists(toFixPath)){
                    //rename file
                    System.IO.File.Move(toFixPath, this.FullPath);
                }
            }
        }

        /// <summary> Screenshot the entire screen and return in a bitmap object.</summary>
        /// <remarks> You may want to clear resources.</remarks>
        public static Bitmap Screenshot(FileExtension fileExtension){
            //chooses the pixelFormat to use
            PixelFormat pixelFormat;
            if(fileExtension == FileExtension.png){
                pixelFormat = ImageFile._PngQualitySetting._PixelFormat;
            } else if(fileExtension == FileExtension.jpg){
                pixelFormat = ImageFile._PngQualitySetting._PixelFormat;
            } else {
                //to-do: update
                throw new Exception();
            }
            //get the bounds of the screen
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            //create bitmap, with screen size
            //Bitmap bmp = new Bitmap(bounds.Width, bounds.Height, pixelFormat);

            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height, pixelFormat);

            //create a graphics object from the bitmap
            using(Graphics captureGraphics = Graphics.FromImage(bmp)){
                //copying image from The Screen to the bmp
                captureGraphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            //return
            return bmp;
        }

        /// <summary> Determine if the extension is an image extension.</summary>
        public static bool ValidateImageExtension(FileExtension imageExtension){
            if(imageExtension == FileExtension.jpg || imageExtension == FileExtension.png){
                return true;
            }

            return false;
        }

        /// <summary> Allows to reload the png quality setting, next time that png quality setting are fetched.</summary>
        public static void QueueForPngQualitySettingRefresh(){
            ImageFile.pngQualitySettingRefresh = true;
        }


        #region Asynchronous file operations

        //create, delete is copied directly from AsycnFileBase
        /// <summary> Create the image file asynchronously.</summary>
        /// <remarks> No exception is thrown, if success.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file already exists.</exception>
        public async Task CreateFileAsync(){
            //check if already exists
            if(this.FileExists){
                throw new InvalidOperationException("File already exists.");
            }

            //create
            await Task.Run(() => {
                //create the file
                FileStream fs = new FileStream(this.FullPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096, true);
                //dispose
                fs.Dispose();
            });
        }

        /// <summary> Create the image file for asynchronous operation, and pass the filestream.</summary>
        public bool CreateFileAsync(out FileStream fs){
            //default value
            fs = null;

            //check if already exists
            if(this.FileExists){
                return false;
            }

            try{
                //create the filestream, (along with file)
                fs = new FileStream(this.FullPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096, true);
            } catch (Exception){
                //failed
                return false;
            }

            return true;
        }
    
        /// <summary> Delete the image file asynchronously.</summary>
        /// <remarks> No exception is thrown, if success.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        public async Task DeleteFileAsync(){
            //check if already exists
            if(!this.FileExists){
                throw new InvalidOperationException("File doesn't exists.");
            }

            //delete file
            await Task.Run(() => System.IO.File.Delete(this.FullPath));
        }

    
        /// <summary> Overwrites the existing image file asynchronously.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        public async Task OverwriteFileAsync(Bitmap value){
            //file doesn't exists
            if(!this.FileExists){
                //throw exception
                throw new InvalidOperationException("File doesn't exists.");
            }

            //will hold the converted bitmap
            Byte[] convertedBmp;
            //convert the bitmap
            using(MemoryStream ms = new MemoryStream()){
                //save to stream
                value.Save(ms, this.imageFormat);
                //get value
                convertedBmp = ms.ToArray();
            }

            //create the filestream
            using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Write, FileShare.Read, 4096, true)){
                //write to file
                await fs.WriteAsync(convertedBmp);
            }
        }

        /// <summary> Overwrites the existing image file asynchronously, with filestream provided.</summary>
        /// <remarks> Make sure to free resource after.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists, or filestream is invalid.</exception>
        public async Task OverwriteFileAsync(FileStream fs, Bitmap value){
            //file doesn't exists
            if(!this.FileExists){
                //throw exception
                throw new InvalidOperationException("File doesn't exists.");
            }

            //invalid filestream
            if(!fs.CanWrite || fs.Name != this.FullPath){
                //throw exception
                throw new InvalidOperationException("Filestream is invalid.");
            }

            //will hold the converted bitmap
            Byte[] convertedBmp;
            //convert the bitmap
            using(MemoryStream ms = new MemoryStream()){
                //Encoder.Quality;
                //EncoderValue.CompressionRle
                //save to stream
                value.Save(ms, ImageFormat.Png);
                //get value
                convertedBmp = ms.ToArray();
            }

            //write to file
            await fs.WriteAsync(convertedBmp);
        }

        /// <summary> Read the image file asynchronously and return the image in bitmap format.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        /// <exception cref="System.IO.FileFormatException"> When the image file is empty.</exception>
        public async Task<Bitmap> ReadFileAsync(){
            //file doesn't exists
            if(!this.FileExists){
                //throw exception
                throw new InvalidOperationException("File doesn't exists.");
            }

            //will hold the bmp
            Bitmap bmp;
            //will binary of the bitmap
            Byte[] convertedBmp;

            //create the filestream
            using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true)){
                //update length
                convertedBmp = new Byte[fs.Length];
                //read file
                await fs.ReadAsync(convertedBmp, 0, (int)fs.Length);
            }

            try{
                //convert byte of array to bitmap
                using(MemoryStream ms = new MemoryStream(convertedBmp)){
                    bmp = new Bitmap(ms);
                }
            } catch (ArgumentException){
                throw new FileFormatException("Image format is invalid, or is empty.");
            }

            //return
            return bmp;
        }

        #endregion
    }
}