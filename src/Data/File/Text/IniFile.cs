using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text;

using EchoCapture.Exceptions.Data.IniFile;

namespace EchoCapture.Data.File.Text{

    /// <summary> Allows to perform synchronous and asynchronous file-related operations to ini file, allow parsing to ini line, vice-versa and much more.</summary>
    public class IniFile : TextFile{

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

        /// <summary> Hold the instance of the parsed ini which is representing this file.</summary>
        /// <remarks> This will be used to save and update ini files, and it will be used to represent content read from an ini file.</remarks>
        private ParsedIni parsed_ini = null;

        /// <summary> (Get only) Return the parsed ini representing this file.</summary>
        public ParsedIni Parsed_ini{
            get{
                return this.parsed_ini;
            }
        }

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
                startIndex = newLineIndex+1;
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


        /// <summary> Reads the file and update the instance representing the file.</summary>
        /// <remarks> If failed, you might want to update this representing instance to an empty one by using <see cref="EchoCapture.Data.File.Text.IniFile.Emptify"/></remarks>
        /// <returns> True on success of reading and updating the representing instance.</returns>
        public bool Load(){
            //read lines
            string lines;

            //failed to read file
            if(!this.ReadFile(out lines)){
                return false;
            }

            //empty file
            if(lines == "" || String.IsNullOrEmpty(lines)){
                this.parsed_ini = ParsedIni.EmptyParsedIni;

                return true;
            }

            //parse lines
            try{
                this.parsed_ini = new ParsedIni(IniFile.SeperateLines(lines));
            } catch (Exception){
                return false;
            }

            return true;
        }

        /// <summary> Save the representing instance to the file.</summary>
        /// <remarks> True if successfully saved else failed.</remarks>
        public bool Save(){
            //get string
            string combinedLines = this.parsed_ini.ToRawString();

            //if empty string
            //update so that no exception is thrown
            if(combinedLines == "" || String.IsNullOrEmpty(combinedLines)){
                combinedLines = " \n";
            }

            try{
                return this.OverwriteFile(combinedLines);
            } catch (Exception){
                return false;
            }
        }

        /// <summary> Update the instance representing this file to an empty parsed instance.</summary>
        public void Emptify(){
            this.parsed_ini = ParsedIni.EmptyParsedIni;
        }
        
        /// <summary> Reads the file, asynchronously, and update the instance representing the file.</summary>
        /// <remarks> If failed, you might want to update this representing instance to an empty one by using <see cref="EchoCapture.Data.File.Text.IniFile.Emptify"/></remarks>
        /// <returns> True on success of reading and updating the representing instance.</returns>
        public async Task<bool> LoadAsync(){
            //read lines async
            Task<string> readingTask = this.ReadFileAsync();
            //will hold lines
            string lines;

            try{
                //wait for finish task and update
                await readingTask;
                lines = readingTask.Result;
            } catch (Exception){
                return false;
            }

            //empty file
            if(lines == "" || String.IsNullOrEmpty(lines)){
                this.parsed_ini = ParsedIni.EmptyParsedIni;

                return true;
            }

            //parse lines
            try{
                this.parsed_ini = new ParsedIni(IniFile.SeperateLines(lines));
            } catch (Exception){
                return false;
            }

            return true;
        }

        /// <summary> Save the representing instance to the file, asynchronously.</summary>
        /// <remarks> True if successfully saved else failed.</remarks>
        public async Task<bool> SaveAsync(){
            //get string
            string combinedLines = this.parsed_ini.ToRawString();

            //if empty string
            //update so that no exception is thrown
            if(combinedLines == "" || String.IsNullOrEmpty(combinedLines)){
                combinedLines = " \n";
            }

            try{
                //overwrites asynchronously
                await this.OverwriteFileAsync(combinedLines);
            } catch (Exception){
                //failed
                return false;
            }

            return true;
        }


        /// <summary> Overwrite the existing file with <paramref name="combinedLines"/>.</summary>
        /// <param name="combinedLines"> Lines to parse into ini line, and seperated using a line break char.</param>
        /// <remarks> Update the representing instance and use <see cref="EchoCapture.Data.File.Text.IniFile.Save"/> instead of this method. <br/>
        /// On no exception relating to parsing line (argument exception), the instance representing the file will be updated with <paramref name="combinedLines"/>
        /// , even though if failed to update file.</remarks>
        /// <returns> True if overwritten the file otherwise, either failed to save file or get filestream.</returns>
        /// <inheritdoc/>
        public override bool OverwriteFile(string combinedLines){
            //splits into lines
            string[] lines = IniFile.SeperateLines(combinedLines);

            //try to parse lines
            this.parsed_ini = new ParsedIni(lines);

            //base method
            //overwrite file with raw string
            return base.OverwriteFile(this.parsed_ini.ToRawString());
        }

        /// <summary> Overwrite the existing file using the provided file stream.</summary>
        /// <remarks> Update the representing instance and use <see cref="EchoCapture.Data.File.Text.IniFile.Save"/> instead of this method. <br/>
        /// Make sure to free resource after. On no exception relating to parsing line (argument exception), the instance representing the file will be
        /// updated with <paramref name="combinedLines"/>, even though if failed to update file.  Cannot guarantee if data is fully
        /// overwritten, in case when new text length is less than old text length.</remarks>
        /// <param name="fs"> The file stream to use.</param>
        /// <param name="combinedLines"> Lines to parse into ini line, and seperated using a line break char.</param>
        /// <returns> True if overwritten the file otherwise, either failed to save file or invalid filestream.</returns>
        public override bool OverwriteFile(FileStream fs, string combinedLines){
            //splits into lines
            string[] lines = IniFile.SeperateLines(combinedLines);

            //try to parse lines
            this.parsed_ini = new ParsedIni(lines);

            //base method
            //overwrite file with raw string
            return base.OverwriteFile(fs, this.parsed_ini.ToRawString());
        }


        /// <summary> Overwrite the existing file with <paramref name="combinedLines"/>, asynchronously.</summary>
        /// <param name="combinedLines"> Lines to parse into ini line, and seperated using a line break char.</param>
        /// <remarks> Update the representing instance and use <see cref="EchoCapture.Data.File.Text.IniFile.SaveAsync"/> instead of this method. <br/>
        /// On no exception relating to parsing line (argument exception), the instance representing the file will be updated with <paramref name="combinedLines"/>
        /// , even though if failed to update file.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists.</exception>
        /// <exception cref="System.ArgumentException"> Thrown when failed to encode text.</exception>
        /// <inheritdoc/>
        public async override Task OverwriteFileAsync(string combinedLines){
            //splits into lines
            string[] lines = IniFile.SeperateLines(combinedLines);

            //try to parse lines
            this.parsed_ini = new ParsedIni(lines);

            //base async method
            //overwrite file with raw string
            await base.OverwriteFileAsync(this.parsed_ini.ToRawString());
        }

        /// <summary> Overwrite the existing file with <paramref name="combinedLines"/> using the provided file stream, asynchronously.</summary>
        /// <param name="combinedLines"> Lines to parse into ini line, and seperated using a line break char.</param>
        /// <remarks> Update the representing instance and use <see cref="EchoCapture.Data.File.Text.IniFile.SaveAsync"/> instead of this method. <br/>
        /// Make sure to free resource after. On no exception relating to parsing line (argument exception), the instance
        /// representing the file will be updated with <paramref name="combinedLines"/>, even though if failed to update file.
        /// Cannot guarantee if data is fully overwritten, in case when new text length is less than old text length.</remarks>
        /// <exception cref="System.InvalidOperationException"> Thrown when file doesn't exists, or filestream is invalid.</exception>
        /// <exception cref="System.ArgumentException"> Thrown when failed to encode text.</exception>
        /// <inheritdoc/>
        public async override Task OverwriteFileAsync(FileStream fs, string combinedLines){
            //splits into lines
            string[] lines = IniFile.SeperateLines(combinedLines);

            //try to parse lines
            this.parsed_ini = new ParsedIni(lines);

            //base async method
            //overwrite file with raw string
            await base.OverwriteFileAsync(fs, this.parsed_ini.ToRawString());
        }

        /// <summary> Object representing the data structurely of an ini file.</summary>
        public class ParsedIni{
            
            /// <summary> Determine if the instance is representing a section.</summary>
            private bool isSection = false;

            /// <summary> Holds the header line.</summary>
            /// <remarks> Not null only if representing a section.</remarks>
            private IniLine? headerLine = null;

