using System.IO;
using System;

namespace EchoCapture.Data.File{
    
    /// <summary> Base file class, which provide common and basic functionality related to files.</summary>
    public abstract class FileBase<T> : IFile<T>{

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
                return Path.Combine(this.path, $"{this.Name}.{this.extension.ToString().ToLower()}");
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
        public FileBase(string name, FileExtension extension, string path){
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
        /// <returns> True if created, otherwise already exists or failed to create.</returns>
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

        /// <inheritdoc/>
        /// <remarks> Don't forget to close the file stream.</remarks>
        /// <returns> True if created, otherwise already exists or failed to create.</returns>
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

        /// <inheritdoc/>
        /// <returns> True if deleted, otherwise does not exists or failed to create.</returns>
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

        /// <inheritdoc/>
        public abstract bool OverwriteFile(T value);

        /// <inheritdoc/>
        public abstract bool OverwriteFile(FileStream fs, T value);

        /// <inheritdoc/>
        public abstract bool ReadFile(out T value);


        /// <summary> Check if directory.</summary>
        /// <param name="path"> The path to check if directory.</param>
        public static bool IsDirectory(string path){
            //source: https://stackoverflow.com/a/19596821
            //edited a part which was using System.Linq

            if (path == null) throw new ArgumentNullException("path");
            path = path.Trim();

            if (Directory.Exists(path)) 
                return true;

            if (System.IO.File.Exists(path)) 
                return false;

            // neither file nor directory exists. guess intention

            // if has trailing slash then it's a directory
            char[] dirSepChar = new char[2] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
            foreach(char _char in dirSepChar){
                //end with slash or directory seperator
                if(path.EndsWith(_char)){
                    return true;
                }
            }

            // if has extension then its a file; directory otherwise
            return string.IsNullOrWhiteSpace(Path.GetExtension(path));
        }

        /// <summary> Check if a valid file name.</summary>
        /// <param name="name"> The name to check.</param>
        public static bool IsValidFilename(string name){
            try{
                System.IO.File.OpenRead(name).Close();
            } catch (ArgumentException) { 
                //invalid name
                return false;

            //other such does not exists
            } catch (Exception){}

            //valid
            return true;
        }
    }
}