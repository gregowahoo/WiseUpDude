using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    [Table("Topics")]
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

        // Foreign key for related Category
        [ForeignKey("Category")]
        public int? CategoryId { get; set; } // Make CategoryId nullable

        // Navigation property for the related Category
        public Category? Category { get; set; } // Make Category optional

        [ForeignKey("TopicCreationRun")]
        public int TopicCreationRunId { get; set; }

        public required TopicCreationRun TopicCreationRun { get; set; }

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public DateTime CreationDate { get; set; }
    }

}
