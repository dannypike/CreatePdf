using System.Threading;

namespace PdfBuilder.Abstractions
{
    public interface IPdfBuilderOptions
    {
        /// <summary>
        /// The name of the command file that contains the instructions and raw text
        /// to use for building the PDF.
        /// </summary>
        string Input { get; set; }

        /// <summary>
        /// The name of the output PDF file to create.
        /// </summary>
        string Output { get; set; }

        /// <summary>
        /// Set to true to allow the Builder to overwrite the output file if it
        /// already exists.
        /// </summary>
        bool Overwrite { get; set; }

        /// <summary>
        /// The CancellationTokenSource that is used to signal the service to shutdown
        /// when a fatal error occurs. May be null, if cancellation tokens are not
        /// being used.
        /// </summary>
        CancellationTokenSource Cts { get; set; }

        /// <summary>
        /// Setting this to the name of a file will cause PdfBuilder to save the HTML
        /// string that it builds to that file.
        /// </summary>
        string HtmlIntermediate { get; set; }
    }
}