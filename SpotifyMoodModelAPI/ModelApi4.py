from flask import Flask, request, jsonify
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, classification_report

app = Flask(__name__)


# JSON verisini işleyip özellikleri ve hedefi ayrıştıran fonksiyon
def load_data_from_json(json_data):
    df = pd.json_normalize(json_data['audio_features'])
    features = ['acousticness', 'danceability', 'duration_ms', 'energy',
                'instrumentalness', 'key', 'liveness', 'loudness', 'mode',
                'speechiness', 'tempo', 'valence']
    X = df[features]
    y = df['mood'] if 'mood' in df.columns else None
    return X, y


# Model eğitme endpoint'i
@app.route('/train', methods=['POST'])
def train_model():
    json_data = request.get_json()
    X, y = load_data_from_json(json_data)

    # Eğitim ve test setine ayırma
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

    # Modeli eğitme
    model = RandomForestClassifier(random_state=42)
    model.fit(X_train, y_train)

    # Performans değerlendirme
    y_pred = model.predict(X_test)
    accuracy = accuracy_score(y_test, y_pred)
    report = classification_report(y_test, y_pred, output_dict=True)

    return jsonify({
        "accuracy": accuracy,
        "classification_report": report
    })


# Modelle tahmin yapma endpoint'i
@app.route('/predict', methods=['POST'])
def predict_mood():
    json_data = request.get_json()
    X, _ = load_data_from_json(json_data)  # Sadece özellikleri alıyoruz

    # Geçici model eğitme (gerçek bir model varsa bu kısmı değiştirin)
    model = RandomForestClassifier(random_state=42)
    model.fit(X, [0] * len(X))  # Mood verisi olmayan durum için geçici etiketler kullanılıyor

    # Tahmin yapma
    predictions = model.predict(X)

    return jsonify({
        "predictions": predictions.tolist()
    })


if __name__ == '__main__':
    app.run(debug=True)
