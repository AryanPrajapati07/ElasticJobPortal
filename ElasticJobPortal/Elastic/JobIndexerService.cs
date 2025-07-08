using ElasticJobPortal.Models;
using Nest;
using System.Collections.Generic;
using Job = ElasticJobPortal.Models.Job;

namespace ElasticJobPortal.Elastic
{
    public class JobIndexerService
    {
        private readonly ElasticClient _client;

        public JobIndexerService()
        {
            _client = ElasticClientProvider.GetClient();
        }

        public void IndexSampleJobs()
        {
            var jobs = new List<Job>
            {
                new Job
                {
                    Id = 1,
                    Title = "Software Engineer",
                    Company = "TechCorp",
                    Description = "ASP.NET MVC Developer with C# and SQL skills"
                },
                new Job
                {
                    Id = 2,
                    Title = "Frontend Developer",
                    Company = "Designify",
                    Description = "HTML, CSS, React, and Tailwind knowledge"
                },
                new Job
                {
                    Id = 3,
                    Title = "Data Analyst",
                    Company = "DataHub",
                    Description = "Looking for someone with Excel, SQL, Power BI"
                }
            };

            foreach (var job in jobs)
            {
                _client.IndexDocument(job);
            }
        }
    }
}
