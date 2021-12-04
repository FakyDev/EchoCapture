namespace Screenshoter.Data.File{

    /// <summary> Interface for a file class.</summary>
    public interface IFile<T>{

        /// <summary> (Get only) Return the file name.</summary>
        string Name{
            get;
        }

        /// <summary> (Get only) Return the file path, without name and extension.</summary>
        string FilePath{
            get;
        }

        /// <summary> (Get only) Return the full file path.</summary>
        string FullPath{
            get;
        }

        /// <summary> (Get only) Return the file extension.</summary>
        FileExtension Extension{
            get;
        }

        /// <summary> (Get only) Check if file exists on the drive.</summary>
        bool FileExists{
            get;
        }

        /// <summary> Create the file.</summary>
        bool CreateFile();

        /// <summary> Create the file, and pass file stream.</summary>
        bool CreateFile(out System.IO.FileStream fs);

        /// <summary> Delete the file.</summary>
        bool DeleteFile();

        /// <summary> Overwrite the existing file.</summary>
        bool OverwriteFile(T value);

        /// <summary> Overwrite the existing file, with the filestream provided.</summary>
        bool OverwriteFile(System.IO.FileStream fs, T value);

        /// <summary> Read the files and return its content.</summary>
        bool ReadFile(out T value);
    }
}