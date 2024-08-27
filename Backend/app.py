from flask import Flask, redirect, url_for, session, request, jsonify
from authlib.integrations.flask_client import OAuth
from flask_cors import CORS
import os
import requests

app = Flask(__name__)
CORS(app)  # CORS'u etkinleştirme

app.secret_key = os.getenv("SECRET_KEY", "supersecretkey")
app.config.from_object('config.Config')

oauth = OAuth(app)
spotify = oauth.register(
    name='spotify',
    client_id="d610110ac6f147a29bd2e59f37183640",
    client_secret="fc9af4646d264a218adc367fe4944d45",
    access_token_url='https://accounts.spotify.com/api/token',
    authorize_url='https://accounts.spotify.com/authorize',
    authorize_params=None,
    redirect_uri='http://localhost:5000/authorize',
    client_kwargs={'scope': 'user-read-email user-read-private user-top-read user-read-recently-played'}
)

@app.route('/')
def home():
    return 'Welcome to Spotify Mood Analyzer'

@app.route('/login')
def login():
    return spotify.authorize_redirect(redirect_uri='http://localhost:5000/authorize')

@app.route('/authorize')
def authorize():
    token = spotify.authorize_access_token()
    session['user'] = spotify.get('https://api.spotify.com/v1/me', token=token).json()
    session['token'] = token['access_token']
    return redirect('/dashboard')

@app.route('/dashboard')
def dashboard():
    user = session.get('user')
    if user:
        return jsonify(user)
    return redirect('/')

@app.route('/recent-tracks-audio-features')
def recent_tracks_audio_features():
    token = session.get('token')
    if not token:
        return redirect('/login')
    
    headers = {
        'Authorization': f'Bearer {token}'
    }

    # En son dinlenen şarkıları al
    recent_tracks_response = requests.get('https://api.spotify.com/v1/me/player/recently-played?limit=10', headers=headers)
    if recent_tracks_response.status_code != 200:
        return jsonify({"error": "Failed to fetch recently played tracks"}), recent_tracks_response.status_code
    
    recent_tracks = recent_tracks_response.json()
    track_ids = [item['track']['id'] for item in recent_tracks['items']]

    # Müzikal özellikleri al
    audio_features_response = requests.get(f'https://api.spotify.com/v1/audio-features?ids={",".join(track_ids)}', headers=headers)
    if audio_features_response.status_code != 200:
        return jsonify({"error": "Failed to fetch audio features"}), audio_features_response.status_code
    
    audio_features = audio_features_response.json()
    return jsonify(audio_features)


if __name__ == '__main__':
    app.run(debug=True)
