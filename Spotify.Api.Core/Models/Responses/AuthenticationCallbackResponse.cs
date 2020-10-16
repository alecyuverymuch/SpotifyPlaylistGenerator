namespace Spotify.Api.Core.Models.Responses
{
    public class AuthenticationCallbackResponse
    {
        public string Error { get; set; }
        public string State { get; set; }
        public string Code { get; set; }
    }
}
