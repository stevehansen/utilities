using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;

namespace WordValidator
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var path = args.Length > 0 ? args[0] : null;
            //var ofd = new OpenFileDialog();
            ValidateWordDocument(path);
        }

        public static void ValidateWordDocument(string filepath)
        {
            using var wordprocessingDocument = WordprocessingDocument.Open(filepath, true);

            try
            {
                var validator = new OpenXmlValidator();
                var count = 0;
                foreach (var error in validator.Validate(wordprocessingDocument))
                {
                    count++;
                    Console.WriteLine("Error " + count);
                    Console.WriteLine("Description: " + error.Description);
                    Console.WriteLine("ErrorType: " + error.ErrorType);
                    Console.WriteLine("Node: " + error.Node);
                    Console.WriteLine("Path: " + error.Path.XPath);
                    Console.WriteLine("Part: " + error.Part.Uri);
                    Console.WriteLine("-------------------------------------------");
                }

                Console.WriteLine("count={0}", count);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            wordprocessingDocument.Close();
        }

    }
}
