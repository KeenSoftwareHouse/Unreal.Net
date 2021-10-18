// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Core
{
    public enum LogVerbosity : byte
    {
        /// <Summary> Not used</Summary>
        NoLogging = 0,

        /// <Summary> Always prints a fatal error to console (and log file) and crashes (even if logging is disabled)</Summary>
        Fatal,

        /// <Summary> 
        /// Prints an error to console (and log file). 
        /// Commandlets and the editor collect and report errors. Error messages result in commandlet failure.
        /// </Summary>
        Error,

        /// <Summary> 
        /// Prints a warning to console (and log file).
        /// Commandlets and the editor collect and report warnings. Warnings can be treated as an error.
        /// </Summary>
        Warning,

        /// <Summary> Prints a message to console (and log file)</Summary>
        Display,

        /// <Summary> Prints a message to a log file (does not print to console)</Summary>
        Log,

        /// <Summary> 
        /// Prints a verbose message to a log file (if Verbose logging is enabled for the given category, 
        /// usually used for detailed logging) 
        /// </Summary>
        Verbose,

        /// <Summary> 
        /// Prints a verbose message to a log file (if VeryVerbose logging is enabled, 
        /// usually used for detailed logging that would otherwise spam output) 
        /// </Summary>
        VeryVerbose,

        All = VeryVerbose,
        NumVerbosity,
    }
}