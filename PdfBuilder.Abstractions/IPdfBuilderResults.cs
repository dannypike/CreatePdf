using System;
using System.Collections.Generic;

namespace PdfBuilder.Abstractions
{
    /// <summary>
    /// A collection of all of the errors that have been detected by the Pdf creator service
    /// </summary>
    public interface IPdfBuilderResults
    {
        /// <summary>
        /// Record an error code / message
        /// </summary>
        /// <param name="fatal">If true, the the service should shutdown immediately</param>
        /// <param name="errCode"></param>
        /// <param name="message"></param>
        void AddError(bool fatal, PdfErrors errCode, string message = null);

        /// <summary>
        /// Return a snapshot of the errors that have occurred, oldest-first
        /// </summary>
        IEnumerable<IPdfBuilderResult> Snapshot { get; }

        /// <summary>
        /// The first fatal error that has occurred, if any. It is a latching property;
        /// the set accessor has no effect if a fatal error has already been set.
        /// </summary>
        IPdfBuilderResult FatalError { get; set; }

        /// <summary>
        /// Reset the error collection, clearing the FatalError latch
        /// </summary>
        void Clear();

        /// <summary>
        /// Register a function to receive the FatalErrorCode from this instance
        /// </summary>
        /// <param name="handler">The first fatal error code that was added to this instance
        /// <returns>this</returns>
        IPdfBuilderResults RegisterFatalErrorCodeHandler(Action<PdfErrors> handler);
    }
}