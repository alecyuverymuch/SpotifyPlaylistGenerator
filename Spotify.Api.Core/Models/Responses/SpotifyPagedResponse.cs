using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyPagedResponse<T>
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("items")]
        public IEnumerable<T> Items { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("previous")]
        public string Previous { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class SpotifyPagedPlaylistsResponse<T>
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("playlists")]
        public SpotifyPagedResponse<T> Playlists { get; set; }
    }
}
