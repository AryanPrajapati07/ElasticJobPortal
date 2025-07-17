namespace ElasticJobPortal.Models
{
    public class QuizCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Add this navigation property to fix CS1061
        public ICollection<QuizQuestion> Questions { get; set; }
    }

}
