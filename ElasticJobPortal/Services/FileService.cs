namespace ElasticJobPortal.Services
{
    public class FileService
    {
        public async Task<string> SaveResumeAsync(IFormFile resumeFile)
        {
            if (resumeFile == null || resumeFile.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(resumeFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await resumeFile.CopyToAsync(fileStream);
            }

            return $"/resumes/{uniqueFileName}";

        }
    }
}
