using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace PdfBuilder.Abstractions
{
    public interface IPdfBuilderOptions
    {
        string Input { get; }
        string Output { get; }

        /// <summary>
        /// The CancellationTokenSource that is used to signal the service to shutdown
        /// when a fatal error occurs. May be null, if cancellation tokens are not
        /// being used
        /// </summary>
        CancellationTokenSource Cts { get; set; }
    }
}