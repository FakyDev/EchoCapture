using System;
using File = EchoCapture.Data.File.Text;

namespace EchoCapture.Exceptions.Data.IniFile{
    
    /// <summary> Defining failure to pass or search a value in one of the line(s) of an ini file.</summary>
    public sealed class IniLineDataParsingException : Exception{
        public IniLineDataParsingException(){}

        /// <summary> For construction when searching key value line.</summary>
        /// <param name="instance"> The instance, containing the type.</param>
        /// <param name="typeSearchedFor"> The type of the value, which was waiting for.</param>
        public IniLineDataParsingException(Type typeSearchedFor, File.IniFile.IniLine instance) : base($"Value is type of {instance.ValueType.ToString()} instead of {typeSearchedFor.ToString()}."){}


        /// <param name="instance"> The instance, containing the type desired.</param>
        public IniLineDataParsingException(File.IniFile.IniLine instance) : base(IniLineDataParsingException.CreateMessage(instance, null)){}

        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="typeGotInstead"> The type of the value, which was found instead.</param>
        public IniLineDataParsingException(File.IniFile.IniLine instance, Type typeGotInstead) : base(IniLineDataParsingException.CreateMessage(instance, typeGotInstead)){}

        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="inner"> The inner exception.</param>
        public IniLineDataParsingException(File.IniFile.IniLine instance, Exception inner) : base(IniLineDataParsingException.CreateMessage(instance, null), inner){}

        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="typeGotInstead"> The type of the value, which was found instead.</param>
        /// <param name="inner"> The inner exception.</param>
        public IniLineDataParsingException(File.IniFile.IniLine instance, Type typeGotInstead, Exception inner) : base(IniLineDataParsingException.CreateMessage(instance, typeGotInstead), inner){}

        /// <summary> Create the exception message.</summary>
        /// <remarks> <paramref name="typeGotInstead"/> can be leave null, if it isn't discovered.</remarks>
        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="typeGotInstead"> The type of the value, which was found instead.</param>
        private static string CreateMessage(File.IniFile.IniLine instance, Type typeGotInstead){
            //return message
            if(typeGotInstead != null){
                return $"Failed to parse value into {instance.ValueType.ToString()}. Value was found to be type of {typeGotInstead.ToString()}.";
            }
            return $"Failed to parse value into {instance.ValueType.ToString()}.";
        }
    }
}