namespace PdfBuilder.Abstractions
{
    public interface IPdfBuilderResult
    {
        /// <summary>
        /// Machine-readable error code
        /// </summary>
        PdfErrors ErrorCode { get; }

        /// <summary>
        /// Optional human-readable message describing the error
        /// </summary>
        string Message { get; }

        /// <summary>
        /// True if this error was severe enough to cause the service to shut down
        /// </summary>
        bool FatalError { get; }
    }
}