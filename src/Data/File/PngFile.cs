using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using Screenshoter.Data.File.Async;

namespace Screenshoter.Data.File{

    /// <summary> It allows you to perform asynchronous and synchronous file-related operation to PNG image file. Those operation
    /// includes creating, deleting, reading and overwriting</summary>
    public partial class PngFile : FileBase<Bitmap>{
        
        /// <param name="name"> The file name.</param>
        /// <param name="path"> The file path.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public PngFile(string name, string path) : base(name, FileExtension.png, path){}

        /// <summary> Reads the png image, and return in bitmap.</summary>
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

        /// <summary> Overwrite the existing png file.</summary>
        /// <param name="value"> The new bitmap to overwrite with.</param>
        /// <returns> True if overwritten the file otherwise, either failed to save file or get filestream.</returns>
        public override bool OverwriteFile(Bitmap value){
            //does not exists
            if(!FileExists){
                return false;
            }

            try{
                //create file stream
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Write, FileShare.None, 4096, true)){
                    //save
                    value.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                }
            } catch (Exception){
                //failed to save or create file stream
                return false;
            }
            
            //success
            return true;
        }

        /// <summary> Overwrite the existing png file.</summary>
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
                value.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
            } catch (Exception){
                //failed to save or create file stream
                return false;
            }
            
            //success
            return true;
        }

        /// <summary> Screenshot the screen and saved it in the existing file.</summary>
        /// <param name="file"> The file to saved to.</param>
        public static bool Screenshot(PngFile file){
            //does not exists
            if(!file.FileExists){
                return false;
            }

            //get the bounds of the screen
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            //create bitmap, with screen size
            using(Bitmap bmp = new Bitmap(bounds.Width, bounds.Height)){
                //create a graphics object from the bitmap
                using(Graphics captureGraphics = Graphics.FromImage(bmp)){
                    //copying image from The Screen to the bmp
                    captureGraphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
                }

                //try to save
                try{
                    bmp.Save(file.FullPath, System.Drawing.Imaging.ImageFormat.Png);
                } catch(Exception){
                    return false;
                }
            }

            //success
            return true;
        }

        /// <summary> Screenshot the display, and return it in bitmap.</summary>
        /// <remarks> You may want to clear resource after.</remarks>
        public static Bitmap Screenshot(){
            //get the bounds of the screen
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            //create bitmap, with screen size
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);

            //create a graphics object from the bitmap
            using(Graphics captureGraphics = Graphics.FromImage(bmp)){
                //copying image from The Screen to the bmp
                captureGraphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            //return
            return bmp;
        }
    }

    public partial class PngFile : IAsyncFile<Bitmap>{

        //create, delete is copied directly from AsycnFileBase
        /// <inheritdoc/>
        /// <remarks> No exception is thrown, if success.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file already exists.</exception>
        /// <exception cref="System.IO.IOException"></exception>
        public async Task CreateFileAsync(){
            //check if already exists
            if(this.FileExists){
                throw new InvalidOperationException("File already exists.");
            }

            //create
            try{
                //perform
                await Task.Run(() => {
                    //create the file
                    FileStream fs = new FileStream(this.FullPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 4096, true);
                    //dispose
                    fs.Dispose();
                });
            } catch (IOException e){
                //rethrow exception
                throw e;
            }
        }

        /// <inheritdoc/>
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
    
        /// <inheritdoc/>
        /// <remarks> No exception is thrown, if success.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        /// <exception cref="System.IO.IOException"></exception>
        public async Task DeleteFileAsync(){
            //check if already exists
            if(!this.FileExists){
                throw new InvalidOperationException("File doesn't exists.");
            }

            try{
                //delete file
                await Task.Run(() => System.IO.File.Delete(this.FullPath));
            } catch (IOException e){
                //rethrow exception
                throw e;
            }
        }

    
        /// <inheritdoc/>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        /// <exception cref="System.IO.IOException"></exception>
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
                value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //get value
                convertedBmp = ms.ToArray();
            }

            try{
                //create the filestream, (along with file)
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Write, FileShare.Read, 4096, true)){
                    //write to file
                    await fs.WriteAsync(convertedBmp);
                }
            } catch (IOException e){
                //rethrow exception
                throw e;
            }
        }

        /// <inheritdoc/>
        /// <remarks> Make sure to free resource after.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists, or filestream is invalid.</exception>
        /// <exception cref="System.IO.IOException"></exception>
        public async Task OverwriteFileAsync(System.IO.FileStream fs, Bitmap value){
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
                //save to stream
                value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //get value
                convertedBmp = ms.ToArray();
            }

            try{
                //write to file
                await fs.WriteAsync(convertedBmp);
            } catch (IOException e){
                //rethrow exception
                throw e;
            }
        }

        /// <inheritdoc/>
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

            try{
                //create the filestream, (along with file)
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true)){
                    //update length
                    convertedBmp = new Byte[fs.Length];
                    //read file
                    await fs.ReadAsync(convertedBmp, 0, (int)fs.Length);
                }
            } catch (IOException e){
                //rethrow exception
                throw e;
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
    }
}