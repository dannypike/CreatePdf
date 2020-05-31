using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;

namespace PdfBuilder
{
    public class HtmlBodyFactory : IHtmlBodyFactory
    {
        private ILogger<IHtmlBody> logger_;

        public HtmlBodyFactory(ILogger<IHtmlBody> logger)
        {
            logger_ = logger;
        }

        public IHtmlBody CreateHtmlBody() => new HtmlBody(logger_);
    }
}
