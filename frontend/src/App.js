import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import Login from './components/Login/Login.jsx';  // Yeni yol ile Login bileşenini içe aktarıyoruz
import Dashboard from './components/Dashboard';  // Dashboard için dosya yapısı aynı kalmış

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/dashboard" element={<Dashboard />} />
            </Routes>
        </Router>
    );
}

export default App;
