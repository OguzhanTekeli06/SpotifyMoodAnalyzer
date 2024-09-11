import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

// Cookie'den token'ı alma fonksiyonu
const getCookie = (name) => {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
};

const Dashboard = () => {
    const [results, setResults] = useState(null);
    const navigate = useNavigate();
    const [token, setToken] = useState(null);

    useEffect(() => {
        // Token'ı cookie'den alıyoruz
        const tokenFromCookie = getCookie('spotify_token');
        if (tokenFromCookie) {
            setToken(tokenFromCookie);
            console.log("Token bulundu:", tokenFromCookie);
        } else {
            console.log("Token bulunamadı");
        }
    }, []);

    const handleButtonClick = async () => {
        try {
            // Spotify API'sinden veri çekiyoruz
            const response = await axios.get('http://localhost:5000/recent-tracks-audio-features', {
                headers: { Authorization: `Bearer ${token}` }  // Cookie'den alınan token'ı kullanıyoruz
            });
            const audioFeatures = response.data;

            // Modeli çalıştırmak için backend'e POST isteği gönderiyoruz
            const modelResponse = await axios.post('http://localhost:5000/process-audio-features', audioFeatures);
       
            // Sonuçları ResultsPage'e yönlendiriyoruz
            navigate('/MoodResults', { state: { results: modelResponse.data } });
        } catch (error) {
            console.error('Error processing audio features', error);
        }
    };

    return (
        <div>
            <h1>Dashboard</h1>
            <button onClick={handleButtonClick}>Process Audio Features and Show Results</button>
        </div>
    );
};

export default Dashboard;
