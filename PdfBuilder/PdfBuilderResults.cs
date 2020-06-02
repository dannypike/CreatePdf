using PdfBuilder.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfBuilder
{
    /// <see cref="IPdfBuilderResults" />
    internal class PdfBuilderResults : IPdfBuilderResults
    {
        /// <see cref="IPdfBuilderResults.FatalError" />
        public IPdfBuilderResult FatalError
        {
            get
            {
                lock (mx_)
                {
                    return fatal_;
                }
            }

            set
            {
                lock (mx_)
                {
                    if (fatal_ == null)
                    {
                        fatal_ = value;
                    }
                }
            }
        }

        /// <see cref="IPdfBuilderResults.Snapshot" />
        public IEnumerable<IPdfBuilderResult> Snapshot
        {
            get
            {
                lock (mx_)
                {
                    return results_.Select(src => new PdfBuilderResult
                    {
                        ErrorCode = src.ErrorCode,
                        FatalError = src.FatalError,
                        Message = src.Message
                    });
                }
            }
        }

        /// <see cref="IPdfBuilderResults.AddError" />
        public void AddError(bool fatal, PdfErrors errCode, string message = null)
        {
            var result = new PdfBuilderResult
            {
                ErrorCode = errCode,
                FatalError = fatal,
                Message = message
            };
            lock (mx_)
            {
                results_.Add(result);
                if (fatal && FatalError == null)
                {
                    FatalError = result;
                    handler_?.Invoke(errCode);
                }
            }
        }

        /// <see cref="IPdfBuilderResults.Clear" />
        public void Clear()
        {
            lock (mx_)
            {
                results_.Clear();
                fatal_ = null;
            }
        }

        /// <summary>
        /// <see cref="IPdfBuilderResults.RegisterFatalErrorCodeHandler(Action{PdfErrors})"/>
        /// </summary>
        public IPdfBuilderResults RegisterFatalErrorCodeHandler(Action<PdfErrors> handler)
        {
            handler_ = handler;
            return this;
        }

        private IPdfBuilderResult fatal_;
        private Action<PdfErrors> handler_;
        private object mx_ = new object();
        private IList<IPdfBuilderResult> results_ = new List<IPdfBuilderResult>();
    }
}