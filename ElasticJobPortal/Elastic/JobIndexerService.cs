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
        Description = "ASP.NET MVC Developer with C# and SQL skills",
        JobType = "Full-time",
            Skills = new List<string> { "C#", "ASP.NET", "SQL" },
        TitleSuggest = new CompletionField { Input = new[] { "Software Engineer - TechCorp", "Developer - TechCorp" } }
    },
    new Job
    {
        Id = 2,
        Title = "Software Engineer",
        Company = "TechCorp",
        Description = "Backend Developer using .NET Core, Azure",
        JobType = "Full-time",
            Skills = new List<string> { ".NET Core", "Azure", "C#" },
        TitleSuggest = new CompletionField { Input = new[] { "Software Engineer - TechCorp", "Backend Developer - TechCorp" } }
    },
    new Job
    {
        Id = 3,
        Title = "Frontend Developer",
        Company = "Designify",
        Description = "React.js developer with good design sense",
        JobType = "Full-time",
            Skills = new List<string> { "React.js", "JavaScript", "CSS" },
        TitleSuggest = new CompletionField { Input = new[] { "Frontend Developer - Designify", "React Developer - Designify" } }
    },
    new Job
    {
        Id = 4,
        Title = "Frontend Developer",
        Company = "Designify",
        Description = "UI Developer with Figma and Bootstrap",
        JobType = "Full-time",
            Skills = new List<string> { "Figma", "Bootstrap", "HTML", "CSS" },
        TitleSuggest = new CompletionField { Input = new[] { "Frontend Developer - Designify", "UI Developer - Designify" } }
    },
    new Job
    {
        Id = 5,
        Title = "Data Analyst",
        Company = "DataHub",
        Description = "Power BI, SQL, Data Visualization Expert",
        JobType = "Part-Time",
            Skills = new List<string> { "Power BI", "SQL", "Data Visualization" },
        TitleSuggest = new CompletionField { Input = new[] { "Data Analyst - DataHub", "BI Analyst - DataHub" } }
    },
    new Job
    {
        Id = 6,
        Title = "Software Engineer",
        Company = "CodeWorks",
        Description = "Full stack developer (ASP.NET + Angular)",
        JobType = "Part-Time",
            Skills = new List<string> { "ASP.NET", "Angular", "C#", "SQL" },
        TitleSuggest = new CompletionField { Input = new[] { "Software Engineer - CodeWorks", "Full Stack Developer - CodeWorks" } }
    },
    new Job
    {
        Id = 7,
        Title = "QA Tester",
        Company = "TechCorp",
        Description = "Automation QA with Selenium and API testing",
        JobType = "Full-time",
            Skills = new List<string> { "Selenium", "API Testing", "Automation" },
        TitleSuggest = new CompletionField { Input = new[] { "QA Tester - TechCorp", "Automation Tester - TechCorp" } }
    },
    new Job
    {
        Id = 8,
        Title = "QA Tester",
        Company = "Testify",
        Description = "Manual tester for mobile and web apps",
        JobType = "Full-time",
            Skills = new List<string> { "Manual Testing", "Mobile Testing", "Web Testing" },
        TitleSuggest = new CompletionField { Input = new[] { "QA Tester - Testify", "Manual Tester - Testify" } }
    },
    new Job
    {
        Id = 9,
        Title = "Project Manager",
        Company = "AgileForce",
        Description = "Agile PM with SCRUM, Jira, team leadership",
        JobType = "Full-time",
            Skills = new List<string> { "SCRUM", "Jira", "Agile" },

        TitleSuggest = new CompletionField { Input = new[] { "Project Manager - AgileForce", "SCRUM Master - AgileForce" } }
    },
    new Job
    {
        Id = 10,
        Title = "Project Manager",
        Company = "TechCorp",
        Description = "Client communication, delivery planning",
        JobType = "Full-time",
            Skills = new List<string> { "Client Communication", "Delivery Planning", "Team Management" },
        TitleSuggest = new CompletionField { Input = new[] { "Project Manager - TechCorp", "Delivery Manager - TechCorp" } }
    },
    new Job
    {
        Id = 11,
        Title = "Data Analyst",
        Company = "DataHub",
        Description = "ETL, MySQL, and Tableau required",
        JobType = "Full-time",
            Skills = new List<string> { "ETL", "MySQL", "Tableau" },
        TitleSuggest = new CompletionField { Input = new[] { "Data Analyst - DataHub", "Tableau Analyst - DataHub" } }
    },
    new Job
    {
        Id = 12,
        Title = "DevOps Engineer",
        Company = "CloudGenius",
        Description = "CI/CD pipelines, Docker, Kubernetes",
        JobType = "Full-time",
            Skills = new List<string> { "CI/CD", "Docker", "Kubernetes", "Azure" },
        TitleSuggest = new CompletionField { Input = new[] { "DevOps Engineer - CloudGenius", "Cloud Engineer - CloudGenius" } }
    },
    new Job
    {
        Id = 13,
        Title = "UI/UX Designer",
        Company = "Designify",
        Description = "Figma, XD, creative thinker required",
        JobType = "Full-time",
            Skills = new List<string> { "Figma", "Adobe XD", "UI Design", "UX Design" },
        TitleSuggest = new CompletionField { Input = new[] { "UI/UX Designer - Designify", "UX Designer - Designify" } }
    },
    new Job
    {
        Id = 14,
        Title = "Content Writer",
        Company = "Wordify",
        Description = "SEO blog writer for tech articles",
        JobType = "Part-time",
            Skills = new List<string> { "SEO", "Content Writing", "Blogging" },
        TitleSuggest = new CompletionField { Input = new[] { "Content Writer - Wordify", "SEO Writer - Wordify" } }
    },
    new Job
    {
        Id = 15,
        Title = "Software Engineer",
        Company = "Designify",
        Description = "Entry-level role for frontend with JavaScript",
        JobType = "Full-time",
            Skills = new List<string> { "JavaScript", "HTML", "CSS" },
        TitleSuggest = new CompletionField { Input = new[] { "Software Engineer - Designify", "Frontend Dev - Designify" } }
    }
};


            foreach (var job in jobs)
            {
                _client.IndexDocument(job);
            }
        }
    }
}
