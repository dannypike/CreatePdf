using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests
{
    public static class Helpers
    {
        public const string Sample1Txt = @"..\..\..\..\samples\sample1.txt";
        public const string Sample1Pdf = @"..\..\..\..\Tests\outputs\sample1.pdf";
        public const string Sample8Txt = @"..\..\..\..\samples\sample8.txt";
        public const string Sample8Pdf = @"..\..\..\..\Tests\outputs\sample8.pdf";
        public const string AlreadyExistsInput = @"..\..\..\..\samples\outputAlreadyExists.txt";
        public const string AlreadyExistsOutput = @"..\..\..\..\outputs\outputAlreadyExists.pdf";
        public const string ThreePagesTxt = @"..\..\..\..\samples\threePages.txt";
        public const string ExistingPdf = @"..\..\..\..\samples\existing.pdf";
        public const string SampleThreePagesPdf = @"..\..\..\..\Tests\outputs\sampleThreePages.pdf";
        public const string SingleTextPdf = @"..\..\..\..\Tests\outputs\singleText.pdf";
        public const string SingleTextTxt = @"..\..\..\..\Tests\inputs\singleText.txt";
        public const string MultipleTextPdf = @"..\..\..\..\Tests\outputs\multipleText.pdf";
        public const string MultipleTextTxt = @"..\..\..\..\Tests\inputs\multipleText.txt";
        public const string LargeAndNormalPdf = @"..\..\..\..\Tests\outputs\largeAndNormalText.pdf";
        public const string LargeAndNormalTxt = @"..\..\..\..\Tests\inputs\largeAndNormalText.txt";

        /// <summary>
        /// Delete the outFile, if it exists and check that it was deleted
        /// Optionally check that the inFile exists
        /// </summary>
        /// <param name="outFile">Delete this file, if it exists</param>
        /// <param name="inFile">(optional) Check to see that this file exists</param>
        /// <param name="deleteInFile">(optional) Also delete the input file, if
        /// it exists; inFile must be non-null if deleteInFile is true</param>
        public static void ArrangeFiles(string outFile, string inFile = null, bool deleteInFile = false)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            if (deleteInFile)
            {
                if (File.Exists(inFile))
                {
                    File.Delete(inFile);
                }
                Assert.AreEqual(false, File.Exists(inFile));
            }
            else
            {
                Assert.AreEqual(true, inFile != null ? File.Exists(inFile) : true);
            }
            Assert.AreEqual(false, File.Exists(outFile));
        }

        /// <summary>
        /// <see cref="BuildInFile(string, IEnumerable{string})"/>
        /// </summary>
        public static void BuildFile(string inFile, params string[] lines)
        {
            BuildInFile(inFile, lines.ToList());
        }

        /// <summary>
        /// Build a file from a list of strings, one line-per-string
        /// </summary>
        /// <param name="inFile">The name of the input file. Directory will be created
        /// if it does not already exist</param>
        /// <param name="lines">The lines to add to the file</param>
        public static void BuildInFile(string inFile, IEnumerable<string> lines)
        {
            inFile = Path.GetFullPath(inFile);
            string directory = Path.GetDirectoryName(inFile);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Assert.AreEqual(true, Directory.Exists(directory),
                    $"failed to create directory '{directory}'");
            }

            // Make sure that we are not using an old file
            if (File.Exists(inFile))
            {
                File.Delete(inFile);
            }
            Assert.AreEqual(false, File.Exists(inFile));

            var sb = new StringBuilder();
            sb.AppendJoin("\n", lines);
            if (sb.Length > 0)
            {
                // The final line will not have had a separator ("\n") joined to the end
                sb.Append('\n');
            }
            File.WriteAllText(inFile, sb.ToString());

            // It should have thrown an exception if we failed to create the file but it doesn't hurt to verify that
            Assert.AreEqual(true, File.Exists(inFile), $"failed to build input file {inFile}");
        }
    }
}
