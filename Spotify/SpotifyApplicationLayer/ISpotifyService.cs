using SpotifyDomainLayer;
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

    Task<List<Song>> GetRecentlyPlayed(string token);

    //Task<List<dynamic>> GetRecentlyPlayed(string token);
}
