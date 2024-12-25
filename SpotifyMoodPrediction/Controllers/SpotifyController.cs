using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;

public class SpotifyController : Controller
{

    


    private readonly string clientId = "446b21c2d6614b12a3c3c919d119927a";
    private readonly string clientSecret = "65ca845d4b684f56ade2eb69eb45d421";
    private readonly string redirectUri = "http://localhost:5262/Spotify/Callback";
    
    // Reuse HttpClient across the class
    private readonly HttpClient _client = new HttpClient();

    public ActionResult Login()
    {
        var spotifyUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope=user-read-recently-played";
        return Redirect(spotifyUrl);
    }

    public async Task<ActionResult> Callback(string code)
{
    if (string.IsNullOrEmpty(code))
    {
        return BadRequest("Spotify'dan geçersiz yetkilendirme kodu döndü.");
    }

    var token = await GetSpotifyToken(code);

    if (string.IsNullOrEmpty(token))
    {
        return BadRequest("Spotify'dan token alınamadı.");
    }

    HttpContext.Session.SetString("SpotifyToken", token);

    // Örnek bir şarkı ID'si
    var trackId = "49Iygx392WIEaP29lq7FOR"; // Gerçek bir trackId kullanılabilir
    var audioFeatures = await GetAudioFeatures(trackId);

    if (audioFeatures != null)
    {
        // Kullanıcıya audio özelliklerini göstermek için bir View döndürelim.
        ViewBag.AudioFeatures = audioFeatures;
        return View("AudioFeatures");
    }

    return BadRequest("Audio özellikleri alınamadı.");
}


    public async Task<ActionResult> GetRecentlyPlayed()
    {
        var token = HttpContext.Session.GetString("SpotifyToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=10");
        var responseContent = await response.Content.ReadAsStringAsync();

        var songs = new List<dynamic>();

        try
        {
            var jsonDocument = JsonDocument.Parse(responseContent);
            foreach (var item in jsonDocument.RootElement.GetProperty("items").EnumerateArray())
            {
                var track = item.GetProperty("track");
                var trackId = track.GetProperty("id").GetString();
                var name = track.GetProperty("name").GetString();
                var artist = track.GetProperty("artists")[0].GetProperty("name").GetString();

                // Get the audio features for the song
                var audioFeatures = await GetAudioFeatures(trackId);

                songs.Add(new
                {
                    Name = name,
                    Artist = artist,
                    AudioFeatures = audioFeatures ?? new { Danceability = 0, Energy = 0, Tempo = 0, Loudness = 0, Valence = 0 }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("JSON parse hatası: " + ex.Message);
            return View("Error");
        }

        ViewBag.RecentlyPlayed = songs;
        return View("RecentlyPlayed");
    }

   


    








    private async Task<dynamic?> GetAudioFeatures(string trackId)
    {
        var token = HttpContext.Session.GetString("SpotifyToken");

        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"https://api.spotify.com/v1/audio-features/{trackId}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        try
        {
            var jsonDocument = JsonDocument.Parse(responseContent);
            var features = jsonDocument.RootElement;

            return new
            {
                Danceability = features.GetProperty("danceability").GetDouble(),
                Energy = features.GetProperty("energy").GetDouble(),
                Tempo = features.GetProperty("tempo").GetDouble(),
                Loudness = features.GetProperty("loudness").GetDouble(),
                Valence = features.GetProperty("valence").GetDouble()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error parsing audio features: " + ex.Message);
            return null;
        }
    }

    private async Task GetAndDisplayAudioFeatures()
{
    var trackId = "11dFghVXANMlKmJXsNCbNl"; // Şarkı ID'si
    var audioFeatures = await GetAudioFeatures(trackId);

    if (audioFeatures != null)
    {
        Console.WriteLine("Audio Features:");
        Console.WriteLine($"Danceability: {audioFeatures.Danceability}");
        Console.WriteLine($"Energy: {audioFeatures.Energy}");
        Console.WriteLine($"Tempo: {audioFeatures.Tempo}");
        Console.WriteLine($"Loudness: {audioFeatures.Loudness}");
        Console.WriteLine($"Valence: {audioFeatures.Valence}");
    }
    else
    {
        Console.WriteLine("Audio features could not be retrieved.");
    }
}





    private async Task<string?> GetSpotifyToken(string code)
{
    var client = new HttpClient();
    var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

    var tokenRequestBody = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", redirectUri),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
    });

    var response = await client.PostAsync("https://accounts.spotify.com/api/token", tokenRequestBody);
    var responseContent = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        var jsonDocument = JsonDocument.Parse(responseContent);
        if (jsonDocument.RootElement.TryGetProperty("access_token", out var accessToken) && accessToken.ValueKind != JsonValueKind.Null)
        {
            return accessToken.GetString() ?? string.Empty;
        }
    }

    // Log error for better debugging
    Console.WriteLine($"Error: {response.StatusCode}, Message: {responseContent}");
    return null;
}

}
