using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace WiseUpDude.Data.Entities
{
    public class AssignmentType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SpecialQuizAssignment
    {
        public int Id { get; set; }
        public int UserQuizId { get; set; }
        public string AssignedByUserId { get; set; }
        public int AssignmentTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
