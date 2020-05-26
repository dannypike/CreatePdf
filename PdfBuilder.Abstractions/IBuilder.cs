using System;

namespace PdfBuilder.Abstractions
{
    public interface IBuilder
    {
        /// <summary>
        /// Reads PDF command from the inputFile and produces a PDF file as the output
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="inputFile"></param>
        /// <returns>true if it worked, otherwise, consult the error log</returns>
        bool Create(string outputFile, string inputFile);
    }
}
