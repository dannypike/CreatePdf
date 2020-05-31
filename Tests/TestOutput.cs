using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
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
        [TestCase(Helpers.AlreadyExistsOutput, Helpers.AlreadyExistsInput
            , TestName = "Do not overwrite an existing output file")]
        public async Task DoNotOverwriteExistingPdf(string outFile, string inFile)
        {
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

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.OutputFileAlreadyExists, pdfBuilder.FatalErrorCode);
        }

        [Test(Description = "Does Spire.PDF detect a bad PDF?"), Order(1)]
        public void CheckSpireFailure()
        {
            using (var doc = new PdfDocument())
            {
                Assert.Catch<PdfDocumentException>(() =>
                {
                    doc.LoadFromFile(Helpers.Sample1Txt);
                }, "Spire failed to throw exception on loading something that is not a PDF document");
            }
        }

        [Test(Description = "Does Spire.PDF parse a valid PDF?"), Order(1)]
        public void CheckSpireSuccess()
        {
            using (var doc = new PdfDocument())
            {
                doc.LoadFromFile(Helpers.ExistingPdf);
                Assert.AreEqual(3, doc.Pages.Count
                    , "Spire failed to parse a valid PDF");
            }
        }

        [TestCase(@"..\..\..\..\Tests\outputs\autoCreateDirectory\sample1.pdf", Helpers.Sample1Txt
            , TestName = "Create output directory")]
        public async Task AutoCreateOutputDirectory(string outFile, string inFile)
        {
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

            // ... remove the directory, if it still exists from an earlier test run
            var directory = Path.GetDirectoryName(outFile);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Assert.AreEqual(false, Directory.Exists(directory));

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.Success, pdfBuilder.FatalErrorCode);
            Assert.AreEqual(true, File.Exists(outFile), $"failed to create output file {outFile}");
        }

        [TestCase(@"..\..\..\..\Tests\outputs\???invalid???\sample1.pdf", Helpers.Sample1Txt
            , TestName = "Invalid directory string")]
        public async Task InvalidOutputDirectory1(string outFile, string inFile)
        {
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

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.InvalidOutputPath, pdfBuilder.FatalErrorCode);
        }

        [TestCase(@"\\DOESNOTEXIST\sample1.pdf", Helpers.Sample1Txt
            , TestName = "Cannot create directory")]
        public async Task InvalidOutputDirectory2(string outFile, string inFile)
        {
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

            // act
            // Note: this may take a few seconds because Windows will look for a server that 'DOESNOTEXIST' :)
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.InvalidOutputPath, pdfBuilder.FatalErrorCode);
        }

        [TestCase(Helpers.Sample1Pdf, Helpers.Sample1Txt, 1, TestName = "Sample1 is a PDF with one page")]
        [TestCase(Helpers.Sample8Pdf, Helpers.Sample8Txt, 2, TestName = "Sample8 is a PDF with two pages")]
        [TestCase(Helpers.SampleThreePagesPdf, Helpers.ThreePagesTxt, 3, TestName = "SampleThreePagesPdf is a PDF with three pages")]
        public async Task CheckPageCounts(string outFile, string inFile, int pageCount)
        {
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