using ElasticJobPortal.Models;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace ElasticJobPortal.Services
{
    public class ResumeScorer : IResumeScorer
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
           "a","an","the","and","or","but","if","in","on","at","to","from","by","for","with","of",
            "is","are","was","were","be","been","this","that","these","those","as","it","its","they",
            "them","he","she","we","you","i","me","my","mine","your","yours"
        };

        public ResumeScorer(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<double> ScoreResumeAsync(int resumeId, int jobId)
        {
            var resume = await _context.Resumes.FindAsync(resumeId);
            string filePathFromDb = resume.FileName;


            var job = await _context.Set<Job>().FindAsync(jobId);
            //var resume = await _context.Set<Resume>().FindAsync(resumeId);

            if (job == null || resume == null)
                return 0.0;

            string jobText = job.Description ?? string.Empty;
            string resumeText = string.Empty;

            if (!string.IsNullOrEmpty(resume.FileName))
            {
                var combined = resume.FileName.TrimStart('~', '/', '\\');
                var full = Path.Combine(_webHostEnvironment.WebRootPath, combined.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(full)) resumeText = ExtractTextFromPdf(full);

            }

            if (string.IsNullOrWhiteSpace(resumeText) && resume.FileName != null && resume.FileName.Length > 0)
            {
                resumeText = ExtractTextFromPdfFromBytes(resume.FileName);
            }

            if (string.IsNullOrWhiteSpace(resumeText) && !string.IsNullOrEmpty(resume.ContentText))
            {
                resumeText = resume.ContentText;
            }

            double scoreNormalized = await ScoreTextAsync(jobText, resumeText);
            double percentage = Math.Round(scoreNormalized * 100.0, 2);

            var r = new ResumeScore
            {
                ResumeId = resumeId,
                JobId = jobId,
                Score = (int)percentage
            };
            _context.ResumeScores.Add(r);
            await _context.SaveChangesAsync();

            return percentage;

        }


        public Task<double> ScoreTextAsync(string jobDescription, string resumeText)
        {
            var jobTokens = TokenizeAndFilter(jobDescription);
            var resumeTokens = TokenizeAndFilter(resumeText);

            if (!jobTokens.Any() || !resumeTokens.Any())
                return Task.FromResult(0.0);

            var tfJob = TermFrequency(jobTokens);
            var tfResume = TermFrequency(resumeTokens);

            var allTerms = tfJob.Keys.Union(tfResume.Keys).ToList();

            var tfidfJob = ComputeTfidf(tfJob, tfResume, allTerms);
            var tfidfResume = ComputeTfidf(tfResume, tfJob, allTerms);

            double dot = 0, normJob = 0, normResume = 0;
            foreach (var term in allTerms)
            {
                tfidfJob.TryGetValue(term, out var v1);
                tfidfResume.TryGetValue(term, out var v2);
                dot += v1 * v2;
                normJob += v1 * v1;
                normResume += v2 * v2;
            }

            double similarity = 0.0;
            if (normJob > 0 && normResume > 0)
            {
                similarity = dot / (Math.Sqrt(normJob) * Math.Sqrt(normResume));
            }

            double boost = ComputeSkillBoost(jobDescription, resumeTokens);
            double final = Math.Min(1.0, similarity + boost);

            return Task.FromResult(final);


        }


        //start region methods
        private string ExtractTextFromPdf(string fullPath)
        {
            var sb = new StringBuilder();
            using (var doc = PdfDocument.Open(fullPath))
            {
                foreach (var page in doc.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }
            return sb.ToString();
        }

        private static string ExtractTextFromPdfFromBytes(string fileName)
        {
            
            var sb = new StringBuilder();
            using (var doc = PdfDocument.Open(fileName))
            {
                foreach (var page in doc.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }
            return sb.ToString();
        }

        private static List<string> TokenizeAndFilter(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();
            // to lower
            text = text.ToLowerInvariant();
            // remove common punctuation preserved as spaces
            var tokens = Regex.Split(text, "\\W+")
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Where(t => t.Length > 1) // drop single letters
                .Where(t => !StopWords.Contains(t))
                .Where(t => !Regex.IsMatch(t, "^\\d+$")) // remove pure numbers
                .ToList();
            return tokens;
        }

        private static Dictionary<string, double> TermFrequency(List<string> tokens)
        {
            var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in tokens)
            {
                if (!dict.ContainsKey(t)) dict[t] = 0;
                dict[t] += 1;
            }
            // normalize by token count
            var total = tokens.Count;
            if (total > 0)
            {
                var keys = dict.Keys.ToList();
                foreach (var k in keys) dict[k] = dict[k] / total;
            }
            return dict;
        }

        private static Dictionary<string, double> ComputeTfidf(Dictionary<string, double> tfThis, Dictionary<string, double> tfOther, List<string> allTerms)
        {
            // N = 2 (job doc + resume doc)
            const double N = 2.0;
            var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var term in allTerms)
            {
                int df = 0;
                if (tfThis.ContainsKey(term) && tfThis[term] > 0) df++;
                if (tfOther.ContainsKey(term) && tfOther[term] > 0) df++;
                double idf = Math.Log((1.0 + N) / (1.0 + df)) + 1.0; // smooth idf
                tfThis.TryGetValue(term, out double tfValue);
                result[term] = tfValue * idf;
            }
            return result;
        }

        private static double ComputeSkillBoost(string jobDescription, List<string> resumeTokens)
        {
            if (string.IsNullOrWhiteSpace(jobDescription)) return 0.0;

            // crude extraction: find lines mentioning "skill" or "requirement" and collect words after the colon
            var skillWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var lines = jobDescription.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var low = line.ToLowerInvariant();
                if (low.Contains("skill") || low.Contains("require") || low.Contains("tech"))
                {
                    var parts = Regex.Split(line, ":|\\-|\")");
                    foreach (var p in parts.Skip(1))
                    {
                        var tokens = Regex.Split(p, "\\W+")
                            .Where(t => t.Length > 1 && !StopWords.Contains(t));
                        foreach (var t in tokens) skillWords.Add(t.Trim());
                    }
                }
            }

            if (!skillWords.Any()) return 0.0;

            var resumeSet = new HashSet<string>(resumeTokens, StringComparer.OrdinalIgnoreCase);
            int matches = skillWords.Count(s => resumeSet.Contains(s));
            double matchRatio = (double)matches / (double)skillWords.Count;
            // max boost 0.20 (20%), linear with matchRatio
            return Math.Min(0.20, matchRatio * 0.20);
        }
        //end region methods



    }
}
