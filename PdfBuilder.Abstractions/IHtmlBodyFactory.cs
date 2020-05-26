using System;

namespace PdfBuilder.Abstractions
{
    public interface IHtmlBodyFactory
    {
        IHtmlBody Create(IServiceProvider DI);
    }
}
