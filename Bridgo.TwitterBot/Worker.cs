using System.Text.Json;

namespace Bridgo.TwitterBot;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly TwitterApiClient _twitter;
    private readonly TwitterConfig _config;
    private readonly List<TweetItem> _tweets;
    private readonly Random _rnd = new();

    private BotState _state;

    public Worker(ILogger<Worker> logger, TwitterApiClient twitter, TwitterConfig config)
    {
        _logger = logger;
        _twitter = twitter;
        _config = config;

        var tweetsPath = Path.Combine(AppContext.BaseDirectory, "tweets.json");
        var json = File.ReadAllText(tweetsPath);
        _tweets = JsonSerializer.Deserialize<List<TweetItem>>(json) ?? new();

        _state = BotState.Load();

        _logger.LogInformation("Bot baslatildi. {Count} tweet yuklendi, {Posted} atilmis.",
            _tweets.Count, _state.PostedIds.Count);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Tick();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tick hatasi");
            }

            // Her dakika kontrol et
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task Tick()
    {
        var now = DateTime.UtcNow;
        var trHour = (now.Hour + 3) % 24;
        var trMinute = now.Minute;
        var trTotalMinutes = trHour * 60 + trMinute;
        var today = now.ToString("yyyy-MM-dd");

        // Yeni gun mu? Plan yap.
        if (_state.Today != today)
        {
            _state.Today = today;
            _state.Plan = MakeDailyPlan(trHour, trMinute);

            var planStr = _state.Plan.Select(p => $"{p.Minute / 60:D2}:{p.Minute % 60:D2}").ToList();
            _logger.LogInformation("Yeni gun plani: TR {Plan}", string.Join(", ", planStr));
            _state.Save();
        }

        if (_state.Plan.Count == 0)
            return;

        // Saati gelmis ve atilmamis slot var mi?
        foreach (var slot in _state.Plan)
        {
            if (slot.Done)
                continue;

            if (trTotalMinutes < slot.Minute)
                continue;

            // Siradaki tweet
            var tweet = _tweets.FirstOrDefault(t => !_state.PostedIds.Contains(t.Id));
            if (tweet == null)
            {
                _logger.LogInformation("Tum tweetler atildi ({Count}/{Total})", _state.PostedIds.Count, _tweets.Count);
                return;
            }

            var plannedStr = $"{slot.Minute / 60:D2}:{slot.Minute % 60:D2}";
            _logger.LogInformation("TR {Planned} planliydi, simdi {Hour:D2}:{Min:D2}. Tweet atiliyor #{Id}",
                plannedStr, trHour, trMinute, tweet.Id);

            var tweetId = await _twitter.PostTweetAsync(tweet.Text);
            if (tweetId != null)
            {
                _state.PostedIds.Add(tweet.Id);
                slot.Done = true;
                _state.Save();
                _logger.LogInformation("BASARILI! #{Id}: {Text}... | Twitter ID: {TweetId}",
                    tweet.Id, tweet.Text[..Math.Min(60, tweet.Text.Length)], tweetId);
            }

            return; // Bir tick'te 1 tweet at
        }
    }

    private List<PlannedSlot> MakeDailyPlan(int trHourNow, int trMinuteNow)
    {
        var nowMinutes = trHourNow * 60 + trMinuteNow;
        var startMinutes = Math.Max(_config.BusinessStartHour * 60, nowMinutes + 5);
        var endMinutes = _config.BusinessEndHour * 60;

        if (startMinutes >= endMinutes)
            return new List<PlannedSlot>();

        var gapMinutes = _config.MinGapHours * 60;
        var count = _config.TweetsPerDay;

        // Rastgele zamanlar sec, aralarinda en az gapMinutes olsun
        for (int attempt = 0; attempt < 200; attempt++)
        {
            var picks = Enumerable.Range(0, count)
                .Select(_ => _rnd.Next(startMinutes, endMinutes))
                .OrderBy(x => x)
                .ToList();

            bool valid = true;
            for (int i = 1; i < picks.Count; i++)
            {
                if (picks[i] - picks[i - 1] < gapMinutes)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                return picks.Select(m => new PlannedSlot { Minute = m, Done = false }).ToList();
        }

        // Fallback: esit dagit
        var mid = (startMinutes + endMinutes) / 2;
        return new List<PlannedSlot>
        {
            new() { Minute = _rnd.Next(startMinutes, mid - gapMinutes / 2) },
            new() { Minute = _rnd.Next(mid + gapMinutes / 2, endMinutes) }
        };
    }
}
