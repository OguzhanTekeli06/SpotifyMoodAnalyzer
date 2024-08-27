import React, { useState, useEffect } from 'react';
import axios from 'axios';

const Dashboard = () => {
    const [audioFeatures, setAudioFeatures] = useState(null);

    useEffect(() => {
        // En son dinlenen şarkıların müzikal özelliklerini al
        const fetchRecentTracksAudioFeatures = async () => {
            try {
                const response = await axios.get('http://localhost:5000/recent-tracks-audio-features');
                setAudioFeatures(response.data);
            } catch (error) {
                console.error("Error fetching recent tracks audio features", error);
            }
        };

        fetchRecentTracksAudioFeatures();
    }, []);

    return (
        <div>
            <h1>Recent Spotify Track Audio Features</h1>

            {audioFeatures && audioFeatures.audio_features ? (
                audioFeatures.audio_features.map((track, index) => (
                    <div key={index}>
                        <h2>Track {index + 1}</h2>
                        <p><strong>Danceability:</strong> {track.danceability}</p>
                        <p><strong>Energy:</strong> {track.energy}</p>
                        <p><strong>Valence:</strong> {track.valence}</p>
                        <p><strong>Tempo:</strong> {track.tempo}</p>
                        <hr />
                    </div>
                ))
            ) : (
                <p>Loading...</p>
            )}
        </div>
    );
}

export default Dashboard;
