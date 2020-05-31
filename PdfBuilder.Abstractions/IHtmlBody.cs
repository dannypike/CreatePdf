namespace PdfBuilder.Abstractions
{
    public interface IHtmlBody
    {
        /// <summary>
        /// Adds an &lt;h1&gt; element to start building a heading (text in a large font).
        /// Has no effect if already building a heading.        ///
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors StartHeading();

        /// <summary>
        /// Fnishes building a heading and reverts to building body text.
        /// Has no effect if not building a heading.
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success</returns>
        PdfErrors StartBody();

        /// <summary>
        ///
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors NewParagraph();

        PdfErrors RightJustify();

        PdfErrors CancelJustify();

        PdfErrors NormalText();

        PdfErrors ItalicText();

        PdfErrors BoldText();

        PdfErrors Indent(int increment);

        PdfErrors AddText(string text);

        /// <summary>
        /// Builds an Html string from the elements that have been added
        /// </summary>
        /// <returns></returns>
        PdfErrors Render();

        /// <summary>
        /// Returns the Html string that was created by <see cref="Render()"/>. Is
        /// null if Render() failed or has not been called.
        /// </summary>
        string RenderedHtml { get; }
    }
}