using Discord.Commands;
using Serilog;
using System.Threading.Tasks;

namespace StreamMusicBot.Modules
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("Ping")]
        public async Task Pong()
        {
            Log.Information("PING PING PING");
            await ReplyAsync("PONG!");
        }
    }
}
