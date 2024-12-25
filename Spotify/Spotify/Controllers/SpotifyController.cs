using Microsoft.AspNetCore.Mvc;
using SpotifyApplicationLayer;

namespace Spotify.Controllers
{
    public class SpotifyController : Controller
    {
        private readonly ISpotifyService _spotifyService;



        private readonly HttpClient _client;

        

        






        public SpotifyController(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }



        public IActionResult Login2()
        {
            return View("Login", "Spotify");
        }



        public IActionResult Login()
        {
            var Spotifyurl = _spotifyService.GetLoginUrl();
            return Redirect(Spotifyurl);
        }

        public async Task<IActionResult> Callback(string code)
        {
            var token = await _spotifyService.GetSpotifyToken(code);
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Spotify'dan token alınamadı.");
            }

            HttpContext.Session.SetString("SpotifyToken", token);
            return RedirectToAction("GetRecentlyPlayed", "Spotify");
        }

        public async Task<IActionResult> GetRecentlyPlayed()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                var songs = await _spotifyService.GetRecentlyPlayed(token);
                return View(songs);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                Console.WriteLine("ulaaaaaaaaaaaa");
                return View("Error");
            }
        }

        

        public async Task<IActionResult> getdeneme()
        {
            var sonuc = "Bu, deneme sonucudur.";

            // View'e sonuca göndermek
            return View("Sonuc", sonuc);
        }
    }
}
