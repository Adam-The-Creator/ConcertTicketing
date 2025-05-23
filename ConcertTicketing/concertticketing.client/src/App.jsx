import { useState, useEffect, useRef } from 'react';
import SignIn from './components/SignIn';
import SignUp from './components/SignUp';
import AdminContent from './components/AdminContent';
import UpcomingEvents from './components/UpcomingEvents';
import Concerts from './components/Concerts';
import EventDetails from './components/EventDetails';
import Profile from './components/Profile';
import EditEvent from './components/EditEvent';
import './css/App.css';
import './css/AuthForm.css';
import './css/ProfileDropdown.css';
import './css/UpcomingEvents.css';
import './css/EventDetails.css';
import './css/AdminContent.css';

function App() {
    const [authView, setAuthView] = useState('home');
    const [userData, setUserData] = useState(null);
    const [profileDropdownOpen, setProfileDropdownOpen] = useState(false);
    const [selectedEvent, setSelectedEvent] = useState(null);
    const [editEvent, setEditEvent] = useState(null);
    const profileRef = useRef(null);

    useEffect(() => {
        const storedUserCredentials = localStorage.getItem('user');
        if (storedUserCredentials) {
            try {
                const parsed = JSON.parse(storedUserCredentials);
                setUserData(parsed);
            } catch {
                localStorage.removeItem('user');
            }
        }
    }, []);

    useEffect(() => {
        function handleClickOutsideProfileDropdown(e) {
            if (profileRef.current && !profileRef.current.contains(e.target)) {
                setProfileDropdownOpen(false);
            }
        }
        document.addEventListener('mousedown', handleClickOutsideProfileDropdown);
        return () => document.removeEventListener('mousedown', handleClickOutsideProfileDropdown);
    }, []);

    const handleLoginSuccess = (data) => {
        setUserData(data);
        localStorage.setItem('user', JSON.stringify(data));
        setTimeout(() => {
            if (data.roleName === 'Admin') {
                setAuthView('admin');
            } else {
                setAuthView('home');
            }
        }, 2500);
    };

    const handleLogout = () => {
        setUserData(null);
        localStorage.removeItem('user');
        setProfileDropdownOpen(false);
        setAuthView('home');
    };

    return (
        <div className="App">
            <header className="navbar">
                <div className="navbar-left">
                    <a href="#" className="brand-link nav-button nav-button-brand" onClick={(e) => {
                        e.preventDefault();
                        setAuthView('home');
                        setSelectedEvent(null);
                    }}> 
                        <img src="./src/assets/tickets_icon.png" alt="Logo" className="logo" width="30px"/>
                        <span className="brand">Concert Ticketing</span>
                    </a>
                </div>
                <nav className="navbar-right">
                    {userData && userData.roleName === 'Admin' && (
                        <div className="nav-button" onClick={() => {
                            setAuthView('admin');
                            setSelectedEvent(null);
                        }}>
                            Dashboard
                        </div>
                    )}
                    <div className="nav-button" onClick={() => {
                        setAuthView('home');
                        setSelectedEvent(null);
                    }}>Concerts and Events</div>
                    <div className="nav-button">Festivals</div>
                    {userData ? (
                        <div className="nav-button profile-container" ref={profileRef}>
                            <div className="profile-button" onClick={() => setProfileDropdownOpen(open => !open)}>
                                <img src="./src/assets/user_icon.png" alt={userData.username} className="user-icon" width="30px" />
                            </div>

                            {profileDropdownOpen && (
                                <div className="profile-dropdown">
                                    <button className="dropdown-item" onClick={() => {
                                        setAuthView('profile');
                                        setProfileDropdownOpen(false);
                                    }}>
                                        Profile
                                    </button>
                                    <button className="dropdown-item" onClick={handleLogout}>Logout</button>
                                </div>
                            )}
                        </div>
                    ) : (
                        <div className="nav-button" onClick={() => setAuthView('signin')}>Login</div>
                    )}
                </nav>
            </header>

            <main id="main" className="main-content">
                {authView === 'signin' ? (
                    <SignIn onSwitch={() => setAuthView('signup')} onSuccess={handleLoginSuccess} />
                ) : authView === 'signup' ? (
                    <SignUp onSwitch={() => setAuthView('signin')} />
                ) : authView === 'profile' ? (
                    <Profile />
                ) : editEvent ? (
                    <EditEvent eventData={editEvent} goBack={() => setEditEvent(null)} />
                ) : selectedEvent ? (
                    <EventDetails event={selectedEvent} onBack={() => setSelectedEvent(null)} isAdmin={userData?.roleName === 'Admin'} onEdit={() => setEditEvent(selectedEvent)} />
                ) : authView === 'admin' ? (
                    <AdminContent />
                ) : (
                    <>
                        <UpcomingEvents onSelectEvent={setSelectedEvent} />
                        {/*<Concerts />*/}
                    </>
                )}
            </main>

            <footer className="footer">
                <a href="#">About</a>
            </footer>
        </div>
    );
}

export default App;