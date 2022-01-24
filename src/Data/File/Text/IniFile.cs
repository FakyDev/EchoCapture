using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using EchoCapture.Exceptions.Data.IniFile;

namespace EchoCapture.Data.File.Text{

    public class IniFile : TextFile{

        /// <summary> Regex for checking numeric string.</summary>
        private static readonly Regex IsNumericRegex = new Regex(@"^\d+$");
        
        /// <summary> The character used to define the value by its key.</summary>
        private const char SELECTOR = '=';

        /// <summary> The character which determine the line is commented or inline comment.</summary>
        private const char COMMENT = ';';

        /// <summary> The alternate character which determine the line is commented or inline comment.</summary>
        private const char ALT_COMMENT = '#';

        /// <summary> The character which determine the start of a section name.</summary>
        private const char START_SECTION = '[';

        /// <summary> The character which determine the end of a section name.</summary>
        private const char END_SECTION = ']';

        /// <summary> Array of char for commenting in the ini file.</summary>
        private static char[] COMMENT_CHARS{
            get{
                return new char[2]{IniFile.COMMENT, IniFile.ALT_COMMENT};
            }
        }


        /// <summary> Array of characters to escape.</summary>
        private static char[] ToEscapeChar = new char[]{IniFile.SELECTOR, IniFile.COMMENT, IniFile.ALT_COMMENT};

        public IniFile(string name, string path) : base(name, path, Encoding.UTF8, FileExtension.ini){}

        /// <summary> Split line for each break line character, return them in an array of string.</summary>
        /// <remarks> If no break line char is found, return an array of size one with the text passed.</remarks>
        /// <exception cref="System.ArgumentException"> Thrown when <paramref name="text"/> is empty.</exception>
        public static string[] SeperateLines(string text){
            //check
            if(text.Length == 0 || text == string.Empty){
                throw new ArgumentException("Text passed is empty");
            }

            //will hold the index of new line char
            List<int> newLineCharsFound = new List<int>();
            //hold the start that will start search
            int startIndex = 0;

            //search for new lines
            while(true){
                int newLineIndex;
                try{
                    //get of the new line char
                    newLineIndex = text.IndexOf("\n", startIndex);
                } catch (ArgumentOutOfRangeException){
                    break;
                }

                //stop loop, not found
                if(newLineIndex == -1){
                    break;
                }

                try{
                    //check if escaped
                    if(text[newLineIndex-1] == '\\'){
                        break;
                    }
                } catch (IndexOutOfRangeException){
                    break;
                }

                //update
                newLineCharsFound.Add(newLineIndex);
                startIndex = newLineIndex+2;
            }

            //not break line char
            if(newLineCharsFound.Count == 0){
                return new string[1]{text};
            }

            //array that will hold the line
            string[] lines = new string[newLineCharsFound.Count+1];

            //seperates line
            for(int i = 0; i <= newLineCharsFound.Count; i++){
                //for the unidentified part
                if(i == newLineCharsFound.Count){
                    //get line
                    lines[i] = text.Substring(newLineCharsFound[newLineCharsFound.Count - 1]+1);
                    break;
                }

                int startingIndex = (i == 0 ? 0 : newLineCharsFound[i-1]+1);
                //hold ending index
                int endingIndex;
                
                //check if its both first and last
                if(newLineCharsFound[i] == newLineCharsFound[0] && newLineCharsFound[i] == newLineCharsFound[newLineCharsFound.Count - 1]){
                    endingIndex = newLineCharsFound[newLineCharsFound.Count - 1];

                //check if first only
                } else if(newLineCharsFound[i] == newLineCharsFound[0]){
                    endingIndex = newLineCharsFound[0];

                //check if last only
                } else if(newLineCharsFound[i] == newLineCharsFound[newLineCharsFound.Count - 1]){
                    endingIndex = newLineCharsFound[newLineCharsFound.Count - 1];
                } else {
                    endingIndex = newLineCharsFound[i];
                }

                //get length
                int length = startingIndex == 0 ? endingIndex : endingIndex - startingIndex;

                //get line
                lines[i] = text.Substring(startingIndex, length);
            }

            return lines;
        }

        private class ParsedIni{
            
            /// <summary> Determine if the instance is representing a section.</summary>
            private bool isSection;

            /// <summary> Hold key-value pair, of the data</summary>
            private Dictionary<string, bool> dataValue = null;

            //private LinkedList<string, bool> a;

            /// <summary> Hold sections of the parsed ini.</summary>
            private ParsedIni[] sections = null;

            private ParsedIni(bool section){
                this.isSection = section;
            }

            private ParsedIni(bool section, ParsedIni[] sections) : this(section){
                if(sections == null){
                    //to-do: throw exception
                }
                //update
                this.sections = sections;
            }

            private ParsedIni(string[] content){
                
            }
        }

