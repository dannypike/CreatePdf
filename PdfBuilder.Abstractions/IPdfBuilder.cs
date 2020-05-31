using Microsoft.Extensions.Hosting;

namespace PdfBuilder.Abstractions
{
    public interface IPdfBuilder : IHostedService
    {
        /// <summary>
        /// Error reports from the HTML builder engine
        /// </summary>
        IPdfBuilderResults Results { get; }

        /// <summary>
        /// Helper property - returns the error code, if there was any fatal error
        /// </summary>
        PdfErrors FatalErrorCode { get; }
    }
}