using PdfBuilder.Abstractions;
using System;

namespace PdfBuilder.HtmlBuilder
{
    public class HtmlBodyFactory : IHtmlBodyFactory
    {
        public IHtmlBody Create(IServiceProvider DI)
        {
            return new HtmlBody
            {
                DI = DI
            };
        }
    }
}
