using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using EchoCapture.Data.File.Async;

namespace EchoCapture.Data.File.Text{

    /// <summary> Allows to perform synchronous and asynchronous file-related operations to text file. The encoding type, is determine when constructing.</summary>
    public partial class TextFile : AsyncFileBase<string>, IAsyncFile<string>{

        /// <summary> Encoding use to represent text.</summary>
        private Encoding textEncoding;
        
        public TextFile(string name, string path, Encoding encoding) : base(name, FileExtension.txt, path){
            //update encoding
            this.textEncoding = encoding;
        }

        /// <summary> Constructed for derived classes, which are allowed to change the extension of the file.</summary>
        protected TextFile(string name, string path, Encoding encoding, FileExtension fileExtension) : base(name, fileExtension, path){
            //update encoding
            this.textEncoding = encoding;
        }


        /// <inheritdoc/>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        /// <exception cref="System.ArgumentException"> Thrown when failed to encode text.</exception>
        public async override Task OverwriteFileAsync(string text){
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
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Write, FileShare.Read, 4096, true)){
                    //write to file
                    await fs.WriteAsync(convertedValue);
                }
            } catch (IOException e){
                //rethrow exception
                throw e;
            }
        }

        /// <inheritdoc/>
        /// <remarks> Make sure to free resource after.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists, or filestream is invalid.</exception>
        /// <exception cref="System.ArgumentException"> Thrown when failed to encode text.</exception>
        public async override Task OverwriteFileAsync(FileStream fs, string text){
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

            //will hold the encoded text
            byte[] convertedValue;

            try{
                //convert to byte
                convertedValue = this.TextToByte(text);
            } catch(EncoderFallbackException){
                throw new ArgumentException("Failed to encode the argument passed.");
            }

            try{
                //write to file
                await fs.WriteAsync(convertedValue);
            } catch (IOException e){
                //rethrow exception
                throw e;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        public async sealed override Task<string> ReadFileAsync(){
            //file doesn't exists
            if(!this.FileExists){
                //throw exception
                throw new InvalidOperationException("File doesn't exists.");
            }

            //will hold the text in byte
            Byte[] encodedText;

            try{
                //create the filestream
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true)){
                    //update length
                    encodedText = new Byte[fs.Length];
                    //read file
                    await fs.ReadAsync(encodedText);
                }
            } catch (IOException e){
                //rethrow exception
                throw e;
            }

            //decode and return text
            return ByteToString(encodedText);
        }


        /// <summary> Convert <paramref name="text"/> to byte array.</summary>
        /// <param name="text"> The text to convert.</param>
        protected byte[] TextToByte(string text){
            return this.textEncoding.GetBytes(text);
        }

        /// <summary> Convert <paramref name="bytes"/> to string.</summary>
        /// <param name="text"> The bytes array to convert to string.</param>
        protected string ByteToString(byte[] bytes){
            return this.textEncoding.GetString(bytes);
        }
    }

    //part for synchronous operation
    public partial class TextFile : IFile<string>{
        
        /// <inheritdoc cref="EchoCapture.Data.File.FileBase{T}.CreateFile"/>
        public bool CreateFile(){
            //check if already exists
            if(this.FileExists){
                return false;
            }
            
            //hold the stream
            FileStream fileStream = null;
            try{
                //create folder
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(this.FullPath));
                //create file
                fileStream = System.IO.File.Create(this.FullPath);
            } catch (Exception){
                return false;
            } finally {
                if(fileStream != null){
                    //free resource
                    fileStream.Dispose();
                }
            }

            //created
            return true;
        }

        /// <inheritdoc cref="EchoCapture.Data.File.FileBase{T}.CreateFile(out FileStream)"/>
        public bool CreateFile(out FileStream fs){
            //default value
            fs = null;

            //check if already exists
            if(this.FileExists){
                return false;
            }

            try{
                //create folder
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(this.FullPath));
                //create file
                fs = System.IO.File.Create(this.FullPath);
            } catch (Exception){
                return false;
            }

            //created
            return true;
        }

        /// <inheritdoc cref="EchoCapture.Data.File.FileBase{T}.DeleteFile"/>
        public bool DeleteFile(){
            //check if already exists
            if(!this.FileExists){
                return false;
            }

            try{
                //delete file
                System.IO.File.Delete(this.FullPath);
            } catch (Exception){
                return false;
            }

            //created
            return true;
        }
        

        /// <summary> Overwrite the existing file.</summary>
        /// <param name="text"> The new text to overwrite the file with.</param>
        /// <returns> True if overwritten the file otherwise, either failed to save file or get filestream.</returns>
        /// <exception cref="System.ArgumentException"> Thrown when failed to encode text.</exception>
        public bool OverwriteFile(string text){
            //does not exists
            if(!FileExists){
                return false;
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
                //create file stream
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Write, FileShare.None, 4096, false)){
                    //save
                    fs.Write(convertedValue);
                }
            } catch (Exception){
                //failed to save or create file stream
                return false;
            }
            
            //success
            return true;
        }

        /// <summary> Overwrite the existing file.</summary>
        /// <remarks> Make sure to free resource after.</remarks>
        /// <param name="fs"> The file stream to use.</param>
        /// <param name="value"> The new text to overwrite with.</param>
        /// <returns> True if overwritten the file otherwise, either failed to save file or invalid filestream.</returns>
        /// <exception cref="System.ArgumentException"> Thrown when failed to encode text.</exception>
        public bool OverwriteFile(FileStream fs, string text){
            //not same file or cannot write
            if(fs.Name != this.FullPath || !fs.CanWrite){
                return false;
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
                //save
                fs.Write(convertedValue);
            } catch (Exception){
                //failed to save or create file stream
                return false;
            }
            
            //success
            return true;
        }
    
        /// <inheritdoc cref="EchoCapture.Data.File.FileBase{T}.ReadFile(out T)"/>
        public bool ReadFile(out string text){
            //default value
            text = null;

            //file doesn't exists
            if(!this.FileExists){
                return false;
            }

            //will hold the text in byte
            Byte[] encodedText;

            try{
                //create the filestream
                using(FileStream fs = new FileStream(this.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, false)){
                    //update length
                    encodedText = new Byte[fs.Length];
                    //read file
                    fs.Read(encodedText);
                }
            } catch (IOException){
                //failed
                return false;
            }

            //decode
            text = ByteToString(encodedText);

            //success
            return true;
        }
    }
}