using PdfBuilder.Abstractions;

namespace PdfBuilder
{
    public class Builder : IBuilder
    {
        /// <see cref="IBuilder"/>
        public bool Create(string outputFile, string inputFile)
        {
            return false;
        }
    }
}
