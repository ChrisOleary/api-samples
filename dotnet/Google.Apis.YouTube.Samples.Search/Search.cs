using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace Google.Apis.YouTube.Samples
{
    internal class Search
  {
        [STAThread]
        static void Main(string[] args)
        {
          Console.WriteLine("YouTube Data API: Search");
          Console.WriteLine("========================");

          try
          {
            new Search().Run().Wait();
          }
          catch (AggregateException ex)
          {
            foreach (var e in ex.InnerExceptions)
            {
              Console.WriteLine("Error: " + e.Message);
            }
          }

          Console.WriteLine("Press any key to continue...");
          Console.ReadKey();
        }

        private async Task Run()
        {
            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> thumbnails = new List<string>();
            List<string> publishedAt = new List<string>();
            List<string> videoId = new List<string>();
            var folder = "C:\\Users\\Chris\\Desktop\\InternetTodayThumbnails";

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCOCEoet-VxdL_PAdupTMQki-y_SCdsyxk",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.ChannelId = "UCyZVCV9xhrCyz4hPehvb4Wg"; // Internet Todays Channel ID
            searchListRequest.MaxResults = 50; // max that api spits out
            searchListRequest.Order = (SearchResource.ListRequest.OrderEnum?)1;
            searchListRequest.PublishedAfter =  new DateTime(2021,9,19); 

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and thumbnails.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        videos.Add($"{searchResult.Snippet.Title}");
                        videoId.Add($"{searchResult.Id.VideoId}");
                        thumbnails.Add($"{searchResult.Snippet.Thumbnails.Medium.Url}");
                        publishedAt.Add($"{searchResult.Snippet.PublishedAt}");
                        break;
                    case "youtube#channel":
                        channels.Add($"{searchResult.Snippet.Title}");
                        break;
                }


            }

            // save image to local folder
            for (int i = 0; i < videoId.Count; i++)
            {
                try
                {
                    DownloadImage(folder, videoId[i], new Uri(thumbnails[i]));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"shits gone wrong: {ex.Message}");
                }
            }

            Console.WriteLine(String.Format("Channel Name:\n{0}\n", string.Join("\n", channels)));
            Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
            Console.WriteLine(String.Format("VideoId:\n{0}\n", string.Join("\n", videoId)));
            Console.WriteLine(String.Format("Thumbnail:\n{0}\n", string.Join("\n", thumbnails)));
            Console.WriteLine(String.Format("Published:\n{0}\n", string.Join("\n", publishedAt)));
            Console.WriteLine($"Images saved in {folder}");


        }
        private void DownloadImage(string directoryPath, string fileName, Uri uri)
        {
            var httpClient = new HttpClient();

            // Get the file extension
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            // Create file path and ensure directory exists
            var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
            Directory.CreateDirectory(directoryPath);

            // Download the image and write to the file
            var imageBytes = httpClient.GetByteArrayAsync(uri);
            File.WriteAllBytes(path, imageBytes.Result);
        }
    }

}
