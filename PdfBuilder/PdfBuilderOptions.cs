using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfBuilder.Abstractions;
using System.Collections.Generic;
using System.Threading;

namespace PdfBuilder
{
    public class PdfBuilderOptions : IPdfBuilderOptions
    {
        public enum PdfBuilderModes
        {
            Automatic,  // Choose mode according to whether Input is a file or a folder
            SingleFile,
            WatchFolder
        }

        public string Input { get; private set; }
        public string Output { get; private set; }
        public PdfBuilderModes Mode { get; set; } = PdfBuilderModes.Automatic;
        public CancellationTokenSource Cts { get; set; }

        public PdfBuilderOptions(string input, string output)
        {
            Input = input;
            Output = output;
        }

        public PdfBuilderOptions(IEnumerable<string> args)
        {
            var cfg = new ConfigurationBuilder()
                .AddCommandLine(source => source.Args = args)
                .Build();
            Input = cfg["input"];
            Output = cfg["output"];
        }
    }
}