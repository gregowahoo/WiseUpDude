namespace WiseUpDude.Model
{
    public class TopicCreationRun
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public List<Topic>? Topics { get; set; } // Use the model version of Topic
        public string Llm { get; set; } = string.Empty; // Default value
    }
}
