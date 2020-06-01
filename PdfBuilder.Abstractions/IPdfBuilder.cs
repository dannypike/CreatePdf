using Microsoft.Extensions.Hosting;
using System;

namespace PdfBuilder.Abstractions
{
    public interface IPdfBuilder : IHostedService
    {
        /// <summary>
        /// Helper property - returns the error code, if there was any fatal error
        /// </summary>
        PdfErrors FatalErrorCode { get; }

        /// <summary>
        /// Error reports from the HTML builder engine
        /// </summary>
        IPdfBuilderResults Results { get; }

        /// <summary>
        /// Register a function to receive the FatalErrorCode from this instance
        /// (I cannot work out a good way to return a value from an IHostedService
        /// using Microsoft's DI system)
        /// </summary>
        /// <param name="handler">The first fatal error code that was detecetd by this IPdfBuilder</param>
        /// <returns>this</returns>
        IPdfBuilder RegisterFatalErrorCodeHandler(Action<PdfErrors> handler);
    }
}