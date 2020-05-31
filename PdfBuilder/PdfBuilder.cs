using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PdfBuilder
{
    public class Builder : IPdfBuilder
    {
        /// <summary>
        /// <see cref="IPdfBuilder.Results"/>
        /// </summary>
        public IPdfBuilderResults Results { get; private set; }

        /// <summary>
        /// <see cref="IPdfBuilder.FatalErrorCode"/>
        /// </summary>
        public PdfErrors FatalErrorCode { get => Results?.FatalError?.ErrorCode ?? PdfErrors.Success; }

        public Builder(IPdfBuilderOptions options, ILogger<IPdfBuilder> logger
            , IPdfBuilderResults results, IHtmlBodyFactory factory)
        {
            Results = results;
            log_ = logger;
            options_ = options;
            factory_ = factory;
        }

        /// <summary>
        /// Reads PDF command from the inputFile and produces a PDF file as the output
        /// </summary>
        /// <returns>An error result, <see cref="PdfErrors"> for details</see></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var errCode = await Create(cancellationToken);
            if (errCode != PdfErrors.Success)
            {
                log_?.LogError($"failed to create PDF: {errCode}");
                Results.AddError(true, errCode, errCode.ToString());
            }

            // Tell the console to shutdown
            options_?.Cts?.Cancel();
        }

        /// <summary>
        /// Clean up
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads PDF command from the inputFile and produces a PDF file as the output
        /// </summary>
        /// <param name="cancellationToken">(optional) cancellation token to abort the processing</param>
        /// <returns>An error result, <see cref="PdfErrors"> for details</see></returns>
        public async Task<PdfErrors> Create(CancellationToken? cancellationToken)
        {
            PdfErrors errCode = PdfErrors.Unexpected;

            // Check the options
            if (options_?.Input != null)
            {
                // If there is an input file, it must exist
                if (!File.Exists(options_.Input))
                {
                    log_?.LogError($"input file '{options_.Input}' does not exist");
                    return PdfErrors.InputFileNotFound;
                }

                if (options_?.Output == null)
                {
                    log_?.LogError("invalid output file");
                    return PdfErrors.InvalidOutputFile;
                }

                // We do not override output files (for safety)
                if (!options_.Overwrite && File.Exists(options_.Output))
                {
                    log_?.LogError($"output file '{Path.GetFullPath(options_.Output)}' already exists");
                    return PdfErrors.OutputFileAlreadyExists;
                }
            }
            else
            {
                log_?.LogError("no input file specified");
                return PdfErrors.InputFileNotFound;
            }

            // Without a comprehensive syntax, I am making the following
            // assumptions from the example that was supplied and adding
            // some of my own that seem like a good idea:
            //
            // * It is OK to assume single-threaded only (easier to code in the time available).
            // * It is OK to use HTML as an intermediate syntax (I am not familiar with the PDF format definition).
            // * All commands are on their own line and start with a '.' (easier to parse).
            // * Commands are not case-sensitive (more user-friendly).
            // * Extra blank lines and leading and trailing whitespace should be ignored (more user-friendly).
            // * Command arguments are delimited by normal spaces (not tabs etc)
            // * Extra and unexpected command arguments are ignored (to save coding time).
            // * combined font formatting is not supported, e.g. do not support simultaneous bold and italic, to save coding time.
            // * .fill and .nofill are idempotent
            // * logging should be to the console, rather than a more suitable system such as log4net (to save time)
            var allLines = await File.ReadAllLinesAsync(options_.Input, cancellationToken ?? default);
            if (cancellationToken?.IsCancellationRequested ?? false)
            {
                return PdfErrors.Cancelled;
            }

            // Trim the lines but do not delete empty ones, so that we can report line numbers
            var lines = allLines.Select(line => line.Trim());

            // Build the Html from the commands and text in the input file
            var html = factory_.CreateHtmlBody();
            log_.LogInformation($"scanning input file {options_.Input} for commands");
            if ((errCode = Build(html, lines, cancellationToken)) != PdfErrors.Success)
            {
                // Assume that the Build() method has already output a suitable message
                return errCode;
            }

            // Generate the HTML that has been built from the command-file
            log_.LogInformation("building intermediate HTML from commands");
            if ((errCode = html.Render()) != PdfErrors.Success)
            {
                // Assume that the Render() method has already output a suitable message
                return errCode;
            }

            var renderedHtml = html.RenderedHtml;
            if (renderedHtml?.Length == 0)
            {
                log_?.LogWarning($"the command-file {Path.GetFullPath(options_.Input)}' produced no output");
                return PdfErrors.EmptyOutput;
            }

            // Render the HTML as a PDF file
            var rdr = new IronPdf.HtmlToPdf();
            rdr.PrintOptions.PaperSize = IronPdf.PdfPrintOptions.PdfPaperSize.A4;
            log_.LogInformation("converting HTML to PDF");
            var pdf = await rdr.RenderHtmlAsPdfAsync(renderedHtml);
            if (cancellationToken?.IsCancellationRequested ?? false)
            {
                return PdfErrors.Cancelled;
            }

            // Create the output folder, if it does not exist
            string directory = string.Empty;
            try
            {
                directory = Path.GetDirectoryName(options_.Output); // may be null if the outputFile is badly formed
                if (!Directory.Exists(directory))
                {
                    log_.LogInformation($"creating output directory {directory}");
                    Directory.CreateDirectory(directory);   // will throw IOException, if the outputFile folder is not valid
                }
            }
            catch (Exception ex)
            {
                log_?.LogError($"invalid output folder '{directory}': {ex.Message}");
                return PdfErrors.InvalidOutputPath;
            }

            if (cancellationToken?.IsCancellationRequested ?? false)
            {
                return PdfErrors.Cancelled;
            }
            log_.LogInformation($"writing PDF to output file {options_.Output}");
            return pdf.TrySaveAs(options_.Output) ? PdfErrors.Success : PdfErrors.IronFailed;
        }

        /// <summary>
        /// Run through the command file lines and add HTML elements
        /// </summary>
        /// <param name="html">the IHtmlBody being built</param>
        /// <param name="lines">The lines in the command file</param>
        /// <param name="cancellationToken">cancel the build</param>
        /// <returns></returns>
        private PdfErrors Build(IHtmlBody html, IEnumerable<string> lines
            , CancellationToken? cancellationToken)
        {
            var errCode = PdfErrors.Success;

            var lineIndex = 0;
            foreach (var line in lines)
            {
                if (cancellationToken?.IsCancellationRequested ?? false)
                {
                    log_?.LogWarning("PDF creation was cancelled");
                    return PdfErrors.Cancelled;
                }

                ++lineIndex;
                if (line.Length == 0)
                {
                    continue;
                }
                if (line[0] == '.')
                {
                    var words = line.Split(delims_);
                    if (commands_.TryGetValue(words[0], out CommandHandler handler))
                    {
                        if ((errCode = handler(html, words.Skip(1))) != PdfErrors.Success)
                        {
                            // Assume that the handler has already output a suitable message
                            return errCode;
                        }
                    }
                    else
                    {
                        log_?.LogError($"unrecognized command '{words[0]}' "
                            + $"at '{Path.GetFullPath(options_.Input)}':{lineIndex}");
                        return PdfErrors.SyntaxError;
                    }
                }
                else if ((errCode = html.AddText(line)) != PdfErrors.Success)
                {
                    // Assume that the AddText() method has already output a suitable message
                    return errCode;
                }
            }
            return errCode;
        }

        /// <summary>
        /// The '.paragraph' command starts a new paragraph
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.paragraph' command</param>
        /// <returns></returns>
        private static PdfErrors OnParagraph(IHtmlBody body, IEnumerable<string> extra)
            => body.NewParagraph();

        /// <summary>
        /// The '.large' command starts a heading
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.large' command</param>
        /// <returns></returns>
        private static PdfErrors OnLarge(IHtmlBody body, IEnumerable<string> extra)
            => body.StartHeading();

        /// <summary>
        /// The '.normal' command starts the body text
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.normal' command</param>
        /// <returns></returns>
        private static PdfErrors OnBody(IHtmlBody body, IEnumerable<string> extra)
            => body.StartBody();

        /// <summary>
        /// The '.fill' command starts text-justification
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.fill' command</param>
        /// <returns></returns>
        private static PdfErrors OnFill(IHtmlBody body, IEnumerable<string> extra)
            => body.RightJustify();

        /// <summary>
        /// The '.nofill' command ends text-justification
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.nofill' command</param>
        /// <returns></returns>
        private static PdfErrors OnNoFill(IHtmlBody body, IEnumerable<string> extra)
            => body.CancelJustify();

        /// <summary>
        /// The '.regular' command selects normal text font, cancelling any active
        /// italic or bold setting.
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.regular' command</param>
        /// <returns></returns>
        private static PdfErrors OnRegular(IHtmlBody body, IEnumerable<string> extra)
            => body.NormalText();

        /// <summary>
        /// The '.italic' command enables italic text. This may be combined with the '.bold'
        /// command.
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.italic' command</param>
        /// <returns></returns>
        private static PdfErrors OnItalic(IHtmlBody body, IEnumerable<string> extra)
            => body.ItalicText();

        /// <summary>
        /// The '.bold' command enables bold text
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">This should be empty for the '.bold' command</param>
        /// <returns></returns>
        private static PdfErrors OnBold(IHtmlBody body, IEnumerable<string> extra)
            => body.BoldText();

        /// <summary>
        /// The '.indent' command specifies a new indentation for the text
        /// </summary>
        /// <param name="body">The HTML being rendered</param>
        /// <param name="extra">An integer multiple of 25px, as per the CSS padding-left style</param>
        /// <returns></returns>
        private static PdfErrors OnIndent(IHtmlBody body, IEnumerable<string> extra)
        {
            var indent = extra.FirstOrDefault();
            if (int.TryParse(indent ?? string.Empty, out var increment))
            {
                return body.Indent(increment);
            }
            return PdfErrors.InvalidIndent;
        }

        /// <summary>
        /// The commands in the input file map directly to handlers that apply the appropriate formatting
        /// to HtmlBody
        /// </summary>
        private delegate PdfErrors CommandHandler(IHtmlBody body, IEnumerable<string> extra);

        private IDictionary<string, CommandHandler> commands_
            = new Dictionary<string, CommandHandler>(StringComparer.CurrentCultureIgnoreCase)
            {
                { ".large", OnLarge },
                { ".normal", OnBody },
                { ".paragraph", OnParagraph },
                { ".fill", OnFill },
                { ".nofill", OnNoFill },
                { ".regular", OnRegular },
                { ".italics", OnItalic },
                { ".bold", OnBold },
                { ".indent", OnIndent },
            };

        //
        private static char[] delims_ = new char[] { ' ', '\t' };

        private ILogger log_;
        private IPdfBuilderOptions options_;
        private IHtmlBodyFactory factory_;
    }
}