from flask import Flask, request, jsonify
import joblib
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
import os

# Flask uygulaması
app = Flask(__name__)

# Model dosya yolu
MODEL_PATH = 'mood_prediction_model.pkl'

# JSON verisinden verileri yükleyen ve işleyen fonksiyon
# JSON verisinden verileri yükleyen ve işleyen fonksiyon
def load_data_from_json(json_data):
    # JSON verisini normalize edelim (veriyi tabloya çevirelim)
    df = pd.json_normalize(json_data['audio_features'])

    # Gerekli 9 özellik seçelim (diğer sütunları çıkarıyoruz)
    features = ['valence', 'energy', 'danceability', 'loudness', 'tempo',
                'liveness', 'acousticness', 'speechiness', 'instrumentalness']
    X = df[features].values

    # 'mood' etiketlerini ekleyelim veya yükleyelim (eğer JSON'da mood sütunu varsa)
    y = df['mood'] if 'mood' in df.columns else None

    # Veriyi eğitim ve test seti olarak ayıralım
    if y is not None:
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
        return X_train, X_test, y_train, y_test
    else:
        return X  # Sadece özellikler döndürülür, tahmin için kullanılacaksa

# Model eğitme işlemi, aynı 9 özellik kullanılarak yapılmalı
# predict() işlevinde de yukarıdaki 9 özellik ile model tahmini yapılabilir.


# Model eğitme fonksiyonu
def train_model(X_train, y_train):
    clf = RandomForestClassifier(n_estimators=100, random_state=42)
    clf.fit(X_train, y_train)
    joblib.dump(clf, MODEL_PATH)  # Modeli .pkl dosyası olarak kaydet
    return clf

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

    # Modeli eğit
    clf = train_model(X_train, y_train)

    # Modeli değerlendirelim
    report = evaluate_model(clf, X_test, y_test)

    # Tahmin sonuçlarını JSON olarak döndürelim
    return jsonify({'classification_report': report, 'message': 'Model trained and saved successfully.'})

# JSON verisinden sadece tahmin yapmak için (model eğitmeden)
@app.route('/model/predict', methods=['POST'])
def predict():
    # Modelin eğitilip kaydedildiğinden emin olun
    if not os.path.exists(MODEL_PATH):
        return jsonify({'error': 'Model not found. Train the model first.'}), 400

    # Kaydedilmiş modeli yükleyin
    clf = joblib.load(MODEL_PATH)

    # JSON'dan gelen veriyi alalım
    json_data = request.json

    # JSON verisini işleyelim (sadece özellikleri alalım)
    df = pd.json_normalize(json_data['audio_features'])

    # Gerekli özellikleri seçelim
    X = df[['valence', 'energy', 'danceability', 'loudness', 'tempo', 'liveness',
            'acousticness', 'speechiness', 'instrumentalness']].values

    # Model ile tahmin yapalım
    predictions = clf.predict(X)

    # Tahmin sonucunu döndürelim (örneğin, mood: Happy/Sad)
    mood_prediction = predictions.tolist()

    return jsonify({'predictions': mood_prediction})

if __name__ == '__main__':
    app.run(debug=True)
