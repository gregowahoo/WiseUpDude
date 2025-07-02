using WiseUpDude.Services.Interfaces;

namespace WiseUpDude.Services
{
    public class UrlSuggestionCacheService : IUrlSuggestionCacheService
    {
        private List<string> _cachedUrls = new List<string>();
        private DateTime? _cacheTime;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public bool HasUrls()
        {
            if (IsExpired())
            {
                _cachedUrls.Clear();
                return false;
            }
            return _cachedUrls.Any();
        }

        public List<string> GetUrls()
        {
            if (IsExpired())
            {
                _cachedUrls.Clear();
            }
            return _cachedUrls;
        }

        public void SetUrls(List<string> urls)
        {
            _cachedUrls = urls ?? new List<string>();
            _cacheTime = DateTime.Now;
        }

        private bool IsExpired()
        {
            if (!_cacheTime.HasValue)
                return false;

            return (DateTime.Now - _cacheTime.Value) > _cacheDuration;
        }
    }
}
