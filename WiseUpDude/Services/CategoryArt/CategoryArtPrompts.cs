using System.Collections.Generic;

namespace WiseUpDude.Services.CategoryArt;

public static class CategoryArtPrompts
{
    public static readonly IReadOnlyDictionary<string, string> Map =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Featured"]             = "spotlight and star highlight",
        ["Seniors Need To Know"] = "active senior silhouette with wellness leaf",
        ["Fun Facts"]            = "lightbulb exploding confetti playful",
        ["Wow"]                  = "burst of awe with sparkling rays",
        ["Brain Boosters"]       = "brain with puzzle pieces logic memory",
        ["History Mysteries"]    = "aged scroll magnifying glass ancient artifact",
        ["Tech Trends"]          = "ai chip over subtle circuit",
        ["Pop Culture Picks"]    = "film slate with music note",
        ["Travel Treasures"]     = "globe with landmark pin",
        ["Health Wellness"]      = "heart with heartbeat line and small dumbbell",
        ["Financial Smarts"]     = "stacked coins with upward arrow",
        ["Science Wonders"]      = "atom and lab flask with small star",
        ["Literary Legends"]     = "open book with quill",
        ["Everyday Hacks"]       = "wrench with lightbulb",
        ["Local Legends"]        = "city skyline with location pin"
    };

    public static string BuildPrompt(string label)
    {
        var motifs = Map.TryGetValue(label, out var m) ? m : "simple abstract symbol";
        return $"Minimal, modern icon for {label}: {motifs}. Flat 2.5D, soft gradient, smooth vector edges, centered, transparent background PNG, no text, clean lighting, high detail.";
    }
}
