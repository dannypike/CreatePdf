namespace PdfBuilder.Abstractions
{
    public interface IHtmlBody
    {
        PdfErrors StartHeading();
        PdfErrors StartBody();
        PdfErrors NewParagraph();
        PdfErrors RightJustify();
        PdfErrors CancelJustify();
        PdfErrors NormalText();
        PdfErrors ItalicText();
        PdfErrors BoldText();
        PdfErrors Indent(int increment);
        PdfErrors AddText(string text);
        PdfErrors Render();

        string RenderedHtml { get; }
    }
}
