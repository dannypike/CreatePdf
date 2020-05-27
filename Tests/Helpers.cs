using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Delete the outFile, if it exists and check that it was deleted
        /// Optionally check that the inFile exists
        /// </summary>
        /// <param name="outFile">Delete this file, if it exists</param>
        /// <param name="inFile">(optional) Check to see that this file exists</param>
        public static void ArrangeFiles(string outFile, string inFile = null)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            Assert.AreEqual(true, inFile != null ? File.Exists(inFile) : true);
            Assert.AreEqual(false, File.Exists(outFile));
        }
    }
}
