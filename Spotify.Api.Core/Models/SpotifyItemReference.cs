using System.Collections.Generic;

namespace Spotify.Api.Core.Models
{
    public class SpotifyItemReference
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string By { get; set; }
        public string Opt { get; set; }

        public SpotifyItemReference(string id, string name, string by)
        {
            Name = name;
            Id = id;
            By = by;
        }

        public SpotifyItemReference(string id, string name, string by, string opt)
        {
            Name = name;
            Id = id;
            By = by;
            Opt = opt;
        }
    }

    public class SpotifyPageItemReference
    {
        public int Total { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public bool HasNext { get; set; }
        public string Next { get; set; }
        public bool HasPrev { get; set; }
        public string Previous { get; set; }
        public IEnumerable<SpotifyItemReference> Items { get; set; }
    }
}
