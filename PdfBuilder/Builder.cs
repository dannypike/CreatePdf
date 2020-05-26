using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;
using PdfBuilder.HtmlBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfBuilder
{
    public class Builder
    {
        private IServiceProvider DI { get; set; }

        public Builder()
        {
            var services = new ServiceCollection()
                .AddLogging(builder => builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace)
                    );
            services.Add(new ServiceDescriptor(typeof(IHtmlBodyFactory), typeof(HtmlBodyFactory), ServiceLifetime.Singleton));
            DI = services.BuildServiceProvider();

            log_ = DI.GetService<ILoggerFactory>()?.CreateLogger<Builder>();
        }

        /// <summary>
        /// Reads PDF command from the inputFile and produces a PDF file as the output
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="inputFile"></param>
        /// <returns>An error result, <see cref="PdfErrors"> for details</see></returns>
        public PdfErrors Create(string outputFile, string inputFile)
        {
            PdfErrors errCode = PdfErrors.Unexpected;
            try
            {
                if (File.Exists(outputFile))
                {
                    log_?.LogError($"output file '{Path.GetFullPath(outputFile)}' already exists");
                    return PdfErrors.OutputFileAlreadyExists;
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

                var lines = File.ReadAllLines(inputFile)
                    .Select(line => line.Trim())    // Trim the lines but do not delete them,m so that we can report line numbers
                    ;

                // Build the Html from the commands and text in the input file
                var html = DI.GetRequiredService<IHtmlBodyFactory>().Create(DI);

                var lineIndex = 0;
                foreach (var line in lines)
                {
                    ++lineIndex;
                    if (line.Length ==  0)
                    {
                        continue;
                    }
                    if (line[0] == '.')
                    {
                        var words = line.Split(delims_);
                        if (commands_.TryGetValue(words[0], out CommandHandler handler))
                        {
                            if ((errCode = handler(html, words.Skip(1))) != PdfErrors.None)
                            {
                                // Assume that the handler has already output a suitable message
                                return errCode;
                            }
                        }
                        else
                        {
                            log_?.LogError($"unrecognized command '{words[0]}' "
                                + $"at '{Path.GetFullPath(inputFile)}':{lineIndex}");
                            return PdfErrors.SyntaxError;
                        }
                    }
                    else
                    {
                        if ((errCode = html.AddText(line)) != PdfErrors.None
                            || (errCode = html.AddText(" ")) != PdfErrors.None
                            )
                        {
                            // Assume that the AddText() method has already output a suitable message
                            return errCode;
                        }
                    }
                }

                // Generate the HTML that has been built from the command-file
                if ((errCode = html.Render()) != PdfErrors.None)
                {
                    // Assume that the Render() method has already output a suitable message
                    return errCode;
                }

                var renderedHtml = html.RenderedHtml;
                if (renderedHtml.Length == 0)
                {
                    log_?.LogWarning($"the command-file {Path.GetFullPath(inputFile)}' produced no output");
                        return PdfErrors.EmptyOutput;
                }

                // Render the HTML as a PDF file
                var rdr = new IronPdf.HtmlToPdf();
                rdr.PrintOptions.PaperSize = IronPdf.PdfPrintOptions.PdfPaperSize.A4;
                var pdf = rdr.RenderHtmlAsPdf(renderedHtml);
                return pdf.TrySaveAs(outputFile) ? PdfErrors.None : PdfErrors.IronFailed;
            }
            catch (Exception ex)
            {
                log_?.LogError($"caught exception: {ex.Message}");
                return PdfErrors.Exception;
            }
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
        delegate PdfErrors CommandHandler(IHtmlBody body, IEnumerable<string> extra);
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
    }
}
