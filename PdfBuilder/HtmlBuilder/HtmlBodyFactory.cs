using PdfBuilder.Abstractions;

namespace PdfBuilder.HtmlBuilder
{
    public class HtmlBodyFactory : IHtmlBodyFactory
    {
        public IHtmlBody Create()
        {
            return new HtmlBody();
        }
    }
}
