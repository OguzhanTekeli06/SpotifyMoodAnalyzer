using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyApplicationLayer;

public class SpotifyService : ISpotifyService
{
    private readonly string clientId = "446b21c2d6614b12a3c3c919d119927a";
    private readonly string clientSecret = "65ca845d4b684f56ade2eb69eb45d421";
    private readonly string redirectUri = "http://localhost:5271/Spotify/Callback";
    private readonly HttpClient _client;

    public SpotifyService()
    {
        _client = new HttpClient();
    }

    public string GetLoginUrl()
    {
        return $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=user-read-recently-played";
    }

    public async Task<string?> GetSpotifyToken(string code)
    {
        var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var tokenRequestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        var response = await _client.PostAsync("https://accounts.spotify.com/api/token", tokenRequestBody);
        if (response.IsSuccessStatusCode)
        {
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("access_token").GetString();
        }
        return null;
    }

    //public async Task<List<dynamic>> GetRecentlyPlayed(string token)
    //{
    //    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    //    var response = await _client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=10");
    //    var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    //    var songs = new List<dynamic>();

    //    foreach (var item in json.RootElement.GetProperty("items").EnumerateArray())
    //    {
    //        var track = item.GetProperty("track");
    //        songs.Add(new
    //        {
    //            Name = track.GetProperty("name").GetString(),
    //            Artist = track.GetProperty("artists")[0].GetProperty("name").GetString()
    //        });
    //    }
    //    return songs;
    //}



}
