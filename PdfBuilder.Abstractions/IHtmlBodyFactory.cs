using System;
using System.Collections.Generic;
using System.Text;

namespace PdfBuilder.Abstractions
{
    public interface IHtmlBodyFactory
    {
        IHtmlBody CreateHtmlBody();
    }
}
