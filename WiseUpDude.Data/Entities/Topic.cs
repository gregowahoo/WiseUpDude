using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    [Table("Topics")] // Optional: Specify the table name
    public class Topic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // New foreign key to associate with a TopicCreationRun
        [ForeignKey("TopicCreationRun")]
        public int TopicCreationRunId { get; set; }

        // Navigation property for the related TopicCreationRun
        public required TopicCreationRun TopicCreationRun { get; set; }
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreationDate { get; set; }
    }
}
