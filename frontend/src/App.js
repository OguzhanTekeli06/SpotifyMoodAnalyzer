import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import Login from './components/Login/Login.jsx';  
import MoodResults from './components/Results/MoodResults.jsx';
import Dashboard from './components/Dashboard/dashboard.jsx';



function App() {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/Dashboard" element={<Dashboard />} />
                <Route path="/MoodResults" element={<MoodResults />} />
            </Routes>
        </Router>
    );
}

export default App;
