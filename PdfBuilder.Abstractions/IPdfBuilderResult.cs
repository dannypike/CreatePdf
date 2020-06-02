namespace PdfBuilder.Abstractions
{
    /// <summary>
    /// Describes an informational, warning or error message that was
    /// output by the <see cref="IPdfBuilder"/> implementation.
    /// </summary>
    /// <seealso cref="IPdfBuilder.Results"/>
    /// <seealso cref="IPdfBuilder.FatalErrorCode"/>
    /// <seealso cref="IPdfBuilderResults"/>
    public interface IPdfBuilderResult
    {
        /// <summary>
        /// Machine-readable error code
        /// </summary>
        PdfErrors ErrorCode { get; }

        /// <summary>
        /// True if this error was severe enough to cause the service to shut down
        /// </summary>
        bool FatalError { get; }

        /// <summary>
        /// Optional human-readable message describing the error
        /// </summary>
        string Message { get; }
    }
}