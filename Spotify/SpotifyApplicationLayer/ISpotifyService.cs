using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyApplicationLayer;

public interface ISpotifyService
{
    string GetLoginUrl();
    Task<string?> GetSpotifyToken(string code);
    //Task<List<dynamic>> GetRecentlyPlayed(string token);
}
