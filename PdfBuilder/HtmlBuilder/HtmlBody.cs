using PdfBuilder.Abstractions;
using System;
using System.Text;
using System.Web;

namespace PdfBuilder.HtmlBuilder
{
    public class HtmlBody : IHtmlBody
    {
        public IServiceProvider DI { get; set; }

        public HtmlBody()
        {
            // Start with a basic HTML structure that works for IronPdf
            sb_.Append("<body><div style=\"font-size: 16pt;\"<p>");
        }

        public PdfErrors AddText(string text)
        {
            sb_.Append(HttpUtility.HtmlEncode(text));
            return PdfErrors.None;
        }

        public PdfErrors BoldText()
        {
            if (!bold_)
            {
                // Cancel any other font setting (so we don't have to have a stack-based state machine)
                NormalText();

                sb_.Append("<strong>");
                bold_ = true;
            }
            return PdfErrors.None;
        }

        public PdfErrors CancelJustify()
        {
            if (justifying_)
            {
                sb_.Append("/div>");
                justifying_ = false;
            }
            return PdfErrors.None;
        }

        public PdfErrors Indent(int increment)
        {
            if (indentation_ != 0)
            {
                // Cancel the previous padding
                sb_.Append("</div>");
            }

            // And apply the new one, if there is one
            indentation_ += increment;
            if (indentation_ != 0)
            {
                sb_.Append($"<div style=\"left-padding:{25 * indentation_}px;\">");
            }

            return PdfErrors.None;
        }

        public PdfErrors ItalicText()
        {
            if (!italic_)
            {
                // Cancel any other font setting (so we don't have to have a stack-based state machine)
                NormalText();

                sb_.Append("<em>");
                italic_ = true;
            }
            return PdfErrors.None;
        }

        public PdfErrors NewParagraph()
        {
            sb_.Append("</p><p>");
            return PdfErrors.None;
        }

        public PdfErrors NormalText()
        {
            if (italic_)
            {
                if (bold_)
                {
                    return PdfErrors.BoldAndItalic;
                }
                sb_.Append("</em>");
                italic_ = false;
            }
            else if (bold_)
            {
                sb_.Append("</strong>");
                bold_ = false;
            }
            return PdfErrors.None;
        }

        public PdfErrors Render()
        {
            // Terminate the HTML document
            NormalText();
            CancelJustify();
            Indent(-indentation_);  // This will close any div that we created for indentation

            sb_.Append("</p></div?</body>");
            RenderedHtml = sb_.ToString();

            // and get ready for a new one
            sb_ = new StringBuilder(1024);
            return PdfErrors.None;
        }

        public PdfErrors RightJustify()
        {
            // .fill is idempotent
            if (!justifying_)
            {
                sb_.Append("<div style=\"text-align:justify;\">");
                justifying_ = true;
            }
            return PdfErrors.None;
        }

        public string RenderedHtml { get; private set; }

        public PdfErrors StartBody()
        {
            if (heading_)
            {
                sb_.Append("</h1>");
                heading_ = false;
            }
            return PdfErrors.None;
        }

        public PdfErrors StartHeading()
        {
            if (!heading_)
            {
                sb_.Append("<h1>");
                heading_ = true;
            }
            return PdfErrors.None;
        }

        private bool bold_;
        private bool italic_;
        private bool justifying_;
        private bool heading_;
        private int indentation_;
        private StringBuilder sb_ = new StringBuilder(1024);
    }
}
