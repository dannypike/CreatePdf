using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System;
using System.Threading.Tasks;

namespace Tests
{
    public class TestHtmlBody
    {
        public delegate void MockSetup(Mock<IHtmlBody> mockBody);

        /// <summary>
        /// An IHtmlBodyFactory implementation that uses a singleton IHtmlBody for us to setup with Moq
        /// </summary>
        class Factory : IHtmlBodyFactory
        {
            public Factory(MockSetup mockSetup)
            {
                mock_ = new Mock<IHtmlBody>();
                mockSetup(mock_);
            }
            public IHtmlBody Create(IServiceProvider DI) => mock_.Object;

            private Mock<IHtmlBody> mock_ { get; set; }
        }

        /// <summary>
        /// A helper for setting up a Mock&lt;IHtmlBody&gt; that will be returned by
        /// the IServiceProvider logic inside PdfBuilder.Builder.
        /// </summary>
        /// <param name="setup">A setup function that is invoked when the IHtmlBodyFactory
        /// constructs the mock IHtmlBody</param>
        /// <returns>Returns the PdfBuilder.Builder to use in the test</returns>
        public static Builder MockBuilder(MockSetup setup)
        {
            // Because we have a new IServiceProvider for each test, we can capture the Factory instance
            // and setup the Mock<IHtmlBody> that the test will use.
            var services = new ServiceCollection();
            services.AddTransient<IHtmlBodyFactory>(ss => new Factory(setup));

            // Construct and return a Builder that will use this injector (as an [out] parameter)
            return new Builder { DI = services.BuildServiceProvider() };
        }

        [TestCase(Helpers.SingleTextPdf, Helpers.SingleTextTxt, TestName = "Single line of text only")]
        public async Task SingleLineTextOnly(string outFile, string inFile)
        {
            Helpers.ArrangeFiles(outFile, inFile, true);
            Helpers.BuildFile(inFile, "Hello, world");

            var pdfBuilder = MockBuilder(mocker =>
            {
                var seq = new MockSequence();

                // Builder.Create() will call AddText() ...
                mocker.InSequence(seq)
                    .Setup(bb => bb.AddText("Hello, world"))
                    .Returns(PdfErrors.Success);

                // .. and then it will call IHtmlBody.Render() ...
                mocker.InSequence(seq)
                    .Setup(bb => bb.Render())
                    .Returns(PdfErrors.Success)
                    ;

                // ... and then it will retrieve the rendered HTML
                mocker.InSequence(seq)
                    .Setup(bb => bb.RenderedHtml)
                    .Returns("<body>Hello, world</body>");
            });

            Assert.AreEqual(PdfErrors.Success, await pdfBuilder.Create(outFile, inFile));
        }

        [TestCase(Helpers.MultipleTextPdf, Helpers.MultipleTextTxt, TestName = "Multiple lines of text")]
        public async Task MultipleLinesTextOnly(string outFile, string inFile)
        {
            Helpers.ArrangeFiles(outFile, inFile, true);
            Helpers.BuildFile(inFile, @"
Hello,
world
");
            var pdfBuilder = MockBuilder(mocker =>
            {
                var seq = new MockSequence();

                mocker.InSequence(seq)
                    .Setup(bb => bb.AddText("Hello,"))
                    .Returns(PdfErrors.Success);

                mocker.InSequence(seq)
                    .Setup(bb => bb.AddText(" "))
                    .Returns(PdfErrors.Success);

                mocker.InSequence(seq)
                    .Setup(bb => bb.AddText("world"))
                    .Returns(PdfErrors.Success);

                mocker.InSequence(seq)
                    .Setup(bb => bb.Render())
                    .Returns(PdfErrors.Success)
                    ;

                mocker.InSequence(seq)
                    .Setup(bb => bb.RenderedHtml)
                    .Returns("<body>Hello, world</body>");
            });

            Assert.AreEqual(PdfErrors.Success, await pdfBuilder.Create(outFile, inFile));
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

            int sequence = 0;
            var pdfBuilder = MockBuilder(mocker =>
            {
                mocker
                    .Setup(bb => bb.StartHeading())
                    .Callback(() => { Assert.AreEqual(1, ++sequence); })
                    .Returns(PdfErrors.Success);

                mocker
                    .Setup(bb => bb.AddText("Hello!"))
                    .Callback(() => { Assert.AreEqual(2, ++sequence); })
                    .Returns(PdfErrors.Success);

                mocker
                    .Setup(bb => bb.StartBody())
                    .Callback(() => { Assert.AreEqual(3, ++sequence); })
                    .Returns(PdfErrors.Success);

                mocker
                    .Setup(bb => bb.AddText("Hi, world"))
                    .Callback(() => { Assert.AreEqual(4, ++sequence); })
                    .Returns(PdfErrors.Success);

                mocker
                    .Setup(bb => bb.Render())
                    .Callback(() => { Assert.AreEqual(5, ++sequence); })
                    .Returns(PdfErrors.Success)
                    ;

                mocker
                    .Setup(bb => bb.RenderedHtml)
                    .Callback(() => { Assert.AreEqual(6, ++sequence); })
                    .Returns("<body>Hello, world</body>");
            });

            var result = await pdfBuilder.Create(outFile, inFile);

            Assert.AreEqual(PdfErrors.Success, result);
            Assert.AreEqual(6, sequence, "Not all methods were invoked");
        }
    }
}
