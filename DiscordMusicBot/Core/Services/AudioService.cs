using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;
using System.Linq;

namespace DiscordMusicBot.Core.Services
{
    public class AudioService
    {
        private LavaPlayer player;
        private LavaSocketClient lavaSocketClient;
        private LavaRestClient lavaRestClient;
        private bool isSearching;
        private List<LavaTrack> searchedTracks;

        public AudioService(LavaSocketClient _lavaSocketClient, LavaRestClient _lavaRestClient)
        {
            lavaSocketClient = _lavaSocketClient;
            lavaRestClient = _lavaRestClient;
            isSearching = false;

        }



        public async Task<string> SearchAsync(string query)
        {
            searchedTracks = new List<LavaTrack>();
            var search = await lavaRestClient.SearchYouTubeAsync(query);
            if (search.LoadType == LoadType.NoMatches ||
                search.LoadType == LoadType.LoadFailed)
            {
                return "Nothing found";

            }

            var tracks = search.Tracks.Take(9).ToList();
            searchedTracks.AddRange(tracks);
            string tracksToChoose = String.Empty;
            for (int i = 0; i < tracks.Count(); ++i)
            {
                tracksToChoose += "`" + i + "`" + "\t" + tracks[i].Title.Replace("`", "\\`") + " by " + tracks[i].Author.Replace("`", "``")+"\t "+tracks[i].Length + "\n";
            }

            isSearching = true;
            return tracksToChoose;
        }

        public async Task<string> PlayAsync(int query, IVoiceChannel voiceChannel, ITextChannel textChannel, ulong guildId)
        {
            await lavaSocketClient.ConnectAsync(voiceChannel,textChannel);
            if (player == null)
            {
                player = lavaSocketClient.GetPlayer(guildId);
            }

            if (query >= 0 && query < 9)
            {
                var track = searchedTracks[query];
                if (player.IsPlaying)
                {
                    player.Queue.Enqueue(track);
                    isSearching = false;
                    return $"{track.Title} has been queued.";
                }
                else
                {
                    await player.PlayAsync(track);
                    isSearching = false;
                    return $"Now Playing: {track.Title}";
                }
            }
            else
            {
                return "Please choose a number between 0 and 8";
            }
        }



        //[Command("Pay", RunMode = RunMode.Async)]
        //public async Task PlayAsync(params string[] queries)
        //{
        //    foreach (var query in queries)
        //    {
        //        await PlayAsync(query);
        //    }
        //}

        public async Task<string> StopAsync()
        {
            await player.StopAsync();
            await lavaSocketClient.DisconnectAsync(player.VoiceChannel);
            return ("Disconnected!");
        }

        public async Task<string> SkipAsync()
        {
            var skipped = await player.SkipAsync();
            return ($"Skipped: {skipped.Title}\nNow Playing: {player.CurrentTrack.Title}");
        }
    }
}

