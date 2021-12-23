using System.Threading.Tasks;

namespace EchoCapture.Data.File.Async{

    /// <summary> Interface for file class, which uses asynchronous operation.</summary>
    public interface IAsyncFile<T>{

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

        /// <summary> Create the file asynchronously.</summary>
        Task CreateFileAsync();

        /// <summary> Create the file for asynchronous operation, and pass the filestream.</summary>
        /// <remarks> You may want to use Task.Run(), then pass this function.</remarks>
        /// <param name="fs"> The filestream passed.</param>
        bool CreateFileAsync(out System.IO.FileStream fs);

        /// <summary> Delete the file asynchronously.</summary>
        Task DeleteFileAsync();

        /// <summary> Overwrite the existing file asynchronously.</summary>
        Task OverwriteFileAsync(T value);

        /// <summary> Overwrite the existing file asynchronously, with the filestream provided.</summary>
        Task OverwriteFileAsync(System.IO.FileStream fs, T value);

        /// <summary> Read the files asynchronously and return its content.</summary>
        Task<T> ReadFileAsync();
    }
}