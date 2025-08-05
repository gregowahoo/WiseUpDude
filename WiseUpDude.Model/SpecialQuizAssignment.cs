using System;

namespace WiseUpDude.Model
{
    public class SpecialQuizAssignment
    {
        public int Id { get; set; }
        public int UserQuizId { get; set; } // FK to UserQuiz
        public string AssignedByUserId { get; set; } // FK to User
        public int AssignmentTypeId { get; set; } // FK to AssignmentType
        public DateTime StartDate { get; set; } // When to start featuring
        public DateTime EndDate { get; set; } // When to stop featuring
        public string Notes { get; set; } // Optional admin notes
        public DateTime CreatedAt { get; set; } // For auditing
    }
}
