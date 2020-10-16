using Spotify.Api.Core.Enums;

namespace Spotify.Api.Core.Models
{
    public class PlaylistProfileEntry
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Count { get; set; }
        public SourceTypes Type { get; set; }
    }
}
