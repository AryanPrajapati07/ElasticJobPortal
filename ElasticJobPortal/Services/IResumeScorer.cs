using System.Threading.Tasks;
namespace ElasticJobPortal.Services
{
    public interface IResumeScorer
    {
        Task<double> ScoreResumeAsync(int resumeId, int jobId);
        Task<double> ScoreTextAsync(string jobDescription, string resumeText);
    }
}
