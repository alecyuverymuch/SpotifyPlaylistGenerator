using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spotify.Api.Core.Clients;
using Spotify.Api.Core.Extensions;
using Spotify.Api.Core.Models;
using Spotify.Api.Core.Models.Responses;

namespace Spotify.Api.Core.Services
{
    public class SpotifyPlaylistService
    {
        private static SpotifyApiClient _client;
        private static ILogger<SpotifyPlaylistService> _logger;

        public SpotifyPlaylistService(SpotifyApiClient client, ILoggerFactory logger)
        {
            _client = client;
            _logger = logger.CreateLogger<SpotifyPlaylistService>();
        }

        public async Task<SpotifyItemReference> GetUserAsync(CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => "Reading current user data");
            var userData = await _client.GetCurrentUser(cancellation);
            return new SpotifyItemReference(userData.Id, userData.DisplayName, userData.Type);
        }

        public async Task<SpotifyPageItemReference> GetUserPlaylistsAsync(string userId, int limit, int offset, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading playlists {offset + 1} to {offset + limit} from user {userId}");
            var playlistData = await _client.GetUserPlaylists(userId, limit, offset, cancellation);
            return new SpotifyPageItemReference
            {
                HasNext = !string.IsNullOrEmpty(playlistData.Next),
                Next = playlistData.Next,
                HasPrev = !string.IsNullOrEmpty(playlistData.Previous),
                Previous = playlistData.Previous,
                Limit = playlistData.Limit,
                Offset = playlistData.Offset,
                Total = playlistData.Total,
                Items = playlistData.Items.Select(p => new SpotifyItemReference(p.Id, p.Name, p.Owner.DisplayName, p.Tracks.Total.ToString()))
            };
        }

        public async Task<SpotifyPageItemReference> GetFeaturedPlaylistsAsync(string userId, int limit, int offset, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading playlists {offset + 1} to {offset + limit} from featured for user {userId}");
            var playlistData = await _client.GetFeaturedPlaylists(limit, offset, cancellation).ThenAsync(p => p.Playlists);
            return new SpotifyPageItemReference
            {
                HasNext = !string.IsNullOrEmpty(playlistData.Next),
                Next = playlistData.Next,
                HasPrev = !string.IsNullOrEmpty(playlistData.Previous),
                Previous = playlistData.Previous,
                Limit = playlistData.Limit,
                Offset = playlistData.Offset,
                Total = playlistData.Total,
                Items = playlistData.Items.Select(p => new SpotifyItemReference(p.Id, p.Name, p.Owner.DisplayName, p.Tracks.Total.ToString()))
            };
        }

        public async Task<SpotifyPageItemReference> GetPopularPlaylistsAsync(string userId, int limit, int offset, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading popular playlists {offset + 1} to {offset + limit} for user {userId}");
            var playlistData = await _client.GetTopPlayLists(limit, offset, cancellation).ThenAsync(p => p.Playlists);
            return new SpotifyPageItemReference
            {
                HasNext = !string.IsNullOrEmpty(playlistData.Next),
                Next = playlistData.Next,
                HasPrev = !string.IsNullOrEmpty(playlistData.Previous),
                Previous = playlistData.Previous,
                Limit = playlistData.Limit,
                Offset = playlistData.Offset,
                Total = playlistData.Total,
                Items = playlistData.Items.Select(p => new SpotifyItemReference(p.Id, p.Name, p.Owner.DisplayName, p.Tracks.Total.ToString()))
            };
        }

        public async Task<SpotifyPageItemReference> GetUserTopArtistsAsync(string userId, int limit, int offset, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading top artists {offset + 1} to {offset + limit} from user {userId}");
            var playlistData = await _client.GetUserTopArtists(limit, offset, cancellation);
            return new SpotifyPageItemReference
            {
                HasNext = !string.IsNullOrEmpty(playlistData.Next),
                Next = playlistData.Next,
                HasPrev = !string.IsNullOrEmpty(playlistData.Previous),
                Previous = playlistData.Previous,
                Limit = playlistData.Limit,
                Offset = playlistData.Offset,
                Total = playlistData.Total,
                Items = playlistData.Items.Select(p => new SpotifyItemReference(p.Id, p.Name, p.Uri))
            };
        }

        public async Task<SpotifyPageItemReference> GetPlaylistTracksAsync(string playlistId, int limit, int offset, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading tracks {offset + 1} to {offset + limit} from playlist {playlistId}");
            var tracks = await _client.GetTracksFromPlaylist(playlistId, limit, offset, cancellation);
            return new SpotifyPageItemReference
            {
                HasNext = !string.IsNullOrEmpty(tracks.Next),
                Next = tracks.Next,
                HasPrev = !string.IsNullOrEmpty(tracks.Previous),
                Previous = tracks.Previous,
                Limit = tracks.Limit,
                Offset = tracks.Offset,
                Total = tracks.Total,
                Items = tracks.Items.Select(t => new SpotifyItemReference(t.Track.Uri, t.Track.Name, string.Join(", ", t.Track.Artists.Select(a => a.Name)), t.Track.Album.Name))
            };
        }

        public async Task<IList<string>> GetAllPlaylistTrackUris(string playlistId, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading all track uris from playlist {playlistId}");
            var offset = 0;
            SpotifyPagedResponse<SpotifyPlaylistTrackResponse> tracks;
            var uriList = new List<string>();
            do
            {
                tracks = await _client.GetTracksFromPlaylist(playlistId, 25, offset, cancellation);
                uriList.AddRange(tracks.Items.Select(i => i.Track.Uri));
                offset += 25;
            } while (!string.IsNullOrEmpty(tracks.Next));

            return uriList;
        }

        public async Task<IList<string>> GetAllUserTopTrackUris(string userId, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading all track uris from user {userId}'s top played tracks");
            var offset = 0;
            SpotifyPagedResponse<SpotifyTrackResponse> tracks;
            var uriList = new List<string>();
            do
            {
                tracks = await _client.GetUserTopTracks(25, offset, cancellation);
                uriList.AddRange(tracks.Items.Select(i => i.Uri));
                offset += 25;
            } while (!string.IsNullOrEmpty(tracks.Next));

            return uriList;
        }

        public async Task<IList<string>> GetArtistTopTrackUris(string artistId, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Reading all track uris from artist {artistId}'s top tracks");
            return await _client.GetArtistTopTracks(artistId, cancellation)
                .ThenAsync(a => a.Tracks.Select(t => t.Uri).ToList());
        }

        public async Task<SpotifyItemReference> CreatePlaylistAsync(string userId, string name, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Creating playlist {name} for user {userId}");
            var result = await _client.PostNewPlaylist(userId, name, cancellation);
            return new SpotifyItemReference(result.Id, result.Name, result.Owner.DisplayName);
        }

        public async Task PostSongsToPlaylistAsync(string playlistId, IEnumerable<string> trackUris, CancellationToken cancellation = default)
        {
            _logger.LazyLog(LogLevel.Information, () => $"Posting {trackUris.Count()} tracks to playlist {playlistId}");
            await _client.PostTrackUriListToPlaylist(playlistId, trackUris, cancellation);
        }
    }
}
