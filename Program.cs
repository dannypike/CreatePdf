using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CreatePdf
{
    internal class Program
    {
        private static PdfErrors exitCode_ = 0;

        /// <summary>
        /// Accepts an input and output file name. The input file name contains the definition
        /// (command and raw text) for a PDF file and the PDF is written to the output filename
        /// </summary>
        /// <param name="args">Command-line arguments: '--input=file_name_1 --output=file_name_2'</param>
        private static async Task<int> Main(string[] args)
        {
            Console.Title = "CreatePdf - a sample PDF generator";

            var cts = new CancellationTokenSource();
            await new HostBuilder()
                .ConfigureServices(services =>
                {
                    services
                        .PdfBuilderBasic()
                        .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions(args, cts))
                        .AddSingleton<IHostedService>(_ => _
                            .GetService<IPdfBuilder>()
                            .RegisterFatalErrorCodeHandler(exitCode => exitCode_ = exitCode)
                            )
                        ;
                })
                .ConfigureLogging(builder => builder
                    .AddConsole(options => options.Format = ConsoleLoggerFormat.Systemd)
                    )
                .RunConsoleAsync(configure => configure.SuppressStatusMessages = true, cts.Token);

            return (int)exitCode_;
        }
    }
}