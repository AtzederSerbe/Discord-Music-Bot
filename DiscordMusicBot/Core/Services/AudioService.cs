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
            searchedTracks = new List<LavaTrack>();
        }



        public async Task<string> PlayAsync(string query,ulong guildId,IVoiceChannel voiceChannel)
        {
            if (!isSearching)
            {
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
                    tracksToChoose += "`" + i + "`" + "\t" + tracks[i].Title.Replace("`", "\\`") + " by " + tracks[i].Author.Replace("`", "``") + "\n";
                }

                isSearching = true;
                return tracksToChoose;
            }
            else
            {
                if (lavaSocketClient.ServerStats == null)
                {
                    await lavaSocketClient.ConnectAsync(voiceChannel);
                }
                if (player == null)
                {
                    player = lavaSocketClient.GetPlayer(guildId);
                }
                var choice = int.MinValue;
                if (int.TryParse(query, out choice))
                {
                    if (choice >= 0 && choice < 9)
                    {
                        var track = searchedTracks[choice];
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
                else
                {
                    return "Please choose a valid number";
                }
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
            return($"Skipped: {skipped.Title}\nNow Playing: {player.CurrentTrack.Title}");
        }
    }
}
