namespace PdfBuilder.Abstractions
{
    public enum PdfErrors
    {
        None = 0,   // Although zero will be the default for enums, I like to emphasize this here so that 
                    // the "None" value can be used as the return value from Main() on success.
        InputFileNotFound,
        OutputFileAlreadyExists,    // For safety, we do not delete any existing files

        Exception,
        NotImplemented,
    };
}
