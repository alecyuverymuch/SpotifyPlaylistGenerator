using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyUserResponse : SpotifyBaseResponse
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("followers")]
        public JObject Followers { get; set; }

        [JsonProperty("images")]
        public JArray Images { get; set; }
    }
}
