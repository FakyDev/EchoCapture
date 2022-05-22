namespace EchoCapture.Data.File.Image{

    /// <summary> Allows you to perform asynchronous and synchronous file operation to PNG image file.</summary>
    public sealed class PngFile : ImageFile{
        
        /// <param name="name">The name of the png image.</param>
        /// <param name="path">The directory the png image is located in.</param>
        public PngFile(string name, string path) : base(name, path, FileExtension.png){}

    }
}