using Microsoft.Extensions.Logging;
using PdfBuilder.Abstractions;
using System.Text;
using System.Web;

namespace PdfBuilder
{
    /// <summary>
    /// <see cref="IHtmlBody"/>
    /// </summary>
    internal class HtmlBody : IHtmlBody
    {
        /// <summary>
        /// An implementation of <see cref="IHtmlBody"/>.
        /// 
        /// The constructor prepares the HTML with a suitable font-size
        /// </summary>
        /// <param name="logger">(optional) logger for outputting status messages</param>
        public HtmlBody(ILogger<IHtmlBody> logger)
        {
            log_ = logger;

            // Start with a basic HTML structure that works for IronPdf
            sb_.Append(HtmlStrings.BodyBegin);
        }

        /// <summary>
        /// <see cref="IHtmlBody.AddText(string)"/>
        /// </summary>
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

        /// <summary>
        /// <see cref="IHtmlBody.BoldText()"/>
        /// </summary>
        public PdfErrors BoldText()
        {
            if (!bold_)
            {
                // Cancel any other font setting (so we don't have to have a stack-based state machine)
                var errCode = NormalText();
                if (errCode != PdfErrors.Success)
                {
                    return errCode;
                }

                sb_.Append(HtmlStrings.BoldBegin);
                bold_ = true;
            }
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.CancelJustify()"/>
        /// </summary>
        public PdfErrors CancelJustify()
        {
            if (justifying_)
            {
                sb_.Append(HtmlStrings.JustifyEnd);
                justifying_ = false;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.Indent(int)"/>
        /// </summary>
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
                addSpace_ = false;
                sb_.Append(HtmlStrings.Indentation(indentation_));
            }

            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.ItalicText()"/>
        /// </summary>
        public PdfErrors ItalicText()
        {
            if (!italic_)
            {
                // Cancel any other font setting (so we don't have to have a stack-based state machine)
                var errCode = NormalText();
                if (errCode != PdfErrors.Success)
                {
                    return errCode;
                }

                sb_.Append(HtmlStrings.ItalicBegin);
                italic_ = true;
            }
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.NewParagraph()"/>
        /// </summary>
        public PdfErrors NewParagraph()
        {
            sb_.Append(HtmlStrings.NewParagraph);
            addSpace_ = false;
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.NormalText()"/>
        /// </summary>
        public PdfErrors NormalText()
        {
            if (italic_)
            {
                sb_.Append(HtmlStrings.ItalicEnd);
                italic_ = false;
            }
            
            if (bold_)
            {
                sb_.Append(HtmlStrings.BoldEnd);
                bold_ = false;
            }
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.Render()"/>
        /// </summary>
        public PdfErrors Render()
        {
            // Terminate the HTML document
            var errCode = NormalText();
            if (errCode != PdfErrors.Success)
            {
                return errCode;
            }

            errCode = CancelJustify();
            if (errCode != PdfErrors.Success)
            {
                return errCode;
            }
            
            errCode = Indent(-indentation_);  // This will close any div that we created for indentation
            if (errCode != PdfErrors.Success)
            {
                return errCode;
            }

            sb_.Append(HtmlStrings.BodyEnd);
            RenderedHtml = sb_.ToString();
            addSpace_ = false;

            // and get ready for a new one
            sb_ = new StringBuilder(1024);
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.RightJustify()"/>
        /// </summary>
        public PdfErrors RightJustify()
        {
            // .fill is idempotent
            if (!justifying_)
            {
                sb_.Append(HtmlStrings.JustifyBegin);
                justifying_ = true;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.RenderedHtml"/>
        /// </summary>
        public string RenderedHtml { get; private set; }

        /// <summary>
        /// <see cref="IHtmlBody.StartBody()"/>
        /// </summary>
        public PdfErrors StartBody()
        {
            if (heading_)
            {
                // Cancel any emphasis and indent when swapping between .large and .regular
                var errCode = NormalText();
                if (errCode != PdfErrors.Success)
                {
                    return errCode;
                }

                errCode = Indent(-indentation_);
                if (errCode != PdfErrors.Success)
                {
                    return errCode;
                }

                sb_.Append(HtmlStrings.HeadingEnd);
                heading_ = false;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        /// <summary>
        /// <see cref="IHtmlBody.StartHeading()"/>
        /// </summary>
        public PdfErrors StartHeading()
        {
            if (!heading_)
            {
                // Cancel any emphasis and indentation when swapping between .large and .regular
                var errCode = NormalText();
                if (errCode != PdfErrors.Success)
                {
                    return errCode;
                }

                errCode = Indent(-indentation_);
                if (errCode != PdfErrors.Success)
                {
                    return errCode;
                }

                // and start a heading
                sb_.Append(HtmlStrings.HeadingBegin);
                heading_ = true;
                addSpace_ = false;
            }
            return PdfErrors.Success;
        }

        private ILogger<IHtmlBody> log_;
        private bool bold_;
        private bool italic_;
        private bool justifying_;
        private bool heading_;
        private bool addSpace_;
        private int indentation_;
        private StringBuilder sb_ = new StringBuilder(1024);
    }
}