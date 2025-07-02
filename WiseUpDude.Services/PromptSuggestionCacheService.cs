
namespace WiseUpDude.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WiseUpDude.Services.Interfaces;

    public class PromptSuggestionCacheService : IPromptSuggestionCacheService
    {
        private List<string> _cachedPrompts = new List<string>();
        private DateTime? _cacheTime;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public bool HasPrompts()
        {
            if (IsExpired())
            {
                _cachedPrompts.Clear();
                return false;
            }
            return _cachedPrompts.Any();
        }

        public List<string> GetPrompts()
        {
            if (IsExpired())
            {
                _cachedPrompts.Clear();
            }
            return _cachedPrompts;
        }

        public void SetPrompts(List<string> prompts)
        {
            _cachedPrompts = prompts ?? new List<string>();
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
