from flask import Flask, redirect, url_for, session, request, jsonify
from authlib.integrations.flask_client import OAuth
from flask_cors import CORS
from config import Config
import os
import requests

app = Flask(__name__)
CORS(app)  # CORS'u etkinleştirme

CORS(app, resources={r"/*": {"origins": "http://localhost:3000"}})

app.secret_key = os.getenv("SECRET_KEY", "supersecretkey")
app.config.from_object('config.Config')

oauth = OAuth(app)
spotify = oauth.register(
    name='spotify',
    client_id=Config.SPOTIFY_CLIENT_ID,
    client_secret=Config.SPOTIFY_CLIENT_SECRET,
    access_token_url='https://accounts.spotify.com/api/token',
    authorize_url='https://accounts.spotify.com/authorize',
    authorize_params=None,
    redirect_uri='http://localhost:5000/authorize',
    client_kwargs={'scope': 'user-read-email user-read-private user-top-read user-read-recently-played'},
    # `state` özelliğini kaldırarak Spotify'ın CSRF korumasını kullanmasını sağlıyoruz.
)

@app.route('/')
def home():
    return 'Welcome to Spotify Mood Analyzer'

@app.route('/login')
def login():
    return spotify.authorize_redirect(redirect_uri='http://localhost:5000/authorize')

def get_user_data(token):
    if token:
        headers = {
            'Authorization': f'Bearer {token["access_token"]}'
        }
        resp = requests.get('https://api.spotify.com/v1/me', headers=headers)
        if resp.status_code == 200:
            return resp.json()  # Eğer kullanıcı verileri başarıyla dönerse
    return None

@app.route('/authorize')
def authorize():
    token = spotify.authorize_access_token()  # Tokeni alıyoruz
    session['token'] = token  # Tüm token objesini session'a kaydediyoruz
    user_data = get_user_data(token)  # Kullanıcı verilerini getiriyoruz

    if user_data:
        session['user'] = user_data  # Kullanıcı verilerini session'a kaydediyoruz
        return redirect('http://localhost:3000/dashboard')  # Başarılı oturum açma sonrası yönlendirme
    else:
        return jsonify({"error": "Failed to get user data"}), 401

@app.route('/dashboard')
def dashboard():
    user = session.get('user')
    if user:
        return jsonify(user)  # Kullanıcı verilerini JSON formatında döndürüyoruz
    return jsonify({"error": "User not logged in"}), 401

@app.route('/recent-tracks-audio-features')
def recent_tracks_audio_features():
    token = session.get('token')
    if not token:
        return redirect('/login')
    
    headers = {
        'Authorization': f'Bearer {token["access_token"]}'
    }

    # En son dinlenen şarkıları alıyoruz
    recent_tracks_response = requests.get('https://api.spotify.com/v1/me/player/recently-played?limit=10', headers=headers)
    if recent_tracks_response.status_code != 200:
        return jsonify({"error": "Failed to fetch recently played tracks"}), recent_tracks_response.status_code
    
    recent_tracks = recent_tracks_response.json()
    track_ids = [item['track']['id'] for item in recent_tracks['items']]

    # Müzikal özellikleri alıyoruz
    audio_features_response = requests.get(f'https://api.spotify.com/v1/audio-features?ids={",".join(track_ids)}', headers=headers)
    if audio_features_response.status_code != 200:
        return jsonify({"error": "Failed to fetch audio features"}), audio_features_response.status_code
    
    audio_features = audio_features_response.json()
    return jsonify(audio_features)

if __name__ == '__main__':
    app.run(debug=True)
