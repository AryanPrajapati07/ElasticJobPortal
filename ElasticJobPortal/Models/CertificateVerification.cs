namespace ElasticJobPortal.Models
{
    public class CertificateVerification
    {
        public int Id { get; set; }
        public int QuizResultId { get; set; }
        public string Code { get; set; }
        public DateTime IssuedOn { get; set; }
        public QuizResult QuizResult { get; set; }
    }
}
