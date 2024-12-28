using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyDomainLayer
{
    public class Playlist
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Uri { get; set; }
        public string Href { get; set; }
        public Dictionary<string, string> ExternalUrls { get; set; }
        public int TracksCount { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }

    public class SpotifyPlaylistsResponse
    {
        public List<Playlist> Items { get; set; }
    }

}
