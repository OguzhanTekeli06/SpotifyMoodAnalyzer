using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class SpotifyController : Controller
{
    private readonly string clientId = "b0c7121f314b4c0b9559cbcaee0e44dc";
    private readonly string clientSecret = "b3bc4324c34c40a69a93368790f48cc3";
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

        // Token'ı session'a kaydediyoruz
        HttpContext.Session.SetString("SpotifyToken", token);

        // Token alındıktan sonra ResultPage sayfasına yönlendirelim
        return RedirectToAction("SendAudioFeaturesToModel", "Spotify");
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

        var response = await client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=30");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Gelen veriyi parse ederek şarkı listesi çıkarın ve View'a gönderin.
        ViewBag.Songs = responseContent;
        return View();
    }







    public async Task<ActionResult> GetRecentlyPlayedAudioFeatures()
    {
        var token = HttpContext.Session.GetString("SpotifyToken");

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login"); // Eğer token yoksa tekrar girişe yönlendir
        }

        // Son dinlenen şarkıları alalım
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=30");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Şarkı ID'lerini parse edelim
        var jsonDocument = JsonDocument.Parse(responseContent);
        var trackIds = new List<string>();

        foreach (var item in jsonDocument.RootElement.GetProperty("items").EnumerateArray())
        {
            var trackId = item.GetProperty("track").GetProperty("id").GetString();
            if (!string.IsNullOrEmpty(trackId))
            {
                trackIds.Add(trackId);
            }
        }

        // Eğer şarkı ID'leri alındıysa audio özelliklerini çekelim
        if (trackIds.Count > 0)
        {
            // Şarkı ID'lerini virgülle ayırarak formatlayalım
            var trackIdsParam = string.Join(",", trackIds);

            // Audio özelliklerini alalım
            var audioFeaturesResponse = await client.GetAsync($"https://api.spotify.com/v1/audio-features?ids={trackIdsParam}");
            var audioFeaturesContent = await audioFeaturesResponse.Content.ReadAsStringAsync();

            // Gelen veriyi View'a gönderelim
            ViewBag.AudioFeatures = audioFeaturesContent;
        }
        else
        {
            ViewBag.AudioFeatures = "No tracks found.";
        }

        return View("AudioFeaturesResult");
    }


    public async Task<ActionResult> SendAudioFeaturesToModel()
{
    var token = HttpContext.Session.GetString("SpotifyToken");

    if (string.IsNullOrEmpty(token))
    {
        return RedirectToAction("Login"); // Eğer token yoksa tekrar girişe yönlendirin
    }

    // Son dinlenen şarkıların audio_features'larını alalım
    var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    var response = await client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=30");
    var responseContent = await response.Content.ReadAsStringAsync();

    var jsonDocument = JsonDocument.Parse(responseContent);
    var trackIds = new List<string>();

    foreach (var item in jsonDocument.RootElement.GetProperty("items").EnumerateArray())
    {
        var trackId = item.GetProperty("track").GetProperty("id").GetString();
        if (!string.IsNullOrEmpty(trackId))
        {
            trackIds.Add(trackId);
        }
    }

    if (trackIds.Count > 0)
    {
        var trackIdsParam = string.Join(",", trackIds);
        var audioFeaturesResponse = await client.GetAsync($"https://api.spotify.com/v1/audio-features?ids={trackIdsParam}");
        var audioFeaturesContent = await audioFeaturesResponse.Content.ReadAsStringAsync();

        // Audio_features verilerini modele gönderelim (API ile)
        var modelClient = new HttpClient();
        var modelResponse = await modelClient.PostAsync("http://localhost:5000/model/predict", new StringContent(audioFeaturesContent, Encoding.UTF8, "application/json"));
        var modelResult = await modelResponse.Content.ReadAsStringAsync();

        // Model sonucunu mood result sayfasına yönlendirelim
        ViewBag.ModelResult = modelResult;
        return View("MoodResultPage");
    }

    return View("Error");
}
















    public async Task<ActionResult> ResultPage()
    {
        var token = HttpContext.Session.GetString("SpotifyToken");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Session'da token bulunamadı.");
            return RedirectToAction("Login"); // Token yoksa tekrar giriş yapmaya yönlendirin.
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://api.spotify.com/v1/me/player/recently-played?limit=30");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Gelen yanıtı loglayalım ve View'a gönderelim
        Console.WriteLine("Spotify API Yanıtı: " + responseContent);
        ViewBag.SpotifyData = responseContent;

        return View();
    }








    private async Task<string?> GetSpotifyToken(string code)
    {
        var client = new HttpClient();
        var tokenRequestBody = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", redirectUri),  // http://localhost:5262/Spotify/Callback
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
    });

        var response = await client.PostAsync("https://accounts.spotify.com/api/token", tokenRequestBody);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Yanıtı loglayalım
        Console.WriteLine("Spotify Token Yanıtı: " + responseContent);

        if (response.IsSuccessStatusCode)
        {
            // Token'ı JSON'dan parse edelim
            var jsonDocument = JsonDocument.Parse(responseContent);
            if (jsonDocument.RootElement.TryGetProperty("access_token", out var accessToken ) && accessToken.ValueKind != JsonValueKind.Null)
            {
                 return accessToken.GetString() ?? string.Empty;
            }
        }

        return null;  // Başarısız olursa null döner
    }

}
