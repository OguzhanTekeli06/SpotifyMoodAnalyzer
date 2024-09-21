import json
import pandas as pd
from flask import Flask, redirect, url_for, session, request, jsonify
from authlib.integrations.flask_client import OAuth
from flask_cors import CORS
from Model.denem import build_model, categorize_valence, normalize_data, prepare_data
from config import Config
import os
import requests
import sys
sys.path.append('C:/Users/ouzte/Desktop/spotifyproject/Backend')




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
    token = spotify.authorize_access_token()
    session['token'] = token  # Tüm token objesini session'a kaydediyoruz

    user_data = get_user_data(token)
    if user_data:
        session['user'] = user_data
        response = redirect('http://localhost:3000/dashboard')
        response.set_cookie('spotify_token', token['access_token'], httponly=False)
        return response
    else:
        return jsonify({"error": "Failed to get user data"}), 401




@app.route('/process-audio-features', methods=['POST'])
def process_audio_features():
    # Frontend'den gelen JSON verisini alıyoruz
    audio_features = request.json

    # Audio özelliklerini pandas DataFrame'e dönüştürüyoruz
    df = pd.json_normalize(audio_features)

    # Modeli yüklemek ve tahmin yapmak için deneme.py'deki kodu kullanıyoruz
    df = categorize_valence(df)  # Eğer kategorize etme gerekiyorsa
    
    # Girdi verilerini hazırlıyoruz
    X_train, X_test, y_train, y_test = prepare_data(df)  # Eğitim yerine tüm veriyi kullanın
    X_train_scaled, X_test_scaled = normalize_data(X_train, X_test)

    # Modeli oluşturup eğittikten sonra, eğitilmiş modelini bir yerden yükleyebilirsin
    model = build_model(X_train.shape[1])
    
    # Eğitilmiş modelin ağırlıklarını yükleyin
    model.load_weights('model_weights.h5')  # Model ağırlıklarını doğru yoldan yüklediğinden emin ol

    # Tahmin yapıyoruz
    predictions = model.predict(X_test_scaled)
    
    # Model sonuçlarını JSON formatında döndür
    return jsonify(predictions.tolist())



















@app.route('/dashboard')
def dashboard():
    auth_header = request.headers.get('Authorization')
    if auth_header:
        # "Bearer <token>" formatından token'ı alıyoruz
        token = auth_header.split(" ")[1]
        print(f"Backend'de alınan token: {token}")

        # Session'daki token ile karşılaştırıyoruz
        session_token = session.get('token', {}).get('access_token')
        if session_token and token == session_token:
            user = session.get('user')
            if user:
                return jsonify(user)  # Kullanıcı verilerini JSON formatında döndürüyoruz
            return jsonify({"error": "User not logged in"}), 401
        else:
            return jsonify({"error": "Invalid token"}), 401
    else:
        return jsonify({"error": "Authorization header missing"}), 401

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
