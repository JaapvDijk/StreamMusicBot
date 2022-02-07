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

namespace StreamMusicBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Start();

            try
            {
                Log.Information("Starting bot..");

                var client = host.Services.GetRequiredService<StreamMusicBotClient>();
                await client.InitializeAsync();
            }
            catch (Exception e)
            {
                Log.Fatal("Host terminated unexpecedly", e);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static IHost Start()
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
            
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services
                    .AddSingleton<StreamMusicBotClient>()
                    .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                     {
                         AlwaysDownloadUsers = true,
                         MessageCacheSize = 25,
                         LogLevel = LogSeverity.Debug
                     }))
                    .AddSingleton(new CommandService(new CommandServiceConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        CaseSensitiveCommands = false
                    }))
                    .AddSingleton<MusicService>()
                    .AddSingleton<IConfiguration>(Configuration)
                    .AddSingleton<FavoritesService>()
                    .AddLavaNode(x => { x.SelfDeaf = false; x.Port = 2333; x.Hostname = Configuration["lavahostname"]; })
                    .AddSingleton<TrackFactory>()
                    .AddSingleton<SpotifyService>();
                })
                .UseSerilog()
                .Build();
        }
    }
}
