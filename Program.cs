using Microsoft.Extensions.Configuration;
using PdfBuilder.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CreatePdf
{
    class Program
    {
        /// <summary>
        /// Accepts an input and output file name. The input file name contains the definition
        /// (command and raw text) for a PDF file and the PDF is written to the output filename
        /// </summary>
        /// <param name="args">Command-line arguments: '--input=file_name_1 --output=file_name_2'</filename></param>
        static async Task<int> Main(string[] args)
        {
            // Parse the command-line for the input and output file names
            var cfg = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var inputFile = cfg["input"];
            var outputFile = cfg["output"];
            
            // Create a PDF file
            var builder = new PdfBuilder.Builder();

            var errCode = await builder.Create(outputFile, inputFile);
            if (errCode != PdfErrors.None)
            {
                Console.WriteLine($"failed to create PDF file, error code = {errCode}");
                return 1;
            }
            Console.WriteLine($"successfully created PDF file '{outputFile}'");
            return 0;
        }
    }
}
