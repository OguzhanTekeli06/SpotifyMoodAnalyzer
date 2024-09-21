import json
import os
import tensorflow as tf
import pandas as pd
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout
from tensorflow.keras.callbacks import EarlyStopping
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler


# Veriyi JSON dosyasından yükleyip pandas DataFrame'e dönüştürme fonksiyonu
def load_data(filepath):
    with open(filepath, 'r') as file:
        data = json.load(file)

    # JSON formatındaki 'audio_features' listesini normalize edip DataFrame'e dönüştürüyoruz
    df = pd.json_normalize(data['audio_features'])

    # Kullanmayacağımız sütunları kaldırıyoruz
    df.drop(['analysis_url', 'track_href', 'type', 'uri', 'id'], axis=1, inplace=True)

    return df


# Valence değerini üç kategoriye ayırma fonksiyonu
def categorize_valence(df):
    # Kategoriler arası sınırlar
    bins = [0, 0.33, 0.66, 1]

    # Kategoriler
    labels = ['Sad', 'Neutral', 'Happy']

    # Valence değerini kategorilere ayırıyoruz ve 'mood' adında yeni bir sütun oluşturuyoruz
    df['mood'] = pd.cut(df['valence'], bins=bins, labels=labels)

    return df


# Özellikler ve hedefi ayırma fonksiyonu
def prepare_data(df):
    # Valence ve mood sütunlarını ayırıyoruz
    X = df.drop(['valence', 'mood'], axis=1)  # Özellik sütunları (X)
    y = pd.get_dummies(df['mood'])  # Mood kategorilerini one-hot encoding'e çeviriyoruz (y)

    # Eğitim ve test setlerine ayırma
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

    return X_train, X_test, y_train, y_test


# Veriyi normalleştirme fonksiyonu
def normalize_data(X_train, X_test):
    scaler = StandardScaler()
    X_train_scaled = scaler.fit_transform(X_train)
    X_test_scaled = scaler.transform(X_test)
    return X_train_scaled, X_test_scaled


# Modeli oluşturma ve derleme fonksiyonu
def build_model(input_shape):
    model = Sequential([
        Dense(64, activation='relu', input_shape=(input_shape,)),
        Dropout(0.5),  # Dropout ekliyoruz
        Dense(32, activation='relu'),
        Dropout(0.5),  # Dropout ekliyoruz
        Dense(16, activation='relu'),
        Dense(3, activation='softmax')  # Çıkış katmanı (3 sınıf için)
    ])

    # Modeli derliyoruz
    model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])

    return model


# Modeli eğitme ve değerlendirme fonksiyonu
def train_and_evaluate_model(model, X_train, y_train, X_test, y_test):
    # Early stopping ekliyoruz
    early_stopping = EarlyStopping(monitor='val_loss', patience=3)

    # Modeli eğitiyoruz
    model.fit(X_train, y_train, epochs=50, validation_data=(X_test, y_test), verbose=1, callbacks=[early_stopping])

    model.save_weights('model_weights.h5')


    # Modeli test verisi üzerinde değerlendiriyoruz
    loss, accuracy = model.evaluate(X_test, y_test)
    print(f'Test Loss: {loss}, Test Accuracy: {accuracy}')

    # Tahminler
    predictions = model.predict(X_test)
    print(predictions[:5])  # İlk 5 tahmini yazdırıyoruz



# Ağırlıkları yükleyip tahmin yapma fonksiyonu                   sonradan eklendi

def load_and_predict(model, X_test):
    if os.path.exists('model_weights.h5'):
        # Modelin ağırlıkları varsa yükleyin
        model.load_weights('model_weights.h5')
        print("Ağırlıklar yüklendi, tahmin yapılıyor.")
        predictions = model.predict(X_test)
        print(predictions[:5])  # İlk 5 tahmini yazdırıyoruz
    else:
        print("Ağırlık dosyası bulunamadı. Modeli eğitmeniz gerek.")


# Ana işlev
def main():
    # Veri dosyasının tam yolu
    filepath = 'C:/Users/ouzte/Desktop/homedata/deneme.json'

    # Veriyi yükle
    df = load_data(filepath)

    # Valence değerini kategorilere ayır
    df = categorize_valence(df)

    # Veriyi hazırla
    X_train, X_test, y_train, y_test = prepare_data(df)

    # Veriyi normalize et
    X_train, X_test = normalize_data(X_train, X_test)

    # Modeli oluştur
    model = build_model(X_train.shape[1])

    # Modeli eğit ve değerlendir
    train_and_evaluate_model(model, X_train, y_train, X_test, y_test)


if __name__ == '__main__':
    main()
