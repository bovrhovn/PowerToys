// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.CmdPal.Extensions;
using Microsoft.CmdPal.Extensions.Helpers;
using YouTubeExtension.Actions;

namespace YouTubeExtension.Pages;

internal sealed partial class YouTubeVideosPage : DynamicListPage
{
    public YouTubeVideosPage()
    {
        Icon = new("https://www.youtube.com/favicon.ico");
        Name = "YouTube (Video Search)";
        this.ShowDetails = true;
    }

    public override ISection[] GetItems(string query)
    {
        return DoGetItems(query).GetAwaiter().GetResult(); // Fetch and await the task synchronously
    }

    private async Task<ISection[]> DoGetItems(string query)
    {
        // Fetch YouTube videos based on the query
        List<YouTubeVideo> items = await GetYouTubeVideos(query);

        // Create a section and populate it with the video results
        var section = new ListSection()
        {
            Title = "Search Results",
            Items = items.Select(video => new ListItem(new OpenVideoLinkAction(video.Link))
            {
                Title = video.Title,
                Subtitle = $"{video.Channel}",
                Details = new Details()
                {
                    Title = video.Title,
                    HeroImage = new(video.ThumbnailUrl),
                    Body = $"{video.Channel}",
                },
                Tags = [new Tag()
                               {
                                   Text = video.PublishedAt.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture), // Show the date of the video post
                               }
                        ],
                MoreCommands = [
                    new CommandContextItem(new OpenChannelLinkAction(video.ChannelUrl)),
                    new CommandContextItem(new YouTubeVideoInfoMarkdownPage(video)),
                    new CommandContextItem(new YouTubeAPIPage()),
                ],
            }).ToArray(),
        };

        return new[] { section }; // Properly return an array of sections
    }

    // Method to fetch videos from YouTube API
    private static async Task<List<YouTubeVideo>> GetYouTubeVideos(string query)
    {
        var state = File.ReadAllText(YouTubeHelper.StateJsonPath());
        var jsonState = JsonNode.Parse(state);
        var apiKey = jsonState["apiKey"]?.ToString() ?? string.Empty;

        var videos = new List<YouTubeVideo>();

        using HttpClient client = new HttpClient();
        {
            try
            {
                // Send the request to the YouTube API with the provided query
                var response = await client.GetStringAsync($"https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&q={query}&key={apiKey}&maxResults=20");
                var json = JsonNode.Parse(response);

                // Parse the response
                if (json?["items"] is JsonArray itemsArray)
                {
                    foreach (var item in itemsArray)
                    {
                        // Add each video to the list with title, link, author, thumbnail, and captions (if available)
                        videos.Add(new YouTubeVideo
                        {
                            Title = item["snippet"]?["title"]?.ToString() ?? string.Empty,
                            VideoId = item["id"]?["videoId"]?.ToString() ?? string.Empty,
                            Link = $"https://www.youtube.com/watch?v={item["id"]?["videoId"]?.ToString()}",
                            Channel = item["snippet"]?["channelTitle"]?.ToString() ?? string.Empty,
                            ChannelId = item["snippet"]?["channelId"]?.ToString() ?? string.Empty,
                            ChannelUrl = $"https://www.youtube.com/channel/{item["snippet"]?["channelId"]?.ToString()}" ?? string.Empty,
                            ThumbnailUrl = item["snippet"]?["thumbnails"]?["default"]?["url"]?.ToString() ?? string.Empty, // Get the default thumbnail URL
                            PublishedAt = DateTime.Parse(item["snippet"]?["publishedAt"]?.ToString(), CultureInfo.InvariantCulture), // Use CultureInfo.InvariantCulture
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors from the API call or parsing
                videos.Add(new YouTubeVideo
                {
                    Title = "Error fetching data",
                    Channel = $"Error: {ex.Message}",
                });
            }
        }

        return videos;
    }
}
