import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';
import "../css/AddArtist.css";

export default function EditArtist({ goBack }) {
    const [artists, setArtists] = useState([]);
    const [allGenres, setAllGenres] = useState([]);
    const [selectedArtistId, setSelectedArtistId] = useState('');
    const [artistGenres, setArtistGenres] = useState([]);
    const [error, setError] = useState('');

    useEffect(() => {
        async function fetchData() {
            try {
                const [artistRes, genreRes] = await Promise.all([
                    axios.get(`${DOMAIN}/api/artists`),
                    axios.get(`${DOMAIN}/api/genres`)
                ]);
                setArtists(artistRes.data);
                setAllGenres(genreRes.data);
            } catch (err) {
                console.error("Error loading artists or genres:", err);
                setError("Failed to load data.");
            }
        }
        fetchData();
    }, []);

    useEffect(() => {
        if (selectedArtistId) {
            fetchArtistGenres(selectedArtistId);
        }
    }, [selectedArtistId]);

    const fetchArtistGenres = async (artistId) => {
        try {
            const res = await axios.get(`${DOMAIN}/api/artists/${artistId}/genres`);
            setArtistGenres(res.data);
        } catch (err) {
            console.error("Failed to fetch artist genres:", err);
            setArtistGenres([]);
        }
    };

    const handleAddGenre = (e) => {
        const selectedId = parseInt(e.target.value);
        if (selectedId && !artistGenres.includes(selectedId)) {
            setArtistGenres([...artistGenres, selectedId]);
        }
        e.target.value = "";
    };

    const handleRemoveGenre = (id) => {
        setArtistGenres(artistGenres.filter(g => g !== id));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            await axios.put(`${DOMAIN}/api/artists/${selectedArtistId}/genres`, {
                genreIds: artistGenres
            });
            alert("Artist genres updated successfully.");
            goBack();
        } catch (err) {
            console.error("Update failed:", err);
            setError("Failed to update artist genres.");
        }
    };

    return (
        <div className="add-artist-container">
            <h2>Edit Artist</h2>
            <form onSubmit={handleSubmit} className="add-artist-form">
                <label>
                    Select Artist:
                    <select
                        value={selectedArtistId}
                        onChange={(e) => setSelectedArtistId(e.target.value)}
                        required
                    >
                        <option value="">-- Choose an artist --</option>
                        {artists.map(artist => (
                            <option key={artist.id} value={artist.id}>{artist.artistName}</option>
                        ))}
                    </select>
                </label>

                {selectedArtistId && (
                    <>
                        <label>
                            Add Genre:
                            <select onChange={handleAddGenre} defaultValue="">
                                <option value="" disabled>-- Choose a genre --</option>
                                {allGenres.map(genre => (
                                    <option key={genre.id} value={genre.id}>{genre.genreName}</option>
                                ))}
                            </select>
                        </label>

                        <div className="selected-genres">
                            <p>Assigned Genres:</p>
                            <ul>
                                {artistGenres.map(id => {
                                    const genre = allGenres.find(g => g.id === id);
                                    return (
                                        <li key={id}>
                                            {genre?.genreName}
                                            <button type="button" onClick={() => handleRemoveGenre(id)}>✕</button>
                                        </li>
                                    );
                                })}
                            </ul>
                        </div>
                    </>
                )}

                {error && <p className="error">{error}</p>}

                <div className="form-buttons">
                    <button type="submit" disabled={!selectedArtistId}>Save Changes</button>
                    <button type="button" onClick={goBack}>Cancel</button>
                </div>
            </form>
        </div>
    );
}
