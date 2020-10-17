using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Spotify.Api.Core.Enums;
using Spotify.Api.Core.Models;
using Spotify.Api.Core.Services;

namespace SpotifyPlaylistGenerator
{
    public class ConsoleMenuGenerator
    {
        private static IMemoryCache _cache;
        private static SpotifyPlaylistService _service;
        private const string FilePath = @"../../../Profiles/";

        private const string UserCacheKey = "CurrentUserCacheKey";

        public ConsoleMenuGenerator(SpotifyPlaylistService service, IMemoryCache cache)
        {
            _cache = cache;
            _service = service;
        }

        public async Task GenerateMainMenu(CancellationToken cancellation)
        {
            Print(0, "Welcome to the Spotify Playlist Generator!");
            Print(0, "This tool will generate a new playlist for you by randomly selecting songs based on a configuration you provide");
            Print(0, "Please 'Log into Spotify' to begin.");
            Print(0, "To create a new playlist, please enter the profile settings menu and create a new playlist profile");
            Print(0, "then run 'Generate playlist' and choose your profile.\n");

            while (true)
            {
                var options = new Dictionary<string, string>
                {
                    {"0", "Log into Spotify"},
                    {"1", "See current playlists"},
                    {"2", "Profile settings"},
                    {"3", "Generate playlist"},
                    {"x", "Exit"}
                };
                Print(0, "Main Menu:");
                PrintOptions(options, 1);
                var input = ReadInclude(0, options.Keys);

                switch (input)
                {
                    case "0":
                        await GenerateLogin(1, cancellation);
                        break;
                    case "1":
                        await GeneratePlaylistSelectionMenu(1, cancellation);
                        break;
                    case "2":
                        await GenerateSettingsMenu(1, cancellation);
                        break;
                    case "3":
                        await GenerateRunGeneratorMenu(1, cancellation);
                        break;
                    default:
                        return;
                }
            }
        }

        private static async Task GenerateLogin(int printDepth, CancellationToken cancellation)
        {
            if (!_cache.TryGetValue(UserCacheKey, out SpotifyItemReference userData))
            {
                Print(printDepth, "Logging into Spotify...");
                userData = await _service.GetUserAsync(cancellation);
            }

            Print(printDepth, $"Logged in as user: {userData.Name}\n");
            _cache.Set(UserCacheKey, userData);
        }

        private async Task GenerateRunGeneratorMenu(int printDepth, CancellationToken cancellation)
        {
            if (!_cache.TryGetValue(UserCacheKey, out SpotifyItemReference userData))
            {
                Print(printDepth, "User not logged in, please log in to Spotify through the main menu to use this function:\n");
                return;
            }

            var profileSelection = GenerateSelectProfileMenu(userData.Id, printDepth + 1);
            var profile = JsonConvert.DeserializeObject<IDictionary<string, PlaylistProfileEntry>>(await File.ReadAllTextAsync(FilePath + $"{userData.Id}-{profileSelection}.json", cancellation));
            if (!profile.Any())
            {
                Print(printDepth, $"Profile {profileSelection} does not contain any sources. Please add sources to continue\n");
                return;
            }

            Print(printDepth, "Would you like to remove duplicated tracks (y/n)?");
            var input = ReadInclude(printDepth, new List<string> {"y", "n"});
            var removeDupe = input.Equals("y", StringComparison.InvariantCultureIgnoreCase);

            var songUriList = new List<string>();
            foreach (var (_, value) in profile)
            {
                var rng = new Random();
                Print(printDepth, $"Reading {value.Count} songs from source {value.Name}");
                var tracks = value.Type switch
                {
                    SourceTypes.Playlist => await _service.GetAllPlaylistTrackUris(value.Id, cancellation),
                    SourceTypes.Artist => await _service.GetArtistTopTrackUris(value.Id, cancellation),
                    SourceTypes.UserFavorite => await _service.GetAllUserTopTrackUris(userData.Id, cancellation),
                    _ => throw new InvalidOperationException("Program reached an unknown state"),
                };
                songUriList.AddRange(tracks.OrderBy(track => rng.Next()).Take(value.Count));
            }

            Print(0, $"Creating playlist {profileSelection}...");
            var newPlaylist = await _service.CreatePlaylistAsync(userData.Id, profileSelection, cancellation);
            Print(0, "Adding songs...");
            await _service.PostSongsToPlaylistAsync(newPlaylist.Id, removeDupe ? songUriList.Distinct() : songUriList, cancellation);
            Print(0, $"Playlist {profileSelection} has been created\n");
        }

