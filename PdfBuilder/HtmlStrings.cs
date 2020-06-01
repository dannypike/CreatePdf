namespace PdfBuilder
{
    public class HtmlStrings
    {
        public const string BodyBegin = "<body><div style=\"font-size:16pt;\"><p>";
        public const string BodyEnd = "</p></div></body>";
        public const string BoldBegin = "<b>";
        public const string BoldEnd = "</b>";
        public const string HeadingBegin = "<h1>";
        public const string HeadingEnd = "</h1>";
        public static string Indentation(int indentation) =>
            (0 == indentation) ? "</div>" : $"<div style=\"padding-left:{25 * indentation}px;\">";
        public const string ItalicBegin = "<em>";
        public const string ItalicEnd = "</em>";
        public const string JustifyBegin = "<div style=\"text-align:justify;\">";
        public const string JustifyEnd = "</div>";
        public const string NewParagraph = "</p><p>";
    }
}
