using System;
using File = EchoCapture.Data.File.Text;

namespace EchoCapture.Exceptions.Data.IniFile{
    /// <summary> Defining failure to pass a value in one of the line(s) of an ini file.</summary>
    public sealed class IniLineDataParsingException<T> : Exception{
        public IniLineDataParsingException(){}

        /// <param name="instance"> The instance, containing the type desired.</param>
        public IniLineDataParsingException(File.IniFile.IniLine<T> instance) : base(IniLineDataParsingException<T>.CreateMessage(instance, null)){}

        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="typeGotInstead"> The type of the value, which was found instead.</param>
        public IniLineDataParsingException(File.IniFile.IniLine<T> instance, Type typeGotInstead) : base(IniLineDataParsingException<T>.CreateMessage(instance, typeGotInstead)){}

        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="inner"> The inner exception.</param>
        public IniLineDataParsingException(File.IniFile.IniLine<T> instance, Exception inner) : base(IniLineDataParsingException<T>.CreateMessage(instance, null), inner){}

        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="typeGotInstead"> The type of the value, which was found instead.</param>
        /// <param name="inner"> The inner exception.</param>
        public IniLineDataParsingException(File.IniFile.IniLine<T> instance, Type typeGotInstead, Exception inner) : base(IniLineDataParsingException<T>.CreateMessage(instance, typeGotInstead), inner){}

        /// <summary> Create the exception message.</summary>
        /// <remarks> <paramref name="typeGotInstead"/> can be leave null, if it isn't discovered.</remarks>
        /// <param name="instance"> The instance, containing the type desired.</param>
        /// <param name="typeGotInstead"> The type of the value, which was found instead.</param>
        private static string CreateMessage(File.IniFile.IniLine<T> instance, Type typeGotInstead){
            //return message
            if(typeGotInstead == null){
                return $"Failed to parse value into {instance.ValueType.ToString()}. Value was found to be type of {typeGotInstead.ToString()}.";
            }
            return $"Failed to parse value into {instance.ValueType.ToString()}.";
        }
    }
}