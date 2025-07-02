
using System.Collections.Generic;

namespace WiseUpDude.Services.Interfaces
{
    public interface IPromptSuggestionCacheService
    {
        bool HasPrompts();
        List<string> GetPrompts();
        void SetPrompts(List<string> prompts);
    }
}
