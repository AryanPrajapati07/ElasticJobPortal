using Nest;
using ElasticJobPortal.Models;
using System;

namespace ElasticJobPortal.Elastic
{
    public static class ElasticClientProvider
    {
        public static ElasticClient GetClient()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
     .DefaultIndex("jobs");

            var client = new ElasticClient(settings);

            // Create index with proper mapping
            if (!client.Indices.Exists("jobs").Exists)
            {
                client.Indices.Create("jobs", c => c
                    .Map<Models.Job>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Completion(c => c
                                .Name(n => n.TitleSuggest)
                            )
                        )
                    )
                );
            }


            return new ElasticClient(settings);
        }
    }
}
