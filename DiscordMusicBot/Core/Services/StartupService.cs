using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;

namespace DiscordMusicBot.Core
{
    public class StartupService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly LavaSocketClient _lavaLink;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            LavaSocketClient lavaLink)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
            _lavaLink = lavaLink;

            StartLavalinkServer();
            discord.Ready += Discord_Ready;

        }

        private async Task Discord_Ready()
        {
            _lavaLink.Log += _lavaLink_Log;
            _lavaLink.OnPlayerUpdated += OnPlayerUpdated;
            _lavaLink.OnServerStats += OnServerStats;
            _lavaLink.OnSocketClosed += OnSocketClosed;
            _lavaLink.OnTrackException += OnTrackException;
            _lavaLink.OnTrackFinished += OnTrackFinished;
            _lavaLink.OnTrackStuck += OnTrackStuck;

            await _lavaLink.StartAsync(_discord, new Configuration
            {
                LogSeverity = LogSeverity.Debug,
                ReconnectAttempts = 3,
                Port = 2333
            });

        }

        private Task _lavaLink_Log(LogMessage arg)
        {
            Console.WriteLine($"**lavalink: {arg}");
            return Task.CompletedTask;
        }
        private void StartLavalinkServer()
        {
            Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = false,
                Arguments = @"-jar Lavalink.jar",
                FileName = "java",
                WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), @"Ressources\"),
                UseShellExecute = true
            });

        }
        public async Task StartAsync()
        {
            string discordToken = _config["tokens:discord"];     // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `_configuration.json` file found in the applications root directory.");

            await _discord.LoginAsync(TokenType.Bot, discordToken);     // Login to discord
            await _discord.StartAsync();                                // Connect to the websocket

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);     // Load commands and modules into the command service
        }

        private Task OnPlayerUpdated(LavaPlayer player, LavaTrack track, TimeSpan position)
        {
            Console.WriteLine($"Victoria:\t Player Updated For {player.VoiceChannel.GuildId}: {position}");
            return Task.CompletedTask;
        }

        private Task OnServerStats(ServerStats stats)
        {
            Console.WriteLine($"Victoria: \t Uptime: {stats.Uptime}");
            return Task.CompletedTask;
        }

        private Task OnSocketClosed(int code, string reason, bool remote)
        {
            Console.WriteLine($"Victoria: \t LavaSocket closed: {code} | {reason} | {remote}");
            return Task.CompletedTask;
        }

        private Task OnTrackException(LavaPlayer player, LavaTrack track, string error)
        {
            Console.WriteLine($"Victoria: \t Player {player.VoiceChannel.GuildId} {error} for {track.Title}");
            return Task.CompletedTask;
        }

        private async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (!reason.ShouldPlayNext())
                return;

            if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
            {
                await player.TextChannel?.SendMessageAsync($"There are no more items left in queue.");
                return;
            }

            await player.PlayAsync(nextTrack);
        }

        private Task OnTrackStuck(LavaPlayer player, LavaTrack track, long threshold)
        {
            Console.WriteLine($"Victoria: \t {track.Title} stuck after {threshold}ms for {player.VoiceChannel.GuildId}.");
            return Task.CompletedTask;
        }
    }
}