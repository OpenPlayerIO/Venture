using System;
using System.Collections.Generic;
using System.Reflection;

namespace PlayerIOClient
{
    /// <summary>
    /// The Player.IO ErrorLog service.
    /// 
    /// <para>
    /// This service is used for writing error messages which can be later viewed within the
    /// Player.IO admin panel, primarily for debugging purposes.
    /// </para>
    /// </summary>
    public class ErrorLog
    {
        internal ErrorLog(PlayerIOChannel channel)
        {
            this.Channel = channel;
        }

        /// <summary>
        /// Write an entry to the game's error log. In development the errors are just written to the
        /// console, in production they're written to a database and browseable from the admin panel
        /// </summary>
        /// <param name="error">
        /// A short string describing the error without details. Example 'Unhandled exception'
        /// </param>
        public void WriteError(string error)
        {
            this.WriteError(error, string.Empty, string.Empty);
        }

        /// <summary>
        /// Write an entry to the game's error log. In development the errors are just written to the
        /// console, in production they're written to a database and browseable from the admin panel
        /// </summary>
        /// <param name="error">
        /// A short string describing the error without details. Example 'Unhandled exception'
        /// </param>
        /// <param name="exception">
        /// The exception that caused the error
        /// </param>
        public void WriteError(string error, Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            this.WriteError(error, exception.Message, exception.StackTrace);
        }


        /// <summary>
        /// Write an entry to the game's error log. In development the errors are just written to the
        /// console, in production they're written to a database and browseable from the admin panel
        /// </summary>
        /// <param name="error">
        /// A short string describing the error without details. Example 'Unhandled exception'
        /// </param>
        /// <param name="details">
        /// Describe the error in more detail if you have it. Example 'couldn't find the user 'bob'
        /// in the current game'
        /// </param>
        /// <param name="stacktrace">
        /// The stacktrace (if available) of the error
        /// </param>
        /// <param name="extraData">
        /// Any extra data you'd like to associate with the error log entry
        /// </param>
        public void WriteError(string error, string details, string stacktrace, params KeyValuePair<string, string>[] extraData)
        {
            var (success, response, err) = this.Channel.Request<WriteErrorArgs, WriteErrorOutput>(50, new WriteErrorArgs
            {
                Source = Source,
                Error = error,
                Details = details,
                Stacktrace = stacktrace,
                ExtraData = extraData
            });

            if (!success)
                throw new PlayerIOError(err.ErrorCode, err.Message);
        }

        internal PlayerIOChannel Channel { get; }
        private readonly string Source = Assembly.GetExecutingAssembly().GetName().Name;
    }
}
