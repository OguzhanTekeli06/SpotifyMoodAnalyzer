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

        public async Task<IActionResult> mainpage()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                var userProfile = await _spotifyService.GetUserProfile(token);

                // Debugging: Profil verisini kontrol et
                Console.WriteLine($"User Profile: {userProfile}");

                if (userProfile == null)
                {
                    ViewBag.ErrorMessage = "Kullanıcı profili alınamadı.";
                    return View();
                }

                return View(userProfile);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Bir hata oluştu: " + ex.Message;
                return View();
            }
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
            return RedirectToAction("mainpage");
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


        public async Task<IActionResult> GetUserProfile()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                var userProfile = await _spotifyService.GetUserProfile(token);

                if (userProfile == null)
                {
                    return Json(new { success = false, message = "asdfasdfasdfasdfasfdasdfasdfas." });
                }

                return Json(new { success = true, data = userProfile });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        public async Task<IActionResult> PausePlayback()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Pause playback API çağrısı
                await _spotifyService.PausePlayback(token);
                return RedirectToAction("MainPage");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Bir hata oluştu: " + ex.Message;
                return View("Error");
            }
        }

        public async Task<IActionResult> SkipToNext()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                // Skip to next track API çağrısı
                await _spotifyService.SkipToNext(token);
                return RedirectToAction("MainPage");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Bir hata oluştu: " + ex.Message;
                return View("Error");
            }
        }



    }
}
