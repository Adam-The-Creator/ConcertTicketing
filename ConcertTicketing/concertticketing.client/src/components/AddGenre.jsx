import React, { useState } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';
import "../css/AddGenre.css";

export default function AddGenre({ goBack }) {
    const [genreName, setGenreName] = useState('');
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');

        if (!genreName.trim()) {
            setError('Genre name is required.');
            return;
        }

        try {
            await axios.post(`${DOMAIN}/api/genres`, { genreName });
            setSuccess('Genre added successfully!');
            setGenreName('');
        } catch (err) {
            console.error(err);
            setError('Failed to add genre.');
        }
    };

    return (
        <div className="add-genre-container">
            <h2>Add Genre</h2>
            <form onSubmit={handleSubmit} className="add-genre-form">
                <label>
                    Genre name:
                    <input
                        type="text"
                        value={genreName}
                        onChange={(e) => setGenreName(e.target.value)}
                        placeholder="e.g., Jazz, Pop, Metal"
                        required
                    />
                </label>

                {error && <p className="error">{error}</p>}
                {success && <p className="success">{success}</p>}

                <div className="form-buttons">
                    <button type="submit">Add Genre</button>
                    <button type="button" onClick={goBack}>Cancel</button>
                </div>
            </form>
        </div>
    );
}
