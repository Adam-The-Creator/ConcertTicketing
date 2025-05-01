import { useEffect, useState } from 'react';
import './css/App.css';

function App() {
    return (
        <div className="App">
            <header className="navbar">
                <div className="navbar-left">
                    <a href="#" className="brand-link nav-button nav-button-brand"> 
                        <img src="./src/assets/tickets_icon.png" alt="Logo" className="logo" width="30px"/>
                        <span className="brand">Concert Ticketing</span>
                    </a>
                </div>
                <nav className="navbar-right">
                    <div className="nav-button">Concerts and Events</div>
                    <div className="nav-button">Festivals</div>
                    <div className="nav-button">Login</div>
                </nav>
            </header>

            <main id="main" className="main-content">
                <h1>Welcome to Concert Ticketing</h1>
            </main>

            <footer className="footer">
                <a href="#">About</a>
            </footer>
        </div>
    );
}

export default App;