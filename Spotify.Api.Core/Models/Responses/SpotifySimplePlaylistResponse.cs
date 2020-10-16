using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifySimplePlaylistResponse : SpotifyBaseResponse
    {
        [JsonProperty("collaborative")]
        public bool Collaborative { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("followers", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JObject Followers { get; set; }

        [JsonProperty("images")]
        public JArray Images { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("owner")]
        public SpotifyUserResponse Owner { get; set; }

        [JsonProperty("snapshot_id")]
        public string SnapshotId { get; set; }

        [JsonProperty("tracks")]
        public TracksObject Tracks { get; set; }
    }

    public class TracksObject
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
