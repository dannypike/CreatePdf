using PdfBuilder.Abstractions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

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

                var rdr = new IronPdf.HtmlToPdf();
                rdr.PrintOptions.PaperSize = IronPdf.PdfPrintOptions.PdfPaperSize.A4;
                var text = @"<h1>My PDF Document</h1>
<p>This is my <em>very first</em> pdf document, and the output is formatted really well. While this
paragraph is not filled, the following one has automatic filling set:
</p><p style=""text-align: justify;indent: 50px;"">
“Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor
incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis
nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.
Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu
fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa
qui officia deserunt mollit anim id est laborum.”
</p><p>
Well that was <strong>exciting</strong>, good luck!
</p>
";
                const int count = 8;
                var sb = new StringBuilder(count * text.Length + 256);
                sb.Append(@"<body><div style=""font-size:16pt;"">");
                for (var ii = 0; ii < count; ++ii)
                {
                    sb.Append(text);
                }
                sb.Append(@"</div></body>");
                var pdf = rdr.RenderHtmlAsPdf(sb.ToString());
                pdf.SaveAs(outputFile);

                return PdfErrors.NotImplemented;
            }
            catch (Exception)
            {
                return PdfErrors.Exception;
            }
        }
    }
}
