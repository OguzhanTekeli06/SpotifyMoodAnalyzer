from flask import Flask, request, jsonify
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report

# Flask uygulaması
app = Flask(__name__)


# JSON verisinden verileri yükleyen ve işleyen fonksiyon
def load_data_from_json(json_data):
    df = pd.json_normalize(json_data['audio_features'])

    # Gerekli özellikleri seçelim
    X = df[['valence', 'energy', 'danceability', 'loudness', 'tempo', 'liveness',
            'acousticness', 'speechiness', 'instrumentalness']].values

    # 'mood' etiketlerini ekleyelim veya yükleyelim
    y = df['mood'] if 'mood' in df.columns else None

    # Veriyi eğitim ve test seti olarak ayıralım
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

    return X_train, X_test, y_train, y_test


# Model eğitme fonksiyonu
def train_model(X_train, y_train):
    clf = RandomForestClassifier(n_estimators=100, random_state=42)
    clf.fit(X_train, y_train)
    return clf


# Model değerlendirme fonksiyonu
def evaluate_model(clf, X_test, y_test):
    y_pred = clf.predict(X_test)
    report = classification_report(y_test, y_pred, output_dict=True)
    return report


# Flask API'ye gelen isteklerle JSON verisini işleyen endpoint
@app.route('/model/train', methods=['POST'])
def train():
    json_data = request.json
    X_train, X_test, y_train, y_test = load_data_from_json(json_data)

    # Modeli eğit
    clf = train_model(X_train, y_train)

    # Modeli değerlendirelim
    report = evaluate_model(clf, X_test, y_test)

    # Tahmin sonuçlarını JSON olarak döndürelim
    return jsonify({'classification_report': report})


# Flask API üzerinden tahmin yapmak için endpoint
@app.route('/model/predict', methods=['POST'])
def predict():
    json_data = request.json

    # JSON verisini işleyelim (sadece özellikleri alalım)
    df = pd.json_normalize(json_data['audio_features'])
    X = df[['valence', 'energy', 'danceability', 'loudness', 'tempo', 'liveness',
            'acousticness', 'speechiness', 'instrumentalness']].values

    # Model eğitme (örnek verilerle model eğitiyoruz)
    clf = RandomForestClassifier(n_estimators=100, random_state=42)
    clf.fit(X, [0, 1, 2, 3, 4])  # Beş mood kategorisi için örnek etiketler

    # Tahmin yapalım
    predictions = clf.predict(X)

    # Tahmin sonuçlarını mood etiketlerine dönüştürelim
    mood_labels = {
        0: "Neşeli ve Hareketli",
        1: "Hareketli",
        2: "Sakin ve Mutlu",
        3: "Melankolik",
        4: "Düşük Enerji"
    }
    mood_prediction = [mood_labels[pred] for pred in predictions]

    return jsonify({'predictions': mood_prediction})


if __name__ == '__main__':
    app.run(debug=True)
