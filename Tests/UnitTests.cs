using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System.IO;
using System.Threading.Tasks;

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
        public async Task DoNotOverwriteExistingPdf(string outFile, string inFile)
        {
            Assert.AreEqual(PdfErrors.OutputFileAlreadyExists, await pdfBuilder_.Create(outFile, inFile));
        }

        {
            Assert.AreEqual(PdfErrors.OutputFileAlreadyExists, pdfBuilder_.Create(outFile, inFile));
        }

        private Builder pdfBuilder_;
    }
}
