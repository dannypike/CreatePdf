using Microsoft.Extensions.Configuration;
using PdfBuilder.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PdfBuilder
{
    public class PdfBuilderOptions : IPdfBuilderOptions
    {
        /// <summary>
        /// Default constructor for setting the options explicitly
        /// </summary>
        public PdfBuilderOptions()
        {
        }

        /// <summary>
        /// For setting up from flags passed to the command-line
        /// </summary>
        /// <param name="args">The command-line arguments, excluding the program filename</param>
        /// <param name="cts">(optional) cancellation token source</param>
        public PdfBuilderOptions(IEnumerable<string> args, CancellationTokenSource cts)
        {
            var cfg = new ConfigurationBuilder()
                .AddCommandLine(source => source.Args = args)
                .Build();
            Input = cfg["input"];
            Output = cfg["output"];
            Overwrite = Convert.ToBoolean(cfg["overwrite"] ?? "False");

            Cts = cts;
        }

        /// <summary>
        /// <see cref="IPdfBuilderOptions.Cts"/>
        /// </summary>
        public CancellationTokenSource Cts { get; set; }

        /// <summary>
        /// <see cref="IPdfBuilderOptions.HtmlIntermediate"/>
        /// </summary>
        public string HtmlIntermediate { get; set; }

        /// <summary>
        /// <see cref="IPdfBuilderOptions.Input"/>
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// <see cref="IPdfBuilderOptions.Output"/>
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// <see cref="IPdfBuilderOptions.Overwrite"/>
        /// </summary>
        public bool Overwrite { get; set; }
    }
}