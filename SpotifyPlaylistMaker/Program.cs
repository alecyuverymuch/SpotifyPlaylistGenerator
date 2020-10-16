using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Spotify.Api.Core.Clients;
using Spotify.Api.Core.Configuration;
using Spotify.Api.Core.Services;

namespace SpotifyPlaylistMaker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var spotifyClientConfig = new SpotifyClientConfiguration
            {
                BaseUrl = "https://api.spotify.com/v1",
                County = "US"
            };

            var spotifyAuthConfig = new SpotifyAuthenticationConfiguration
            {
                AuthBaseUrl = "https://accounts.spotify.com",
                ClientId = "4e36239421de4767b73db120dad0846d",
                ClientSecret = "392434330d0e4bcfaa8a947db4beed1e",
                RedirectUri = "http://localhost:8888/callback/",
                Scope = "user-read-private user-read-email user-top-read"
            };

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = new LoggerFactory();
            var authClient = new SpotifyAuthenticationClient(spotifyAuthConfig, cache, logger);
            var client = new SpotifyApiClient(spotifyClientConfig, authClient, logger);
            var service = new SpotifyPlaylistService(client, logger);
            var generator = new ConsoleMenuGenerator(service, cache);
            await generator.GenerateMainMenu(cts.Token);
        }
    }
}
