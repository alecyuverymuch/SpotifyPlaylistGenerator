using Newtonsoft.Json;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyArtistResponse : SpotifyBaseResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
