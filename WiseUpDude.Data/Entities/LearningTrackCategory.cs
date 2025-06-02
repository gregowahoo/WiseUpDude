using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
{
    public class LearningTrackCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        [MaxLength(50)]
        public string? Difficulty { get; set; }
        [Required]
        public int LearningTrackId { get; set; }
        [ForeignKey("LearningTrackId")]
        public LearningTrack LearningTrack { get; set; }
        public DateTime CreationDate { get; set; }
        public ICollection<LearningTrackSource> Sources { get; set; } = new List<LearningTrackSource>();
    }
}
