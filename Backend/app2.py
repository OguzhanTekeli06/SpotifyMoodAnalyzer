from flask import Flask, redirect, request, session, jsonify, url_for
from flask_cors import CORS
import requests
from urllib.parse import urlencode
import os
from config import Config

app = Flask(__name__)
app.secret_key = os.getenv("SECRET_KEY", "super_secret_key")

# Spotify App credentials
CLIENT_ID = Config.SPOTIFY_CLIENT_ID
CLIENT_SECRET = Config.SPOTIFY_CLIENT_SECRET
REDIRECT_URI = "http://localhost:5000/callback"

# CORS Ayarı: Sadece React frontend'e izin veriyoruz
CORS(app, resources={r"/*": {"origins": "http://localhost:3000"}})

# Spotify OAuth URLs
SPOTIFY_AUTH_URL = "https://accounts.spotify.com/authorize"
SPOTIFY_TOKEN_URL = "https://accounts.spotify.com/api/token"
SPOTIFY_API_BASE_URL = "https://api.spotify.com/v1/"
SPOTIFY_SCOPE = "user-read-email user-read-private user-top-read user-read-recently-played"


@app.route("/")
def home():
    return "Welcome to the Spotify OAuth Flow Tutorial"


# 1. Kullanıcıyı Spotify'ın OAuth yetkilendirme sayfasına yönlendirme
@app.route("/login")
def login():
    auth_url_params = {
        "client_id": CLIENT_ID,
        "response_type": "code",
        "redirect_uri": REDIRECT_URI,
        "scope": SPOTIFY_SCOPE,
        "show_dialog": True
    }
    url = f"{SPOTIFY_AUTH_URL}?{urlencode(auth_url_params)}"
    return redirect(url)


# 2. Spotify'dan yetkilendirme kodunu alıp, access token isteği yapma
@app.route("/callback")
def callback():
    code = request.args.get("code")
    token_data = {
        "grant_type": "authorization_code",
        "code": code,
        "redirect_uri": REDIRECT_URI,
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET
    }
    token_response = requests.post(SPOTIFY_TOKEN_URL, data=token_data)
    token_json = token_response.json()

    # Access token ve refresh token'ı session'a kaydet
    session["access_token"] = token_json.get("access_token")
    session["refresh_token"] = token_json.get("refresh_token")

    return redirect('http://localhost:3000/dashboard')


# 3. Dashboard: Kullanıcı bilgilerini ve Spotify verilerini çekme
@app.route("/dashboard")
def dashboard():
    access_token = session.get("access_token")
    if access_token is None:
        return redirect(url_for("login"))

    headers = {"Authorization": f"Bearer {access_token}"}
    
    # Kullanıcı bilgilerini çek
    user_profile_response = requests.get(f"{SPOTIFY_API_BASE_URL}me", headers=headers)
    user_profile = user_profile_response.json()
    
    return jsonify(user_profile)


# 4. En son dinlenen şarkıların müzikal özelliklerini al
@app.route("/recent-tracks-audio-features")
def recent_tracks_audio_features():
    access_token = session.get("access_token")
    if not access_token:
        return redirect(url_for("login"))

    headers = {"Authorization": f"Bearer {access_token}"}

    # En son dinlenen şarkıları al
    recent_tracks_response = requests.get(f"{SPOTIFY_API_BASE_URL}me/player/recently-played?limit=10", headers=headers)
    if recent_tracks_response.status_code != 200:
        return jsonify({"error": "Failed to fetch recently played tracks"}), recent_tracks_response.status_code

    recent_tracks = recent_tracks_response.json()
    track_ids = [item['track']['id'] for item in recent_tracks['items']]

    # Müzikal özellikleri al
    audio_features_response = requests.get(f'{SPOTIFY_API_BASE_URL}audio-features?ids={",".join(track_ids)}', headers=headers)
    if audio_features_response.status_code != 200:
        return jsonify({"error": "Failed to fetch audio features"}), audio_features_response.status_code

    audio_features = audio_features_response.json()
    return jsonify(audio_features)


if __name__ == "__main__":
    app.run(debug=True)