        private async Task GeneratePlaylistSelectionMenu(int printDepth, CancellationToken cancellation)
        {
            while (true)
            {
                if (!_cache.TryGetValue(UserCacheKey, out SpotifyItemReference userData))
                {
                    Print(printDepth, "User not logged in, please log in to Spotify through the main menu to use this function:\n");
                    return;
                }
                var playlistSelection = await GeneratePagedSelectionMenu(_service.GetUserPlaylistsAsync, userData.Id, 10, 0, "Select a playlist:", printDepth, cancellation);
                if (playlistSelection == null)
                    return;
                await GeneratePlaylistTrackSelectionMenu(playlistSelection, printDepth + 1, cancellation);
            }
        }

        private async Task GeneratePlaylistTrackSelectionMenu(SpotifyItemReference playlist, int printDepth, CancellationToken cancellation)
        {
            while (true)
            {
                var options = new Dictionary<string, string>
                {
                    {"0", "View playlist songs" },
                    {"1", "Select another playlist" }
                };
                Print(printDepth, $"Selected playlist {playlist.Name}:");
                PrintOptions(options, printDepth + 1);
                var input = ReadInclude(printDepth, options.Keys);

                if (input.Equals("0"))
                    await GeneratePlaylistTracksMenu(_service.GetPlaylistTracksAsync, playlist.Id, printDepth + 1, 10, 0, cancellation);
                else
                    return;
            }
        }

        private static async Task GeneratePlaylistTracksMenu(Func<string, int, int, CancellationToken, Task<SpotifyPageItemReference>> stepFunc, string playlistId, int printDepth, int limit, int offset, CancellationToken cancellation)
        {
            var tracks = await stepFunc(playlistId, limit, offset, cancellation);
            while (true)
            {
                Print(printDepth, $"Displaying tracks {tracks.Offset+1}-{tracks.Offset+tracks.Limit} out of {tracks.Total}\n");
                foreach (var track in tracks.Items)
                {
                    Print(printDepth + 2, $"> Song: {track.Name}");
                    Print(printDepth + 3, $"Album: {track.Opt}");
                    Print(printDepth + 3, $"Artists: {track.By}\n");
                }

                var accept = new List<string>();
                if (tracks.HasNext)
                {
                    Print(printDepth + 1, ">n: Next Page of Items");
                    accept.Add("n");
                }
                if (tracks.HasPrev)
                {
                    Print(printDepth + 1, ">p: Previous Page of Items");
                    accept.Add("p");
                }
                Print(printDepth + 1, ">x: Exit to previous menu");
                accept.Add("x");

                var input = ReadInclude(printDepth, accept);

                switch (input)
                {
                    case "n":
                        tracks = await stepFunc(playlistId, tracks.Limit, tracks.Offset + tracks.Limit, cancellation);
                        break;
                    case "p":
                        tracks = await stepFunc(playlistId, tracks.Limit, Math.Max(0, tracks.Offset - tracks.Limit), cancellation);
                        break;
                    default:
                        return;
                }
            }
        }

