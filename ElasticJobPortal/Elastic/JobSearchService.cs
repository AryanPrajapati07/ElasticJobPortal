using ElasticJobPortal.Models;
using Nest;
using System.Collections.Generic;
using System.Linq;

namespace ElasticJobPortal.Elastic
{
    public class JobSearchService
    {
        private readonly ElasticClient _client;

        public JobSearchService()
        {
            _client = ElasticClientProvider.GetClient();
        }

        public List<Models.Job> SearchJobs(string keyword)
        {
            var response = _client.Search<Models.Job>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(j => j.Title)
                            .Field(j => j.Description)
                            .Field(j => j.Company)
                        )
                        .Query(keyword)
                    )
                )
            );

            return response.Documents.ToList();
        }
    }
}
