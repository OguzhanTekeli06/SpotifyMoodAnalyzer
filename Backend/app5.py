from flask import Flask, redirect, url_for, session, request, jsonify
from authlib.integrations.flask_client import OAuth
from flask_cors import CORS
from config import Config
import os
import requests
import numpy as np
from your_model_module import load_model, predict_mood  # Model yükleme ve tahmin fonksiyonlarınızı içe aktarın

#model ile yapmaya çalıştığı

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
            return resp.json()
    return None

@app.route('/authorize')
def authorize():
    token = spotify.authorize_access_token()
    session['token'] = token
    user_data = get_user_data(token)

    if user_data:
        session['user'] = user_data
        return redirect('http://localhost:3000/dashboard')
    else:
        return jsonify({"error": "Failed to get user data"}), 401

@app.route('/dashboard')
def dashboard():
    user = session.get('user')
    if user:
        return jsonify(user)
    return jsonify({"error": "User not logged in"}), 401

@app.route('/recent-tracks-audio-features')
def recent_tracks_audio_features():
    token = session.get('token')
    if not token:
        return redirect('/login')
    
    headers = {
        'Authorization': f'Bearer {token["access_token"]}'
    }

    recent_tracks_response = requests.get('https://api.spotify.com/v1/me/player/recently-played?limit=10', headers=headers)
    if recent_tracks_response.status_code != 200:
        return jsonify({"error": "Failed to fetch recently played tracks"}), recent_tracks_response.status_code
    
    recent_tracks = recent_tracks_response.json()
    track_ids = [item['track']['id'] for item in recent_tracks['items']]

    audio_features_response = requests.get(f'https://api.spotify.com/v1/audio-features?ids={",".join(track_ids)}', headers=headers)
    if audio_features_response.status_code != 200:
        return jsonify({"error": "Failed to fetch audio features"}), audio_features_response.status_code
    
    audio_features = audio_features_response.json()
    return jsonify(audio_features)

@app.route('/predict-mood', methods=['POST'])
def predict_mood_endpoint():
    audio_features = request.json  # Ön uçtan gelen ses özelliklerini al

    # Modeli yükle
    model = load_model()  # Model yükleme fonksiyonunu buraya ekleyin

    # Gerekli öznitelikleri seçin ve modelin beklediği biçime getirin
    features = extract_features(audio_features)  # Özellikleri çıkartma fonksiyonunu buraya ekleyin

    # Tahmin yapın
    mood_prediction = predict_mood(model, features)  # Duygu durumu tahmin fonksiyonunu buraya ekleyin

    return jsonify({"predicted_mood": mood_prediction})

if __name__ == '__main__':
    app.run(debug=True)
