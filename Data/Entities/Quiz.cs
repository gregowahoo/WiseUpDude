using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WiseUpDude.Data.Entities
  {
      public class Quiz
      {
          [Key]
          public int Id { get; set; }
          [Required]
          [StringLength(100)]
          public string Name { get; set; }
          public List<QuizQuestion> Questions { get; set; }
          public string UserId { get; set; }
          [ForeignKey("UserId")]
          public ApplicationUser User { get; set; }
      }
  }
  