        /// <summary> Structure representing a line in an ini file, with parsed value.</summary>
        public struct IniLine<T>{
            
            /// <summary> The full parsed line.</summary>
            private string line;

            /// <summary> The commented text, including the comment char.</summary>
            private string inlineComment;

            /// <summary> Hold the name of the section header.</summary>
            /// <remarks> Null if there's no header.</remarks>
            private string sectionHeader;

            /// <summary> The key for retrieving the value.</summary>
            private string key;

            /// <summary> The value retrived.</summary>
            private object value;

            /// <summary> The type of this line.</summary>
            private IniLineType? lineType;


            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine{T}.line"/>
            /// <remarks> (Get only)</remarks>
            public string Line{
                get{
                    return this.line;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine{T}.inlineComment"/>
            /// <remarks> (Get only)</remarks>
            public string InlineComment{
                get{
                    return this.inlineComment;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine{T}.sectionHeader"/>
            /// <remarks> (Get only)</remarks>
            public string SectionHeader{
                get{
                    return this.sectionHeader;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine{T}.key"/>
            /// <remarks> (Get only)</remarks>
            public string Key{
                get{
                    return this.key;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine{T}.value"/>
            /// <remarks> (Get only)</remarks>
            public T Value{
                get{
                    return (T) this.value;
                }
            }

            /// <summary> (Get only) Return the value type.</summary>
            public Type ValueType{
                get{
                    return typeof(T);
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine{T}.lineType"/>
            /// <remarks> (Get only)</remarks>
            public IniLineType LineType{
                get{
                    return (IniLineType) this.lineType;
                }
            }


            /// <summary> Create instance from parsing the line.</summary>
            /// <exception cref="System.InvalidOperationException"> Thrown when type is invalid.</exception>
            public IniLine(string line){
                //specify type
                if(typeof(T) != typeof(int) && typeof(T) != typeof(float) && typeof(T) != typeof(string) && typeof(T) != typeof(bool)){
                    throw new InvalidOperationException("Invalid T type passed. T can only be int, float, boolean or string");
                }

                //nullify fields
                this.line = null;
                this.inlineComment = null;
                this.key = null;
                this.value = null;
                this.sectionHeader = null;
                this.lineType = null;

                //parse line
                this.ParseLine(line);
            }

            /// <summary> Retrieves the information from the line and update the fields.</summary>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException{T}"> Thrown when failed to parse value.</exception>
            private void ParseLine(string line){
                //update reference
                this.line = line;

                //split into two part; one for key and the other value
                //and trim whitespace
                string[] part = line.Split(IniFile.SELECTOR, 2, StringSplitOptions.TrimEntries);

                //check if it's a valid key-value
                if(part.Length != 2 || String.IsNullOrEmpty(part[0]) || String.IsNullOrEmpty(part[1])){
                    //determine if it's a section header
                    bool? isSectionHeader = null;

                    //check if it's a section header
                    if(part[0].StartsWith(IniFile.START_SECTION)){
                        //get index of end section header char
                        int endSectionCharIndex = part[0].IndexOf(IniFile.END_SECTION);

                        //check if exists end section header char
                        if(endSectionCharIndex != -1){
                            //check if does have a section name
                            if(endSectionCharIndex - 1 > 0){
                                //update state
                                isSectionHeader = true;
                                //update section header
                                this.sectionHeader = part[0].Substring(1, endSectionCharIndex-1);
                            }
                        }
                    }

                    //check if it's a comment
                    for(int i = 0; i < IniFile.COMMENT_CHARS.Length; i++){
                        if(isSectionHeader == true){
                            //seperate string
                            string[] subPart = part[0].Split(IniFile.COMMENT_CHARS[i], 2, StringSplitOptions.TrimEntries);

                            //check if contains addional start/end section
                            //thus making it invalid
                            if((subPart[0].Split(IniFile.START_SECTION).Length - 1) != 1 || (subPart[0].Split(IniFile.END_SECTION).Length - 1) != 1){
                                //update state
                                isSectionHeader = null;
                                break;
                            }
                            
                            try{
                                //update comment
                                this.inlineComment = IniFile.COMMENT_CHARS[i] + subPart[1];
                                break;
                            } catch (IndexOutOfRangeException){}
                        }
                        if(part[0][0] == IniFile.COMMENT_CHARS[i]){
                            isSectionHeader = false;

                            //update comment
                            this.inlineComment = IniFile.COMMENT_CHARS[i] + part[0];
                            break;
                        }
                    }

                    if(isSectionHeader == null){
                        //update reference
                        this.lineType = IniLineType.Invalid;
                    }

                    //for section
                    if(isSectionHeader == true){
                        //update reference
                        this.lineType = IniLineType.SectionHeader;
                    //for comment
                    } else {
                        //update reference
                        this.lineType = IniLineType.FullyCommented;
                    }
                } else {
                    //update line type
                    this.lineType = IniLineType.KeyValue;

                    //check if valid key
                    if(part[0].Split(' ').Length == 0){
                        //update key
                        this.key = part[0];
                    } else {
                        //update line type
                        this.lineType = IniLineType.Invalid;
                    }

                    //get type code
                    TypeCode tTypeCode = Type.GetTypeCode(typeof(T));
                    //hold the value part and comment part
                    string valuePart = null;
                    string commentPart = null;

                    //check if it's a comment
                    for(int i = 0; i < IniFile.COMMENT_CHARS.Length; i++){
                        //hold the index of comment char
                        int? charIndex = null;
                        //loop until the char is found or is declare not found
                        while(true){
                            try{
                                //update
                                charIndex = part[1].IndexOf(IniFile.COMMENT_CHARS[i], (charIndex == null ? 0 : (int)charIndex+1));
                            } catch (IndexOutOfRangeException){
                                //nullify
                                charIndex = null;
                                break;
                            }

                            //not found
                            if(charIndex == -1){
                                //nullify
                                charIndex = null;
                                break;
                            }

                            //check if esacaped
                            if(charIndex - 1 > 0 && part[1][(int)charIndex-1] == '\\'){
                                continue;
                            }

                            //stop while loop with found char index
                            break;
                        }

                        //start other iteration
                        if(charIndex == null){
                            continue;
                        }

                        //check if comment char is first character
                        if(charIndex == 0){
                            //update parts
                            commentPart = part[1];
                            break;
                        }

                        //update parts
                        valuePart = part[1].Substring(0, (int)charIndex-1);
                        commentPart = part[1].Substring((int)charIndex);
                        break;
                    }
                    
                    if(valuePart == null){
                        //update line type
                        this.lineType = IniLineType.Invalid;

                        return;
                    }

                    //update comment
                    this.inlineComment = commentPart;

                    switch(tTypeCode){
                        case TypeCode.String:
                            //check if invalid
                            if(valuePart == bool.TrueString || valuePart == bool.FalseString){
                                //throw exception
                                throw new IniLineDataParsingException<T>(this, typeof(bool));
                            } else {
                                if(IniFile.IsNumericRegex.IsMatch(valuePart)){
                                    //throw exception
                                    throw new IniLineDataParsingException<T>(this, typeof(int));
                                } else {
                                    //split into two; for float
                                    string[] splittedNumber = valuePart.Split('.', 2);
                                    //check if float
                                    if(splittedNumber.Length == 2){
                                        if(IniFile.IsNumericRegex.IsMatch(splittedNumber[0]) && IniFile.IsNumericRegex.IsMatch(splittedNumber[1])){
                                            //throw exception
                                            throw new IniLineDataParsingException<T>(this, typeof(float));
                                        }
                                    }
                                }
                            }
                            
                            //will hold the removed escape comment value
                            string removedEspaceValue = valuePart;
                            //loop through comment chars
                            foreach(char comment in IniFile.COMMENT_CHARS){
                                //upate
                                removedEspaceValue = valuePart.Replace($"\\{comment.ToString()}", comment.ToString());
                            }
                            //update value
                            this.value = (object) removedEspaceValue;
                        break;

                        case TypeCode.Boolean:
                            //true
                            if(valuePart == bool.TrueString){
                                //update value
                                this.value = (object) true;

                            //false
                            } else if(valuePart == bool.FalseString){
                                //update value
                                this.value = (object) false;
                            } else {
                                //throw exception
                                throw new IniLineDataParsingException<T>(this);
                            }
                        break;

                        case TypeCode.Int32:
                            //will hold the value
                            int i_value;

                            try{
                                //parse
                                i_value = Int32.Parse(valuePart);
                            } catch(Exception){
                                //throw exception
                                throw new IniLineDataParsingException<T>(this);
                            }

                            //update value
                            this.value = (object) i_value;
                        break;

                        case TypeCode.Single:
                            //will hold the value
                            float f_value;

                            try{
                                //parse
                                f_value = Single.Parse(valuePart);
                            } catch(Exception){
                                //throw exception
                                throw new IniLineDataParsingException<T>(this);
                            }

                            //update value
                            this.value = (object) f_value;
                        break;
                    }
                }
            }
        }

        /// <summary> List of constant defining line type based on the line-content.</summary>
        public enum IniLineType{
            
            /// <summary> Defines a line which is has a key and value pair.</summary>
            /// <remarks> Inline-comment is allowed.</remarks>
            KeyValue = 0,

            /// <summary> Defines a line which creates a section.</summary>
            /// <remarks> Inline-comment is allowed.</remarks>
            SectionHeader,

            /// <summary> Defines a fully commented line.</summary>
            FullyCommented,

            /// <summary> Defines an invalid line.</summary>
            Invalid
        }
    }
}