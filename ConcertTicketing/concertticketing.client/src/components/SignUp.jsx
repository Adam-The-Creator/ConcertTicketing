import { useState } from 'react';
import { DOMAIN } from './Utils';

function SignUp({ onSwitch }) {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const res = await fetch(`${DOMAIN}/api/auth/signup`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Username: username, Email: email, Password: password }),
            });
            if (res.ok) {
                setMessage('Signup successful! You can now sign in.');
                setTimeout(() => {
                    onSwitch();
                }, 2500);
            } else {
                const err = await res.text();
                setMessage(`Error: ${err}`);
            }
        } catch {
            setMessage('Network error, please try again.');
        }
    };

    return (
        <div className="auth-container">
            <div className="auth-card">
                <h2 className="auth-title">Sign Up</h2>

                <form className="auth-form" onSubmit={handleSubmit}>
                    <div className="auth-field">
                        <label htmlFor="username">Username</label>
                        <input
                            id="username"
                            type="text"
                            placeholder="Your username"
                            value={username}
                            onChange={e => setUsername(e.target.value)}
                            required
                        />
                    </div>

                    <div className="auth-field">
                        <label htmlFor="email">Email</label>
                        <input
                            id="email"
                            type="email"
                            placeholder="you@example.com"
                            value={email}
                            onChange={e => setEmail(e.target.value)}
                            required
                        />
                    </div>

                    <div className="auth-field">
                        <label htmlFor="password">Password</label>
                        <input
                            id="password"
                            type="password"
                            placeholder="password"
                            value={password}
                            onChange={e => setPassword(e.target.value)}
                            required
                        />
                    </div>

                    <button className="auth-submit" type="submit">
                        Sign Up
                    </button>
                </form>

                {message && <div className="auth-message">{message}</div>}

                <div className="auth-switch">
                    Already have an account?{' '}
                    <button className="auth-link" onClick={onSwitch}>
                        Sign In
                    </button>
                </div>
            </div>
        </div>
    );
}

export default SignUp;
