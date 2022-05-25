using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace EchoCapture.Data.File.Image{

    /// <summary> Allows you to perform asynchronous and synchronous file operation to JPEG image file.</summary>
    public sealed class JpgFile : ImageFile{
        
        /// <summary> Holds the image encoder.</summary>
        private static ImageCodecInfo imageEncoder = null;

        /// <summary> (Get only) Return the image encoder.</summary>
        private static ImageCodecInfo ImageEncoder{
            get{
                //find and update encoder
                if(JpgFile.imageEncoder == null){
                    foreach(ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders()){
                        if(encoder.FormatID == ImageFormat.Jpeg.Guid){
                            JpgFile.imageEncoder = encoder;
                            break;
                        }
                    }
                }

                //return
                return JpgFile.imageEncoder;
            }
        }
        
        /// <summary> Reference to object holding the encoder parameters.</summary>
        private EncoderParameters encoderParameters;

        /// <param name="name">The name of the jpeg image.</param>
        /// <param name="path">The directory the jpeg image is located in.</param>
        public JpgFile(string name, string path) : base(name, path, FileExtension.jpeg){
            //create encoder params with size of one param
            this.encoderParameters = new EncoderParameters(1);
        }

        /// <summary> Overwrite the existing image file.</summary>
        /// <param name="value"> The new bitmap to overwrite with.</param>
        /// <returns> True if overwritten the file otherwise, either failed to save file or get filestream.</returns>
        public override bool OverwriteFile(Bitmap value){
            //does not exists
            if(!FileExists){
                return false;
            }

            //update encoder param
            this.encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long) ImageFile._ImageQualitySetting.JpegImageQuality);

            try{
                //create file stream
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Write, FileShare.Read, 4096, false)){
                    //save
                    //along with compression if set
                    value.Save(fs, JpgFile.ImageEncoder, this.encoderParameters);
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

            //update encoder param
            this.encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long) ImageFile._ImageQualitySetting.JpegImageQuality);

            try{
                //save
                //along with compression if set
                value.Save(fs, JpgFile.ImageEncoder, this.encoderParameters);
            } catch (Exception){
                //failed to save or create file stream
                return false;
            }
            
            //success
            return true;
        }

        /// <summary> Overwrites the existing image file asynchronously.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        public override async Task OverwriteFileAsync(Bitmap value){
            //file doesn't exists
            if(!this.FileExists){
                //throw exception
                throw new InvalidOperationException("File doesn't exists.");
            }

            //update encoder param
            this.encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long) ImageFile._ImageQualitySetting.JpegImageQuality);

            //will hold the converted bitmap
            Byte[] convertedBmp;
            //convert the bitmap
            using(MemoryStream ms = new MemoryStream()){
                //save to stream
                //along with compression if set
                value.Save(ms, JpgFile.ImageEncoder, this.encoderParameters);
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
        public override async Task OverwriteFileAsync(FileStream fs, Bitmap value){
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

            //update encoder param
            this.encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long) ImageFile._ImageQualitySetting.JpegImageQuality);

            //will hold the converted bitmap
            Byte[] convertedBmp;
            //convert the bitmap
            using(MemoryStream ms = new MemoryStream()){
                //save to stream
                value.Save(ms, JpgFile.ImageEncoder, this.encoderParameters);
                //get value
                convertedBmp = ms.ToArray();
            }

            //write to file
            await fs.WriteAsync(convertedBmp);
        }
    }
}