using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PdfBuilder;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CreatePdf
{
    internal class Program
    {
        private static int exitCode_ = 0;

        public static void SetExitCode(int value)
        {
            if (exitCode_ == 0)
            {
                // Capture the first non-zero exit code (ignore cascading errors)
                exitCode_ = value;
            }
        }

        /// <summary>
        /// Accepts an input and output file name. The input file name contains the definition
        /// (command and raw text) for a PDF file and the PDF is written to the output filename
        /// </summary>
        /// <param name="args">Command-line arguments: '--input=file_name_1 --output=file_name_2'</filename></param>
        private static async Task<int> Main(string[] args)
        {
            Console.Title = "CreatePdf - a sample PDF generator";

            var cts = new CancellationTokenSource();
            await new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.PdfBuilderFromCommandLine(args, cts);
                })
                .ConfigureLogging(builder => builder
                    .AddConsole(options =>
                        options.Format = ConsoleLoggerFormat.Systemd
                        )
                    )
                .RunConsoleAsync(cts.Token);

            return exitCode_;
        }
    }
}