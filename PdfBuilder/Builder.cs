using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;
using PdfBuilder.HtmlBuilder;
using StructureMap;
using System;
using System.Diagnostics;
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
                    .SetMinimumLevel(LogLevel.Trace)    // Control the debug levels using log4net config file
                    .AddConsole(/*options => options.LogToStandardErrorThreshold = LogLevel.Debug*/)
                    );
            services.Add(new ServiceDescriptor(typeof(IHtmlBodyFactory), typeof(HtmlBodyFactory), ServiceLifetime.Singleton));
            DI = services.BuildServiceProvider();
        }

        /// <summary>
        /// Reads PDF command from the inputFile and produces a PDF file as the output
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="inputFile"></param>
        /// <returns>An error result, <see cref="PdfErrors"> for details</see></returns>
        public PdfErrors Create(string outputFile, string inputFile)
        {
            try
            {
                if (File.Exists(outputFile))
                {
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

                var lines = File.ReadAllLines(inputFile)
                    .Select(line => line.Trim())
                    .Where(line => 0 < line.Length)
                    ;

                // Build the Html from the commands and text in the input file
                var html = DI.GetRequiredService<IHtmlBodyFactory>().Create();

                foreach (var line in lines)
                {
                    Debug.Assert(0 < line.Length);  // Just in case someone changes the LINQ above!
                    if ('.' == line[0])
                    {

                    }
                }

                //var pdf = rdr.RenderHtmlAsPdf(sb.ToString());
                //pdf.SaveAs(outputFile);

                return PdfErrors.NotImplemented;
            }
            catch (Exception)
            {
                return PdfErrors.Exception;
            }
        }
    }
}
