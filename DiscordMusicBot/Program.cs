using DiscordMusicBot.Core;
using System.Threading.Tasks;

namespace DiscordMusicBot
{
    internal class Program
    {
        public static Task Main(string[] args)
            => Startup.RunAsync(args);
    }
}