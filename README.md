## 🎶 Spotify Mood Prediction with ASP.NET Core MVC & Flask API
This ASP.NET Core MVC application integrates with the Spotify API to fetch audio features of recently played tracks. It sends these features to a Flask API hosting a machine learning model, which predicts the user's mood (e.g., happy, sad) based on listening habits.

## 🔧 Features
Spotify API Integration: Retrieves audio features like valence, energy, and tempo.
Mood Prediction: Uses a Flask API model to predict emotional states.
Interactive Dashboard: Displays mood insights on the web interface.
## 📁 Project Structure
Model: Flask API hosts the machine learning model for mood prediction.
View: ASP.NET Core MVC views display user mood predictions and data.
Controller: Manages data flow between Spotify, Flask API, and the frontend.
## 🚀 Getting Started
Prerequisites
.NET SDK
Python & Flask
Spotify Developer Account


Clone the Repository
```
git clone https://github.com/OguzhanTekeli06/SpotifyMoodAnalyzer
```

Configure Spotify API. Set up Client ID and Secret in the app configuration.From your spotifyapi app.

```
python ModelApi.py
```

Start project
```
dotnet run
```


Usage
Log in via Spotify to fetch recent audio data.
The app displays your mood based on audio features.

## 🤖 Model Details
The Flask API model processes audio features for mood predictions using machine learning.

## 📜 License
Licensed under MIT.
