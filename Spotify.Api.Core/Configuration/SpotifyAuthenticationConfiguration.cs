namespace Spotify.Api.Core.Configuration
{
    public class SpotifyAuthenticationConfiguration
    {
        public string AuthBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
    }
}
