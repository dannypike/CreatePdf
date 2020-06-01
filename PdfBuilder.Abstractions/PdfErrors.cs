namespace PdfBuilder.Abstractions
{
    public enum PdfErrors
    {
        Success = 0,    // Although zero will be the default for enums, I like to emphasize this here so that

                        // the "Success" value can also be used as the ERRORLEVEL returned by Main()
        Unexpected,                 // An unexpected error occurred

        InputFileNotFound,
        InvalidOutputFile,
        OutputFileAlreadyExists,    // For safety, we do not delete any existing files
        EmptyOutput,                // The command file did not produce any output
        SyntaxError,                // unrecognized or inappropriate command in the input file
        InvalidIndent,              // The indent command argument was not an integer
        IronFailed,                 // IronPdf failed to render / save the file
        InvalidOutputPath,          // Output folder is invalid (cannot be created)
        Cancelled,                  // Pdf creation was cancelled, e.g. SIGTERM
        IntermediateHtml,           // Failed to save the intermediate Html file
    };
}