using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using DiscordMusicBot.Core.Services;
using System.Threading.Tasks;

namespace DiscordMusicBot.Core.Commands
{
    public class Play : ModuleBase<SocketCommandContext>
    {
        private AudioService audioService;
        private readonly DiscordSocketClient discord;


        public Play(AudioService _audioService, DiscordSocketClient _discord)
        {
            audioService = _audioService;
            discord = _discord;
        }

        [Command("leave"), Summary("Leave Channel")]
        public async Task LeaveCommand()
        {
            await Context.Channel.SendMessageAsync(audioService.StopAsync().Result);
        }


        [Command("play"), Summary("Search for music in YT")]
        public async Task PlayCommand([Remainder] string rq = null)
        {
            if (rq != null)
            {
                ulong guildId = Context.Guild.Id;
                await Context.Channel.SendMessageAsync(audioService.SearchAsync(rq).Result);
            }
        }
        [Command("choose"), Summary("Play the chosen music")]
        public async Task ChooseCommand([Remainder] int rq )
        {

                ulong guildId = Context.Guild.Id;
                await Context.Channel.SendMessageAsync(audioService.PlayAsync(rq, (Context.User as IGuildUser).VoiceChannel, guildId).Result);

        }

        [Command("skip"),Summary("Skip song")]
        public async Task SkipCommand()
        {
            await Context.Channel.SendMessageAsync(audioService.SkipAsync().Result);
        }
    }
}