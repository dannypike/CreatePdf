using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;

namespace Tests
{
    public class UnitTests
    {
        [SetUp]
        public void Setup()
        {
            pdfBuilder_ = new Builder();
        }

        [TestCase("..\\output\\sample1.pdf", "..\\samples\\sample1.txt")]
        public void Test1(string outFile, string inFile)
        {
            Assert.AreEqual(true, pdfBuilder_.Create(outFile, inFile));
        }

        private IBuilder pdfBuilder_;
    }
}
