namespace ElasticJobPortal.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CategoryId { get; set; }
        public QuizCategory Category { get; set; }

        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime TakenOn { get; set; }
    }

}
