using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace EchoCapture.Data.File{

    /// <summary> Allows you to read, create and parse JSON file.</summary>
    public class JsonFile<T> : FileBase<List<T>>{

        /// <param name="name"> The file name.</param>
        /// <param name="path"> The file path.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public JsonFile(string name, string path) : base(name, FileExtension.json, path){}

        /// <inheritdoc/>
        /// <returns> Return true, if file is overwriten else file does not exists
        /// or failed to overwrite.</returns>
        public override bool OverwriteFile(List<T> value){
            //check if don't exists
            if(!this.FileExists){
                return false;
            }

            //serialise object to json
            byte[] serialisedJSON = JsonSerializer.SerializeToUtf8Bytes(value);

            try{
                //overwrite file; File.Create() remove all content if already exists
                using(FileStream fs = System.IO.File.Create(this.FullPath)){
                    //update file
                    fs.Write(serialisedJSON, 0, serialisedJSON.Length);
                }
            } catch (Exception){
                //failed
                return false;
            }

            return true;
        }
    
        /// <inheritdoc/>
        /// <remarks> Make sure to free resource after.</remarks>
        /// <returns> Return true, if file is overwritten else file does not exist,
        /// failed to overwrite or filestream is invalid.</returns>
        public override bool OverwriteFile(FileStream fs, List<T> value){
            //check if not file or cannot write
            if(fs.Name != this.FullPath || !fs.CanWrite){
                return false;
            }

            //serialise object to json
            byte[] serialisedJSON = JsonSerializer.SerializeToUtf8Bytes(value);

            try{
                //update file
                fs.Write(serialisedJSON, 0, serialisedJSON.Length);
            } catch (Exception){
                //failed
                return false;
            }
            
            return true;
        }

        /// <inheritdoc/>
        /// <returns> Return true if file is read and parsed json to object successfully, otherwise
        /// failed to read file or parse json.</returns>
        public override bool ReadFile(out List<T> value){
            //default value
            value = null;

            //check if don't exists
            if(!this.FileExists){
                return false;
            }

            //will hold the file content
            string fileContent;

            try{
                //get steam reader for file
                using(StreamReader sr = System.IO.File.OpenText(this.FullPath)){
                    //get file content as string
                    fileContent = sr.ReadToEnd();
                }
            } catch (Exception){
                //failed
                return false;
            }

            //try to convert to object
            try{
                value = JsonSerializer.Deserialize<List<T>>(fileContent);
            } catch (Exception){
                //failed to parse json
                return false;
            }

            return true;
        }

    }
}