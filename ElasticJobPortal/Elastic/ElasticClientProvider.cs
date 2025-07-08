using Nest;
using System;

namespace ElasticJobPortal.Elastic
{
    public static class ElasticClientProvider
    {
        public static ElasticClient GetClient()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("jobs"); // default index name

            return new ElasticClient(settings);
        }
    }
}
