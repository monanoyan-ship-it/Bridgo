using Bridgo.TwitterBot;

var builder = Host.CreateApplicationBuilder(args);

// Windows Service olarak calisabilsin
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "CorplynkTwitterBot";
});

// Config
var twitterConfig = builder.Configuration.GetSection("Twitter").Get<TwitterConfig>() ?? new TwitterConfig();
builder.Services.AddSingleton(twitterConfig);

// HttpClient
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TwitterApiClient>(sp =>
    new TwitterApiClient(
        sp.GetRequiredService<TwitterConfig>(),
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        sp.GetRequiredService<ILogger<TwitterApiClient>>()
    ));

// Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
