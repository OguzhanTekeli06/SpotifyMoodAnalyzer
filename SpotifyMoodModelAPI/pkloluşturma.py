import pickle
import pandas as pd
import json
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report


# Eğitim verisi hazırlama fonksiyonu
def prepare_training_data(json_data):
    # JSON verisini normalize edip özellikleri ve etiketleri ayırıyoruz
    df = pd.json_normalize(json_data['audio_features'])

    # Gerekli özellikleri ve etiketleri seçiyoruz
    X = df[['valence', 'energy', 'danceability', 'loudness', 'tempo', 'liveness',
            'acousticness', 'speechiness', 'instrumentalness']].values
    y = df['mood'].map({
        'Neşeli ve Hareketli': 0,
        'Hareketli': 1,
        'Sakin ve Mutlu': 2,
        'Melankolik': 3,
        'Düşük Enerji': 4,
        'Karmaşık Ruh Hali': 5
    })

    # NaN değerleri olan satırları çıkarıyoruz
    valid_indices = y.notna()
    X = X[valid_indices]
    y = y[valid_indices].values

    return train_test_split(X, y, test_size=0.2, random_state=42)


# JSON dosyasını yükleyelim
with open("C:/Users/ouzte/Desktop/veri bilimi yedek/audio_features_with_mood.json", "r", encoding="utf-8") as file:
    json_data = json.load(file)

# Modeli eğitme ve kaydetme
X_train, X_test, y_train, y_test = prepare_training_data(json_data)
clf = RandomForestClassifier(n_estimators=100, random_state=42)
clf.fit(X_train, y_train)

# Modeli değerlendirip çıktı alalım
y_pred = clf.predict(X_test)
print(classification_report(y_test, y_pred))

# Modeli .pkl dosyasına kaydedelim
with open('mood_prediction_model.pkl', 'wb') as f:
    pickle.dump(clf, f)
