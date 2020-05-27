using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System.IO;
using System.Threading.Tasks;

namespace Tests
{
    public class UnitTests
    {
        const string Sample1Txt = @"..\..\..\..\samples\sample1.txt";
        const string Sample1Pdf = @"..\..\..\..\samples\sample1.pdf";

        [SetUp]
        public void Setup()
        {
            pdfBuilder_ = new Builder();
        }

        [TestCase(@"..\..\..\..\outputs\outputAlreadyExists.pdf", @"..\..\..\..\samples\outputAlreadyExists.txt"
            , TestName = "Do not overwrite an existing output file")]
        public async Task DoNotOverwriteExistingPdf(string outFile, string inFile)
        {
            Assert.AreEqual(PdfErrors.OutputFileAlreadyExists, await pdfBuilder_.Create(outFile, inFile));
        }

        [TestCase(@"..\..\..\..\Tests\outputs\autoCreateDirectory\sample1.pdf", Sample1Txt
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

        [TestCase(@"..\..\..\..\Tests\outputs\???invalid???\sample1.pdf", Sample1Txt
            , TestName = "Invalid directory string")]
        public async Task InvalidOutputDirectory1(string outFile, string inFile)
        {
            Assert.AreEqual(PdfErrors.InvalidOutputPath, await pdfBuilder_.Create(outFile, inFile));
        }

        [TestCase(@"\\DOESNOTEXIST\sample1.pdf", Sample1Txt
            , TestName = "Cannot create directory")]
        public async Task InvalidOutputDirectory2(string outFile, string inFile)
        {
            Assert.AreEqual(PdfErrors.InvalidOutputPath, await pdfBuilder_.Create(outFile, inFile));
        }

        private Builder pdfBuilder_;
    }
}
