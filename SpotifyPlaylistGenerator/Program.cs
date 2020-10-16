using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spotify.Api.Core.Clients;
using Spotify.Api.Core.Configuration;
using Spotify.Api.Core.Services;

namespace SpotifyPlaylistGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var spotifyClientConfig = configuration.GetSection(nameof(SpotifyClientConfiguration)).Get<SpotifyClientConfiguration>();
            var spotifyAuthConfig = configuration.GetSection(nameof(SpotifyAuthenticationConfiguration)).Get<SpotifyAuthenticationConfiguration>();

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

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
