import React, { useState } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';

export default function AddArtist({ goBack }) {
    const [artistName, setArtistName] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            await axios.post(`${DOMAIN}/api/artists`, { artistName });
            alert("Artist added successfully.");
            goBack();
        } catch (err) {
            setError('Failed to add artist.');
            console.error(err);
        }
    };

    return (
        <div className="add-artist">
            <h2>Add Artist</h2>
            <form onSubmit={handleSubmit}>
                <label>
                    Artist Name
                    <input
                        type="text"
                        value={artistName}
                        onChange={(e) => setArtistName(e.target.value)}
                        required
                    />
                </label>
                {error && <p style={{ color: 'red' }}>{error}</p>}
                <button type="submit">Add Artist</button>
                <button type="button" onClick={goBack}>Cancel</button>
            </form>
        </div>
    );
}
