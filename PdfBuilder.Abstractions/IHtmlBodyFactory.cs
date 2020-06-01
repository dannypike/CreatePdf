namespace PdfBuilder.Abstractions
{
    /// <summary>
    /// The factory object is injected into the IPdfBuilder to create instances of IHtmlBody
    /// </summary>
    public interface IHtmlBodyFactory
    {
        /// <summary>
        /// Create an instance of a class that implements IHtmlBody for building the HTML body text
        /// to pass into the PDF generator
        /// </summary>
        /// <returns>An implementation of IHtmlBody</returns>
        IHtmlBody CreateHtmlBody();
    }
}
