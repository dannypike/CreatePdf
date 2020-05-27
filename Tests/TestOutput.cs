using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using Spire.Pdf;
using Spire.Pdf.Exceptions;
using System.IO;
using System.Threading.Tasks;

namespace Tests
{
    public class TestBuilder
    {
        [SetUp]
        public void Setup()
        {
            pdfBuilder_ = new Builder();
        }

        [TestCase(Constants.AlreadyExistsOutput, Constants.AlreadyExistsInput
            , TestName = "Do not overwrite an existing output file")]
        public async Task DoNotOverwriteExistingPdf(string outFile, string inFile)
        {
            Assert.AreEqual(PdfErrors.OutputFileAlreadyExists, await pdfBuilder_.Create(outFile, inFile));
        }

        [Test(Description = "Does Spire.PDF detect a bad PDF?"), Order(1)]
        public void CheckSpireFailure()
        {
            using (var doc = new PdfDocument())
            {
                Assert.Catch<PdfDocumentException>(() =>
                {
                    doc.LoadFromFile(Constants.Sample1Txt);
                }, "Spire failed to throw exception on loading something that is not a PDF document");
            }
        }

        [Test(Description = "Does Spire.PDF parse a valid PDF?"), Order(1)]
        public void CheckSpireSuccess()
        {
            using (var doc = new PdfDocument())
            {
                doc.LoadFromFile(Constants.ExistingPdf);
                Assert.AreEqual(3, doc.Pages.Count
                    , "Spire failed to parse a valid PDF");
            }
        }

        [TestCase(@"..\..\..\..\Tests\outputs\autoCreateDirectory\sample1.pdf", Constants.Sample1Txt
            , TestName = "Create output directory")]
        public async Task AutoCreateOutputDirectory(string outFile, string inFile)
        {
            // Delete the output directory, if it still exists from an earlier run
            var directory = Path.GetDirectoryName(outFile);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Assert.AreEqual(false, Directory.Exists(directory));

            // Run the test
            Assert.AreEqual(PdfErrors.None, await pdfBuilder_.Create(outFile, inFile));
            Assert.AreEqual(true, File.Exists(outFile), $"failed to create output file {outFile}");
        }

        [TestCase(@"..\..\..\..\Tests\outputs\???invalid???\sample1.pdf", Constants.Sample1Txt
            , TestName = "Invalid directory string")]
        public async Task InvalidOutputDirectory1(string outFile, string inFile)
        {
            Assert.AreEqual(PdfErrors.InvalidOutputPath, await pdfBuilder_.Create(outFile, inFile));
        }

        [TestCase(@"\\DOESNOTEXIST\sample1.pdf", Constants.Sample1Txt
            , TestName = "Cannot create directory")]
        public async Task InvalidOutputDirectory2(string outFile, string inFile)
        {
            // Note: this may take a few seconds because Windows will look for a server that 'DOESNOTEXIST' :)
            Assert.AreEqual(PdfErrors.InvalidOutputPath, await pdfBuilder_.Create(outFile, inFile));
        }

        [TestCase(Constants.Sample1Pdf, Constants.Sample1Txt, 1, TestName = "Sample1 is a PDF with one page")]
        [TestCase(Constants.Sample8Pdf, Constants.Sample8Txt, 2, TestName = "Sample8 is a PDF with two pages")]
        [TestCase(Constants.SampleThreePagesPdf, Constants.ThreePagesTxt, 3, TestName = "SampleThreePagesPdf is a PDF with three pages")]
        public async Task CheckPageCounts(string outFile, string inFile, int pageCount)
        {
            if (File.Exists(outFile))
            {
                Assert.DoesNotThrow(() => File.Delete(outFile)
                    , $"failed to delete output file {outFile}, is it in use?");
            }
            Assert.AreEqual(PdfErrors.None, await pdfBuilder_.Create(outFile, inFile));
            using (var doc = new PdfDocument())
            {
                await Task.Run(() =>
                {
                    doc.LoadFromFile(outFile);
                    Assert.AreEqual(pageCount, doc.Pages.Count, $"{outFile} has an unexpected page count");
                });
            }
        }

        private Builder pdfBuilder_;
    }
}
