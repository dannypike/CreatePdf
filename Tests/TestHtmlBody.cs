using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PdfBuilder;
using PdfBuilder.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class TestHtmlBody
    {
        class Factory : IHtmlBodyFactory
        {
            public Mock<IHtmlBody> Mocker { get; set; }

            public IHtmlBody Create(IServiceProvider DI)
            {
                Mocker = new Mock<IHtmlBody>();
                return Mocker.Object;
            }
        }

        [SetUp]
        public void Setup()
        {
            // Set up a DI provider that allows us to mock the IHtmlBody
            var services = new ServiceCollection()
                .AddLogging(builder => builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace)
                    );
            services.Add(new ServiceDescriptor(typeof(IHtmlBodyFactory), (services) =>
            {
                // Export the IHtmlBodyFactory to the Test class, so that we can mock the IHtmlBody
                return factory_ = new Factory();
            }, ServiceLifetime.Singleton));

            // And construct a Builder from it
            pdfBuilder_ = new Builder();
            pdfBuilder_.DI = services.BuildServiceProvider();
        }

        [TestCase(Helpers.Sample1Pdf, Helpers.Sample1Txt, TestName = "Verify that the DI creates a Mockable IHtmlBody")]
        public async Task CreatesMockableHtmlBody(string outFile, string inFile)
        {
            Helpers.ArrangeFiles(outFile, inFile);

            Assert.IsNotNull(pdfBuilder_);

            // PdfBuilder.Create() will throw an exception, because the Mock<> version has no logic
            // We're not interested in that exception ...
            try
            {
                await pdfBuilder_.Create(outFile, inFile);
            }
            catch { }

            // ... but we do want to verify that a Mocker was cerated, i.e. the Create() call used our DI system
            Assert.IsNotNull(factory_.Mocker, "DI failed to mock the IHtmlBodyFactory");
        }

        private Factory factory_;
        private Builder pdfBuilder_;
    }
}
