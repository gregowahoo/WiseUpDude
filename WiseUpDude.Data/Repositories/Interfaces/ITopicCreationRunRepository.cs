using WiseUpDude.Model;

namespace WiseUpDude.Data.Repositories.Interfaces
{
    public interface ITopicCreationRunRepository<T>
    {
        Task AddAsync(TopicCreationRun entity);
        Task AddAsync(TopicCreationRun topicCreationRun, List<Topic> modelTopics);
        Task DeleteAsync(int id);
        Task<IEnumerable<TopicCreationRun>> GetAllAsync();
        Task<TopicCreationRun> GetByIdAsync(int id);
        Task UpdateAsync(TopicCreationRun model);
    }
}