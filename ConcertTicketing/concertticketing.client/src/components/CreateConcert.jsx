import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';
import '../css/CreateConcert.css';

export default function CreateConcert({ goBack }) {
    const [formData, setFormData] = useState({
        concertName: '',
        description: '',
        date: '',
        venueName: '',
        venueLocation: '',
        imageUrl: '',
        status: 'Upcoming',
        mainArtistId: '',
    });
    const [artists, setArtists] = useState([]);

    useEffect(() => {
        axios.get(`${DOMAIN}/api/artists`)
            .then(res => setArtists(res.data))
            .catch(err => console.error("Failed to fetch artists:", err));
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            await axios.post(`${DOMAIN}/api/concerts`, formData);
            alert("Concert created successfully.");
            goBack();
        } catch (err) {
            console.error("Failed to create concert:", err);
            alert("Error occurred while creating concert.");
        }
    };

    return (
        <div className="create-concert">
            <h2>Create New Concert</h2>
            <form onSubmit={handleSubmit} className="concert-form">
                <label>
                    Concert Name*
                    <input type="text" name="concertName" value={formData.concertName} onChange={handleChange} required />
                </label>
                <label>
                    Description
                    <textarea name="description" value={formData.description} onChange={handleChange} />
                </label>
                <label>
                    Main Artist*
                    <select name="mainArtistId" value={formData.mainArtistId} onChange={handleChange} required>
                        <option value="">-- Select an Artist --</option>
                        {artists.map(artist => (
                            <option key={artist.id} value={artist.id}>
                                {artist.artistName}
                            </option>
                        ))}
                    </select>
                </label>
                <label>
                    Concert Image URL
                    <input type="text" name="imageUrl" value={formData.imageUrl} onChange={handleChange} />
                </label>
                <label>
                    Date*
                    <input type="datetime-local" name="date" value={formData.date} onChange={handleChange} required />
                </label>
                <label>
                    Venue Name*
                    <input type="text" name="venueName" value={formData.venueName} onChange={handleChange} required />
                </label>
                <label>
                    Venue Location*
                    <input type="text" name="venueLocation" value={formData.venueLocation} onChange={handleChange} required />
                </label>
                <label>
                    Status*
                    <select name="status" value={formData.status} onChange={handleChange}>
                        <option value="Upcoming">Upcoming</option>
                        <option value="Cancelled">Cancelled</option>
                        <option value="Finished">Finished</option>
                    </select>
                </label>

                <div className="form-buttons">
                    <button type="submit">Create Concert</button>
                    <button type="button" onClick={goBack}>Cancel</button>
                </div>
            </form>
        </div>
    );
}