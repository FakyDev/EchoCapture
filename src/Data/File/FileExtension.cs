namespace EchoCapture.Data.File{

    /// <summary> Hold valid extension which the application can parse.</summary>
    public enum FileExtension{

        /// <summary> The extension of json file.</summary>
        json = 0,

        /// <summary> The extension of an image file.</summary>
        png,

        /// <summary> The extension of an image file.</summary>
        jpeg,

        /// <summary> The extension of a text file.</summary>
        txt,

        /// <summary> The extension of a text file, which is used to store logs.</summary>
        log,

        /// <summary> The extension of a text file, which is used to store config.</summary>
        ini
    }
}