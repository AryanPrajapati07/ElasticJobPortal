using System;
using System.ComponentModel.DataAnnotations;

namespace ElasticJobPortal.Models
{
    public class SavedJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int JobId { get; set; }

        public DateTime SavedOn { get; set; } = DateTime.UtcNow;

        // Navigation (Optional)
        public ApplicationUser User { get; set; }
        public Job Job { get; set; }
    }
}
