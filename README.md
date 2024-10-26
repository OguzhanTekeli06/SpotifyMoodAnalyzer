# 🎶 Spotify Mood Prediction with MVC & Flask API
This project is a web application built using the MVC (Model-View-Controller) architecture that utilizes the Spotify API to fetch audio features of the user’s recently played tracks. These audio features are then sent to a Flask-based machine learning model that predicts the user’s mood (e.g., happy, sad). The mood prediction is displayed on the web interface.

## 🔧 Features
Spotify API Integration: Fetches audio features like energy, valence, tempo, and more for recent tracks.
Mood Prediction: Uses a machine learning model served via Flask to determine the user's mood.
User-Friendly Interface: Displays mood predictions on the screen, giving insights into emotional trends.
## 📁 Project Structure
Model: The ML model is hosted on a Flask API, which takes audio features as input and returns a mood prediction.
View: Displays mood information on a user interface built with HTML and JavaScript.
Controller: Manages Spotify API requests and coordinates data flow between the view and model.
## 🚀 Getting Started
Prerequisites
Python 3.x
Flask
Spotify Developer Account
Installation
Clone the Repository

bash
Copy code
git clone https://github.com/username/spotify-mood-detection.git
cd spotify-mood-detection
Install Dependencies

bash
Copy code
pip install -r requirements.txt
Set Up Spotify API Credentials

Go to the Spotify Developer Dashboard and create an application.
Note your Client ID and Client Secret.
Add these to your project’s configuration.
Run Flask API Model

bash
Copy code
cd model_api
flask run
Run the MVC Application

bash
Copy code
python app.py
Usage
Log in with Spotify to allow the app to fetch your recent listening history.
The app sends audio features to the Flask API, which processes and returns a mood prediction.
View your mood prediction on the dashboard.
## 📊 API and Model Details
Spotify API: Collects audio_features data such as energy, valence, tempo, etc.
Flask Model API: The model uses these features to make mood predictions.
Endpoints:
/predict: Takes audio features as JSON and returns a mood label.
## 🤖 Model Training (Optional)
The machine learning model was trained on a dataset of audio features and mood labels. If you wish to improve or retrain the model:

Prepare your data in a CSV file with audio features and corresponding mood labels.
Use train_model.py to train a new model.
## 📜 License
This project is licensed under the MIT License.
