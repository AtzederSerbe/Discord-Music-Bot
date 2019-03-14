using Discord.Audio;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordMusicBot.Core.Services
{
    public class TransmitAudioService
    {
        public async Task TransmitAudio(IAudioClient audioClient, string audioFilePath)
        {
            await SendAsync(audioClient, audioFilePath);
        }

        private Process CreateStream(string audioFilePath)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = @".\Ressources\ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel debug -i \"{audioFilePath}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        //private async Task SendAsync1(IAudioClient client, string audioFilePath)
        //{
        //    // Create FFmpeg using the previous example
        //    using (var ffmpeg = CreateStream(audioFilePath))
        //    using (var output = ffmpeg.StandardOutput.BaseStream)
        //    using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
        //    {
        //        try { await output.CopyToAsync(discord); }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message + "\t at " + e.Source);
        //        }
        //        finally { await discord.FlushAsync(); }

        //    }
        //}

        //private async Task SendAsync2(IAudioClient client, string audioFilePath)
        //{
        //    using (var audioFile = new MediaFoundationReader(audioFilePath))
        //    using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
        //    {
        //        try {
        //                await audioFile.CopyToAsync(discord); }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message + "\t at " + e.Source);
        //        }
        //        finally { await discord.FlushAsync(); }
        //    }
        //}

        private async Task SendAsync(IAudioClient client, string audioFilePath)
        {
            Mp3FileReader.FrameDecompressorBuilder builder = new Mp3FileReader.FrameDecompressorBuilder(wf => new AcmMp3FrameDecompressor(wf));
            var reader = new Mp3FileReader(audioFilePath, builder);
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                await reader.CopyToAsync(discord);
            }
        }
        //private async Task SendAsync3(IAudioClient client, string audioFilePath)
        //{
        //    var libDirectory =
        //        new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
        //    using (var mediaPlayer = new Vlc.DotNet.Core.VlcMediaPlayer(libDirectory))
        //    {
        //        mediaPlayer.SetMedia(new FileInfo(audioFilePath));
        //        mediaPlayer.
        //    }
        //}
    }
}