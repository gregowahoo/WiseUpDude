using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    [Table("Topics")] // Optional: Specify the table name
    public class Topic
    {
        [Key] // Marks this property as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-incrementing ID
        public int Id { get; set; }

        [Required] // Makes this property non-nullable
        [MaxLength(100)] // Optional: Limit the length of the Name column
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)] // Optional: Limit the length of the Description column
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)] // Optional: Limit the length of the Llm column
        public string Llm { get; set; } = string.Empty;
    }
}
