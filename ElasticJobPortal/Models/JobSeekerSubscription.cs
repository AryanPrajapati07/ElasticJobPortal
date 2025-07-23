namespace ElasticJobPortal.Models
{
    public class JobSeekerSubscription
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int PlanId { get; set; }

        public SubscriptionPlan Plan { get; set; }

        public DateTime SubscribedOn { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

}
