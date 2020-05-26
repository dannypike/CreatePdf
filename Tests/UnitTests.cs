using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System.IO;

namespace Tests
{
    public class UnitTests
    {
        [SetUp]
        public void Setup()
        {
            pdfBuilder_ = new Builder();
        }

        [TestCase(@"..\..\..\..\outputs\outputAlreadyExists.pdf", @"..\..\..\..\samples\outputAlreadyExists.txt")]
        public void DoNotOverwriteExistingPdf(string outFile, string inFile)
        {
            Assert.AreEqual(PdfErrors.OutputFileAlreadyExists, pdfBuilder_.Create(outFile, inFile));
        }

        private IBuilder pdfBuilder_;
    }
}
