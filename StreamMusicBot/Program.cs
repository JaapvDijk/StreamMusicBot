using Microsoft.AspNetCore.Builder;
using StreamMusicBot.Services;
using StreamMusicBot.MyExtensions;
using Serilog;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Victoria;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Http;
using StreamMusicBot;

#region building
IConfiguration Configuration = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
  //.AddUserSecrets<StreamMusicBotClient>(optional: true)
  .AddEnvironmentVariables()
  .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

#region services
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(Configuration);
builder.Services.AddSingleton<StreamMusicBotClient>();
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    AlwaysDownloadUsers = true,
    MessageCacheSize = 25,
    LogLevel = LogSeverity.Debug,
    //GatewayIntents = GatewayIntents.DirectMessages (requires verification when bot reaches 100+ servers)
}));
builder.Services.AddSingleton(new CommandService(new CommandServiceConfig
{
    LogLevel = LogSeverity.Verbose,
    CaseSensitiveCommands = false
}));
builder.Services.AddSingleton<MusicService>();
builder.Services.AddSingleton<FavoritesService>();
builder.Services.AddSingleton<TrackFactory>();
builder.Services.AddSingleton<SpotifyService>();
builder.Services.AddLavaNode(x =>
{
    x.SelfDeaf = false;
    x.Port = Convert.ToUInt16(Configuration["lavaport"]);
    x.Hostname = Configuration["lavahostname"];
    x.Authorization = Configuration["lavapass"];
});
builder.Services.AddHangfire(config => config.UseLiteDbStorage("./hangfire.db"));
builder.Services.AddHangfireServer();
builder.Host.UseSerilog();
#endregion

var app = builder.Build();
app.UseHangfireDashboard();

var spotifyClient = app.Services.GetRequiredService<SpotifyService>();
var botClient = app.Services.GetRequiredService<StreamMusicBotClient>();

Extensions.Initialize(spotifyClient);

#endregion

Log.Information("Starting bot..");
try
{
    app.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
    app.Run();
    //await botClient.InitializeAsync(); //Bot stats here

}
catch (Exception e)
{
    Log.Fatal($"Host terminated unexpectedly. \n {e}", e);
}
finally
{
    Log.CloseAndFlush();
}
        
