using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [TestCase(Helpers.SingleTextPdf, Helpers.SingleTextTxt, TestName = "Single line of text only")]
        public async Task SingleLineTextOnly(string outFile, string inFile)
        {
            Helpers.ArrangeFiles(outFile, inFile, true);
            Helpers.BuildFile(inFile, "Hello, world");

            // arrange
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
                    .Returns("<body><p>Hello, world</p></body>");
            };

            var cts = new CancellationTokenSource();
            var di = new ServiceCollection()
                .AddSingleton<IPdfBuilder, Builder>()
                .AddSingleton<IHtmlBodyFactory>(di => new Factory(mockSetup, MockBehavior.Strict))
                .AddTransient<IPdfBuilderResult, PdfBuilderResult>()
                .AddTransient<IPdfBuilderResults, PdfBuilderResults>()
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions(inFile, outFile)
                {
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

        [TestCase(Helpers.MultipleTextPdf, Helpers.MultipleTextTxt, TestName = "Multiple lines of text")]
        public async Task MultipleLinesTextOnly(string outFile, string inFile)
        {
            Helpers.ArrangeFiles(outFile, inFile, true);
            Helpers.BuildFile(inFile, @"
Hello,
world
");

            // arrange
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
                    .Returns("<body>Hello, world</body>");
            };

            var cts = new CancellationTokenSource();
            var di = new ServiceCollection()
                .AddSingleton<IPdfBuilder, Builder>()
                .AddSingleton<IHtmlBodyFactory>(di => new Factory(mockSetup, MockBehavior.Strict))
                .AddTransient<IPdfBuilderResult, PdfBuilderResult>()
                .AddTransient<IPdfBuilderResults, PdfBuilderResults>()
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions(inFile, outFile)
                {
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

        [TestCase(Helpers.LargeAndNormalPdf, Helpers.LargeAndNormalTxt, TestName = "Large and normal commands")]
        public async Task LargeAndSmallText(string outFile, string inFile)
        {
            Helpers.ArrangeFiles(outFile, inFile, true);
            Helpers.BuildFile(inFile, @"
.large
Hello!
.normal
Hi, world
");

            // arrange
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
                .AddSingleton<IPdfBuilderOptions>(_ => new PdfBuilderOptions(inFile, outFile)
                {
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