﻿using SpotifyDomainLayer;
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
    private readonly HttpClient _client = new HttpClient();

    public SpotifyService()
    {
        _client = new HttpClient();
    }

    public string GetLoginUrl()
    {
        return $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=user-read-recently-played user-read-private user-read-email user-modify-playback-state user-read-currently-playing";
    }
    public async Task<string?> GetSpotifyToken(string code)
    {
        var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var tokenRequestBody = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", redirectUri),
        new KeyValuePair<string, string>("client_id", clientId), // client_id burada gerekli değil ama Spotify API dökümantasyonunda bazen eklenmesi beklenebilir.
        new KeyValuePair<string, string>("client_secret", clientSecret), // Burada client_secret de genellikle gereklidir.
    });

        try
        {
            // Spotify'a token talep gönderiyoruz
            var response = await _client.PostAsync("https://accounts.spotify.com/api/token", tokenRequestBody);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Başarılı ise access_token'ı döndür
            if (response.IsSuccessStatusCode)
            {
                var jsonDocument = JsonDocument.Parse(responseContent);
                if (jsonDocument.RootElement.TryGetProperty("access_token", out var accessToken) && accessToken.ValueKind != JsonValueKind.Null)
                {
                    return accessToken.GetString() ?? string.Empty;
                }
            }

            // Başarısız olduğunda hata logunu al
            Console.WriteLine($"Error: {response.StatusCode}, Message: {responseContent}");
        }
        catch (Exception ex)
        {
            // Hata durumunda loglama yapılacak
            Console.WriteLine($"Exception occurred: {ex.Message}");
        }

        return null;
    }

    public async Task<List<Song>> GetRecentlyPlayed(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=10");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Spotify API'sinden veriler alınırken bir hata oluştu.");
        }

        var songs = new List<Song>();

        try
        {
            var jsonDocument = JsonDocument.Parse(responseContent);
            foreach (var item in jsonDocument.RootElement.GetProperty("items").EnumerateArray())
            {
                var track = item.GetProperty("track");
                var name = track.GetProperty("name").GetString();
                var artist = track.GetProperty("artists")[0].GetProperty("name").GetString();

                songs.Add(new Song
                {
                    Name = name,
                    Artist = artist
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("JSON parse hatası: " + ex.Message);
            throw new Exception("Spotify'dan alınan veriler işlenirken bir hata oluştu.");
        }

        return songs;
    }




    public async Task<UserProfile?> GetUserProfile(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await _client.GetAsync("https://api.spotify.com/v1/me");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error fetching user profile: {response.StatusCode}, {responseContent}");
                return null;
            }

            var jsonDocument = JsonDocument.Parse(responseContent);
            var userProfile = new UserProfile
            {
                DisplayName = jsonDocument.RootElement.GetProperty("display_name").GetString(),
                Country = jsonDocument.RootElement.GetProperty("country").GetString(),
                Email = jsonDocument.RootElement.GetProperty("email").GetString(),
                ProfilePictureUrl = jsonDocument.RootElement.TryGetProperty("images", out var images) && images.GetArrayLength() > 0
                ? images[0].GetProperty("url").GetString()
                : null,
                Product = jsonDocument.RootElement.GetProperty("product").GetString(),
                Followers = jsonDocument.RootElement.TryGetProperty("followers", out var followers)
                ? followers.GetProperty("total").GetInt32()
                : 0,
                
            };

            return userProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while fetching user profile: {ex.Message}");
            return null;
        }
    }

    public async Task PausePlayback(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await _client.PutAsync("https://api.spotify.com/v1/me/player/pause", null);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Playback paused successfully.");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error pausing playback: {response.StatusCode}, {responseContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while pausing playback: {ex.Message}");
        }
    }


    public async Task SkipToNext(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("https://api.spotify.com/v1/me/player/next", null);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Bir sonraki şarkıya geçilemedi.");
        }
    }

    public async Task SkipToPrevious(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("https://api.spotify.com/v1/me/player/previous", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Spotify API'sinden şarkı geçişi yapılırken bir hata oluştu.");
        }
    }

    public async Task<Song?> GetCurrentlyPlayingTrack(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("https://api.spotify.com/v1/me/player/currently-playing");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Spotify API'sinden çalan şarkı alınırken bir hata oluştu.");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        try
        {
            if (string.IsNullOrEmpty(responseContent) || responseContent == "null")
            {
                return null; // Eğer şu anda çalan bir şarkı yoksa, null dönebiliriz.
            }

            var jsonDocument = JsonDocument.Parse(responseContent);
            var track = jsonDocument.RootElement.GetProperty("item");
            var name = track.GetProperty("name").GetString();
            var artist = track.GetProperty("artists")[0].GetProperty("name").GetString();

            return new Song
            {
                Name = name,
                Artist = artist
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("JSON parse hatası: " + ex.Message);
            throw new Exception("Spotify'dan alınan veriler işlenirken bir hata oluştu.");
        }
    }


    //public async Task<List<Playlist>> GetPlaylists(string token)
    //{
    //    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    //    var response = await _client.GetAsync("https://api.spotify.com/v1/me/playlists");
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        throw new Exception("Error fetching playlists from Spotify API.");
    //    }

    //    var content = await response.Content.ReadAsStringAsync();
    //    var jsonDocument = JsonDocument.Parse(content);

    //    var playlists = new List<Playlist>();
    //    foreach (var item in jsonDocument.RootElement.GetProperty("items").EnumerateArray())
    //    {
    //        var name = item.GetProperty("name").GetString();
    //        var uri = item.GetProperty("external_urls").GetProperty("spotify").GetString();
    //        var description = item.GetProperty("description").GetString();
    //        var imageUrl = item.GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString();

    //        playlists.Add(new Playlist
    //        {
    //            Name = name,
    //            Uri = uri,
    //            Description = description,
    //            ImageUrl = imageUrl
    //        });
    //    }

    //    return playlists;
    //}

    public async Task<string?> GetDeviceId(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("https://api.spotify.com/v1/me/player/devices");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Cihazlar alınırken bir hata oluştu.");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        var devices = jsonDocument.RootElement.GetProperty("devices").EnumerateArray();
        foreach (var device in devices)
        {
            var isActive = device.GetProperty("is_active").GetBoolean();
            if (isActive)
            {
                return device.GetProperty("id").GetString();
            }
        }

        return null; // Eğer aktif cihaz yoksa null döndür
    }

    public async Task SetVolume(string token, int volume)
    {
        if (volume < 0 || volume > 100)
            throw new ArgumentOutOfRangeException("Volume must be between 0 and 100.");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var deviceId = await GetDeviceId(token);
        if (deviceId == null)
            throw new Exception("Aktif cihaz bulunamadı.");

        var response = await _client.PutAsync($"https://api.spotify.com/v1/me/player/volume?volume_percent={volume}&device_id={deviceId}", null);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error setting volume: {response.StatusCode}, {responseContent}");
        }
    }


}
