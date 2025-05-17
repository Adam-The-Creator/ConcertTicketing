import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';

export default function EditEvent({ eventData, goBack }) {
    const [formData, setFormData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [artistOptions, setArtistOptions] = useState([]);

    useEffect(() => {
        const fetchEventDetails = async () => {
            try {
                const response = await fetch(`${DOMAIN}/api/events/eventdetails/${eventData.id}`);
                if (!response.ok) throw new Error("Failed to fetch event details");
                const data = await response.json();

                const formattedDate = new Date(data.date).toISOString().slice(0, 16);

                setFormData({
                    concertName: data.concertName || '',
                    description: data.description || '',
                    date: formattedDate,
                    venueName: data.venueName || '',
                    venueLocation: data.venueLocation || '',
                    status: data.status || 'Upcoming',
                    mainArtistId: data.mainArtistId || '',
                });

                console.log("mainArtistId from API:", data.mainArtistId);
                console.log("Type of mainArtistId:", typeof data.mainArtistId);

            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        const fetchArtists = async () => {
            try {
                const res = await fetch(`${DOMAIN}/api/artists`);
                const list = await res.json();
                setArtistOptions(list);
            } catch (err) {
                console.error("Failed to fetch artists:", err);
            }
        };

        fetchEventDetails();
        fetchArtists();
    }, [eventData.id]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        const newValue = name === "mainArtistId" ? Number(value) : value;
        setFormData(prev => ({ ...prev, [name]: newValue }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const selectedArtist = artistOptions.find(artist => artist.id === formData.mainArtistId);

        const updatedEvent = {
            concertName: formData.concertName,
            description: formData.description,
            date: new Date(formData.date).toISOString(),
            venueName: formData.venueName,
            venueLocation: formData.venueLocation,
            mainArtistId: formData.mainArtistId,
            mainArtistName: selectedArtist ? selectedArtist.artistName : '',
            status: formData.status,
        };

        try {
            await axios.put(`${DOMAIN}/api/events/${eventData.id}`, updatedEvent);
            alert("Concert updated successfully.");
            goBack();
        } catch (err) {
            console.error("Failed to update concert:", err);
            alert("Error occurred while updating concert.");
        }
    };

    if (loading) return <p>Loading concert data...</p>;
    if (error) return <p>Error loading concert data: {error}</p>;
    if (!formData) return null;


    return (
        <div className="create-concert">
            <h2>Edit Concert</h2>
            <form onSubmit={handleSubmit} className="concert-form">
                <label>
                    Concert Name*
                    <input type="text" name="concertName" value={formData.concertName} onChange={handleChange} required />
                </label>
                <label>
                    Main Artist*
                    <select name="mainArtistId" value={formData.mainArtistId} onChange={handleChange} required>
                        <option value="">-- Select an Artist --</option>
                        {artistOptions.map(artist => (
                            <option key={artist.id} value={artist.id}>
                                {artist.artistName}
                            </option>
                        ))}
                    </select>
                </label>
                <label>
                    Description
                    <textarea name="description" value={formData.description} onChange={handleChange} />
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

                <button type="submit">Save Changes</button>
                <button type="button" onClick={goBack} style={{ marginLeft: '1rem' }}>Cancel</button>
            </form>
        </div>
    );
}