using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyBaseResponse
    {
        [JsonProperty("external_urls")]
        public KeyValuePair<string, string> ExternalUrls { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