            /// <summary> List holding parsed ini line.</summary>
            /// <remarks> Represent data for current section or global, depends on <see cref="EchoCapture.Data.File.Text.IniFile.ParsedIni.isSection"/></remarks>
            private List<IniLine> parsedLines = new List<IniLine>();

            /// <summary> Hold a sections of the ini.</summary>
            /// <remarks> Not null only if representing global.</remarks>
            private Dictionary<string, ParsedIni> sections = null;
            
            /// <summary> Hold the name of the current key checking against for.</summary>
            private string currentKeyNameCheck = null;

            /// <summary> (Get only) Return empty parsed ini.</summary>
            public static ParsedIni EmptyParsedIni{
                get{
                    return new ParsedIni();
                }
            }

            /// <summary> Create with string array to parse into ini lines.</summary>
            /// <exception cref="System.ArgumentException"> Thrown when string array is empty, or its content is invalid.</exception>
            /// <exception cref="System.ArgumentNullException"> Thrown when string array is null.</exception>
            public ParsedIni(string[] content){
                //throw exception
                if(content == null){
                    throw new ArgumentNullException();
                }
                if(content.Length == 0){
                    throw new ArgumentException("Array is empty.");
                }
                //update field
                this.sections = new Dictionary<string, ParsedIni>();
                //do all work
                this.Initialise(content);
            }

            /// <summary> Private constructor for sections.</summary>
            /// <remarks> List of ini line, includes the header line which will later be removed.</remarks>
            private ParsedIni(List<IniLine> parsedLines){
                //update state
                this.isSection = true;
                //do all work for section
                this.SectionInitialise(parsedLines);
            }

            /// <summary> Private default constructor.</summary>
            private ParsedIni(){}

            /// <summary> Initialise the instance for global and create section instances.</summary>
            /// <param name="content"> The ini file content to parse.</param>
            /// <exception cref="System.ArgumentException"> Thrown when one of <paramref name="content"/>, failed to parsed into an ini line. Another case is when
            /// two different parsed lines contain same key.</exception>
            private void Initialise(string[] content){
                //will hold index of header lines
                List<int> headerIndex = new List<int>();
                //hold temp the parsed lines
                List<IniLine> tempParsedLines = new List<IniLine>();
                //determine if allowed to update list
                bool canUpdateList = true;

                //loop through lines
                for(int i = 0; i < content.Length; i++){
                    //parse line
                    IniLine parsedLine = new IniLine(content[i]);
                    //update list
                    tempParsedLines.Add(parsedLine);

                    //invalid line
                    if(parsedLine.LineType == IniLineType.Invalid){
                        throw new ArgumentException("Failed to parse a line into an ini line, from the string array.");

                    //header line type
                    } else if(parsedLine.LineType == IniLineType.SectionHeader){
                        //update header index
                        headerIndex.Add(i);
                        //disable
                        canUpdateList = false;

                    } else if(canUpdateList){
                        //update list
                        this.parsedLines.Add(parsedLine);
                    }
                }

                //contains duplicate key
                if(!this.DuplicateKeyCheck()){
                    throw new ArgumentException("Parsed ini lines contain duplicate keys.");
                }

                //looping through headerIndex to create new instance of ParsedIni as subsection
                for(int i = 0; i < headerIndex.Count; i++){
                    //get starting index
                    int startIndex = headerIndex[i];
                    //get ending index
                    int endingIndex;
                    try{
                        //calculate ending index
                        endingIndex = headerIndex[i+1]-1;
                    } catch (ArgumentOutOfRangeException){
                        //get last index
                        endingIndex = content.GetUpperBound(0);
                    }

                    //copy the section part
                    List<IniLine> sectionIniLines = tempParsedLines.GetRange(startIndex, endingIndex-startIndex+1);
                    //create instance for section
                    ParsedIni sectionIni = new ParsedIni(sectionIniLines);

                    //update with key, name
                    this.sections.Add((string)sectionIni.headerLine.Value.SectionHeader, sectionIni);
                }
            }

            /// <summary> Initialise the instance for section.</summary>
            /// <param name="parsedLines"> The already passed ini lines.</param>
            /// <exception cref="System.ArgumentException"> Thrown when two different parsed lines contain same key.</exception>
            private void SectionInitialise(List<IniLine> parsedLines){
                //update header
                this.headerLine = parsedLines[0];
                //remove from list
                parsedLines.Remove(parsedLines[0]);

                //update list
                this.parsedLines = parsedLines;

                //contains duplicate key
                if(!this.DuplicateKeyCheck()){
                    throw new ArgumentException("Parsed ini lines contain duplicate keys.");
                }
            }

            /// <summary> Duplicate key check for the current instance.</summary>
            /// <returns> True is there's no duplicate key.</returns>
            private bool DuplicateKeyCheck(){
                //will hold the keys name
                List<string> keysFound = new List<string>();

                try{
                    //loop throught lines
                    foreach(IniLine line in this.parsedLines){
                        //key value line
                        if(line.LineType == IniLineType.KeyValue){
                            //update value
                            this.currentKeyNameCheck = line.Key;

                            //check if key is not found
                            if(keysFound.FindIndex(KeyCheckPredicate) == -1){
                                //add to list
                                keysFound.Add(line.Key);
                            } else {
                                return false;
                            }
                        }
                    }
                } finally {
                    //update value
                    this.currentKeyNameCheck = null;
                }

                return true;
            }

            /// <summary> Compare current key to parameter, for duplicate key.</summary>
            private bool KeyCheckPredicate(string keyName){
                return keyName == this.currentKeyNameCheck;
            }

            /// <summary> Search for value based on key, in this section.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> Will hold the searched value.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SearchValue<T>(string keyName, out T value){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //nullify value
                value = default(T);
                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //loop through lines
                foreach(IniLine line in this.parsedLines){
                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //check name
                        if(line.Key == keyName){
                            //check if same type
                            if(line.ValueType != tType){
                                throw new IniLineDataParsingException(tType, line);
                            }

                            //special case for string
                            if(tType == typeof(string)){
                                //get value as object
                                object objValue = line.Value;
                                //convert object to string
                                string objValueAsString = (string) objValue;
                                
                                //unescaped char and convert to T
                                value = (T)((object)IniLine.TrimEscapeChar(objValueAsString));

                                return true;
                            }

                            //update value
                            value = (T) line.Value;
                            return true;
                        }
                    }
                }

                //failed
                return false;
            }
            
