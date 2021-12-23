using System;
using EchoCapture.Exceptions;

namespace EchoCapture.Command{

    /// <summary> Structure representing data about a command argument.</summary>
    public struct CommandArg{
        
        /// <summary> Hold the argument name.</summary>
        private string argName;

        /// <summary> The argument number, starting from 1.</summary>
        private int argNumber;
        
        /// <summary> Hold the argument description.</summary>
        private string argDescription;

        /// <summary> Hold the argument types.</summary>
        private Type[] argType;

        /// <summary> (Get only) Return the arg name.</summary>
        public string ArgName{
            get{
                return this.argName;
            }
        }

        /// <summary> (Get only) Return the argument number.</summary>
        public int ArgNumber{
            get{
                return this.argNumber;
            }
        }

        /// <summary> (Get only) Return the arg description.</summary>
        public string ArgDescription{
            get{
                return this.argDescription;
            }
        }

        /// <summary> (Get only) Return the arg type(s).</summary>
        public Type[] ArgType{
            get{
                return this.argType;
            }
        }


        /// <param name="argName"> The name of the argument.</param>
        /// <param name="argNumber"> The number of the argument, starting from 1.</param>
        /// <param name="argDescription"> The description of the argument.</param>
        /// <param name="argType"> The argument type. It can only be type of: string, bool or int.</param>
        /// <exception cref="System.ArgumentException"> If <paramref name="argType"/> is not type of: string, bool or int.</exception>
        public CommandArg(string argName, int argNumber, string argDescription, Type argType){
            //check if type is incorrect
            if(argType != typeof(string) && argType != typeof(bool) && argType != typeof(int)){
                throw new ArgumentException("Parameter 4 can only be type of: string, bool or int.");
            }

            //update reference
            this.argName = argName;
            this.argNumber = argNumber;
            this.argDescription = argDescription;
            this.argType = new Type[1]{argType};
        }

        /// <param name="argName"> The name of the argument.</param>
        /// <param name="argNumber"> The number of the argument, starting from 1.</param>
        /// <param name="argDescription"> The description of the argument.</param>
        /// <param name="argTypes"> The argument types. It can only be type of: string, bool or int.</param>
        /// <exception cref="System.ArgumentException"> If <paramref name="argType"/> is not type of: string, bool or int.</exception>
        public CommandArg(string argName, int argNumber, string argDescription, Type[] argTypes){
            foreach(Type t in argTypes){
                //check if type is incorrect
                if(t != typeof(string) && t != typeof(bool) && t != typeof(int)){
                    throw new ArgumentException("Parameter 4 can only be type of: string, bool or int.");
                }
            }

            //update reference
            this.argName = argName;
            this.argNumber = argNumber;
            this.argDescription = argDescription;
            this.argType = argTypes;
        }

        /// <summary> Parse argument to stated type.</summary>
        /// <param name="argument"> Argument to pass.</param>
        /// <param name="value"> Hold the passed value.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        public void Parse(string argument, out bool value){
            //check if not same type
            if(!this.IsType(typeof(bool))){
                throw new InvalidOperationException("Line argument's type is " + this.argType[0] + ", not bool.");
            }

            //hold output
            bool? output;

            //try to parse
            CommandArg.ParseBool(argument, out output);

            //check if failed
            if(output == null){
                //throw exception
                throw new InvalidLineArgumentException(this.argNumber, this.argName, typeof(bool));
            }

            //update
            value = (bool) output;
        }

        /// <summary> Parse argument to stated type.</summary>
        /// <param name="argument"> Argument to pass.</param>
        /// <param name="value"> Hold the passed value.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="EchoCapture.Exceptions.InvalidLineArgumentException"></exception>
        public void Parse(string argument, out int value){
            //check if not same type
            if(!this.IsType(typeof(int))){
                throw new InvalidOperationException("Line argument's type is " + this.argType[0] + ", not integer.");
            }

            //hold output
            int? output;

            //try to parse
            try{
                output = Int32.Parse(argument);
            } catch(OverflowException){
                //throw exception
                throw new InvalidLineArgumentException(this.argNumber, this.argName, typeof(int), "Value is too large.");
            } catch(Exception){
                //throw exception
                throw new InvalidLineArgumentException(this.argNumber, this.argName, typeof(int));
            }
            
            //update
            value = (int) output;
        }


        /// <summary> Show whether argument type is same as <paramref name="argType"/>.</summary>
        /// <param name="argType"> The type to check for.</param>
        public bool IsType(Type argType){
            foreach (Type t in this.argType){
                //check if type is correct
                if(t == argType){
                    return true;
                }
            }

            //incorrect
            return false;
        }

        /// <summary> Parse argument into boolean.</summary>
        /// <param name="string"> The argument to parse.</param>
        /// <param name="output"> The parsed argument, null if failed to parse.</param>
        public static void ParseBool(string argument, out bool? output){
            //lower case
            argument = argument.ToLower();

            //check if true
            if(argument == "true" || argument == "1"){
                //update
                output = true;

                return;
            //check if not false
            } else if(argument == "false" || argument == "0"){
                //update
                output = false;

                return;
            }

            output = null;
        }
        
        /// <summary> Parse argument into integer.</summary>
        /// <param name="string"> The argument to parse.</param>
        /// <param name="output"> The parsed argument, null if failed to parse.</param>
        public static void ParseInt(string argument, out int? output){
            //will hold the value
            int parsedValue;

            try{
                //parse
                parsedValue = Int32.Parse(argument);
            } catch (Exception){
                //update
                output = null;
                //stop
                return;
            }

            //update
            output = parsedValue;
        }
    }
}