import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';  // useNavigate hook'u ile yönlendirme yapacağız


const Dashboard = () => {
    const [userData, setUserData] = useState(null);  // Kullanıcı verilerini tutmak için state
    const navigate = useNavigate();  // useNavigate hook'u ile yönlendirme fonksiyonunu çağırıyoruz

    useEffect(() => {
        const fetchUserData = async () => {
            try {
                const response = await axios.get('http://localhost:5000/dashboard');
                console.log(response.data);  // Veriyi konsolda görmek için
                setUserData(response.data);  // Kullanıcı verilerini state'e kaydet
            } catch (error) {
                console.error("Error fetching user data", error);
            }
        };
    
        fetchUserData();  // API çağrısı: Kullanıcı verileri için
    }, []);
    

    // Yönlendirme işlemi için buton tıklama fonksiyonu
    const handleButtonClick = () => {
        navigate('/MoodResults');  // /results sayfasına yönlendir
    };

    return (
        <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
            <h2>User Product Information</h2>
            {userData && userData.product ? (  // Sadece "product" özelliğini kontrol ediyoruz
                <div>
                    <p><strong>Product:</strong> {userData.product}</p>  {/* product bilgisi */}
                </div>
            ) : (
                <p>Loading user product information...</p>
            )}

            {/* Buton ekliyoruz */}
            <button onClick={handleButtonClick} style={{ marginTop: '20px', padding: '10px 20px', backgroundColor: 'green', color: 'white' }}>
                Go to Results Page
            </button>
        </div>
    );
}

export default Dashboard;
