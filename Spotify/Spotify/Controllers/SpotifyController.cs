﻿using Microsoft.AspNetCore.Mvc;
using SpotifyApplicationLayer;

namespace Spotify.Controllers
{
    public class SpotifyController : Controller
    {
        private readonly ISpotifyService _spotifyService;

        public SpotifyController(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }

        public IActionResult Login()
        {
            var loginUrl = _spotifyService.GetLoginUrl();
            return Redirect(loginUrl);
        }

        public async Task<IActionResult> Callback(string code)
        {
            var token = await _spotifyService.GetSpotifyToken(code);
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Spotify'dan token alınamadı.");
            }

            HttpContext.Session.SetString("SpotifyToken", token);
            return RedirectToAction("GetRecentlyPlayed");
        }

        public async Task<IActionResult> GetRecentlyPlayed()
        {
            var token = HttpContext.Session.GetString("SpotifyToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var songs = await _spotifyService.GetRecentlyPlayed(token);
            return View(songs);
        }
    }
}
