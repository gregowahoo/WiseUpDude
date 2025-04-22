using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    [Table("TopicCreationRuns")]
    public class TopicCreationRun
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Automatically populated with GETUTCDATE()
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreationDate { get; set; }

        // Navigation property for associated Topics
        public ICollection<Topic> Topics { get; set; } = new List<Topic>();

        [MaxLength(50)]
        public string Llm { get; set; } = "DefaultLLM"; // Supply a default value
    }
}
