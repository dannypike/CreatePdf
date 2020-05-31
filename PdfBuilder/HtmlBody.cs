using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;
using System.Text;
using System.Web;

namespace PdfBuilder
{
    public class HtmlBody : IHtmlBody
    {
        public ILogger<IHtmlBody> log_;

        public HtmlBody(ILogger<IHtmlBody> logger)
        {
            log_ = logger;

            // Start with a basic HTML structure that works for IronPdf
            sb_.Append("<body><div style=\"font-size:16pt;\"><p>");
        }

        public PdfErrors AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // add a space delimiter if required and the next character is not punctuation
                if (addSpace_ && !char.IsPunctuation(text[0]))
                {
                    sb_.Append(" ");
                }
                sb_.Append(HttpUtility.HtmlEncode(text));
                addSpace_ = !text.EndsWith(" ");
            }
            return PdfErrors.Success;
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
            return PdfErrors.Success;
        }

        public PdfErrors CancelJustify()
        {
            if (justifying_)
            {
                sb_.Append("</div>");
                justifying_ = false;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        public PdfErrors Indent(int increment)
        {
            if (indentation_ != 0)
            {
                // Cancel the previous padding
                sb_.Append("</div>");
                addSpace_ = false;
            }

            // And apply the new one, if there is one
            indentation_ += increment;
            if (indentation_ != 0)
            {
                sb_.Append($"<div style=\"padding-left:{25 * indentation_}px;\">");
                addSpace_ = false;
            }

            return PdfErrors.Success;
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
            return PdfErrors.Success;
        }

        public PdfErrors NewParagraph()
        {
            sb_.Append("</p><p>");
            addSpace_ = false;
            return PdfErrors.Success;
        }

        public PdfErrors NormalText()
        {
            if (italic_)
            {
                sb_.Append("</em>");
                italic_ = false;
            }
            if (bold_)
            {
                sb_.Append("</strong>");
                bold_ = false;
            }
            return PdfErrors.Success;
        }

        public PdfErrors Render()
        {
            // Terminate the HTML document
            NormalText();
            CancelJustify();
            Indent(-indentation_);  // This will close any div that we created for indentation

            sb_.Append("</p></div></body>");
            RenderedHtml = sb_.ToString();
            addSpace_ = false;

            // and get ready for a new one
            sb_ = new StringBuilder(1024);
            return PdfErrors.Success;
        }

        public PdfErrors RightJustify()
        {
            // .fill is idempotent
            if (!justifying_)
            {
                sb_.Append("<div style=\"text-align:justify;\">");
                justifying_ = true;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        public string RenderedHtml { get; private set; }

        public PdfErrors StartBody()
        {
            if (heading_)
            {
                // Cancel any emphasis and indent when swapping between .large and .regular
                NormalText();
                Indent(-indentation_);

                sb_.Append("</h1>");
                heading_ = false;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        public PdfErrors StartHeading()
        {
            if (!heading_)
            {
                // Cancel any emphasis and indentation when swapping between .large and .regular
                NormalText();
                Indent(-indentation_);

                // and start a heading
                sb_.Append("<h1>");
                heading_ = true;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        private bool bold_;
        private bool italic_;
        private bool justifying_;
        private bool heading_;
        private bool addSpace_;
        private int indentation_;
        private StringBuilder sb_ = new StringBuilder(1024);
    }
}