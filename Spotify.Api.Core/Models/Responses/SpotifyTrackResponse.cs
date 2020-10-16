using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyTrackResponse : SpotifyBaseResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("album")]
        public SpotifyAlbumResponse Album { get; set; }

        [JsonProperty("artists")]
        public IEnumerable<SpotifyArtistResponse> Artists { get; set; }

        [JsonProperty("available_markets")]
        public IEnumerable<string> AvailableMarkets { get; set; }

        [JsonProperty("disc_number")]
        public ushort DiscNumber { get; set; }

        [JsonProperty("duration_ms")]
        public long DurationMs { get; set; }

        [JsonProperty("explicit")]
        public bool Explicit { get; set; }

        [JsonProperty("episode")]
        public bool Episode { get; set; }

        [JsonProperty("external_ids")]
        public KeyValuePair<string, string> ExternalIds { get; set; }

        [JsonProperty("is_local")]
        public bool IsLocal { get; set; }

        [JsonProperty("popularity")]
        public ushort Popularity { get; set; }

        [JsonProperty("preview_url")]
        public string PreviewUrl { get; set; }

        [JsonProperty("track_number")]
        public ushort TrackNumber { get; set; }
    }

    public class SpotifyTracklistResponse
    {
        [JsonProperty("tracks")]
        public IEnumerable<SpotifyTrackResponse> Tracks { get; set; }
    }
}
