using System.IO;
using System.Threading.Tasks;
using System;

namespace Screenshoter.Data.File.Async{

    /// <summary> Base class for file with asynchronous functionality./summary>
    public abstract class AsyncFileBase<T> : IAsyncFile<T>{

        /// <summary> Hold the name of the file.</summary>
        private string name;

        /// <summary> Hold the extension of the file.</summary>
        private FileExtension extension;

        /// <summary> Hold the path of the file.</summary>
        private string path;

        /// <inheritdoc/>
        public string Name{
            get{
                return this.name;
            }
        }

        /// <inheritdoc/>
        public FileExtension Extension{
            get{
                return this.extension;
            }
        }

        /// <inheritdoc/>
        public string FilePath{
            get{
                return this.path;
            }
        }

        /// <inheritdoc/>
        public string FullPath{
            get{
                //will hold final path
                string final;

                //update
                final  = this.path;
                if(!Path.EndsInDirectorySeparator(final)){
                    //add backslash
                    final += Path.DirectorySeparatorChar;
                }

                //add final part
                final += this.Name + "." + (this.extension.ToString().ToLower());

                //return path
                return final;
            }
        }

        /// <inheritdoc/>
        public bool FileExists{
            get{
                if(System.IO.File.Exists(this.FullPath)){
                    return true;
                }

                return false;
            }
        }

        /// <param name="name"> The file name.</param>
        /// <param name="extension"> The file extension.</param>
        /// <param name="path"> The file path.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public AsyncFileBase(string name, FileExtension extension, string path){
            //update reference
            this.name = name;
            this.extension = extension;
            this.path = path;

            //invalid directory
            if(!FileBase<T>.IsDirectory(path)){
                throw new ArgumentException("Path is invalid.");
            }

            if(!FileBase<T>.IsValidFilename(name)){
                throw new ArgumentException("File name is invalid.");
            }
        }

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
        public abstract Task OverwriteFileAsync(T value);

        /// <inheritdoc/>
        public abstract Task OverwriteFileAsync(System.IO.FileStream fs, T value);

        /// <inheritdoc/>
        public abstract Task<T> ReadFileAsync();
    }
}