using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using StreamMusicBot.Services;

namespace StreamMusicBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var botClient = new StreamMusicBotClient();
            await botClient.InitializeAsync();

            //var builder = WebApplication.CreateBuilder(args);
            //var app = builder.Build();

            //app.MapGet("/ping", (x) => Task.Run(() => "pong"));

            //app.Run();
        }
    }
}
