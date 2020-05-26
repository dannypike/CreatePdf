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
        /// <returns>An error result, <see cref="PdfErrors"> for details</see></returns>
        PdfErrors Create(string outputFile, string inputFile);
    }
}
