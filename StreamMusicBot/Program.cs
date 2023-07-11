using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using StreamMusicBot.Services;
using Serilog;
using Serilog.Formatting.Json;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Victoria;
using StreamMusicBot.Extensions;
using Hangfire;
using Microsoft.Extensions.Options;
using Hangfire.LiteDB;
using System.Configuration;
using Microsoft.AspNetCore.Http;

namespace StreamMusicBot
{
    public class Program
    {
        public static async Task Main()
        {
            var app = BuildApp();
            
            try
            {
                Log.Information("Starting bot..");

                var spotifyClient = app.Services.GetRequiredService<SpotifyService>();
                Extensions.Extensions.Initialize(spotifyClient);

                Log.Information("initialized with spotify..");

                var botClient = app.Services.GetRequiredService<StreamMusicBotClient>();
                await botClient.InitializeAsync(); //Bot stats here

                Log.Information("Started bot succesfully..");

                app.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                app.Run();
            }
            catch (Exception e)
            {
                Log.Fatal($"Host terminated unexpectedly. \n {e}", e);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static WebApplication BuildApp()
        {
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

            var builder = WebApplication.CreateBuilder();
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
            //builder.Services.AddHangfire(config => config.UseLiteDbStorage("./hangfire.db"));
            //builder.Services.AddHangfireServer();
            builder.Host.UseSerilog();

            var app = builder.Build();
            //app.UseHangfireDashboard();

            return app;
        }
    }
}
