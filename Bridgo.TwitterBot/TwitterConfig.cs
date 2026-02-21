namespace Bridgo.TwitterBot;

public class TwitterConfig
{
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string AccessTokenSecret { get; set; } = "";

    /// <summary>TR saati ile is baslangici (default 10:00)</summary>
    public int BusinessStartHour { get; set; } = 10;

    /// <summary>TR saati ile is bitisi (default 20:00)</summary>
    public int BusinessEndHour { get; set; } = 20;

    /// <summary>Gunde kac tweet (default 2)</summary>
    public int TweetsPerDay { get; set; } = 2;

    /// <summary>Iki tweet arasi minimum saat (default 3)</summary>
    public int MinGapHours { get; set; } = 3;
}
