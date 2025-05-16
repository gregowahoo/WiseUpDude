namespace WiseUpDude.Services
{
    // MyCacheService.cs with timeout

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WiseUpDude.Services.Interfaces;
    using WiseUpDude.Model;

    public class TopicsCacheService<T> : ITopicsCacheService<T>
    {
        // Simple in-memory storage
        private List<Topic> _cachedItems = new List<Topic>();

        // When we stored the cache
        private DateTime? _cacheTime;

        // How long the cache is good for
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public bool HasItems()
        {
            // If it's expired, clear it out so we don't serve stale data
            if (IsExpired())
            {
                _cachedItems.Clear();
                return false;
            }

            return _cachedItems.Any();
        }

        public List<Topic> GetItems()
        {
            // If they're expired, clear them out
            if (IsExpired())
            {
                _cachedItems.Clear();
            }

            // Return a copy if you want to avoid external modifications
            return _cachedItems;
        }

        public void SetItems(List<Topic> items)
        {
            _cachedItems = items ?? new List<Topic>();
            _cacheTime = DateTime.Now; // record when we set these items
        }

        // Helper to see if our cache is expired
        private bool IsExpired()
        {
            // If no time is set, it's not expired
            if (!_cacheTime.HasValue)
                return false;

            // If we've passed the duration, it is expired
            return (DateTime.Now - _cacheTime.Value) > _cacheDuration;
        }
    }
}
