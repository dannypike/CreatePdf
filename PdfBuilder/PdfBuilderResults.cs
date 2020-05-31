using PdfBuilder.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace PdfBuilder
{
    /// <see cref="IPdfBuilderResults" />
    public class PdfBuilderResults : IPdfBuilderResults
    {
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
                if (fatal)
                {
                    FatalError = result;    // Will have no effect if the FatalError is already set
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

        private object mx_ = new object();
        private IPdfBuilderResult fatal_;
        private IList<IPdfBuilderResult> results_ = new List<IPdfBuilderResult>();
    }
}