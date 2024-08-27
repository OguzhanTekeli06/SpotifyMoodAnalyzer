import os

class Config:
    SECRET_KEY = os.getenv("SECRET_KEY", "supersecretkey")
    SPOTIFY_CLIENT_ID = os.getenv("SPOTIFY_CLIENT_ID", "buraya yazın")
    SPOTIFY_CLIENT_SECRET = os.getenv("SPOTIFY_CLIENT_SECRET", "buraya yazın")
    SESSION_COOKIE_NAME = "spotify-login-session"
