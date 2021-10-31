using System;
using System.Collections.Generic;

namespace Screenshoter{

    /// <summary> Static class that is used for debugging.</summary>
    public static class Debug{
        
        /// <summary> Output the length of string, along with its content to console.</summary>
        /// <param name="value"> The string to debug.</param>
        public static void Dump(string value){
            //create info
            value = "string(" + value.Length + ", \"" + value + "\")";

            //output as new line
            Console.WriteLine(value);
        }

        /// <summary> Update reference with a debugged string. It contains length of string, along with its content to console.</summary>
        /// <param name="value"> The string to debug.</param>
        /// <param name="output"> Reference that will be updated with the debugged string.</param>
        public static void Dump(string value, out string output){
            //update
            output = "string(" + value.Length + ", \"" + value + "\")";
        }

        /// <summary> Output the integer to console.</summary>
        public static void Dump(int value){
            //create line
            string output = "int(" + value + ")";

            //output as new line
            Console.WriteLine(output);
        }

        /// <summary> Output the integer to console.</summary>
        /// <param name="output"> Reference that will be updated with the debugged string.</param>
        public static void Dump(int value, out string output){
            //update
            output = "int(" + value + ")";
        }

        /// <summary> Debug the List and output to console.</summary>
        /// <param name="value"> The LinkedList to debug.</summary>
        public static void Dump<T>(List<T> value){
            //check if is a valid type
            if(typeof(T) != typeof(int) && typeof(T) != typeof(string)){
                //to-do: throw exception
                return;
            }

            //the string that will be output
            string output = "List<" + typeof(T) + ">(" + value.Count + ")";

            //prepare for element
            if(value.Count > 0){
                output += ":";
            }

            //loop through link list
            for(int i = 0; i < value.Count; i++){
                //get elem
                T elem = value[i];

                try{
                    //will hold the element debugged line
                    string elemDebug;
                    //get debug
                    SearchDebug<T>(elem, out elemDebug);

                    //update output
                    output += "\n\t[" + i + "] " + elemDebug;
                } catch(InvalidCastException){
                    continue;
                }
            }

            //output
            Console.WriteLine(output);
        }

        /// <summary> Debug the List and output to console.</summary>
        /// <param name="value"> The LinkedList to debug.</summary>
        /// <param name="output"> Reference that will be updated with the debugged string or null.</param>
        public static void Dump<T>(List<T> value, out string output){
            //default value
            output = null;

            //check if is a valid type
            if(typeof(T) != typeof(int) && typeof(T) != typeof(string)){
                //to-do: throw exception
                return;
            }

            //the string that will be output
            string nonFinalOutput = "List<" + typeof(T) + ">(" + value.Count + ")";

            //prepare for element
            if(value.Count > 0){
                nonFinalOutput += ":";
            }

            //loop through link list
            for(int i = 0; i < value.Count; i++){
                //get elem
                T elem = value[i];

                try{
                    //will hold the element debugged line
                    string elemDebug;
                    //get debug
                    SearchDebug<T>(elem, out elemDebug);

                    //update output
                    nonFinalOutput += "\n\t[" + i + "] " + elemDebug;
                } catch(InvalidCastException){
                    continue;
                }
            }

            //output
            output = nonFinalOutput;
        }

        /// <summary> Debug the LinkedList and output to console.</summary>
        /// <param name="value"> The LinkedList to debug.</summary>
        public static void Dump<T>(LinkedList<T> value){
            //check if is a valid type
            if(typeof(T) != typeof(int) && typeof(T) != typeof(string)){
                //to-do: throw exception
                return;
            }

            //the string that will be output
            string output = "LinkedList<" + typeof(T) + ">(" + value.Count + ")";

            //prepare for element
            if(value.Count > 0){
                output += ":";
            }

            //loop through link list
            foreach(T elem in value){
                try{
                    //will hold the element debugged line
                    string elemDebug;
                    //get debug
                    SearchDebug<T>(elem, out elemDebug);

                    //update output
                    output += "\n\t" + elemDebug;
                } catch(InvalidCastException){
                    continue;
                }
            }

            //output
            Console.WriteLine(output);
        }

        /// <summary> Debug the LinkedList and output to console.</summary>
        /// <param name="value"> The LinkedList to debug.</summary>
        /// <param name="output"> Reference that will be updated with the debugged string or null.</param>
        public static void Dump<T>(LinkedList<T> value, out string output){
            //default value
            output = null;

            //check if is a valid type
            if(typeof(T) != typeof(int) && typeof(T) != typeof(string)){
                //to-do: throw exception
                return;
            }

            //the string that will be output
            string nonFinalOutput = "LinkedList<" + typeof(T) + ">(" + value.Count + ")";

            //prepare for element
            if(value.Count > 0){
                nonFinalOutput += ":";
            }

            //loop through link list
            foreach(T elem in value){
                try{
                    //will hold the element debugged line
                    string elemDebug;
                    //get debug
                    SearchDebug<T>(elem, out elemDebug);

                    //update output
                    nonFinalOutput += "\n\t" + elemDebug;
                } catch(InvalidCastException){
                    continue;
                }
            }

            //output
            output = nonFinalOutput;
        }

        /// <summary> Try search for the undefined value's type and debug it.</summary>
        /// <remarks> Most use case, is for run-time.</remarks>
        /// <param name="value"> Reference with undefined type to debug.</param>
        /// <param name="output"> Reference that will be updated with the debugged string.</param>
        /// <exception cref="System.InvalidCastException"></exception>
        private static void SearchDebug<T>(Object value, out string output){
            //convert object
            T newValue = (T) value;

            //check and debug
            if(newValue.GetType() == typeof(string)){
                Dump((string)value, out output);

                return;
            }

            //check and debug
            if(newValue.GetType() == typeof(int)){
                Dump((int)value, out output);

                return;
            }

            output = null;
        }

        /// <summary> Leave an empty line, basically <see cref="System.Console.WriteLine"/>.</summary>
        public static void SkipLine(){
            Console.WriteLine();
        }
    }
}