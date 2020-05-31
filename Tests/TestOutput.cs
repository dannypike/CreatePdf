using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PdfBuilder;
using PdfBuilder.Abstractions;
using Spire.Pdf;
using Spire.Pdf.Exceptions;
using System.Configuration.Internal;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class TestBuilder
    {
        [TestCase(TestName = "Disallow overwriting of an existing output file")]
        public async Task DoNotOverwriteExistingPdf()
        {
            // arrange
            const string testString = "This file should not be overwritten";
            var outFile = FileNames.TestOutput("doNotOverwrite");
            var inFile = FileNames.TestInput("singleLine");
            File.WriteAllText(outFile, testString);
            Assert.IsTrue(File.Exists(outFile));
            Assert.AreEqual(testString, File.ReadAllText(outFile));

            // Set up a PdfBuilder that cannot overwite the output
            var di = new ServiceCollection()
                .AddTransient<IPdfBuilder, Builder>()
                .PdfBuilderOneFile(inFile, outFile)
                .AddLogging(configure => configure.AddConsole(options => options.Format = ConsoleLoggerFormat.Systemd))
                .BuildServiceProvider()
                ;
            var pdfBuilder = di.GetService<IPdfBuilder>();
            Assert.IsNotNull(pdfBuilder, "failed to construct PdfBuilder");
            var cts = new CancellationTokenSource();

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.OutputFileAlreadyExists, pdfBuilder.FatalErrorCode);
            Assert.IsTrue(File.Exists(outFile));
            Assert.AreEqual(testString, File.ReadAllText(outFile));
        }

        [TestCase(TestName = "Allow overwriting of an existing output file")]
        public async Task OverwriteExistingPdf()
        {
            // arrange

            // Create an outFile that will be overwritten by this test
            var outFile = FileNames.TestOutput("overwriteThisFile");
            const string testString = "This file should be overwritten";
            File.WriteAllText(outFile, testString);
            Assert.IsTrue(File.Exists(outFile));
            Assert.AreEqual(testString, File.ReadAllText(outFile));

            // Set up the PDF Builder that can overwrite the outFile
            // with valid PDF created from the inFile
            var inFile = FileNames.TestInput("singleLine");
            var di = new ServiceCollection()
                .AddTransient<IPdfBuilder, Builder>()
                .PdfBuilderOneFile(inFile, outFile, true)
                .AddLogging(configure => configure.AddConsole(options => options.Format = ConsoleLoggerFormat.Systemd))
                .BuildServiceProvider()
                ;
            var pdfBuilder = di.GetService<IPdfBuilder>();
            Assert.IsNotNull(pdfBuilder, "failed to construct PdfBuilder");
            var cts = new CancellationTokenSource();

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.Success, pdfBuilder.FatalErrorCode);
            Assert.IsTrue(Helpers.IsValidPdfFile(outFile));
        }

        [Test(Description = "Does Spire.PDF detect a bad PDF?"), Order(1)]
        public void CheckSpireFailure()
        {
            using (var doc = new PdfDocument())
            {
                Assert.Catch<PdfDocumentException>(() =>
                {
                    doc.LoadFromFile(FileNames.TestInput("textOnly"));
                }, "Spire failed to throw exception on loading something that is not a PDF document");
            }
        }

        [Test(Description = "Does Spire.PDF parse a valid PDF?"), Order(1)]
        public void CheckSpireSuccess()
        {
            using (var doc = new PdfDocument())
            {
                doc.LoadFromFile(FileNames.TestInput("existing", "pdf"));
                Assert.AreEqual(3, doc.Pages.Count
                    , "Spire failed to parse a valid PDF");
            }
        }

        [TestCase(TestName = "Create output directory")]
        public async Task AutoCreateOutputDirectory()
        {
            // arrange
            var inFile = FileNames.TestInput("singleLine");
            var outFile = FileNames.TestOutput(Path.Combine("autoCreate", "sample1"));

            var di = new ServiceCollection()
                .AddTransient<IPdfBuilder, Builder>()
                .PdfBuilderOneFile(inFile, outFile)
                .AddLogging(configure => configure.AddConsole(options => options.Format = ConsoleLoggerFormat.Systemd))
                .BuildServiceProvider()
                ;
            var pdfBuilder = di.GetService<IPdfBuilder>();
            Assert.IsNotNull(pdfBuilder, "failed to construct PdfBuilder");
            var cts = new CancellationTokenSource();

            // ... remove the directory, if it still exists from an earlier test run
            var directory = Path.GetDirectoryName(outFile);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Assert.IsFalse(Directory.Exists(directory));

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.Success, pdfBuilder.FatalErrorCode);
            Assert.IsTrue(File.Exists(outFile), $"failed to create output file {outFile}");
        }

        [TestCase(TestName = "Invalid directory string")]
        public async Task InvalidOutputDirectory1()
        {
            // arrange
            var inFile = FileNames.TestInput("singleLine");
            var outFile = FileNames.TestOutput(Path.Combine("???invalid???", "sample1"));

            var di = new ServiceCollection()
                .AddTransient<IPdfBuilder, Builder>()
                .PdfBuilderOneFile(inFile, outFile)
                .AddLogging(configure => configure.AddConsole(options => options.Format = ConsoleLoggerFormat.Systemd))
                .BuildServiceProvider()
                ;
            var pdfBuilder = di.GetService<IPdfBuilder>();
            Assert.IsNotNull(pdfBuilder, "failed to construct PdfBuilder");
            var cts = new CancellationTokenSource();

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.InvalidOutputPath, pdfBuilder.FatalErrorCode);
        }

        [TestCase(TestName = "Cannot create directory")]
        public async Task InvalidOutputDirectory2()
        {
            // arrange
            var inFile = FileNames.TestInput("singleLine");
            var outFile = @"\\DOESNOTEXIST\singleLine.pdf";

            var di = new ServiceCollection()
                .AddTransient<IPdfBuilder, Builder>()
                .PdfBuilderOneFile(inFile, outFile)
                .AddLogging(configure => configure.AddConsole(options => options.Format = ConsoleLoggerFormat.Systemd))
                .BuildServiceProvider()
                ;
            var pdfBuilder = di.GetService<IPdfBuilder>();
            Assert.IsNotNull(pdfBuilder, "failed to construct PdfBuilder");
            var cts = new CancellationTokenSource();

            // act
            // Note: this may take a few seconds because Windows will look for a server that 'DOESNOTEXIST' :)
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.InvalidOutputPath, pdfBuilder.FatalErrorCode);
        }

        [TestCase(1, TestName = "onePage.pdf is a PDF with one page")]
        [TestCase(2, TestName = "twoPages.pdf is a PDF with two pages")]
        [TestCase(3, TestName = "threePages.pdf is a PDF with three pages")]
        public async Task CheckPageCounts(int pageCount)
        {
            string inFile, outFile;
            switch (pageCount)
            {
                case 1:
                    inFile = FileNames.SampleInput("sample1");
                    outFile = FileNames.TestOutput("onePage");
                    break;

                case 2:
                    inFile = FileNames.SampleInput("sample8");
                    outFile = FileNames.TestOutput("twoPages");
                    break;

                case 3:
                    inFile = FileNames.SampleInput("threePages");
                    outFile = FileNames.TestOutput("threePages");
                    break;

                default:
                    Assert.Fail($"Unsupported test case pageCount={pageCount}");
                    return;
            }

            // arrange
            var di = new ServiceCollection()
                .AddTransient<IPdfBuilder, Builder>()
                .PdfBuilderOneFile(inFile, outFile)
                .AddLogging(configure => configure.AddConsole(options => options.Format = ConsoleLoggerFormat.Systemd))
                .BuildServiceProvider()
                ;
            var pdfBuilder = di.GetService<IPdfBuilder>();
            Assert.IsNotNull(pdfBuilder, "failed to construct PdfBuilder");
            var cts = new CancellationTokenSource();

            if (File.Exists(outFile))
            {
                Assert.DoesNotThrow(() => File.Delete(outFile)
                    , $"failed to delete output file {outFile}, is it in use?");
            }

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.Success, pdfBuilder.FatalErrorCode);
            using (var doc = new PdfDocument())
            {
                await Task.Run(() =>
                {
                    doc.LoadFromFile(outFile);
                    Assert.AreEqual(pageCount, doc.Pages.Count, $"{outFile} has an unexpected page count");
                });
            }
        }
    }
}