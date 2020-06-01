using Spire.Pdf;
using Spire.Pdf.Exceptions;
using System.IO;

namespace Tests
{
    public static class Helpers
    {
        /// <summary>
        /// Does a file exsts and does it appear to be a valid PDF file?
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true if SpirePdf can open the PDF file that was created by PdfBuilder using IronPdf</returns>
        public static bool IsValidPdfFile(string fileName)
        {
            // Deal with cases that might cause an "unexpected" exception in SpirePdf
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                return false;
            }

            using (var doc = new PdfDocument())
            {
                try
                {
                    doc.LoadFromFile(fileName);
                    return true;
                }
                catch (PdfDocumentException)
                {
                    return false;
                }
            }
        }
    }
}