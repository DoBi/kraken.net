using System;

namespace Kraken.Models
{
    /// <summary>
    /// An error from the Kraken API
    /// </summary>
    public class Error
    {
        /// <summary>
        /// The severity levels
        /// </summary>
        public enum Severity
        {
            /// <summary>
            /// A real error
            /// </summary>
            Error,
            /// <summary>
            /// A warning
            /// </summary>
            Warning
        }

        /// <summary>
        /// The severity level of this error
        /// </summary>
        public Severity SeverityCode { get; set; }

        /// <summary>
        /// The category of this error
        /// </summary>
        /// TODO: Maybe put categories to an enum
        public String Category { get; set; }

        /// <summary>
        /// A rough description of this error
        /// </summary>
        public String ErrorType { get; set; }

        /// <summary>
        /// Some extra information
        /// </summary>
        public String ExtraInfo { get; set; }

        /// <summary>
        /// Create an new error from the Kraken response
        /// </summary>
        /// <param name="errorText">Text with error information</param>
        public Error(String errorText)
        {
            // Error setup from Kraken documentation:
            // <char-severity code><string-error category>:<string-error type>[:<string-extra info>]

            if (!String.IsNullOrWhiteSpace(errorText))
            {
                if (errorText.Substring(0, 1) == "W")
                    SeverityCode = Severity.Warning;
                else
                    SeverityCode = Severity.Error;

                string[] parts = errorText.Split(':');

                Category = parts[0].Substring(1);
                ErrorType = parts[1];

                if (parts.Length > 2)
                    ExtraInfo = parts[2];
            }
        }
    }
}