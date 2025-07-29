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

            if (!File.Exists(fullPath))
                return new List<string>();

            var keywords = new List<string>();



            using (var document = PdfDocument.Open(fullPath))
            {
                foreach (var page in document.GetPages())
                {
                    var text = page.Text;

                    // Simple regex for words
                    var matches = Regex.Matches(text, @"\b[A-Za-z]{3,}\b");
                    foreach (Match match in matches)
                    {
                        keywords.Add(match.Value.ToLower());
                    }
                }
            }

            // return top 20 frequent keywords (optional)
            return keywords
                .GroupBy(k => k)
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Select(g => g.Key)
                .ToList();
        }

    }
}
