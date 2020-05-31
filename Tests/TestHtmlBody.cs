using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class TestHtmlBody
    {
        public delegate void MockSetup(Mock<IHtmlBody> mockBody);

        /// <summary>
        /// An IHtmlBodyFactory implementation that uses a singleton IHtmlBody for us to setup with Moq
        /// </summary>
        private class Factory : IHtmlBodyFactory
        {
            public Factory(MockSetup mockSetup, MockBehavior mockBehaviour)
            {
                MockedBody = new Mock<IHtmlBody>(mockBehaviour);
                mockSetup(MockedBody);
            }

            public IHtmlBody CreateHtmlBody() => MockedBody.Object;
            public Mock<IHtmlBody> MockedBody { get; private set; }
        }

        [TestCase(TestName = "Single line of text only")]
        public async Task SingleLineTextOnly()
        {
            // arrange

            // Ensure that the output file does not exist before
            // we try to create it
            var outFile = FileNames.TestOutput("singleLine");
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            Assert.IsFalse(File.Exists(outFile));

            // Build the input file
            var inFile = FileNames.TestInput("singleLine");
            const string testString = "Hello, world";
            File.WriteAllText(inFile, testString);
            Assert.AreEqual(true, File.Exists(inFile));
            Assert.AreEqual(testString, File.ReadAllText(inFile));

            // Mock the HtmlBody to verify that the calls happen in the
            // expected order
            MockSetup mockSetup = (Mock<IHtmlBody> body) =>
            {
                var seq = new MockSequence();

                // Builder.Create() will call AddText() ...
                body.InSequence(seq)
                    .Setup(bb => bb.AddText("Hello, world"))
                    .Returns(PdfErrors.Success);

                // .. and then it will call IHtmlBody.Render() ...
                body.InSequence(seq)
                    .Setup(bb => bb.Render())
                    .Returns(PdfErrors.Success)
                    ;

                // ... and then it will retrieve the rendered HTML
                body.InSequence(seq)
                    .Setup(bb => bb.RenderedHtml)
                    .Returns($"<body><p>{testString}</p></body>");
            };

            var cts = new CancellationTokenSource();
            var di = new ServiceCollection()
                .AddSingleton<IPdfBuilder, Builder>()
                .AddSingleton<IHtmlBodyFactory>(di => new Factory(mockSetup, MockBehavior.Strict))
                .AddTransient<IPdfBuilderResult, PdfBuilderResult>()
                .AddTransient<IPdfBuilderResults, PdfBuilderResults>()
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions
                {
                    Input = inFile,
                    Output = outFile,
                    Overwrite = false,
                    Cts = cts
                })
                .AddLogging()
                .BuildServiceProvider()
                 ;
            var pdfBuilder = di.GetRequiredService<IPdfBuilder>();

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.Success, pdfBuilder.FatalErrorCode);

            // The output may be formatted differently from a standard call (font-sizing etc),
            // but it should still be a valid PDF
            Assert.IsTrue(Helpers.IsValidPdfFile(outFile));
        }

        [TestCase(TestName = "Multiple lines of text")]
        public async Task MultipleLinesTextOnly()
        {
            // arrange

            // Ensure that the output file does not exist before
            // we try to create it
            var outFile = FileNames.TestOutput("multipleLines");
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            Assert.IsFalse(File.Exists(outFile));

            // Build the input file
            var inFile = FileNames.TestInput("multipleLines");
            const string testString = "Hello,\r\nworld\r\n";
            File.WriteAllText(inFile, testString);
            Assert.AreEqual(true, File.Exists(inFile));
            Assert.AreEqual(testString, File.ReadAllText(inFile));

            MockSetup mockSetup = (Mock<IHtmlBody> body) =>
            {
                var seq = new MockSequence();
                body.InSequence(seq)
                    .Setup(bb => bb.AddText("Hello,"))
                    .Returns(PdfErrors.Success);

                body.InSequence(seq)
                    .Setup(bb => bb.AddText("world"))
                    .Returns(PdfErrors.Success);

                body.InSequence(seq)
                    .Setup(bb => bb.Render())
                    .Returns(PdfErrors.Success)
                    ;

                body.InSequence(seq)
                    .Setup(bb => bb.RenderedHtml)
                    .Returns("<body><p>Hello, world</p></body>");
            };

            var cts = new CancellationTokenSource();
            var di = new ServiceCollection()
                .AddSingleton<IPdfBuilder, Builder>()
                .AddSingleton<IHtmlBodyFactory>(di => new Factory(mockSetup, MockBehavior.Strict))
                .AddTransient<IPdfBuilderResult, PdfBuilderResult>()
                .AddTransient<IPdfBuilderResults, PdfBuilderResults>()
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions
                {
                    Input = inFile,
                    Output = outFile,
                    Overwrite = false,
                    Cts = cts
                })
                .AddLogging()
                .BuildServiceProvider()
                 ;
            var pdfBuilder = di.GetRequiredService<IPdfBuilder>();

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.Success, pdfBuilder.FatalErrorCode);
            Assert.IsTrue(Helpers.IsValidPdfFile(outFile));
        }

        [TestCase(TestName = "Large and normal commands")]
        public async Task LargeAndSmallText()
        {
            // arrange

            // Ensure that the output file does not exist before
            // we try to create it
            var outFile = FileNames.TestOutput("largeAndNormal");
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            Assert.IsFalse(File.Exists(outFile));

            // Build the input file
            var inFile = FileNames.TestInput("largeAndNormal");
            const string testString = ".large\r\nHello!\r\n.normal\r\nHi, world\r\n";
            File.WriteAllText(inFile, testString);
            Assert.AreEqual(true, File.Exists(inFile));
            Assert.AreEqual(testString, File.ReadAllText(inFile));

            MockSetup mockSetup = (Mock<IHtmlBody> body) =>
            {
                var seq = new MockSequence();

                body.InSequence(seq)
                    .Setup(bb => bb.StartHeading())
                    .Returns(PdfErrors.Success);

                body.InSequence(seq)
                    .Setup(bb => bb.AddText("Hello!"))
                    .Returns(PdfErrors.Success);

                body.InSequence(seq)
                    .Setup(bb => bb.StartBody())
                    .Returns(PdfErrors.Success);

                body.InSequence(seq)
                    .Setup(bb => bb.AddText("Hi, world"))
                    .Returns(PdfErrors.Success);

                body.InSequence(seq)
                    .Setup(bb => bb.Render())
                    .Returns(PdfErrors.Success)
                    ;

                body.InSequence(seq)
                    .Setup(bb => bb.RenderedHtml)
                    .Returns("<body>Hello, world</body>");
            };

            var cts = new CancellationTokenSource();
            var di = new ServiceCollection()
                .AddSingleton<IPdfBuilder, Builder>()
                .AddSingleton<IHtmlBodyFactory>(di => new Factory(mockSetup, MockBehavior.Strict))
                .AddTransient<IPdfBuilderResult, PdfBuilderResult>()
                .AddTransient<IPdfBuilderResults, PdfBuilderResults>()
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions
                {
                    Input = inFile,
                    Output = outFile,
                    Overwrite = false,
                    Cts = cts
                })
                .AddLogging()
                .BuildServiceProvider()
                 ;
            var pdfBuilder = di.GetRequiredService<IPdfBuilder>();

            // act
            await pdfBuilder.StartAsync(cts.Token);

            // analyse
            Assert.AreEqual(PdfErrors.Success, pdfBuilder.FatalErrorCode);
        }
    }
}