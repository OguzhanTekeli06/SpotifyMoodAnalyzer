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
    }
}
