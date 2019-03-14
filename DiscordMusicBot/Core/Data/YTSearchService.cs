using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VideoLibrary;

namespace DiscordMusicBot.Core.Data
{
    public class YTSearchService
    {
        public List<string> queue = new List<string>();
        public List<List<string>> searchResultList = new List<List<string>>();
        private readonly IConfigurationRoot config;
        private bool isSearchingMusic = false;
        private string historyFolderPath = Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.LastIndexOf('\\')) + @"\history";

        private const string baseUrl = "https://www.youtube.com/watch?v=";
        private const string baseFolder = @"E:\DiscordMusicBotTemp";
        public Dictionary<string, string> history = new Dictionary<string, string>();

        public YTSearchService(IConfigurationRoot _config)
        {
            config = _config;
            CreateFolder(historyFolderPath);
            var historyPath = historyFolderPath + @"\history.txt";
            if (File.Exists(historyPath))
            {
                fillHistory(historyPath);
            }
            else
            {
                File.Create(historyPath);
            }
        }

        private void fillHistory(string historyPath)
        {
            string[] hContent = File.ReadAllLines(historyPath);
            foreach (string s in hContent)
            {
                var temp = s.Split(';');
                history.Add(temp[0], temp[1]);
            }
        }

        private void writeHistoryToFile()
        {
            var historyPath = historyFolderPath + @"\history.txt";
            using (StreamWriter sw = new StreamWriter(historyPath, false))
                foreach (string key in history.Keys)
                {
                    sw.WriteLine(key + ";" + history[key]);
                }
        }

        public async Task<string> searchForMusic(string rq)
        {
            isSearchingMusic = true;
            //Initiate YT Service
            var ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = config["tokens:youtube"],
                ApplicationName = this.GetType().ToString()
            });

            //Set Search Parameters
            var searchRequest = ytService.Search.List("snippet");
            searchRequest.Q = rq;
            searchRequest.MaxResults = 10;

            //Initiate YT Search and parse result
            var searchResponse = await searchRequest.ExecuteAsync();
            searchResultList = new List<List<string>>();
            int id = 0;
            foreach (var item in searchResponse.Items)
            {
                searchResultList.Add(new List<string>());
                var snippet = item.Snippet;
                string title = snippet.Title;
                string author = snippet.ChannelTitle;
                string url = baseUrl + item.Id.VideoId;
                searchResultList[id].AddRange(new string[] { title, author, url });
                id++;
            }

            string resultString = "";
            for (int i = 0; i < searchResultList.Count; ++i)
            {
                resultString += "`" + i + "`" + "\t" + searchResultList[i][0].Replace("`", "\\`") + " by " + searchResultList[i][1].Replace("`", "``") + "\n";
            }
            return resultString;
        }

        public virtual string YoutubeToMp3(int id)
        {
            var url = searchResultList[id][2];
            //var uri = Url(url).ToString();

            //if (url.Replace(BaseUrl, "").Length != 11)
            //{
            //    return "Looks like this is invalid url/id";
            //}

            if (history.ContainsKey(url))
            {
                return history[url];
            }

            var youtube = YouTube.Default;
            var video = youtube.GetVideoAsync(url);

            try
            {
                var getUri = video.Result.Uri;
            }
            catch (AggregateException ignore)
            {
                return "Looks like this is invalid url/id";
            }
            catch (InvalidOperationException ignore)
            {
                return
                    $"{CleanFilename(video.Result.FullName)} video is properly copyright protected or locked by provider!";
            }

            CreateFolder(baseFolder);

            File.WriteAllBytes(video.Result.FullName, video.Result.GetBytes());

            var inputFile = new MediaFile { Filename = video.Result.FullName };
            var outputFile = new MediaFile { Filename = $"{baseFolder}\\{CleanFilename(video.Result.FullName)}.mp3" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
                engine.Convert(inputFile, outputFile);
            }

            TryToDelete(inputFile.Filename);

            lock (history)
            {
                history.Add(url, outputFile.Filename);
                writeHistoryToFile();
            }

            return outputFile.Filename;
        }

        private static void CreateFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        private static string CleanFilename(string rawFilename)
        {
            return rawFilename
                .Replace(" - YouTube", "")
                .Replace(".webm", "")
                .Replace(".mp3", "")
                .Replace(".mp4", "");
        }

        private static void TryToDelete(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException ex)
            {
            }
        }
    }
}

//namespace YouTuber.Client
//{
//    public class YouTubeService : IYouTubeService
//    {
//        private const string BaseUrl = "https://www.youtube.com/watch?v=";
//        private const string BaseFolder = "download";
//        private readonly HashSet<string> _set = new HashSet<string>();

//        public virtual IEnumerable<string> FileToList(string file)
//        {
//            string[] results;
//            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
//            {
//                using (var sr = new StreamReader(fs))
//                {
//                    results = sr.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
//                }
//            }
//            return results;
//        }

//        private static void CreateFolder(string folder)
//        {
//            var path = Path.Combine(Directory.GetCurrentDirectory(), folder);
//            if (!Directory.Exists(path))
//            {
//                Directory.CreateDirectory(path);
//            }
//        }

//        private static string CleanFilename(string rawFilename)
//        {
//            return rawFilename
//                .Replace(" - YouTube", "")
//                .Replace(".webm", "")
//                .Replace(".mp3", "")
//                .Replace(".mp4", "");
//        }

//        private static Uri Url(string url)
//        {
//            var str = url.Length == 11 ? $"{BaseUrl}{url}" : url;
//            var uri = new Uri(str);
//            return uri;
//        }

//        private static void TryToDelete(string file)
//        {
//            try
//            {
//                File.Delete(file);
//            }
//            catch (IOException ex)
//            {
//            }
//        }
//   }
//}