            /// <summary> Search for value based on key, in a subsection.</summary>
            /// <param name="subsection"> The name of subsection to search in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> Will hold the searched value.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SearchValue<T>(string subsection, string keyName, out T value){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //nullify value
                value = default(T);
                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }
                
                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //loop through lines
                foreach(IniLine line in this.sections[subsection].parsedLines){
                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //check name
                        if(line.Key == keyName){
                            //check if same type
                            if(line.ValueType != tType){
                                throw new IniLineDataParsingException(tType, line);
                            }

                            //special case for string
                            if(tType == typeof(string)){
                                //get value as object
                                object objValue = line.Value;
                                //convert object to string
                                string objValueAsString = (string) objValue;
                                
                                //unescaped char and convert to T
                                value = (T)((object)IniLine.TrimEscapeChar(objValueAsString));

                                return true;
                            }

                            //update value
                            value = (T) line.Value;
                            return true;
                        }
                    }
                }

                //failed
                return false;
            }


            /// <summary> Update the value of key.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="removeInlineComment"> Determine if removes the previous inline-comment of the key-value line searched for.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValue<T>(string keyName, T value, bool removeInlineComment = false){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //loop through lines
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //check if same type
                            if(line.ValueType != tType){
                                throw new IniLineDataParsingException(tType, line);
                            }

                            //hold the created line
                            IniLine newLineCreated;
                            //create new line with old inline comment
                            if(!removeInlineComment && !String.IsNullOrEmpty(line.InlineComment) && line.InlineComment.Length > 2){
                                newLineCreated = IniLine.CreateKeyValueLine<T>(keyName, value, line.InlineComment.Remove(0, 1));

                            //create new line with no comment
                            } else {
                                newLineCreated = IniLine.CreateKeyValueLine<T>(keyName, value);
                            }

                            //update list
                            this.parsedLines[i] = newLineCreated;

                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary> Update the value of key.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="inlineComment"> The new inline comment.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValue<T>(string keyName, T value, string inlineComment){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //loop through lines
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //check if same type
                            if(line.ValueType != tType){
                                throw new IniLineDataParsingException(tType, line);
                            }

                            //create new line
                            IniLine newLineCreated = IniLine.CreateKeyValueLine<T>(keyName, value, inlineComment);

                            //update list
                            this.parsedLines[i] = newLineCreated;

                            return true;
                        }
                    }
                }

                return false;
            }
            
            /// <summary> Update the value of key, in a subsection.</summary>
            /// <param name="subsection"> The name of subsection to set value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="removeInlineComment"> Determine if removes the previous inline-comment of the key-value line searched for.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValue<T>(string subsection, string keyName, T value, bool removeInlineComment = false){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //loop through lines
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //check if same type
                            if(line.ValueType != tType){
                                throw new IniLineDataParsingException(tType, line);
                            }

                            //hold the created line
                            IniLine newLineCreated;
                            //create new line with old inline comment
                            if(!removeInlineComment && !String.IsNullOrEmpty(line.InlineComment) && line.InlineComment.Length > 2){
                                newLineCreated = IniLine.CreateKeyValueLine<T>(keyName, value, line.InlineComment.Remove(0, 1));

                            //create new line with no comment
                            } else {
                                newLineCreated = IniLine.CreateKeyValueLine<T>(keyName, value);
                            }

                            //update list
                            this.sections[subsection].parsedLines[i] = newLineCreated;

                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary> Update the value of key, in a subsection.</summary>
            /// <param name="subsection"> The name of subsection to set value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="inlineComment"> The new inline comment.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValue<T>(string subsection, string keyName, T value, string inlineComment){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //loop through lines
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //check if same type
                            if(line.ValueType != tType){
                                throw new IniLineDataParsingException(tType, line);
                            }

                            //create new line
                            IniLine newLineCreated = IniLine.CreateKeyValueLine<T>(keyName, value, inlineComment);

                            //update list
                            this.sections[subsection].parsedLines[i] = newLineCreated;

                            return true;
                        }
                    }
                }

                return false;
            }
            
            /// <summary> Update the value of key, ignoring value type.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="removeInlineComment"> Determine if removes the previous inline-comment of the key-value line searched for.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValueIgnoringType<T>(string keyName, T value, bool removeInlineComment = false){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //loop through lines
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //change line value type along with value
                            line.ChangeValueType<T>(value, removeInlineComment);

                            //update list
                            this.parsedLines[i] = line;

                            return true;
                        }
                    }
                }

                return false;
            }
            
            /// <summary> Update the value of key, ignoring value type.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="inlineComment"> The new inline comment.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValueIgnoringType<T>(string keyName, T value, string inlineComment){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //loop through lines
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //change line value type along with value
                            line.ChangeValueType<T>(value, inlineComment);

                            //update list
                            this.parsedLines[i] = line;

                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary> Update the value of key, in a subsection, ignoring value type.</summary>
            /// <param name="subsection"> The name of subsection to set value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="removeInlineComment"> Determine if removes the previous inline-comment of the key-value line searched for.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValueIgnoringType<T>(string subsection, string keyName, T value, bool removeInlineComment = false){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //loop through lines
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //change line value type along with value
                            line.ChangeValueType<T>(value, removeInlineComment);

                            //update list
                            this.sections[subsection].parsedLines[i] = line;

                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary> Update the value of key, in a subsection, ignoring value type.</summary>
            /// <param name="subsection"> The name of subsection to set value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The new value to be updated with.</param>
            /// <param name="inlineComment"> The new inline comment.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when value wasn't the expected value.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> Return false when not found.</returns>
            public bool SetValueIgnoringType<T>(string subsection, string keyName, T value, string inlineComment){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //loop through lines
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            //change line value type along with value
                            line.ChangeValueType<T>(value, inlineComment);

                            //update list
                            this.sections[subsection].parsedLines[i] = line;

                            return true;
                        }
                    }
                }

                return false;
            }


            /// <summary> Add a new key-value line with parameters specified, at a specific index.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <param name="index"> The index where the line will be placed.</param>
            /// <remarks> This method is used to create new line and then insert it in-between other lines. Specifying
            /// an index beyond the capacity will update the index automatically to the last.</remarks>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <returns> False if key already exists.</returns>
            public bool AddValue<T>(string keyName, T value, int index){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //check if key already exists
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value);
                //insert line
                this.parsedLines.Insert(index, newLine);

                return true;
            }

            /// <summary> Add a new key-value line, which has a inline-comment, with parameters specified, at a specific index.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <param name="index"> The index where the line will be placed.</param>
            /// <param name="inlineComment"> The commented text to add.</param>
            /// <remarks> This method is used to create new line and then insert it in-between other lines. Specifying
            /// an index beyond the capacity will update the index automatically to the last.</remarks>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <returns> False if key already exists.</returns>
            public bool AddValue<T>(string keyName, T value, int index, string inlineComment){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //check if key already exists
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value, inlineComment);
                //insert line
                this.parsedLines.Insert(index, newLine);

                return true;
            }
            
            /// <summary> Add a new key-value line with parameters specified, in a subsection at a specific index.</summary>
            /// <param name="subsection"> The name of subsection to add value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <param name="index"> The index where the line will be placed.</param>
            /// <remarks> This method is used to create new line and then insert it in-between other lines. Specifying
            /// an index beyond the capacity will update the index automatically to the last.</remarks>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> False if key already exists or section does not exists.</returns>
            public bool AddValue<T>(string subsection, string keyName, T value, int index){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //check if key already exists
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value);
                //insert line
                this.sections[subsection].parsedLines.Insert(index, newLine);

                return true;
            }

            /// <summary> Add a new key-value line, which has a inline-comment, with parameters specified, in a subsection at a specific index.</summary>
            /// <param name="subsection"> The name of subsection to add value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <param name="index"> The index where the line will be placed.</param>
            /// <param name="inlineComment"> The commented text to add.</param>
            /// <remarks> This method is used to create new line and then insert it in-between other lines. Specifying
            /// an index beyond the capacity will update the index automatically to the last.</remarks>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> False if key already exists or section does not exists.</returns>
            public bool AddValue<T>(string subsection, string keyName, T value, int index, string inlineComment){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //check if key already exists
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value, inlineComment);
                //insert line
                this.sections[subsection].parsedLines.Insert(index, newLine);

                return true;
            }

            /// <summary> Add a new key-value line with parameters specified, at the end.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <returns> False if key already exists.</returns>
            public bool AddValueAtEnd<T>(string keyName, T value){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //check if key already exists
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value);
                //add line at end
                this.parsedLines.Add(newLine);

                return true;
            }

            /// <summary> Add a new key-value line, which has a inline-comment, with parameters specified, at the end.</summary>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <param name="inlineComment"> The commented text to add.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <returns> False if key already exists.</returns>
            public bool AddValueAtEnd<T>(string keyName, T value, string inlineComment){
                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //check if key already exists
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value, inlineComment);
                //add line at end
                this.parsedLines.Add(newLine);

                return true;
            }

            /// <summary> Add a new key-value line with parameters specified, in a subsection at the end.</summary>
            /// <param name="subsection"> The name of subsection to add value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName is invalid.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> False if key already exists or section does not exists.</returns>
            public bool AddValueAtEnd<T>(string subsection, string keyName, T value){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //check if key already exists
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value);
                //add line to end
                this.sections[subsection].parsedLines.Add(newLine);

                return true;
            }
            
            /// <summary> Add a new key-value line, which has a inline-comment, with parameters specified, in a subsection at the end.</summary>
            /// <param name="subsection"> The name of subsection to add value in.</param>
            /// <param name="keyName"> The name of the key.</param>
            /// <param name="value"> The value to be represented.</param>
            /// <param name="inlineComment"> The commented text to add.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when keyName/inlineComment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when T is not a valid type reference or when keyName/inlineComment is invalid.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> False if key already exists or section does not exists.</returns>
            public bool AddValueAtEnd<T>(string subsection, string keyName, T value, string inlineComment){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //invalid key
                if(String.IsNullOrEmpty(keyName)){
                    throw new ArgumentNullException("keyName", "Key cannot be null.");
                }
                if(keyName.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "keyName");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //section don't exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //check if key already exists
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return false;
                        }
                    }
                }

                //create line
                IniLine newLine = IniLine.CreateKeyValueLine<T>(keyName, value, inlineComment);
                //add line to end
                this.sections[subsection].parsedLines.Add(newLine);

                return true;
            }


            /// <summary> Add a new fully commented line, with comment specified, at the index specified.</summary>
            /// <param name="comment"> The string, not including the comment char.</param>
            /// <param name="index"> The index to add the line comment.</param>
            /// <remarks> This method is used to create new comment line and then insert it in-between other lines. Specifying
            /// an index beyond the capacity will update the index automatically to the last.</remarks>
            /// <exception cref="System.ArgumentNullException"> Thrown when comment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when comment is invalid.</exception>
            public void AddLineComment(string comment, int index, bool useDefaultChar = true){
                //checking and validating comment
                if(String.IsNullOrEmpty(comment)){
                    throw new ArgumentNullException("Comment cannot be empty or null.", "comment");
                }

                //update comment
                comment = useDefaultChar ? $"{IniFile.COMMENT}{comment}" : $"{IniFile.ALT_COMMENT}{comment}";

                if(!IniLine.ValidateComment(comment)){
                    throw new ArgumentException("Comment is invalid. Possible cause is contains line break char.", "comment");
                }

                //create comment line
                IniLine newLine = IniLine.CreateCommentLine(comment);
                //add line to the end
                this.parsedLines.Insert(index, newLine);
            }

            /// <summary> Add a new fully commented line, with comment specified, in a subsection at the index specified.</summary>
            /// <param name="comment"> The string, not including the comment char.</param>
            /// <param name="index"> The index to add the line comment.</param>
            /// <remarks> This method is used to create new comment line and then insert it in-between other lines. Specifying
            /// an index beyond the capacity will update the index automatically to the last.</remarks>
            /// <exception cref="System.ArgumentNullException"> Thrown when comment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when comment is invalid.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> False if subsection doesn't exists.</returns>
            public bool AddLineComment(string subsection, string comment, int index, bool useDefaultChar = true){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //checking and validating comment
                if(String.IsNullOrEmpty(comment)){
                    throw new ArgumentNullException("Comment cannot be empty or null.", "comment");
                }

                //update comment
                comment = useDefaultChar ? $"{IniFile.COMMENT}{comment}" : $"{IniFile.ALT_COMMENT}{comment}";

                if(!IniLine.ValidateComment(comment)){
                    throw new ArgumentException("Comment is invalid. Possible cause is contains line break char.", "comment");
                }

                //subsection does not exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //create comment line
                IniLine newLine = IniLine.CreateCommentLine(comment);
                //add line to the end of the subsection
                this.sections[subsection].parsedLines.Insert(index, newLine);

                return true;
            }

            /// <summary> Add a new fully commented line, with comment specified, at the end.</summary>
            /// <param name="comment"> The string, not including the comment char.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when comment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when comment is invalid.</exception>
            public void AddLineCommentAtEnd(string comment, bool useDefaultChar = true){
                //checking and validating comment
                if(String.IsNullOrEmpty(comment)){
                    throw new ArgumentNullException("Comment cannot be empty or null.", "comment");
                }

                //update comment
                comment = useDefaultChar ? $"{IniFile.COMMENT}{comment}" : $"{IniFile.ALT_COMMENT}{comment}";

                if(!IniLine.ValidateComment(comment)){
                    throw new ArgumentException("Comment is invalid. Possible cause is contains line break char.", "comment");
                }

                //create comment line
                IniLine newLine = IniLine.CreateCommentLine(comment);
                //add line to the end
                this.parsedLines.Add(newLine);
            }

            /// <summary> Add a new fully commented line, with comment specified, in a subsection at the end.</summary>
            /// <param name="comment"> The string, not including the comment char.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when comment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when comment is invalid.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> False if subsection doesn't exists.</returns>
            public bool AddLineCommentAtEnd(string subsection, string comment, bool useDefaultChar = true){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //checking and validating comment
                if(String.IsNullOrEmpty(comment)){
                    throw new ArgumentNullException("Comment cannot be empty or null.", "comment");
                }

                //update comment
                comment = useDefaultChar ? $"{IniFile.COMMENT}{comment}" : $"{IniFile.ALT_COMMENT}{comment}";

                if(!IniLine.ValidateComment(comment)){
                    throw new ArgumentException("Comment is invalid. Possible cause is contains line break char.", "comment");
                }

                //subsection does not exists
                if(!sections.ContainsKey(subsection)){
                    return false;
                }

                //create comment line
                IniLine newLine = IniLine.CreateCommentLine(comment);
                //add line to the end of the subsection
                this.sections[subsection].parsedLines.Add(newLine);

                return true;
            }
            

            /// <summary> Remove the key-value line, which key matches.</summary>
            /// <param name="keyName"> Name of the key.</param>
            public bool RemoveValue(string keyName){
                //loop through lines
                for (int i = 0; i < this.parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return this.parsedLines.Remove(line);
                        }
                    }
                }

                return false;
            }

            /// <summary> Remove the key-value line in a subsection, which key matches.</summary>
            /// <param name="subsection"> Name of the subsection to remove in.</param>
            /// <param name="keyName"> Name of the key.</param>
            /// <exception cref="System.ArgumentException"> Thrown when subsection specified is not found.</exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            public bool RemoveValue(string subsection, string keyName){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //check subsection
                if(!this.sections.ContainsKey(subsection)){
                    throw new ArgumentException("The subsection specified is not found.", "subsection");
                }

                //loop through lines
                for (int i = 0; i < this.sections[subsection].parsedLines.Count; i++){
                    //get current line
                    IniLine line = this.sections[subsection].parsedLines[i];

                    //key value line
                    if(line.LineType == IniLineType.KeyValue){
                        //same name
                        if(line.Key == keyName){
                            return this.sections[subsection].parsedLines.Remove(line);
                        }
                    }
                }

                return false;
            }

            /// <summary> Removes line at the specified index.</summary>
            /// <exception cref="System.ArgumentOutOfRangeException"></exception>
            public void RemoveLine(int index){
                this.parsedLines.RemoveAt(index);
            }

            /// <summary> Removes line at the specified index, in a subsection.</summary>
            /// <param name="subsection"> The subsection to remove line in.</param>
            /// <exception cref="System.ArgumentException"> Thrown when subsection specified is not found.</exception>
            /// <exception cref="System.ArgumentOutOfRangeException"></exception>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            public void RemoveLine(string subsection, int index){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot perform action for subsection in a subsection.");
                }

                //check subsection
                if(!this.sections.ContainsKey(subsection)){
                    throw new ArgumentException("The subsection specified is not found.", "subsection");
                }

                this.sections[subsection].parsedLines.RemoveAt(index);
            }


            /// <summary> Create and add subsection with header of <paramref name="name"/>, with lines to parse and add to the subsection.</summary>
            /// <param name="name"> The name of the section header.</param>
            /// <param name="content"> The array of lines to parse into IniLine.</param>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when one of <paramref name="content"/>, failed to parsed into an ini line. Another case is when
            /// two different parsed lines contain same key and the last one is when a header line is found.</exception>
            /// <returns> True if created section succesfully else failed.</returns>
            public bool CreateSubsection(string name, string[] content){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot create a new subsection in already define subsection.");
                }

                //already defined
                if(this.sections.ContainsKey(name)){
                    return false;
                }

                //will hold the subsection instance
                ParsedIni subsectionIns = new ParsedIni();

                //add header line and update state
                try{
                    subsectionIns.headerLine = IniLine.CreateSubsectionHeader(name);
                } catch (Exception){
                    //invalid section header
                    return false;
                }
                subsectionIns.isSection = true;

                //loop through lines
                for(int i = 0; i < content.Length; i++){
                    //parse line
                    IniLine parsedLine = new IniLine(content[i]);

                    //invalid line
                    if(parsedLine.LineType == IniLineType.Invalid){
                        throw new ArgumentException("Failed to parse a line into an ini line, from the string array.");

                    //header line type
                    } else if(parsedLine.LineType == IniLineType.SectionHeader){
                        throw new ArgumentException("Parsed ini lines cannot contain header line. Cannot create subsection in another subsection.");

                    } else {
                        //update list
                        subsectionIns.parsedLines.Add(parsedLine);
                    }
                }

                //duplicate key check
                if(!subsectionIns.DuplicateKeyCheck()){
                    throw new ArgumentException("Parsed ini lines contain duplicate keys.");
                }

                //add section
                this.sections.Add(name, subsectionIns);

                return true;
            }

            /// <summary> Create and add subsection with header of <paramref name="name"/>, with lines to parse and add to the subsection.</summary>
            /// <param name="name"> The name of the section header.</param>
            /// <param name="content"> The array of lines to parse into IniLine.</param>
            /// <param name="comment"> The comment to add to the header line.</param>
            /// <remarks> Comment specified is added to the header line of this creating section.</remarks>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when one of <paramref name="content"/>, failed to parsed into an ini line. Another case is when
            /// two different parsed lines contain same key and the last one is when a header line is found.</exception>
            /// <returns> True if created section succesfully else failed.</returns>
            public bool CreateSubsection(string name, string[] content, string comment){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot create a new subsection in a subsection.");
                }

                //already defined
                if(this.sections.ContainsKey(name)){
                    return false;
                }

                //will hold the subsection instance
                ParsedIni subsectionIns = new ParsedIni();
                
                //add header line and update state
                try{
                    subsectionIns.headerLine = IniLine.CreateSubsectionHeader(name, (IniFile.ALT_COMMENT + comment));
                } catch (Exception){
                    //invalid section header
                    return false;
                }
                subsectionIns.isSection = true;

                //loop through lines
                for(int i = 0; i < content.Length; i++){
                    //parse line
                    IniLine parsedLine = new IniLine(content[i]);

                    //invalid line
                    if(parsedLine.LineType == IniLineType.Invalid){
                        throw new ArgumentException("Failed to parse a line into an ini line, from the string array.");

                    //header line type
                    } else if(parsedLine.LineType == IniLineType.SectionHeader){
                        throw new ArgumentException("Parsed ini lines cannot contain header line. Cannot create subsection in another subsection.");

                    } else {
                        //update list
                        subsectionIns.parsedLines.Add(parsedLine);
                    }
                }

                //duplicate key check
                if(!subsectionIns.DuplicateKeyCheck()){
                    throw new ArgumentException("Parsed ini lines contain duplicate keys.");
                }

                //add section
                this.sections.Add(name, subsectionIns);

                return true;
            }

            /// <summary> Create and add subsection with header of <paramref name="name"/>.</summary>
            /// <param name="name"> The name of the section header.</param>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> True if created section succesfully else failed.</returns>
            public bool CreateSubsection(string name){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot create a new subsection in a subsection.");
                }

                //already defined
                if(this.sections.ContainsKey(name)){
                    return false;
                }

                //will hold the subsection instance
                ParsedIni subsectionIns = new ParsedIni();

                //add header line and update state
                try{
                    subsectionIns.headerLine = IniLine.CreateSubsectionHeader(name);
                } catch (Exception){
                    //invalid section header
                    return false;
                }
                subsectionIns.isSection = true;

                //add section
                this.sections.Add(name, subsectionIns);

                return true;
            }

            /// <summary> Create and add subsection with header of <paramref name="name"/>.</summary>
            /// <param name="name"> The name of the section header.</param>
            /// <param name="comment"> The comment to add to the header line.</param>
            /// <remarks> Comment specified is added to the header line of this creating section.</remarks>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            /// <returns> True if created section succesfully else failed.</returns>
            public bool CreateSubsection(string name, string comment){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot create a new subsection in a subsection.");
                }

                //already defined
                if(this.sections.ContainsKey(name)){
                    return false;
                }

                //will hold the subsection instance
                ParsedIni subsectionIns = new ParsedIni();

                //add header line and update state
                try{
                    subsectionIns.headerLine = IniLine.CreateSubsectionHeader(name, (IniFile.ALT_COMMENT + comment));
                } catch (Exception){
                    //invalid section header
                    return false;
                }
                subsectionIns.isSection = true;

                //add section
                this.sections.Add(name, subsectionIns);

                return true;
            }

            /// <summary> Remove the subsection from the lines.</summary>
            /// <param name="name"> The name of the sub-section header.</param>
            /// <exception cref="System.InvalidOperationException"> Thrown when subsection property is set to true.</exception>
            public bool RemoveSubsection(string name){
                //subsection
                if(this.isSection){
                    throw new InvalidOperationException("Cannot remove a subsection in a subsection.");
                }

                return this.sections.Remove(name);
            }
            
            //to-do: maybe remove
            /// <summary> Try to get the subsection and passed it as reference.</summary>
            /// <param name="name"> The name of the subsection header.</param>
            /// <returns> True if found.</returns>
            private bool RetrieveSubsection(string name, out ParsedIni subsection){
                return this.sections.TryGetValue(name, out subsection);
            }


            /// <summary> Return a section represented by reconstructed line(s).</summary>
            public override string ToString(){
                //hold the full output line
                string outputString = "";
                
                //add header
                if(this.isSection){
                    outputString += this.headerLine.ToString();
                    outputString += "\n";
                }

                //each line add to the output
                for (int i = 0; i < this.parsedLines.Count; i++){
                    if(i != 0){
                        outputString += "\n";
                    }
                    outputString += $"{this.parsedLines[i].ToRawString()}";
                }

                if(this.sections != null){
                    //add section
                    foreach (KeyValuePair<string, ParsedIni> section in this.sections){
                        outputString += $"\n{section.Value}";
                    }
                }

                return outputString;
            }

            /// <summary> Return a section represented by raw line(s) got after reading.</summary>
            public string ToRawString(){
                //hold the full output line
                string outputString = "";
                
                //add header
                if(this.isSection){
                    outputString += this.headerLine?.ToRawString();
                    outputString += "\n";
                }

                //each line add to the output
                for (int i = 0; i < this.parsedLines.Count; i++){
                    if(i != 0){
                        outputString += "\n";
                    }
                    outputString += $"{this.parsedLines[i].ToRawString()}";
                }

                if(this.sections != null){
                    //add section
                    foreach (KeyValuePair<string, ParsedIni> section in this.sections){
                        outputString += $"\n{section.Value.ToRawString()}";
                    }
                }

                return outputString;
            }
        }

        /// <summary> Structure representing a line in an ini file.</summary>
        public struct IniLine{
            
            /// <summary> The full parsed line.</summary>
            private string line;

            /// <summary> The commented text, including the comment char.</summary>
            private string inlineComment;

            /// <summary> Hold the name of the section header.</summary>
            /// <remarks> Null if there's no header.</remarks>
            private string sectionHeader;

            /// <summary> The key for retrieving the value.</summary>
            private string key;

            /// <summary> Hold the value type.</summary>
            private Type valueType;

            /// <summary> The value retrived.</summary>
            private object value;

            /// <summary> The type of this line.</summary>
            private IniLineType? lineType;


            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine.line"/>
            /// <remarks> (Get only)</remarks>
            public string Line{
                get{
                    return this.line;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine.inlineComment"/>
            /// <remarks> (Get only)</remarks>
            public string InlineComment{
                get{
                    return this.inlineComment;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine.sectionHeader"/>
            /// <remarks> (Get only)</remarks>
            public string SectionHeader{
                get{
                    return this.sectionHeader;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine.key"/>
            /// <remarks> (Get only)</remarks>
            public string Key{
                get{
                    return this.key;
                }
            }

            /// <summary> (Get only) Return the value type.</summary>
            public Type ValueType{
                get{
                    return this.valueType;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine.value"/>
            /// <remarks> (Get only)</remarks>
            public object Value{
                get{
                    return this.value;
                }
            }

            /// <inheritdoc cref="EchoCapture.Data.File.Text.IniFile.IniLine.lineType"/>
            /// <remarks> (Get only)</remarks>
            public IniLineType LineType{
                get{
                    return (IniLineType) this.lineType;
                }
            }

            /// <summary> Constructor for nullifying value.</summary>
            private IniLine(object value = null){
                //nullify fields
                this.valueType = null;
                this.line = null;
                this.inlineComment = null;
                this.key = null;
                this.value = null;
                this.sectionHeader = null;
                this.lineType = null;
            }

            /// <summary> Create instance from parsing the line and estimating the value type.</summary>
            /// <exception cref="System.ArgumentException"> Thrown when <paramref name="valueType"/> is invalid.</exception>
            public IniLine(string line) : this(){
                //hold the value type
                Type valueType;

                try{
                    //get type
                    valueType = IniLine.EstimateType(line);
                } catch (IniLineDataParsingException){
                    if(IniLine.IsFullyCommented(line) || IniLine.IsHeader(line)){
                        valueType = null;
                    } else {
                        throw new ArgumentException("Failed to retrieve the value type of the key-value line.");
                    }
                }

                //update type
                this.valueType = valueType;
                //parse line
                this.ParseLine(line);
            }
            
            /// <summary> Create instance from parsing the line, along with a specified value type.</summary>
            /// <exception cref="System.ArgumentException"> Thrown when <paramref name="valueType"/> is invalid.</exception>
            public IniLine(string line, Type valueType) : this(){
                //specify type
                if(valueType != typeof(int) && valueType != typeof(float) && valueType != typeof(string) && valueType != typeof(bool)){
                    throw new ArgumentException("Invalid type passed. Type can only be int, float, boolean or string");
                }

                //update type
                this.valueType = valueType;
                //parse line
                this.ParseLine(line);
            }

            /// <summary> Retrieves the information from the line and update the fields.</summary>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when failed to parse value, or comment is invalid.</exception>
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

                                //invalid section header
                                if(this.sectionHeader.Contains('\n')){
                                    throw new IniLineDataParsingException();
                                }
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
                                this.inlineComment = /*IniFile.COMMENT_CHARS[i] +*/ subPart[1];
                                break;
                            } catch (IndexOutOfRangeException){}
                        }
                        if(part[0][0] == IniFile.COMMENT_CHARS[i]){
                            isSectionHeader = false;

                            //update comment
                            this.inlineComment = /*IniFile.COMMENT_CHARS[i] +*/ part[0];
                            break;
                        }
                    }

                    if(!String.IsNullOrEmpty(this.inlineComment)){
                        //invalid inline comment
                        if(!IniLine.ValidateComment(this.inlineComment)){
                            throw new IniLineDataParsingException();
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
                    if(part[0].Split(' ').Length == 1){
                        //update key
                        this.key = part[0];
                    } else {
                        //update line type
                        this.lineType = IniLineType.Invalid;
                    }

                    //get type code
                    TypeCode tTypeCode = Type.GetTypeCode(this.valueType);
                    //hold the value part and comment part
                    string valuePart = null;
                    string commentPart = null;

                    //check if it's a comment
                    for(int i = 0; i < IniFile.COMMENT_CHARS.Length; i++){
                        //hold the index of comment char
                        int charIndex;
                        //search for comment char
                        charIndex = part[1].IndexOf(IniFile.COMMENT_CHARS[i], 0);

                        //not found
                        if(charIndex == -1){
                            continue;
                        }

                        //check if escaped
                        if(charIndex - 1 >= 0 && part[1][charIndex-1] == '\\'){
                            continue;
                        }

                        //check if comment char is first character
                        if(charIndex == 0){
                            //update parts
                            commentPart = part[1];
                            break;
                        }

                        //update parts
                        valuePart = part[1].Substring(0, charIndex);
                        commentPart = part[1].Substring(charIndex);
                        break;
                    }

                    //no comment char was found
                    //update value part
                    if(String.IsNullOrEmpty(commentPart) && String.IsNullOrEmpty(valuePart)){
                        valuePart = part[1];
                    }
                    
                    if(valuePart == null){
                        //update line type
                        this.lineType = IniLineType.Invalid;

                        return;
                    }

                    //invalid inline comment
                    if(commentPart != null && !IniLine.ValidateComment(commentPart)){
                        throw new IniLineDataParsingException();
                    }

                    //update comment
                    this.inlineComment = commentPart;

                    switch(tTypeCode){
                        case TypeCode.String:
                            //decode the string
                            this.value = (object) IniLine.DoubleQuoteTrimString(this.DecodeToString(valuePart));
                        break;

                        case TypeCode.Boolean:
                            //parse the bool
                            this.value = (object) this.ParseToBool(valuePart);
                        break;

                        case TypeCode.Int32:
                            //parse to int
                            this.value = (object) this.ParseToInt(valuePart);
                        break;

                        case TypeCode.Single:
                            //parse to float
                            this.value = (object) this.ParseToFloat(valuePart);
                        break;
                    }
                }
            }


            /// <summary> Creates a line with typeof fully commented line, with <paramref name="comment"/> specified.</summary>
            /// <param name="comment"> The string, including the comment char.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when comment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when comment is invalid.</exception>
            public static IniLine CreateCommentLine(string comment){
                //checking and validating comment
                if(String.IsNullOrEmpty(comment)){
                    throw new ArgumentNullException("Comment cannot be empty or null.", "comment");
                }
                if(!IniLine.ValidateComment(comment)){
                    throw new ArgumentException("Comment is invalid", "comment");
                }

                //create line with all default values
                IniLine line = new IniLine();
                //update values
                line.lineType = IniLineType.FullyCommented;
                line.line = comment;
                line.inlineComment = comment;

                return line;
            }

            /// <summary> Creates a line with typeof subsection header.</summary>
            /// <param name="headerName"> The name of the header, without the square brackets.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when header name is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when header name is invalid.</exception>
            public static IniLine CreateSubsectionHeader(string headerName){
                //checking and validating comment
                if(String.IsNullOrEmpty(headerName)){
                    throw new ArgumentNullException("headerName cannot be empty or null.", "headerName");
                }

                //update name with brackets
                if(!headerName.StartsWith(IniFile.START_SECTION)){
                    headerName = IniFile.START_SECTION + headerName;
                }
                if(!headerName.StartsWith(IniFile.END_SECTION)){
                    headerName += IniFile.END_SECTION;
                }

                if(!IsHeader(headerName)){
                    throw new ArgumentException("Header name is invalid.", "headerName");
                }

                //create line with all default values
                IniLine line = new IniLine();
                //update values
                line.lineType = IniLineType.SectionHeader;
                line.sectionHeader = headerName;

                //update raw
                line.line = $"{headerName}";

                return line;
            }

            /// <summary> Creates a line with typeof subsection header, with <paramref name="comment"/> specified.</summary>
            /// <param name="headerName"> The name of the header.</param>
            /// <param name="comment"> The string, including the comment char.</param>
            /// <exception cref="System.ArgumentNullException"> Thrown when header name or comment is null.</exception>
            /// <exception cref="System.ArgumentException"> Thrown when header name or comment is invalid.</exception>
            public static IniLine CreateSubsectionHeader(string headerName, string comment){
                //checking and validating comment
                if(String.IsNullOrEmpty(headerName)){
                    throw new ArgumentNullException("headerName cannot be empty or null.", "headerName");
                }

                //update name with brackets
                if(!headerName.StartsWith(IniFile.START_SECTION)){
                    headerName = IniFile.START_SECTION + headerName;
                }
                if(!headerName.StartsWith(IniFile.END_SECTION)){
                    headerName = headerName + IniFile.END_SECTION;
                }

                if(!IsHeader(headerName)){
                    throw new ArgumentException("Header name is invalid.", "headerName");
                }

                //checking and validating comment
                if(String.IsNullOrEmpty(comment)){
                    throw new ArgumentNullException("Comment cannot be empty or null.", "comment");
                }
                if(!IniLine.ValidateComment(comment)){
                    throw new ArgumentException("Comment is invalid", "comment");
                }

                //create line with all default values
                IniLine line = new IniLine();
                //update values
                line.lineType = IniLineType.SectionHeader;
                line.sectionHeader = headerName;
                line.inlineComment = comment;

                //update raw
                line.line = $"{headerName} {comment}";

                return line;
            }

            /// <summary> Creates a line with typeof key-value line, with the <paramref name="value"/>.</summary>
            /// <param name="key"> The name of the key.</param>
            /// <param name="value"> The value representing.</param>
            /// <exception cref="System.ArgumentException"> Key contains whitespace.</exception>
            /// <exception cref="System.ArgumentNullException"> Key is null.</exception>
            public static IniLine CreateKeyValueLine<T>(string key, T value){
                //invalid key
                if(String.IsNullOrEmpty(key)){
                    throw new ArgumentNullException("key", "Key cannot be null.");
                }
                if(key.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "key");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //create line with all default values
                IniLine line = new IniLine();

                //update values
                line.valueType = tType;
                line.lineType = IniLineType.KeyValue;
                line.key = key;
                line.value = (object) value;

                //will hold the value represented as string
                //which will be in the line
                string stringValue;
                
                //convert value to string based on type
                if(tType == typeof(string)){
                    //convert to an explicit string
                    stringValue = (string) ((object)value);
                    //correct value for line
                    stringValue = IniLine.EscapeCharAndWrapLine(stringValue);
                } else {
                    stringValue = value.ToString();
                }
                
                //update line
                line.line = $"{key} = {stringValue}";

                //return instance
                return line;
            }
            
            /// <summary> Creates a line with typeof key-value line, with the <paramref name="value"/>.</summary>
            /// <param name="key"> The name of the key.</param>
            /// <param name="value"> The value representing.</param>
            /// <param name="inlineComment"> The inline comment to add to the line.</param>
            /// <exception cref="System.ArgumentException"> Key contains whitespace.</exception>
            /// <exception cref="System.ArgumentNullException"> Key is null.</exception>
            public static IniLine CreateKeyValueLine<T>(string key, T value, string inlineComment){
                //invalid key
                if(String.IsNullOrEmpty(key)){
                    throw new ArgumentNullException("key", "Key cannot be null.");
                }
                if(key.Split(' ').Length > 1){
                    throw new ArgumentException("Key needs to be a word with no whitespace.", "key");
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //create line with all default values
                IniLine line = new IniLine();

                //update values
                line.valueType = tType;
                line.lineType = IniLineType.KeyValue;
                line.key = key;
                line.value = (object) value;
                line.inlineComment = inlineComment;

                //will hold the value represented as string
                //which will be in the line
                string stringValue;
                
                //convert value to string based on type
                if(tType == typeof(string)){
                    //convert to an explicit string
                    stringValue = (string) ((object)value);
                    //correct value for line
                    stringValue = IniLine.EscapeCharAndWrapLine(stringValue);
                } else {
                    stringValue = value.ToString();
                }
                
                //update line
                line.line = $"{key} = {stringValue} {IniFile.ALT_COMMENT.ToString()}{inlineComment}";

                //return instance
                return line;
            }

            /// <summary> Change the current value type for a key-value line to T and the value specified.</summary>
            /// <param name="value"> The new value of type T.</param>
            /// <param name="removeInlineComment"> Determine if removes inline-comment of this line as line will be rewritten.</param>
            /// <remarks> The line represented in string format, will be reconstructed.</remarks>
            /// <exception cref="System.InvalidOperationException"> Current instance line type is invalid.</exception>
            /// <exception cref="System.ArgumentException"> T is not a valid type reference.</exception>
            public void ChangeValueType<T>(T value, bool removeInlineComment = false){
                //invalid line type
                if(this.lineType != IniLineType.KeyValue){
                    throw new InvalidOperationException();
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //if T is string type, will hold the explicit casted string
                string explicitValue;

                //update value type
                this.valueType = tType;
                //update value
                switch(Type.GetTypeCode(tType)){
                    case TypeCode.String:
                        //cast to explicit string
                        explicitValue = (string) ((object)value);
                        //correct line
                        explicitValue = IniLine.EscapeCharAndWrapLine(explicitValue);
                        //decode the string
                        this.value = (object) IniLine.DoubleQuoteTrimString(this.DecodeToString(explicitValue));
                    break;

                    case TypeCode.Boolean:
                        //parse the bool
                        this.value = (object) value;
                    break;

                    case TypeCode.Int32:
                        //parse to int
                        this.value = (object) value;
                    break;

                    case TypeCode.Single:
                        //parse to float
                        this.value = (object) value;
                    break;
                }

                //hold the constructed line
                string keyConstructedLine = $"{this.key} = {this.value.ToString()}";

                //update line
                if(!removeInlineComment && !String.IsNullOrEmpty(this.InlineComment)){
                    //update with alt comment char
                    keyConstructedLine += $" {this.InlineComment.Remove(0, 1).Insert(0, IniFile.ALT_COMMENT.ToString())}";
                }

                //update with new line
                this.line = keyConstructedLine;
            }

            /// <summary> Change the current value type for a key-value line to T and the value specified.</summary>
            /// <param name="value"> The new value of type T.</param>
            /// <param name="newInlineComment"> The inline comment to add to the line.</param>
            /// <remarks> The line represented in string format, will be reconstructed.</remarks>
            /// <exception cref="System.ArgumentNullException"> Thrown when inline comment is null or empty.</exception>
            /// <exception cref="System.InvalidOperationException"> Current instance line type is invalid.</exception>
            /// <exception cref="System.ArgumentException"> T is not a valid type reference.</exception>
            public void ChangeValueType<T>(T value, string inlineComment){
                //invalid line type
                if(this.lineType != IniLineType.KeyValue){
                    throw new InvalidOperationException();
                }

                //invalid inline comment
                if(String.IsNullOrEmpty(inlineComment)){
                    throw new ArgumentNullException("inlineComment", "New inline comment cannot be null or an empty string.");
                }
                if(!IniLine.ValidateComment(inlineComment)){
                    throw new ArgumentException("Inline comment cannot contain line break char.", "inlineComment");
                }

                //get type
                Type tType = typeof(T);
                //check if invalid
                if(tType != typeof(string) && tType != typeof(bool) && tType != typeof(int) && tType != typeof(float)){
                    throw new ArgumentException("T is not a valid type reference");
                }

                //if T is string type, will hold the explicit casted string
                string explicitValue;

                //update value type
                this.valueType = tType;
                //update value
                switch(Type.GetTypeCode(tType)){
                    case TypeCode.String:
                        //cast to explicit string
                        explicitValue = (string) ((object)value);
                        //correct line
                        explicitValue = IniLine.EscapeCharAndWrapLine(explicitValue);
                        //decode the string
                        this.value = (object) IniLine.DoubleQuoteTrimString(this.DecodeToString(explicitValue));
                    break;

                    case TypeCode.Boolean:
                        //parse the bool
                        this.value = (object) value;
                    break;

                    case TypeCode.Int32:
                        //parse to int
                        this.value = (object) value;
                    break;

                    case TypeCode.Single:
                        //parse to float
                        this.value = (object) value;
                    break;
                }

                //hold the constructed line
                string keyConstructedLine = $"{this.key} = {this.value.ToString()} {IniFile.ALT_COMMENT.ToString()}{inlineComment}";

                //update with new line
                this.line = keyConstructedLine;
            }


            /// <summary> Removes the two double quote at the beginning and ending of the string.</summary>
            private static string DoubleQuoteTrimString(string decodedString){
                //remove all white space
                decodedString = decodedString.Trim();

                //remove first char
                if(decodedString.StartsWith('\"')){
                    decodedString = decodedString.Remove(0, 1);
                }
                //remove last char
                if(decodedString.EndsWith('\"')){
                    decodedString = decodedString.Remove(decodedString.Length-1, 1);
                }

                //return string
                return decodedString;
            }

            /// <summary> Un-escaped break line and tab.</summary>
            public static string TrimEscapeChar(string decodedString){
                //remove escape line break
                decodedString = decodedString.Replace("\\n", "\n");
                //remove escape tab
                decodedString = decodedString.Replace("\\t", "\t");
                //remove escape double quote
                decodedString = decodedString.Replace("\\\"", "\"");

                return decodedString;
            }

            /// <summary> Correct the line to a correct state with escaped char and double quote wrapped if needed.</summary>
            /// <exception cref="System.ArgumentNullException"> Input line is empty or null.</exception>
            private static string EscapeCharAndWrapLine(string inputLine){
                //invalid parameter
                if(String.IsNullOrEmpty(inputLine)){
                    throw new ArgumentNullException("Cannot modify empty or nullified string.");
                }

                //clear additional white space
                inputLine = inputLine.Trim();

                //contains spaces
                if(inputLine.Split(" ").Length > 1){
                    //wrap with double quote
                    inputLine = $"\"{inputLine}\"";
                }

                //escape line break
                inputLine = inputLine.Replace("\n","\\n");
                //escape tab
                inputLine = inputLine.Replace("\t","\\t");
                //escape double quote
                inputLine = inputLine.Replace("\"","\\\"");

                return inputLine;
            }

            /// <summary> Determine if inline comment part, is valid.</summary>
            /// <remarks> <paramref name="inlineCommentPart"/> should contain the comment char and should not contain line break char.</remarks>
            /// <returns> True if valid.</returns>
            public static bool ValidateComment(string inlineCommentPart){
                //check if not start comment chars
                if(!inlineCommentPart.StartsWith(IniFile.COMMENT) && !inlineCommentPart.StartsWith(IniFile.ALT_COMMENT)){
                    return false;
                }

                //contains line break char
                if(inlineCommentPart.Contains('\n')){
                    return false;
                }

                return true;
            }

            /// <summary> Decode the parsed string to a normal string.</summary>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when can't parse to a validated string.</exception>
            private string DecodeToString(string parsedValue){
                //check if invalid
                if(parsedValue == bool.TrueString || parsedValue == bool.FalseString){
                    //throw exception
                    throw new IniLineDataParsingException(this, typeof(bool));
                }

                //determine if there was exception
                bool caughtException = false;

                //checking if invalid
                //try to parse int
                try{
                    int.Parse(parsedValue);
                } catch (Exception){
                    //caught
                    caughtException = true;
                } finally {
                    if(!caughtException){
                        //throw exception
                        throw new IniLineDataParsingException(this, typeof(int));
                    }
                }

                //reset
                caughtException = false;

                //checking if invalid
                //try to parse float
                try{
                    float.Parse(parsedValue);
                } catch (Exception){
                    //caught
                    caughtException = true;
                } finally {
                    if(!caughtException){
                        //throw exception
                        throw new IniLineDataParsingException(this, typeof(float));
                    }
                }
                
                //will hold the removed escape comment value
                string removedEspaceValue = parsedValue;
                //loop through comment chars
                foreach(char comment in IniFile.COMMENT_CHARS){
                    //upate
                    removedEspaceValue = parsedValue.Replace($"\\{comment.ToString()}", comment.ToString());
                }

                //return string
                return removedEspaceValue;
            }


            /// <summary> Parse the string to boolean.</summary>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when can't parse to boolean.</exception>
            private bool ParseToBool(string parsedValue){
                //true
                if(parsedValue == bool.TrueString){
                    return true;

                //false
                } else if(parsedValue == bool.FalseString){
                    return false;
                }

                //throw exception
                throw new IniLineDataParsingException(this);
            }

            /// <summary> Parse the string to integer.</summary>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when can't parse to integer.</exception>
            private int ParseToInt(string parsedValue){
                //will hold the value
                int i_value;

                try{
                    //parse
                    i_value = Int32.Parse(parsedValue);
                } catch(Exception){
                    //throw exception
                    throw new IniLineDataParsingException(this);
                }

                //return value
                return i_value;
            }

            /// <summary> Parse the string to float.</summary>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when can't parse to float.</exception>
            private float ParseToFloat(string parsedValue){
                //will hold the value
                float f_value;

                try{
                    //parse
                    f_value = Single.Parse(parsedValue);
                } catch(Exception){
                    //throw exception
                    throw new IniLineDataParsingException(this);
                }

                //return value
                return f_value;
            }


            /// <summary> Estimate the type of the value for a key-value line.</summary>
            /// <exception cref="EchoCapture.Exceptions.Data.IniFile.IniLineDataParsingException"> Thrown when line isn't a key-value line.</exception>
            private static Type EstimateType(string line){
                //split into two part; one for key and the other value
                //and trim whitespace
                string[] part = line.Split(IniFile.SELECTOR, 2, StringSplitOptions.TrimEntries);

                //check if it's a valid key-value
                if(part.Length == 2 && !String.IsNullOrEmpty(part[0]) && !String.IsNullOrEmpty(part[1])){
                    //hold the value part and comment part
                    string valuePart = null;
                    string commentPart = null;

                    //check if it's a comment
                    for(int i = 0; i < IniFile.COMMENT_CHARS.Length; i++){
                        //hold the index of comment char
                        int charIndex;
                        //search for comment char
                        charIndex = part[1].IndexOf(IniFile.COMMENT_CHARS[i], 0);

                        //not found
                        if(charIndex == -1){
                            continue;
                        }

                        //check if escaped
                        if(charIndex - 1 >= 0 && part[1][charIndex-1] == '\\'){
                            continue;
                        }

                        //check if comment char is first character
                        if(charIndex == 0){
                            //update parts
                            commentPart = part[1];
                            break;
                        }

                        //update parts
                        valuePart = part[1].Substring(0, charIndex);
                        commentPart = part[1].Substring(charIndex);
                        break;
                    }

                    //no comment char was found
                    //update value part
                    if(String.IsNullOrEmpty(commentPart) && String.IsNullOrEmpty(valuePart)){
                        valuePart = part[1];
                    }
                    
                    //no value to identify type
                    if(valuePart == null){
                        throw new IniLineDataParsingException();
                    }

                    //boolean type
                    if(valuePart == bool.TrueString || valuePart == bool.FalseString){
                        return typeof(bool);
                    }
                    
                    //try to parse int
                    try{
                        int.Parse(valuePart);
                        return typeof(int);
                    } catch (Exception){}

                    //try to parse float
                    try{
                        float.Parse(valuePart);
                        return typeof(float);
                    } catch (Exception){}

                    //string type
                    return typeof(string);
                }

                //line is not a key-value line
                throw new IniLineDataParsingException();
            }

            /// <summary> Determine if line is a proper section header.</summary>
            private static bool IsHeader(string line){
                //determine if it's a section header
                bool? isSectionHeader = null;

                //check if it's a section header
                if(line.StartsWith(IniFile.START_SECTION)){
                    //get index of end section header char
                    int endSectionCharIndex = line.IndexOf(IniFile.END_SECTION);

                    //check if exists end section header char
                    if(endSectionCharIndex != -1){
                        //check if does have a section name
                        if(endSectionCharIndex - 1 > 0){
                            //update state
                            isSectionHeader = true;

                            //get section header
                            string part = line.Substring(1, endSectionCharIndex-1);

                            part.Dump();
                            //invalid section header
                            if(part.Contains('\n')){
                                return false;
                            }
                        }
                    }
                }

                //check if it's a comment
                for(int i = 0; i < IniFile.COMMENT_CHARS.Length; i++){
                    if(isSectionHeader == true){
                        //seperate string
                        string[] subPart = line.Split(IniFile.COMMENT_CHARS[i], 2, StringSplitOptions.TrimEntries);

                        //check if contains addional start/end section
                        //thus making it invalid
                        if((subPart[0].Split(IniFile.START_SECTION).Length - 1) != 1 || (subPart[0].Split(IniFile.END_SECTION).Length - 1) != 1){
                            //update state
                            isSectionHeader = null;
                            break;
                        }
                    }
                    if(line[0] == IniFile.COMMENT_CHARS[i]){
                        isSectionHeader = false;
                        break;
                    }
                }

                //header
                if(isSectionHeader == true){
                    return true;
                }
                //default
                return false;
            }

            /// <summary> Determine if line is fully commented.</summary>
            private static bool IsFullyCommented(string line){
                //loop through comment chars
                for(int i = 0; i < IniFile.COMMENT_CHARS.Length; i++){
                    if(line[0] == IniFile.COMMENT_CHARS[i]){
                        return true;
                    }
                }

                return false;
            }


            /// <summary> Reconstructed the line and return it.</summary>
            /// <remarks> It's not the raw line. Break line, tab, etc... are represented in the escaped form.</remarks>
            /// <exception cref="System.InvalidOperationException"> Thrown when the current line is invalid.</exception>
            public override string ToString(){
                //line type
                switch(this.LineType){
                    case IniLineType.SectionHeader:
                        //hold the constructed line
                        string constructedLine = $"[{this.sectionHeader}]";

                        //update line
                        if(!String.IsNullOrEmpty(this.InlineComment)){
                            //update with alt comment char
                            constructedLine += $" {this.InlineComment.Remove(0, 1).Insert(0, IniFile.ALT_COMMENT.ToString())}";
                        }

                        return constructedLine;

                    case IniLineType.FullyCommented:
                        //return with default comment char
                        return this.InlineComment.Remove(0, 1).Insert(0, IniFile.COMMENT.ToString());

                    case IniLineType.KeyValue:
                        //hold the constructed line
                        string keyConstructedLine = $"{this.key} = {this.value.ToString()}";

                        //update line
                        if(!String.IsNullOrEmpty(this.InlineComment)){
                            //update with alt comment char
                            keyConstructedLine += $" {this.InlineComment.Remove(0, 1).Insert(0, IniFile.ALT_COMMENT.ToString())}";
                        }

                        return keyConstructedLine;

                    default:
                        //invalid line
                        throw new InvalidOperationException("Current line is invalid.");
                }
            }

            /// <summary> Return the raw line got after reading.</summary>
            /// <remarks> Break line, tab, etc... are represented in the escaped form.</remarks>
            /// <exception cref="System.InvalidOperationException"> Thrown when the current line is invalid.</exception>
            public string ToRawString(){
                if(this.lineType == IniLineType.Invalid){
                    //invalid line
                    throw new InvalidOperationException("Current line is invalid.");
                }

                //return raw line
                return this.line;
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