using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CreatePdf
{
    public class CreatePdfService : IHostedService
    {
        public CreatePdfService(ILogger<CreatePdfService> logger, IPdfBuilderOptions options)
        {
            logger_ = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private ILogger<CreatePdfService> logger_;
    }
}