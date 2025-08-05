using System;

namespace WiseUpDude.Model
{
    public class AssignmentType
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g. "Featured", "SeniorsNeedToKnow", etc.
        public string Description { get; set; } // Optional
    }
}
