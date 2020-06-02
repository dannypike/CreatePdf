using PdfBuilder.Abstractions;

namespace PdfBuilder
{
    /// <see cref="IPdfBuilderResult" />
    internal class PdfBuilderResult : IPdfBuilderResult
    {
        /// <see cref="IPdfBuilderResult.ErrorCode" />
        public PdfErrors ErrorCode { get; internal set; }

        /// <see cref="IPdfBuilderResult.FatalError" />
        public bool FatalError { get; internal set; }

        /// <see cref="IPdfBuilderResult.Message" />
        public string Message { get; internal set; }
    }
}