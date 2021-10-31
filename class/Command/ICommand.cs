using System.Collections.Generic;

namespace Screenshoter.Command{

    /// <summary> Interface for commands.</summary>
    public interface ICommand{
        
        /// <summary> (Get only) Determine if using default prefix.</summary>
        bool UseDefaultPrefix{
            get;
        }

        /// <summary> (Get only) Return the command prefix.</summary>
        char CommandPrefix{
            get;
        }

        /// <summary> (Get only) Return the command name.</summary>
        string CommandName{
            get;
        }

        /// <summary> (Get only) Return the command description.</summary>
        string CommandDescription{
            get;
        }

        /// <summary> (Get only) Return dictionary of command args, and key as the agrs index.</summary>
        /// <remarks> If value of key, is -1, it maybe for no args.</remarks>
        Dictionary<int, CommandArg> ArgsList{
            get;
        }

        /// <summary> (Get only) Determine if has args.</summary>
        bool HasArgs{
            get;
        }

        /// <summary> Called when the command is sent.</summary>
        /// <param name="args"> Array of string passed as arguments.</param>
        void OnSendEvent(string[] args);

        /// <summary> Subscribe from the event.</summary>
        void AddEvent();

        /// <summary> Unsubscribe from the event.</summary>
        void RemoveEvent();
    }
}