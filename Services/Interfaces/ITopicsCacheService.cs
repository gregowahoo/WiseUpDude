namespace WiseUpDude.Services.Interfaces
{
    // ITopicsCacheService.cs

    using System.Collections.Generic;
    //using WiseUpDude.Data.Entities;
    using WiseUpDude.Model;

    public interface ITopicsCacheService<T>
    {
        bool HasItems();
        List<Topic> GetItems();
        void SetItems(List<Topic> items);
    }

}
