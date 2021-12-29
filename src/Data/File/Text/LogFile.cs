using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EchoCapture.Data.File.Text{

    /// <summary> Log file, using UTF-8 encoding, derived from <see cref="EchoCapture.Data.File.Text.TextFile"/>.</summary>
    public class LogFile : TextFile{

        public LogFile(string name, string path) : base(name, path, Encoding.UTF8, FileExtension.log){}
    
        /// <summary> Write the new data (after with old data) to the existing file asynchronously.</summary>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        /// <exception cref="System.ArgumentException"> Thrown when failed to encode text.</exception>
        public async Task AddUpFileAsync(string text){
            //file doesn't exists
            if(!this.FileExists){
                //throw exception
                throw new InvalidOperationException("File doesn't exists.");
            }

            //will hold the encoded text
            byte[] convertedValue;

            try{
                //convert to byte
                convertedValue = this.TextToByte(text);
            } catch(EncoderFallbackException){
                throw new ArgumentException("Failed to encode the argument passed.");
            }

            try{
                //create the filestream
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true)){
                    //write to file
                    await fs.WriteAsync(convertedValue);
                }
            } catch (IOException e){
                //rethrow exception
                throw e;
            }
        }
    }
}