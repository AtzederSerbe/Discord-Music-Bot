using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordMusicBot.Core.Commands
{
    public class HelloWorld : ModuleBase<SocketCommandContext>
    {
        [Command("hello"), Alias("helloworld", "world"), Summary("Hello World Command")]
        public async Task HelloWorldCommand()
        {
            await Context.Channel.SendMessageAsync("Hello " + Context.User.Username);
        }

        [Command("embed"), Summary("embed test command")]
        public async Task EmbedCommand([Remainder]string input = null)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor("TestAuthor",Context.User.GetAvatarUrl());
            embed.WithColor(Color.Green);
            embed.WithDescription("This is a test description");
            if (input != null)
            {
                embed.AddField("User Input:", input);
            }
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}