        private async Task<SpotifyItemReference> GeneratePagedSelectionMenu(Func<string, int, int, CancellationToken, Task<SpotifyPageItemReference>> stepFunc, string userId, int limit, int offset, string prompt, int printDepth, CancellationToken cancellation)
        {
            var page = await stepFunc(userId, limit, offset, cancellation);
            while (true)
            {
                var items = page.Items.ToList();
                var options = Enumerable.Range(0, items.Count).ToDictionary(i => i.ToString(), i => items[i].Name);
                if (page.HasNext)
                    options.Add("n", "Next Page of Items");
                if (page.HasPrev)
                    options.Add("p", "Previous Page of Items");
                options.Add("x", "Exit selection to previous menu");
                Print(printDepth, prompt);
                PrintOptions(options, printDepth + 1);
                var input = ReadInclude(printDepth, options.Keys);

                switch (input)
                {
                    case "n":
                        page = await stepFunc(userId, page.Limit, page.Offset + page.Limit, cancellation);
                        break;
                    case "p":
                        page = await stepFunc(userId, page.Limit, Math.Max(0, page.Offset - page.Limit), cancellation);
                        break;
                    case "x":
                        return null;
                    default:
                        return items[int.Parse(input)];
                }
            }
        }

        private async Task GenerateSettingsMenu(int printDepth, CancellationToken cancellation)
        {
            while (true)
            {
                if (!_cache.TryGetValue(UserCacheKey, out SpotifyItemReference userData))
                {
                    Print(printDepth, "User not logged in, please log in to Spotify through the main menu to begin:\n");
                    return;
                }

                var options = new Dictionary<string, string>
                {
                    {"0", "Add a new profile"},
                    {"1", "Edit an existing profile"},
                    {"2", "Delete a profile"},
                    {"x", "Return to main menu"}
                };
                Print(printDepth, "Select an operation:");
                PrintOptions(options, printDepth + 1);

                var input = ReadInclude(printDepth, options.Keys);

                switch (input)
                {
                    case "0": 
                        await GenerateCreateProfileMenu(userData.Id, printDepth + 1, cancellation);
                        break;
                    case "1":
                        var editProfileName = GenerateSelectProfileMenu(userData.Id, printDepth + 1);
                        if (string.IsNullOrEmpty(editProfileName))
                            continue;
                        await GenerateEditProfileMenu(userData.Id, editProfileName, printDepth + 1, cancellation);
                        break;
                    case "2":
                        var removeProfileName = GenerateSelectProfileMenu(userData.Id, printDepth + 1);
                        File.Delete(FilePath + $"{userData.Id}-{removeProfileName}.json");
                        Print(printDepth + 1, $"Profile {removeProfileName} deleted\n");
                        break;
                    default:
                        return;
                }
            }
        }

