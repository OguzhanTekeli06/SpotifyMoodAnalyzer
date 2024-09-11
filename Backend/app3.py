import requests
from flask import Flask, request, redirect, session, url_for
import base64
import json
import os

# Spotify API'ye erişmek için gerekli bilgileri ayarla
CLIENT_ID = 'b0c7121f314b4c0b9559cbcaee0e44dc'
CLIENT_SECRET = 'b3bc4324c34c40a69a93368790f48cc3'
REDIRECT_URI = 'http://localhost:5000/callback'
SCOPE = 'user-read-private user-read-email'

# Spotify API token endpoint
TOKEN_URL = 'https://accounts.spotify.com/api/token'

app = Flask(__name__)
app.secret_key = os.urandom(24)

# Kullanıcıyı Spotify'da oturum açmaya yönlendiren ana endpoint
@app.route('/')
def login():
    auth_url = (
        'https://accounts.spotify.com/authorize?'
        f'client_id={CLIENT_ID}&response_type=code&redirect_uri={REDIRECT_URI}'
        f'&scope={SCOPE}'
    )
    return redirect(auth_url)

# Spotify'dan yetkilendirme kodunu aldıktan sonra token alma işlemi
@app.route('/callback')
def callback():
    code = request.args.get('code')
    auth_str = f"{CLIENT_ID}:{CLIENT_SECRET}"
    b64_auth_str = base64.b64encode(auth_str.encode()).decode()

    headers = {
        'Authorization': f'Basic {b64_auth_str}',
        'Content-Type': 'application/x-www-form-urlencoded'
    }
    data = {
        'grant_type': 'authorization_code',
        'code': code,
        'redirect_uri': REDIRECT_URI
    }

    response = requests.post(TOKEN_URL, headers=headers, data=data)
    response_data = response.json()

    # Tokenleri kaydet
    session['access_token'] = response_data['access_token']
    return redirect(url_for('profile'))

# Kullanıcının profil bilgilerini almak için Spotify API'yi çağıran endpoint
@app.route('/profile')
def profile():
    access_token = session.get('access_token')
    if not access_token:
        return redirect(url_for('login'))

    headers = {
        'Authorization': f'Bearer {access_token}'
    }
    profile_url = 'https://api.spotify.com/v1/me'
    response = requests.get(profile_url, headers=headers)
    profile_data = response.json()

    # Profil bilgilerini döndür veya işle
    return json.dumps(profile_data, indent=2)

if __name__ == '__main__':
    app.run(debug=True)
