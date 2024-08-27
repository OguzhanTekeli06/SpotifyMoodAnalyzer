from flask import session
from authlib.integrations.flask_client import OAuth

oauth = OAuth()

def get_user_data():
    token = session.get('token')
    if token:
        resp = oauth.spotify.get('https://api.spotify.com/v1/me', token=token)
        return resp.json()
    return None
