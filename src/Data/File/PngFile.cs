using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Screenshoter.Data.File{

    /// <summary> Allows you to read, parse and overwrite png image files. Also contains functionality to get screenshot.</summary>
    public class PngFile : FileBase<Bitmap>{
        
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
        /// failed to overwrite or filestream is not for the same file.</returns>
        public override bool OverwriteFile(FileStream fs, Bitmap value){
            //not same file
            if(fs.Name != this.FullPath){
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
}