using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class SpotifyController : Controller
{
    private readonly string clientId = "446b21c2d6614b12a3c3c919d119927a";
    private readonly string clientSecret = "65ca845d4b684f56ade2eb69eb45d421";
    private readonly string redirectUri = "http://localhost:5262/Spotify/Callback";

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
        return RedirectToAction("GetRecentlyPlayed", "Spotify");
    }

    public async Task<ActionResult> GetRecentlyPlayed()
    {
        var token = HttpContext.Session.GetString("SpotifyToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=10");
        var responseContent = await response.Content.ReadAsStringAsync();

        var songs = new List<dynamic>();

        try
        {
            var jsonDocument = JsonDocument.Parse(responseContent);
            foreach (var item in jsonDocument.RootElement.GetProperty("items").EnumerateArray())
            {
                var track = item.GetProperty("track");
                var name = track.GetProperty("name").GetString();
                var artist = track.GetProperty("artists")[0].GetProperty("name").GetString();

                songs.Add(new
                {
                    Name = name,
                    Artist = artist
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

        return null;
    }
}
