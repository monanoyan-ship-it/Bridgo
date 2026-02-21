using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bridgo.TwitterBot;

/// <summary>
/// tweets.json'daki her bir tweet
/// </summary>
public class TweetItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
}

/// <summary>
/// Gunluk plan - hangi dakikada tweet atilacak
/// </summary>
public class PlannedSlot
{
    /// <summary>Gunun basindan itibaren dakika (orn: 637 = 10:37 TR)</summary>
    public int Minute { get; set; }

    public bool Done { get; set; }
}

/// <summary>
/// State - disk'te tutulur, yari kalmissa devam eder
/// </summary>
public class BotState
{
    public List<int> PostedIds { get; set; } = new();
    public string? Today { get; set; }
    public List<PlannedSlot> Plan { get; set; } = new();

    private static readonly string StatePath = Path.Combine(AppContext.BaseDirectory, "state.json");

    public static BotState Load()
    {
        if (!File.Exists(StatePath))
            return new BotState();

        var json = File.ReadAllText(StatePath);
        return JsonSerializer.Deserialize<BotState>(json) ?? new BotState();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(StatePath, json);
    }
}
