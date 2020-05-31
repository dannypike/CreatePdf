using System.IO;

namespace Tests
{
    public static class FileNames
    {
        /// <summary>
        /// Construct filenames for input and output samples and tests
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static string SampleInput(string fileName, string ext = "txt")
            => Path.Combine(@"..\..\..\..\samples", Path.ChangeExtension(fileName, ext));
        public static string TestInput(string fileName, string ext = "txt")
            => Path.Combine(@"..\..\..\..\Tests\inputs", Path.ChangeExtension(fileName, ext));
        public static string SampleOutput(string fileName, string ext = "pdf")
            => Path.Combine(@"..\..\..\..\outputs", Path.ChangeExtension(fileName, ext));
        public static string TestOutput(string fileName, string ext = "pdf")
            => Path.Combine(@"..\..\..\..\Tests\outputs", Path.ChangeExtension(fileName, ext));

        //public const string Sample1Txt = "sample1.txt";
        //public const string Sample1Pdf = "sample1.pdf";
        //public const string Sample8Txt = "sample8.txt";
        //public const string Sample8Pdf = "sample8.pdf";
        public const string AlreadyExists = "outputAlreadyExists";
        //public const string AlreadyExistsOutput = "outputAlreadyExists.pdf";
        //public const string ThreePagesTxt = "threePages.txt";
        //public const string ExistingPdf = "existing.pdf";
        //public const string SampleThreePagesPdf = "sampleThreePages.pdf";
        //public const string SingleTextPdf = "singleText.pdf";
        //public const string SingleTextTxt = @"..\..\..\..\Tests\inputs\singleText.txt";
        //public const string MultipleTextPdf = "multipleText.pdf";
        //public const string MultipleTextTxt = @"..\..\..\..\Tests\inputs\multipleText.txt";
        //public const string LargeAndNormalPdf = "largeAndNormalText.pdf";
        //public const string LargeAndNormalTxt = @"..\..\..\..\Tests\inputs\largeAndNormalText.txt";
        //public const string OverwriteExisting = @"..\..\..\..\outputs\overwriteExisting.pdf";
    }
}
