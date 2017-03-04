using System;
using System.Collections.Generic;
using System.Linq;

using Kraken.Models;

namespace Kraken
{
    /// <summary>
    /// An exception with occurs if there is a problem with the Kraken API
    /// </summary>
    public class KrakenException : Exception
    {
        /// <summary>
        /// All errors that occur
        /// </summary>
        public IList<Error> Errors { get; }

        /// <summary>
        /// Create a new KrakenException
        /// </summary>
        /// <param name="errors">All errors</param>
        public KrakenException(IList<Error> errors) : base(ErrorMessage(errors))
        {
            Errors = errors;
        }

        /// <summary>
        /// Create the exception message from the error list
        /// </summary>
        /// <param name="errors">All errors</param>
        /// <returns>The exception message</returns>
        private static String ErrorMessage(IList<Error> errors)
        {
            if (errors.Count > 0)
            {
                var firstError = errors.Where(e => e.SeverityCode == Error.Severity.Error).FirstOrDefault();
                if (firstError != null)
                    return String.Concat(firstError.Category, ": ", firstError.ErrorType, " ", firstError.ExtraInfo);
            }

            return "There was an error while calling the Kraken API";
        }
    }
}