using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyAlbumResponse : SpotifyBaseResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("album_group")]
        public string AlbumGroup { get; set; }

        [JsonProperty("album_type")]
        public string AlbumType { get; set; }

        [JsonProperty("artists")]
        public IEnumerable<SpotifyArtistResponse> Artists { get; set; }

        [JsonProperty("available_markets")]
        public IEnumerable<string> AvailableMarkets { get; set; }

        [JsonProperty("images")]
        public JArray Images { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("release_date_precision")]
        public string ReleaseDatePrecision { get; set; }
    }
}
