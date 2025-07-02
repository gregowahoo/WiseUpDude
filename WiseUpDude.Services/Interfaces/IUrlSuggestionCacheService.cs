using System.Collections.Generic;

namespace WiseUpDude.Services.Interfaces
{
    public interface IUrlSuggestionCacheService
    {
        bool HasUrls();
        List<string> GetUrls();
        void SetUrls(List<string> urls);
    }
}
