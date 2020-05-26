namespace PdfBuilder.Abstractions
{
    public enum PdfErrors
    {
        None = 0,   // Although zero will be the default for enums, I like to emphasize this here so that 
                    // the "None" value can be used as the return value from Main() on success.
        InputFileNotFound,
        OutputFileAlreadyExists,    // For safety, we do not delete any existing files
        EmptyOutput,                // The command file did not produce any output
        SyntaxError,                // unrecognized command in the input file
        InvalidIndent,              // The indent command argument was not an integer
        BoldAndItalic,              // Nesting font formats is not supported (for time reasons)
        IronFailed,                 // IronPdf failed to render / save the file
        Exception,                  // A C# exception was thrown and not caught
        Unexpected,                 // An unexpecetd error occurred
    };
}
