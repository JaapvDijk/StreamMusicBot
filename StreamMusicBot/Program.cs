using System.Threading.Tasks;

namespace StreamMusicBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var builder = new ConfigurationBuilder()
            //    .AddJsonFile($"appsettings.json", true, true)
            //    .AddEnvironmentVariables();

            //var config = builder.Build();
            await new StreamMusicBotClient().InitializeAsync();
        }
    }
}
