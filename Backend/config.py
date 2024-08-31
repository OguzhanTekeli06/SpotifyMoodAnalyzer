import os

class Config:
    SECRET_KEY = os.getenv("SECRET_KEY", "supersecretkey")
    SPOTIFY_CLIENT_ID = os.getenv("SPOTIFY_CLIENT_ID", "b0c7121f314b4c0b9559cbcaee0e44dc")
    SPOTIFY_CLIENT_SECRET = os.getenv("SPOTIFY_CLIENT_SECRET", "b3bc4324c34c40a69a93368790f48cc3")
    SESSION_COOKIE_NAME = "spotify-login-session"
