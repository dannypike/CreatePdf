namespace PdfBuilder.Abstractions
{
    /// <summary>
    /// The possible error codes that may be returned by the implementation of
    /// <see cref="IPdfBuilder"/>.
    /// </summary>
    public enum PdfErrors
    {
        /// <summary>
        /// The operation succeeded (there was no error or other status reported
        /// by <see cref="IPdfBuilder"/>).
        /// </summary>
        Success = 0,

        /// <summary>
        /// An unexpected error happened. Unexpected errors indicate that there
        /// is a coding error (bug) somewhere inside <see cref="IPdfBuilder"/>.
        /// </summary>
        /// <seealso cref="WriteFailed"/>
        Unexpected,

        /// <summary>
        /// The command file name given by <see cref="IPdfBuilderOptions.Input"/>
        /// could not be opened, presumably because it does not exist.
        /// </summary>
        InputFileNotFound,

        /// <summary>
        /// No name was supplied for the output file
        /// </summary>
        /// <seealso cref="IPdfBuilderOptions.Output"/>
        MissingOutputFile,

        /// <summary>
        /// The output file cannot be written because that file already exists and the
        /// <see cref="IPdfBuilderOptions.Overwrite"/> was not set to True. By default,
        /// <see cref="IPdfBuilder"/> will not overwrite an existing file.
        /// </summary>
        /// <seealso cref="IPdfBuilderOptions.Output"/>
        /// <seealso cref="IPdfBuilderOptions.Overwrite"/>
        /// <seealso cref="InvalidOutputPath"/>
        OutputFileAlreadyExists,

        /// <summary>
        /// The command file given by <see cref="IPdfBuilderOptions.Input"/> exists but
        /// is empty. No output was produced by <see cref="IPdfBuilder"/> and the output
        /// file given by <see cref="IPdfBuilderOptions.Output"/> was not written. If the
        /// output file already existed, then it was not affected by <see cref="IPdfBuilder"/>.
        /// </summary>
        EmptyOutput,

        /// <summary>
        /// The command file given by <see cref="IPdfBuilderOptions.Input"/> exists and
        /// appears to be valid but it contains an error, such as an invalid command code.
        /// </summary>
        SyntaxError,

        /// <summary>
        /// The '.indent' command in the command file (<see cref="IPdfBuilderOptions.Input"/>)
        /// requires one argument and that argument must be a decimal integer (zero, positive,
        /// or negative). If the argument is any other text, or is absent, then
        /// <see cref="IPdfBuilder"/> outputs this error.
        /// </summary>
        InvalidIndent,

        /// <summary>
        /// The <see cref="IPdfBuilder"/> implementation could not write the PDF file to disk.
        /// Possible causes include:
        /// <list type="number">
        /// <item>the <see cref="IPdfBuilderOptions.Output"/> string is not a valid file name; or</item>
        /// <item>the file exists and <see cref="IPdfBuilderOptions.Overwrite"/> is set to True, but
        /// the file is currently locked by another process.</item>
        /// </list>
        /// </summary>
        /// <seealso cref="IPdfBuilderOptions.Overwrite"/>
        /// <seealso cref="OutputFileAlreadyExists"/>
        /// <seealso cref="InvalidOutputPath"/>
        /// <seealso cref="IntermediateHtml"/>
        /// <seealso cref="Unexpected"/>
        WriteFailed,

        /// <summary>
        /// The output filename refers to a directory that does not exist and that cannot be created,
        /// e.g. it refers to a read-onyl location or contains invalid characters.
        /// </summary>
        /// <seealso cref="MissingOutputFile"/>
        /// <seealso cref="WriteFailed"/>
        InvalidOutputPath,

        /// <summary>
        /// The engine was cancelled, e.g. by Ctrl-C or SIGTERM before it completed. The output file
        /// may or may not have been written.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The <see cref="IPdfBuilderOptions.HtmlIntermediate"/> filename was supplied, but the
        /// HTML file could not be written to disk. Possible causes include:
        /// <list type="number">
        /// <item>a file already exists with that name and is locked by another program, such as a text editor;</item>
        /// <item>the directory that hold sthe file is read-only; or</item>
        /// <item>the string is not a valid file name.</item>
        /// </list>
        /// </summary>
        IntermediateHtml,
    };
}