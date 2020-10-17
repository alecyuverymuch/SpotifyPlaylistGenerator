using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spotify.Api.Core.Configuration;
using Spotify.Api.Core.Extensions;
using Spotify.Api.Core.Http;
using Spotify.Api.Core.Interfaces;
using Spotify.Api.Core.Models.Responses;
using static Spotify.Api.Core.Extensions.FlurlExtensions;

namespace Spotify.Api.Core.Clients
{
    public class SpotifyApiClient : ApiClientBase
    {
        private static ILogger<SpotifyApiClient> _logger;

        public SpotifyApiClient(SpotifyClientConfiguration clientConfig, IAuthenticationClient authClient, ILoggerFactory loggerFactory) : base(clientConfig, authClient)
        {
            _logger = loggerFactory.CreateLogger<SpotifyApiClient>();
        }

        public async Task<SpotifyUserResponse> GetCurrentUser(CancellationToken cancellation = default)
            => await ExecuteWithRetryPolicyAsync(() => 
                CreateRequest(null, cancellation, "me")
                    .WhenAsync(r => r.GetAsync<SpotifyUserResponse>(cancellation)));

        public async Task<SpotifyPagedResponse<SpotifySimplePlaylistResponse>> GetUserPlaylists(string userId, int? limit = null, int? offset = null, CancellationToken cancellation = default)
        {
            var data = new List<KeyValuePair<string, object>>();
            if (limit != null)
                data.Add(new KeyValuePair<string, object>("limit", limit));
            if (offset != null)
                data.Add(new KeyValuePair<string, object>("offset", offset));
            _logger.LazyLog(LogLevel.Information, () => $"Requesting user {userId} playlist data from {BaseUrl}");
            return await ExecuteWithRetryPolicyAsync(() => 
                CreateRequest(data, cancellation, "users", userId, "playlists")
                    .WhenAsync(r => r.GetAsync<SpotifyPagedResponse<SpotifySimplePlaylistResponse>>(cancellation)));
        }

        public async Task<SpotifyPagedPlaylistsResponse<SpotifySimplePlaylistResponse>> GetFeaturedPlaylists(int? limit = null, int? offset = null, CancellationToken cancellation = default)
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("country", ClientConfig.Country)
            };
            if (limit != null)
                data.Add(new KeyValuePair<string, object>("limit", limit));
            if (offset != null)
                data.Add(new KeyValuePair<string, object>("offset", offset));
            _logger.LazyLog(LogLevel.Information, () => $"Requesting featured playlist data from country {ClientConfig.Country}");
            return await ExecuteWithRetryPolicyAsync(() => 
                CreateRequest(data, cancellation, "browse", "featured-playlists")
                    .WhenAsync(r => r.GetAsync<SpotifyPagedPlaylistsResponse<SpotifySimplePlaylistResponse>>(cancellation)));
        }

        public async Task<SpotifyPagedPlaylistsResponse<SpotifySimplePlaylistResponse>> GetTopPlayLists(int? limit = null, int? offset = null, CancellationToken cancellation = default)
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("country", ClientConfig.Country)
            };
            if (limit != null)
                data.Add(new KeyValuePair<string, object>("limit", limit));
            if (offset != null)
                data.Add(new KeyValuePair<string, object>("offset", offset));
            _logger.LazyLog(LogLevel.Information, () => $"Requesting top playlist data from country {ClientConfig.Country}");
            return await ExecuteWithRetryPolicyAsync(() =>
                CreateRequest(data, cancellation, "browse", "categories", "toplists", "playlists")
                    .WhenAsync(r => r.GetAsync<SpotifyPagedPlaylistsResponse<SpotifySimplePlaylistResponse>>(cancellation)));
        }

        public async Task<SpotifyPagedResponse<SpotifyArtistResponse>> GetUserTopArtists(int? limit = null, int? offset = null, CancellationToken cancellation = default)
        {
            var data = new List<KeyValuePair<string, object>>();
            if (limit != null)
                data.Add(new KeyValuePair<string, object>("limit", limit));
            if (offset != null)
                data.Add(new KeyValuePair<string, object>("offset", offset));
            _logger.LazyLog(LogLevel.Information, () => "Requesting current users top artists");
            return await ExecuteWithRetryPolicyAsync(() => 
                CreateRequest(data, cancellation, "me", "top", "artists")
                    .WhenAsync(r => r.GetAsync<SpotifyPagedResponse<SpotifyArtistResponse>>(cancellation)));
        }

        public async Task<SpotifyPagedResponse<SpotifyTrackResponse>> GetUserTopTracks(int? limit, int? offset = null, CancellationToken cancellation = default)
        {
            var data = new List<KeyValuePair<string, object>>();
            if (limit != null)
                data.Add(new KeyValuePair<string, object>("limit", limit));
            if (offset != null)
                data.Add(new KeyValuePair<string, object>("offset", offset));
            _logger.LazyLog(LogLevel.Information, () => "Requesting current users top artists");
            return await ExecuteWithRetryPolicyAsync(() =>
                CreateRequest(data, cancellation, "me", "top", "tracks")
                    .WhenAsync(r => r.GetAsync<SpotifyPagedResponse<SpotifyTrackResponse>>(cancellation)));
        }

        public async Task<SpotifyTracklistResponse> GetArtistTopTracks(string artistId, CancellationToken cancellation = default)
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("country", ClientConfig.Country)
            };
            _logger.LazyLog(LogLevel.Information, () => "Requesting current users top artists");
            return await ExecuteWithRetryPolicyAsync(() =>
                CreateRequest(data, cancellation, "artists", artistId, "top-tracks")
                    .WhenAsync(r => r.GetAsync<SpotifyTracklistResponse>(cancellation)));
        }

        public async Task<SpotifyPagedResponse<SpotifyPlaylistTrackResponse>> GetTracksFromPlaylist(string playlistId, int? limit, int? offset, CancellationToken cancellation = default)
        {
            var data = new List<KeyValuePair<string, object>>();
            if (limit != null)
                data.Add(new KeyValuePair<string, object>("limit", limit));
            if (offset != null)
                data.Add(new KeyValuePair<string, object>("offset", offset));
            _logger.LazyLog(LogLevel.Information, () => $"Requesting playlist tracks from playlist {playlistId} from {BaseUrl}");
            return await ExecuteWithRetryPolicyAsync(() => CreateRequest(data, cancellation, "playlists", playlistId, "tracks").WhenAsync(r => r.GetAsync<SpotifyPagedResponse<SpotifyPlaylistTrackResponse>>(cancellation)));
        }

        public async Task<SpotifySimplePlaylistResponse> PostNewPlaylist(string userId, string name, CancellationToken cancellation = default)
            => await ExecuteWithRetryPolicyAsync(() => 
                CreateJsonRequest(cancellation, "users", userId, "playlists")
                    .WhenAsync(r => r.PostJsonAsync<SpotifySimplePlaylistResponse>(new { name }, cancellation)));

        public async Task<KeyValuePair<string, string>> PostTrackUriListToPlaylist(string playlistId, IEnumerable<string> trackUris, CancellationToken cancellation = default)
            => await ExecuteWithRetryPolicyAsync(() =>
                CreateJsonRequest(cancellation, "playlists", playlistId, "tracks")
                    .WhenAsync(r => r.PostJsonAsync<KeyValuePair<string, string>>(new { uris = trackUris }, cancellation)));
    }
}
