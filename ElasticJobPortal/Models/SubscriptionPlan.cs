namespace ElasticJobPortal.Models
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }  // in INR
        public string Duration { get; set; } // Monthly / Lifetime
        public string Features { get; set; } // Comma-separated or multiline
    }

}