        private async Task GenerateCreateProfileMenu(string userId, int printDepth, CancellationToken cancellation)
        {
            var files = Directory.GetFiles(FilePath)
                .Where(f => f.Contains($"{userId}-", StringComparison.InvariantCultureIgnoreCase))
                .Where(f => f.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                .Select(f => f.Substring(f.LastIndexOf($"{userId}-", StringComparison.InvariantCultureIgnoreCase) + $"{userId}-".Length).Replace(".json", ""))
                .ToList();
            Print(printDepth, "Enter a name for the new playlist:");
            var name = ReadExclude(printDepth, files);
            Console.WriteLine();
            var sources = await GenerateAddSourceMenu(new Dictionary<string, PlaylistProfileEntry>(), userId, name, printDepth + 1, cancellation);
            if (sources == null)
                return;
            await File.WriteAllTextAsync(FilePath + $"{userId}-{name}.json", JsonConvert.SerializeObject(sources), cancellation);
            Print(printDepth, $"Profile {name} created\n");
        }

        private string GenerateSelectProfileMenu(string userId, int printDepth)
        {
            while (true)
            {
                var files = Directory.GetFiles(FilePath)
                    .Where(f => f.Contains($"{userId}-", StringComparison.InvariantCultureIgnoreCase))
                    .Where(f => f.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                    .Select(f => f.Substring(f.LastIndexOf($"{userId}-", StringComparison.InvariantCultureIgnoreCase) + $"{userId}-".Length).Replace(".json", ""))
                    .ToList();
                if (!files.Any())
                {
                    Print(printDepth, $"User {userId} does not have any existing profiles. Please create one.\n");
                    return null;
                }

                var options = Enumerable.Range(0, files.Count).ToDictionary(i => i.ToString(), i => files[i]);
                options.Add("x", "Exit to previous menu");

                Print(printDepth, "Select a profile:");
                PrintOptions(options, printDepth + 1);
                var input = ReadInclude(printDepth, options.Keys);
                return input.Equals("x") ? null : files[int.Parse(input)];
            }
        }

        private async Task GenerateEditProfileMenu(string userId, string profileName, int printDepth, CancellationToken cancellation)
        {
            var profile = JsonConvert.DeserializeObject<IDictionary<string, PlaylistProfileEntry>>(File.ReadAllText(FilePath + $"{userId}-{profileName}.json"));
            while (true)
            {
                var options = new Dictionary<string, string>
                {
                    {"0", "Add sources"},
                    {"1", "Remove sources"},
                    {"s", "Save edits and return"},
                    {"x", "Exit without saving" }
                };

                Print(printDepth, "Select operation:");
                PrintOptions(options, printDepth + 1);
                var input = ReadInclude(printDepth, options.Keys);

                switch (input)
                {
                    case "0":
                        profile = await GenerateAddSourceMenu(profile, userId, profileName, printDepth + 1, cancellation) ?? profile;
                        break;
                    case "1":
                        profile = GenerateRemoveSourceMenu(profile, profileName, printDepth + 1);
                        break;
                    case "s":
                        await File.WriteAllTextAsync(FilePath + $"{userId}-{profileName}.json", JsonConvert.SerializeObject(profile), cancellation);
                        Print(printDepth, $"Profile {profileName} saved\n");
                        return;
                    default:
                        return;
                }
            }
        }

        private async Task<IDictionary<string, PlaylistProfileEntry>> GenerateAddSourceMenu(IDictionary<string, PlaylistProfileEntry> sources, string userId, string name, int printDepth, CancellationToken cancellation)
        {
            var options = new Dictionary<string, string>
            {
                {"0", "Featured playlists"},
                {"1", "Popular playlists"},
                {"2", "Your playlists"},
                {"3", "Your most played artists"},
                {"4", "Your most played songs"},
                {"s", "Save selections and return"},
                {"x", "Exit without saving"}
            };
            while (true)
            {
                Print(printDepth, $"Current sources for playlist {name}:");
                foreach (var (_, value) in sources)
                    Print(printDepth + 1, $">{value.Type:G}: {value.Name}");
                Print(0, "");
                Print(printDepth, "Add a source to seed songs from: ");
                PrintOptions(options, printDepth + 1);
                var input = ReadInclude(printDepth, options.Keys);

                PlaylistProfileEntry entry;
                switch (input)
                {
                    case "0":
                        entry = await MakePlaylistEntryAsync(_service.GetFeaturedPlaylistsAsync, userId, printDepth + 1, cancellation);
                        break;
                    case "1":
                        entry = await MakePlaylistEntryAsync(_service.GetPopularPlaylistsAsync, userId, printDepth + 1, cancellation);
                        break;
                    case "2":
                        entry = await MakePlaylistEntryAsync(_service.GetUserPlaylistsAsync, userId, printDepth + 1, cancellation);
                        break;
                    case "3":
                        entry = await MakeArtistEntryAsync(_service.GetUserTopArtistsAsync, userId, printDepth + 1, cancellation);
                        break;
                    case "4":
                        entry = MakeTopTracksEntry(printDepth + 1);
                        break;
                    case "s":
                        return sources;
                    default:
                        return null;
                }

                if (entry == null)
                    continue;
                if (sources.ContainsKey(entry.Id))
                {
                    Print(printDepth + 1, $"Profile already contains entry: {entry.Name}. Please go to the edit menu to edit this source");
                    continue;
                }
                sources.Add(entry.Id, entry);
            }
        }

        private IDictionary<string, PlaylistProfileEntry> GenerateRemoveSourceMenu(IDictionary<string, PlaylistProfileEntry> sources, string name, int printDepth)
        {
            while (true)
            {
                var values = sources.Values.ToList();
                var optionNames = Enumerable.Range(0, values.Count).ToDictionary(i => i.ToString(), i => values[i].Name);
                optionNames.Add("x", "Return to previous menu");

                Print(printDepth, $"Select a source to remove from {name}:");
                PrintOptions(optionNames, printDepth + 1);
                var input = ReadInclude(printDepth, optionNames.Keys);

                if (input.Equals("x"))
                    return sources;
                sources.Remove(values[int.Parse(input)].Id);
            }
        }

        private async Task<PlaylistProfileEntry> MakePlaylistEntryAsync(Func<string, int, int, CancellationToken, Task<SpotifyPageItemReference>> stepFunc, string userId, int printDepth, CancellationToken cancellation)
        {
            var entry = new PlaylistProfileEntry();
            var playlist = await GeneratePagedSelectionMenu(stepFunc, userId, 10, 0, "Select playlist to seed from:", printDepth, cancellation);
            if (playlist == null)
                return null;
            entry.Name = playlist.Name;
            entry.Id = playlist.Id;
            entry.Type = SourceTypes.Playlist;
            Print(printDepth, $"How many songs from this source would you like to add (max {playlist.Opt})?");
            entry.Count = ReadCount(printDepth, int.Parse(playlist.Opt));
            return entry;
        }

        private async Task<PlaylistProfileEntry> MakeArtistEntryAsync(Func<string, int, int, CancellationToken, Task<SpotifyPageItemReference>> stepFunc, string userId, int printDepth, CancellationToken cancellation)
        {
            var entry = new PlaylistProfileEntry();
            var artist = await GeneratePagedSelectionMenu(stepFunc, userId, 10, 0, "Select artist to seed from:", printDepth, cancellation);
            if (artist == null)
                return null;
            entry.Name = artist.Name;
            entry.Id = artist.Id;
            entry.Type = SourceTypes.Artist;
            Print(printDepth, "How many songs from this source would you like to add (max 10)?");
            entry.Count = ReadCount(printDepth, 10);
            return entry;
        }

        private static PlaylistProfileEntry MakeTopTracksEntry(int printDepth)
        {
            var entry = new PlaylistProfileEntry
            {
                Name = "UserTopTracks", 
                Id = "UserTopTracks", 
                Type = SourceTypes.UserFavorite
            };
            Print(printDepth, "How many songs from this source would you like to add (max 50)?");
            entry.Count = ReadCount(printDepth, 50);
            return entry;
        }

        private static int ReadCount(int depth, int max)
        {
            while (true)
            {
                Console.Write(Depth(depth + 1) + ">");
                var input = Console.ReadLine();
                Console.WriteLine();
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out var result) && result > 0 && result <= max) return result;
                Print(depth, $"Invalid input, {input}, try again:");
            }
        }

        private static string ReadInclude(int depth, ICollection<string> validRange)
        {
            while (true)
            {
                Console.Write(Depth(depth + 1) + ">");
                var input = Console.ReadLine();
                Console.WriteLine();
                if (!string.IsNullOrEmpty(input) && validRange.Contains(input, StringComparer.InvariantCultureIgnoreCase)) return input;
                Print(depth, $"Invalid input, {input}, try again:");
            }
        }

        private static string ReadExclude(int depth, ICollection<string> exclude)
        {
            while (true)
            {
                Console.Write(Depth(depth + 1) + ">");
                var input = Console.ReadLine();
                Console.WriteLine();
                if (!string.IsNullOrEmpty(input) && !exclude.Contains(input)) return input;
                Print(depth, $"Input {input} already exists, try again:");
            }
        }

        private static void Print(int depth, string message)
            => Console.WriteLine(Depth(depth) + message);

        private void PrintOptions(IDictionary<string, string> options, int printDepth)
        {
            foreach (var option in options)
                Print(printDepth, $">{option.Key}: {option.Value}");
        }

        private static string Depth(int depth)
        {
            var output = "";
            while (depth > 0)
            {
                output += "  ";
                depth--;
            }

            return output;
        }
    }
}
