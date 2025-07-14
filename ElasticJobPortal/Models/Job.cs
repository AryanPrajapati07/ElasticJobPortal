
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasticJobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Company is required")]
        public string Company { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "JobType is required")]
        public string JobType { get; set; }

      

        // Save to DB as CSV string
        public string SkillsCsv { get; set; }

        [NotMapped]
        public List<string> Skills
        {
            get => SkillsCsv?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();
            set => SkillsCsv = string.Join(",", value);
        }
    }

}
