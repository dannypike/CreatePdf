using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;
using System.Collections.Generic;
using System.Threading;

namespace PdfBuilder
{
    public static class Extensions
    {
        /// <summary>
        /// Initialize the IServiceCollection with the invariant built-in class factories, where the caller
        /// will provide a factory for IPdfBuilderOptions
        /// </summary>
        /// <param name="services">The IServiceCollection to be updated with the
        /// default PdfBuilder factories, but without any factory for IPdfBuilderOptions</param>
        /// <returns></returns>
        public static IServiceCollection PdfBuilderBasic(this IServiceCollection services)
        {
            services
                .AddSingleton<IHostedService, Builder>()
                .AddSingleton<IHtmlBodyFactory>(di => new HtmlBodyFactory(di.GetService<ILoggerFactory>()?.CreateLogger<IHtmlBody>()))
                .AddTransient<IPdfBuilderResult, PdfBuilderResult>()
                .AddTransient<IPdfBuilderResults, PdfBuilderResults>()
                ;
            return services;
        }

        /// <summary>
        /// Initialize the IServiceCollection with the names of a single input and output file. PdfBuilder checks that
        /// the input file exists and that the output file does not exist, so getting the arguments the wrong way around
        /// should be safe.
        /// </summary>
        /// <param name="services">The IServiceCollection to be updated with the PdfBuilder factories.</param>
        /// <param name="inFile">The name of the command-line to parse for the definition of the PDF file.</param>
        /// <param name="outFile">The name of the output PDF file to create.</param>
        /// <param name="overwrite">Set to true to overwrite the output PDF, if it already exists.</param>
        /// <param name="cts"><see cref="PdfBuilderFromCommandLine(IServiceCollection, IEnumerable{string}, CancellationTokenSource)"/></param>
        /// <returns></returns>
        public static IServiceCollection PdfBuilderOneFile(this IServiceCollection services
            , string inFile, string outFile, bool overwrite = false, CancellationTokenSource cts = null)
        {
            services
                .PdfBuilderBasic()
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions
                {
                    Input = inFile,
                    Output = outFile,
                    Overwrite = overwrite,
                    Cts = cts,
                })
                ;
            return services;
        }

        /// <summary>
        /// Initialize a ServiceCollection from the command-line arguments
        /// </summary>
        /// <param name="services">The IServiceCollection to be updated with the PdfBuilder factories to use
        /// when configuring the IPdfBuilderOptions from the command-line</param>
        /// <param name="args">The command-line arguments to search for PdfBuilder options. Extra options are ignored.</param>
        /// <param name="cts">(optional, may be null) PdfBuilder is async; this CancellationTokenSource may be used to cancel an
        /// operation, if one if supplied.</param>
        public static IServiceCollection PdfBuilderFromCommandLine(this IServiceCollection services
            , IEnumerable<string> args, CancellationTokenSource cts = null)
        {
            services
                .PdfBuilderBasic()
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions(args)
                {
                    Cts = cts,
                })
                ;
            return services;
        }
    }
}