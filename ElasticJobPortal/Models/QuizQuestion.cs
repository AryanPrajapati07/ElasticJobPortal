namespace ElasticJobPortal.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public int CategoryId { get; set; }
        public QuizCategory Category { get; set; }

        public List<QuizAnswer> Answers { get; set; }
    }

}
