import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';
import "../css/AddArtist.css";

export default function AddArtist({ goBack }) {
    const [artistName, setArtistName] = useState('');
    const [selectedGenres, setSelectedGenres] = useState([]);
    const [allGenres, setAllGenres] = useState([]);
    const [error, setError] = useState('');

    useEffect(() => {
        async function fetchGenres() {
            try {
                const res = await axios.get(`${DOMAIN}/api/genres`);
                setAllGenres(res.data);
            } catch (err) {
                console.error("Failed to fetch genres:", err);
                setAllGenres([]);
            }
        }

        fetchGenres();
    }, []);

    const handleGenreSelect = (e) => {
        const selectedId = parseInt(e.target.value);
        if (selectedId && !selectedGenres.includes(selectedId)) {
            setSelectedGenres([...selectedGenres, selectedId]);
        }
    };

    const removeGenre = (id) => {
        setSelectedGenres(selectedGenres.filter(g => g !== id));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            await axios.post(`${DOMAIN}/api/artists`, {
                artistName,
                genreIds: selectedGenres
            });
            alert("Artist added successfully.");
            goBack();
        } catch (err) {
            setError('Failed to add artist.');
            console.error(err);
        }
    };

    return (
        <div className="add-artist-container">
            <h2>Add Artist</h2>
            <form onSubmit={handleSubmit} className="add-artist-form">
                <label>
                    Artist Name:
                    <input
                        type="text"
                        value={artistName}
                        onChange={(e) => setArtistName(e.target.value)}
                        required
                    />
                </label>

                <label>
                    Select Genre:
                    <select onChange={handleGenreSelect} defaultValue="">
                        <option value="" disabled>-- Choose a genre --</option>
                        {allGenres.map(genre => (
                            <option key={genre.id} value={genre.id}>{genre.genreName}</option>
                        ))}
                    </select>
                </label>

                {selectedGenres.length > 0 && (
                    <div className="selected-genres">
                        <p>Selected Genres:</p>
                        <ul>
                            {selectedGenres.map(id => {
                                const genre = allGenres.find(g => g.id === id);
                                return (
                                    <li key={id}>
                                        {genre?.genreName}
                                        <button type="button" onClick={() => removeGenre(id)}>✕</button>
                                    </li>
                                );
                            })}
                        </ul>
                    </div>
                )}

                {error && <p className="error">{error}</p>}

                <div className="form-buttons">
                    <button type="submit">Add Artist</button>
                    <button type="button" onClick={goBack}>Cancel</button>
                </div>
            </form>
        </div>
    );
}