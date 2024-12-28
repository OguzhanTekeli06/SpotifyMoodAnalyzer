using Microsoft.AspNetCore.Mvc;
using SpotifyApplicationLayer;
using SpotifyDomainLayer;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;

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
                // Kullanıcı profilini al
                var userProfile = await _spotifyService.GetUserProfile(token);

                // Şu anda çalan şarkıyı al
                var currentlyPlayingTrack = await _spotifyService.GetCurrentlyPlayingTrack(token);
                if (currentlyPlayingTrack == null)
                {
                    ViewBag.CurrentlyPlayingMessage = "Şu anda çalan bir şarkı yok.";
                }
                else
                {
                    ViewBag.CurrentlyPlayingMessage = $"Şu anda çalan şarkı: {currentlyPlayingTrack.Name} - {currentlyPlayingTrack.Artist}";
                }

                // Profil bilgilerini kontrol et
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

        public async Task<IActionResult> SkipToPrevious()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                await _spotifyService.SkipToPrevious(token);
                return RedirectToAction("mainpage");  // İşlem başarılıysa ana sayfaya yönlendir
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");  // Hata olursa error sayfasına yönlendir
            }
        }

        public async Task<IActionResult> GetCurrentlyPlayingTrack()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                var currentlyPlayingTrack = await _spotifyService.GetCurrentlyPlayingTrack(token);

                // Eğer şarkı yoksa kullanıcıya bilgi ver
                if (currentlyPlayingTrack == null)
                {
                    ViewBag.CurrentlyPlayingMessage = "Şu anda çalan bir şarkı yok.";
                }
                else
                {
                    ViewBag.CurrentlyPlayingMessage = $"Şu anda çalan şarkı: {currentlyPlayingTrack.Name} - {currentlyPlayingTrack.Artist}";
                }

                return View("mainpage");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetVolume([FromBody] VolumeModel model)
        {
            if (model == null)
            {
                return BadRequest("Volume model is null.");
            }

            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                await _spotifyService.SetVolume(token, model.VolumePercent);
                return Ok();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Bir hata oluştu: " + ex.Message;
                return View("Error");
            }
        }

        public class VolumeModel
        {
            public int VolumePercent { get; set; }
        }

        //public async Task<IActionResult> SetVolume(int volumePercent)
        //{
        //    var token = HttpContext.Session.GetString("SpotifyToken");
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    try
        //    {
        //        // Ses seviyesini ayarlıyoruz
        //        await _spotifyService.SetVolume(token, volumePercent);
        //        return RedirectToAction("MainPage");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrorMessage = "Bir hata oluştu: " + ex.Message;
        //        return View("Error");
        //    }
        //}




        //public async Task<IActionResult> GetPlaylists()
        //{
        //    var token = HttpContext.Session.GetString("SpotifyToken");
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    try
        //    {
        //        var playlists = await _spotifyService.GetPlaylists(token);
        //        return View(playlists);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrorMessage = ex.Message;
        //        return View("Error");
        //    }
        //}

    }
}
