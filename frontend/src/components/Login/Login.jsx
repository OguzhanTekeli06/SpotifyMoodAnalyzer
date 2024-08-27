import React from 'react';
import './Login.css';  // CSS dosyasını içe aktarıyoruz

const Login = () => {
    const loginUrl = "http://localhost:5000/login";

    return (
        <div className="login-container">
            <h1>Login with Spotify</h1>
            <a href={loginUrl}>
                <button>Login with Spotify</button>
            </a>
        </div>
    );
}

export default Login;
