import json
from collections import Counter

from flask import Flask, request, jsonify, Response
import numpy as np
import pandas as pd
import pickle
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
import os

# Flask uygulaması
app = Flask(__name__)

# Model dosya ismi
MODEL_PATH = 'mood_prediction_model.pkl'

# JSON verisinden verileri yükleyen ve işleyen fonksiyon
def load_data_from_json(json_data):
    # JSON verisini normalize edelim (veriyi tabloya çevirelim)
    df = pd.json_normalize(json_data['audio_features'])

    # Gerekli özellikleri seçelim (diğer sütunları çıkarıyoruz)
    X = df[['valence', 'energy', 'danceability', 'loudness', 'tempo', 'liveness',
            'acousticness', 'speechiness', 'instrumentalness']].values

    # 'mood' etiketlerini ekleyelim veya yükleyelim (eğer JSON'da mood sütunu varsa)
    y = df['mood'] if 'mood' in df.columns else None

    # Veriyi eğitim ve test seti olarak ayıralım
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

    return X_train, X_test, y_train, y_test


# Model eğitme ve kaydetme fonksiyonu
def train_and_save_model(X_train, y_train):
    clf = RandomForestClassifier(n_estimators=100, random_state=42)
    clf.fit(X_train, y_train)
    # Modeli .pkl dosyasına kaydedelim
    with open(MODEL_PATH, 'wb') as f:
        pickle.dump(clf, f)
    return clf


# Model yükleme fonksiyonu
def load_model():
    if os.path.exists(MODEL_PATH):
        with open(MODEL_PATH, 'rb') as f:
            clf = pickle.load(f)
        return clf
    else:
        return None


# Model değerlendirme fonksiyonu
def evaluate_model(clf, X_test, y_test):
    y_pred = clf.predict(X_test)
    report = classification_report(y_test, y_pred, output_dict=True)
    return report


# Flask API'ye gelen isteklerle JSON verisini işleyen endpoint
@app.route('/model/train', methods=['POST'])
def train():
    # JSON'dan gelen veriyi alalım
    json_data = request.json

    # JSON verisini işleyelim
    X_train, X_test, y_train, y_test = load_data_from_json(json_data)

    # Modeli eğit ve kaydet
    clf = train_and_save_model(X_train, y_train)

    # Modeli değerlendirelim
    report = evaluate_model(clf, X_test, y_test)

    # Tahmin sonuçlarını JSON olarak döndürelim
    return jsonify({'classification_report': report})


# JSON verisinden sadece tahmin yapmak için (model eğitmeden)
@app.route('/model/predict', methods=['POST'])
def predict():
    # JSON'dan gelen veriyi alalım
    json_data = request.json

    # JSON verisini işleyelim (sadece özellikleri alalım, etiketler yok)
    df = pd.json_normalize(json_data['audio_features'])

    # Gerekli özellikleri seçelim (diğer sütunları çıkarıyoruz)
    X = df[['valence', 'energy', 'danceability', 'loudness', 'tempo', 'liveness',
            'acousticness', 'speechiness', 'instrumentalness']].values

    # Eğitim yapılmadan model tahmini (önceden eğitilmiş modeli yükleyelim)
    clf = load_model()
    if clf is None:
        return jsonify({'error': 'Model bulunamadı. Lütfen önce /model/train endpointine POST isteği göndererek modeli eğitin.'}), 400

    mood_labels = {
        0: 'neşeli ve hareketli',
        1: 'hareketli',
        2: 'sakin ve mutlu',
        3: 'melankolik',
        4: 'düşük enerji',
        5: 'karmaşık ruh hali'
    }

    # Tahmin yapalım
    predictions = clf.predict(X)

    # Tahmin sonucunu döndürelim (örneğin, mood: Happy/Sad)
    mood_prediction = [mood_labels[pred] for pred in predictions]

    most_common_mood = Counter(mood_prediction).most_common(1)[0][0]

    # JSON yanıtını UTF-8 karakter kodlaması ile döndürelim
    response = Response(json.dumps({'most_common_prediction': most_common_mood}, ensure_ascii=False),
                        mimetype='application/json; charset=utf-8')
    response.headers['Content-Type'] = 'application/json; charset=utf-8'
    return response


if __name__ == '__main__':
    app.run(debug=True)
