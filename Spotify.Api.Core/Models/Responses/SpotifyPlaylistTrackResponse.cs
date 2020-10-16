using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spotify.Api.Core.Models.Responses
{
    public class SpotifyPlaylistTrackResponse
    {
        [JsonProperty("added_at")]
        public DateTime AddedAt { get; set; }

        [JsonProperty("added_by")]
        public SpotifyBaseResponse AddedBy { get; set; }

        [JsonProperty("is_local")]
        public bool IsLocal { get; set; }

        [JsonProperty("primary_color")]
        public string PrimaryColor { get; set; }

        [JsonProperty("track")]
        public SpotifyTrackResponse Track { get; set; }

        [JsonProperty("video_thumbnail")]
        public KeyValuePair<string, string> VideoThumbnail { get; set; }
    }
}
