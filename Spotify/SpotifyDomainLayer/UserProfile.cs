using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyDomainLayer
{
    public class UserProfile
    {
        public string? DisplayName { get; set; }
        public string? Country { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Id { get; set; } // Kullanıcının Spotify ID'si
        public string? Uri { get; set; } // Kullanıcının Spotify URI'si
        public string? Href { get; set; } // Kullanıcı kaynağının URL'si
        public Dictionary<string, string>? ExternalUrls { get; set; } // Dış bağlantılar (ör. Spotify profil URL'si)
        public string? Product { get; set; } // Spotify abonelik türü
        public int? Followers { get; set; } // Takipçi sayısı
    }
}
