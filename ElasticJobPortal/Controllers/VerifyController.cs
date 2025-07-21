using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElasticJobPortal.Models;
using ElasticJobPortal.Services;

public class VerifyController : Controller
{
    private readonly ApplicationDbContext _context;

    public VerifyController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/verify/{code}")]
    public async Task<IActionResult> Certificate(string code)
    {
        var cert = await _context.CertificateVerifications
            .Include(c => c.QuizResult)
            .ThenInclude(q => q.Category)
            .FirstOrDefaultAsync(c => c.Code == code);

        if (cert == null)
        {
            return View("NotFound");
        }

        return View(cert);
    }
}
