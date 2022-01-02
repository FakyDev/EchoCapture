namespace EchoCapture.Data.File.Image{

    /// <summary> Allows you to perform asynchronous and synchronous file operation to JPEG image file.</summary>
    public sealed class JpgFile : ImageFile{

        /// <param name="name">The name of the jpeg image.</param>
        /// <param name="path">The directory the jpeg image is located in.</param>
        public JpgFile(string name, string path) : base(name, path, FileExtension.jpg){}
    }
}