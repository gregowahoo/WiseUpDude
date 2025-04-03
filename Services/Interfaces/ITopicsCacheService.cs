namespace WiseUpDude.Services.Interfaces
{
    // ITopicsCacheService.cs

    using System.Collections.Generic;
    using WiseUpDude.Model;

    public interface ITopicsCacheService<T>
    {
        bool HasItems();
        List<TopicItem> GetItems();
        void SetItems(List<TopicItem> items);
    }

}
