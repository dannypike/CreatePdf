using PdfBuilder.Abstractions;
using System;
using System.IO;

namespace PdfBuilder
{
    public class Builder : IBuilder
    {
        /// <see cref="IBuilder"/>
        public PdfErrors Create(string outputFile, string inputFile)
        {
            try
            {
                if (File.Exists(outputFile))
                {
                    return PdfErrors.OutputFileAlreadyExists;
                }

                return PdfErrors.NotImplemented;
            }
            catch (Exception)
            {
                return PdfErrors.Exception;
            }
        }
    }
}
