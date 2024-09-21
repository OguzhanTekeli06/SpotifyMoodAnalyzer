import React from 'react';
import { useLocation } from 'react-router-dom';

const MoodResults = () => {
    const location = useLocation();
    const { results } = location.state || {};

    return (
        <div>
            <h1>Mod Analizi Sonuçları</h1>
            {results ? (
                <div>
                    {results.map((result, index) => (
                        <div key={index}>
                            <p>{`Parça ${index + 1} için mod: ${result}`}</p>
                        </div>
                    ))}
                </div>
            ) : (
                <p>Sonuç bulunamadı.</p>
            )}
        </div>
    );
};

export default MoodResults;
