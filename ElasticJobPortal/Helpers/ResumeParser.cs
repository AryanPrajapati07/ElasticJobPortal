using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace ElasticJobPortal.Helpers
{
    public class ResumeParser
    {
        public static List<string> ExtractKeywordsFromPdf(string resumePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resumePath.TrimStart('/'));
            Console.WriteLine("Resume Full Path: " + fullPath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("File NOT found.");
                return new List<string>();
            }

            var text = ExtractTextFromPdf(fullPath);
            Console.WriteLine("Extracted Text Length: " + text.Length);

            var keywords = ExtractKeywordsFromPdf(text);
            Console.WriteLine("Extracted Keywords Count: " + keywords.Count);

            return keywords;
        }
        private static string ExtractTextFromPdf(string pdfPath)
        {
            using (var document = PdfDocument.Open(pdfPath))
            {
                var text = new System.Text.StringBuilder();
                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }
                return text.ToString();
            }
        }

    }
}
