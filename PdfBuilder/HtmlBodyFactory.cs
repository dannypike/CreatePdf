using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;

namespace PdfBuilder
{
    public class HtmlBodyFactory : IHtmlBodyFactory
    {
        public HtmlBodyFactory(ILogger<IHtmlBody> logger)
        {
            logger_ = logger;
        }

        public IHtmlBody CreateHtmlBody() => new HtmlBody(logger_);

        private ILogger<IHtmlBody> logger_;
    }
}
