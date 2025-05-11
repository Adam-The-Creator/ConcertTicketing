import { useState } from 'react';
import { DOMAIN } from './Utils';

function SignIn({ onSwitch, onSuccess }) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const res = await fetch(`${DOMAIN}/api/auth/signin`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Email: email, Password: password }),
            });
            if (res.ok) {
                const data = await res.json();
                setMessage(`Welcome, ${data.Username || data.username}!`);
                onSuccess(data);
            } else {
                const err = await res.text();
                setMessage(`Error: ${err}`);
            }
        } catch (ex) {
            setMessage('Network error, please try again.');
            console.log(ex);
        }
    };

    return (
        <div className="auth-container">
            <div className="auth-card">
                <h2 className="auth-title">Sign In</h2>

                <form className="auth-form" onSubmit={handleSubmit}>
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
                        Login
                    </button>
                </form>

                {message && <div className="auth-message">{message}</div>}

                <div className="auth-switch">
                    Don't have an account?{' '}
                    <button className="auth-link" onClick={onSwitch}>
                        Sign Up
                    </button>
                </div>
            </div>
        </div>
    );
}

export default SignIn;
