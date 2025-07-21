using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;



namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles ="JobSeeker")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public QuizController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        public async Task<IActionResult> Index() 
        {
            var categories = await _context.QuizCategories.ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Start(int categoryId)
        {
            var questions = await _context.QuizQuestions
                .Where(q => q.CategoryId == categoryId)
                .Include(q => q.Answers)
                .Take(25) // 5 questions per quiz
                .ToListAsync();

            ViewBag.CategoryId = categoryId;
            ViewBag.TimeLimitInMinutes = 10;
            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int categoryId, Dictionary<int, int> answers)
        {
            int correct = 0;
            foreach (var entry in answers)
            {
                var answer = await _context.QuizAnswers.FindAsync(entry.Value);
                if (answer != null && answer.IsCorrect)
                {
                    correct++;
                }
            }

            var userId = _usermanager.GetUserId(User);
            var result = new QuizResult
            {
                UserId = userId,
                CategoryId = categoryId,
                Score = correct,
                TotalQuestions = answers.Count,
                TakenOn = DateTime.UtcNow
            };

            _context.QuizResults.Add(result);
            await _context.SaveChangesAsync();


            return RedirectToAction("Result", new { id = result.Id});

        }

        public async Task<IActionResult> Result(int id)
        {
            var result = await _context.QuizResults
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (result == null) return NotFound();

            
            

            return View(result);
        }


        public IActionResult DownloadCertificate(string name, string quizTitle, int score, int totalQuestions)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
            var gfx = XGraphics.FromPdfPage(page);
           

            var titleFont = new XFont("Times New Roman", 28, XFontStyle.Bold);
            var headingFont = new XFont("Times New Roman", 20, XFontStyle.Bold);
            var nameFont = new XFont("Times New Roman", 24, XFontStyle.BoldItalic);
            var bodyFont = new XFont("Times New Roman", 16, XFontStyle.Regular);
            var footerFont = new XFont("Times New Roman", 14, XFontStyle.Italic);

            double width = page.Width;
            double height = page.Height;

            gfx.DrawRectangle(XPens.Black, 30, 30, width - 60, height - 60);
            gfx.DrawString("Certificate of Achievement", titleFont, XBrushes.DarkBlue,
                new XRect(0, 100, width, 40), XStringFormats.TopCenter);
            gfx.DrawString("This certificate is proudly presented to", headingFont, XBrushes.Black,
                new XRect(0, 160, width, 30), XStringFormats.TopCenter);
            gfx.DrawString(name, nameFont, XBrushes.DarkRed,
                new XRect(0, 200, width, 30), XStringFormats.TopCenter);
            gfx.DrawString($"For successfully completing the quiz:", bodyFont, XBrushes.Black,
                new XRect(0, 250, width, 25), XStringFormats.TopCenter);
            gfx.DrawString($"{quizTitle}", new XFont("Times New Roman", 18, XFontStyle.Bold), XBrushes.DarkGreen,
                new XRect(0, 275, width, 30), XStringFormats.TopCenter);


            gfx.DrawString($"Result : ", bodyFont, XBrushes.Black,
               new XRect(-20, 310, width, 30), XStringFormats.TopCenter);

            gfx.DrawString($" Passed", bodyFont, XBrushes.Green,
                new XRect(25, 310, width, 30), XStringFormats.TopCenter);

            gfx.DrawString($"Date: {DateTime.Now:dd MMM yyyy}", bodyFont, XBrushes.Black,
                new XRect(0, 390, width, 30), XStringFormats.TopCenter);

            // Signature Line
            gfx.DrawLine(XPens.Black, width / 2 - 120, height - 150, width / 2 + 120, height - 150);
            gfx.DrawString("Authorized by: ElasticJobPortal", footerFont, XBrushes.Gray,
                new XRect(0, height - 140, width, 20), XStringFormats.TopCenter);

            using (var stream = new MemoryStream())
            {
                document.Save(stream, false);
                stream.Position = 0;

                return File(stream.ToArray(), "application/pdf", "Certificate.pdf");
            }
        }


    }
}

