namespace ElasticJobPortal.Models
{
    public class QuizAnswer
    {
        public int Id { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; }
    }

}
