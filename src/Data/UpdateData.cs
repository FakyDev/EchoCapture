namespace EchoCapture.Data{
    
    /// <summary> Struct used for updating setting in application data file.</summary>
    public struct UpdateData{

        /// <summary> Will hold the data type.</summary>
        private DataType updateType;

        /// <summary> Will hold the new path.</summary>
        private string path;

        /// <summary> Will hold the timeout values.</summary>
        private int? timeout;

        /// <summary> (Get only) Return the update type.</summary>
        public DataType UpdateType{
            get{
                return this.updateType;
            }
        }

        /// <summary> (Get only) Return the new path.</summary>
        /// <remarks> Work only if struct is for updating path.</remarks>
        public string Path{
            get{
                return this.path;
            }
        }

        /// <summary> (Get only) Return the new timeout value.</summary>
        /// <remarks> Work only if struct is for updating timeout.</remarks>
        public int? Timeout{
            get{
                return this.timeout;
            }
        }

        /// <summary> For updating path, in data file.</summary>
        /// <param name="newPath"> The new path.</param>
        public UpdateData(string newPath){
            this.updateType = DataType.path;
            this.path = newPath;
            this.timeout = null;
        }

        /// <summary> For updating timeout, in data file.</summary>
        /// <param name="newTimeout"> The new value.</param>
        public UpdateData(int newTimeout){
            this.updateType = DataType.timeout;
            this.timeout = newTimeout;
            this.path = null;
        }

        /// <summary> Define the update of application data file.</summary>
        public enum DataType{

            /// <summary> Updates the path of the directory to save screenshots.</summary>
            path = 0,

            /// <summary> Updates the time interval between screenshots.</summary>
            timeout,
        }
    }
}