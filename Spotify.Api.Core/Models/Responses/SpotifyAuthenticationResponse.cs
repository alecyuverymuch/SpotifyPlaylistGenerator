using System;
using Newtonsoft.Json;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyAuthenticationResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("expires_in")]
        public ushort ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("token_generated")]
        public DateTime TokenGenerated { get; set; }

        [JsonProperty("can_access_personal_data")]
        public bool CanAccessPersonalData { get; internal set; }
    }
}
