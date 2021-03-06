﻿namespace PdfBuilder.Abstractions
{
    /// <summary>
    /// This Model holds the HTML objects that are used to build the
    /// HTML string to pass to IronPDF.
    /// </summary>
    public interface IHtmlBody
    {
        /// <summary>
        /// Returns the Html string that was created by <see cref="Render()"/>. Is
        /// null if Render() failed or has not been called.
        /// </summary>
        string RenderedHtml { get; }

        /// <summary>
        /// Add some more text to the current paragraph.
        /// </summary>
        /// <param name="text">Text to be added. Any leading or trailing whitespace is ignored.</param>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors AddText(string text);

        /// <summary>
        /// The following text is to be displayed in <b>bold</b>.
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors BoldText();

        /// <summary>
        /// Cancel right-side justification.
        /// 
        /// <seealso cref="RightJustify"/>
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>.
        PdfErrors CancelJustify();

        /// <summary>
        /// Left-indent the following paragraphs.
        /// </summary>
        /// <param name="increment">The amount of indentation to add.</param>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors Indent(int increment);

        /// <summary>
        /// The following text is to be displayed in <em>italics</em>.
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>.
        PdfErrors ItalicText();

        /// <summary>
        /// Insert a blank space to start a new paragraph
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors NewParagraph();

        /// <summary>
        /// Cancel <see cref="ItalicText"/> and <see cref="BoldText"/>.
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>.
        PdfErrors NormalText();

        /// <summary>
        /// Builds an Html string from the elements that have been added
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors Render();

        /// <summary>
        /// Justify the text on the right-hand-side of the page
        /// </summary>
        /// <seealso cref="CancelJustify"/>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors RightJustify();

        /// <summary>
        /// Fnishes building a heading and reverts to building body text.
        /// Has no effect if not building a heading.
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success</returns>
        PdfErrors StartBody();

        /// <summary>
        /// Adds an &lt;h1&gt; element to start building a heading (text in a large font).
        /// Has no effect if already building a heading.        ///
        /// </summary>
        /// <returns>this version always returns PdfErrors.Success/></returns>
        PdfErrors StartHeading();
    }
}
