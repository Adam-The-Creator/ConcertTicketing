import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';
import "../css/AddTickets.css";

export default function AddTickets({ goBack }) {
    const [concerts, setConcerts] = useState([]);
    const [formData, setFormData] = useState({
        concertId: '',
        numberOfTickets: '',
        description: '',
        price: '',
        startDate: '',
        endDate: '',
        area: '',
        seat: ''
    });
    const [message, setMessage] = useState('');

    useEffect(() => {
        async function fetchConcerts() {
            try {
                const res = await axios.get(`${DOMAIN}/api/events/upcoming`);
                setConcerts(res.data);
            } catch (err) {
                console.error("Failed to fetch concerts:", err);
            }
        }

        fetchConcerts();
    }, []);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const {
            concertId, numberOfTickets, description, price, startDate, endDate, area, seat
        } = formData;

        if (!concertId || !numberOfTickets) {
            return setMessage("Concert and ticket count are required.");
        }

        try {
            await axios.post(`${DOMAIN}/api/tickets/generate`, {
                concertId: parseInt(concertId),
                numberOfTickets: parseInt(numberOfTickets),
                description,
                price: parseFloat(price),
                startDate,
                endDate,
                area,
                seat
            });
            setMessage("Tickets successfully generated.");
        } catch (err) {
            console.error("Failed to generate tickets:", err);
            setMessage("An error occurred.");
        }
    };

    return (
        <div className="add-tickets-container">
            <h3>Generate Tickets</h3>
            <form onSubmit={handleSubmit} className="add-tickets-form">
                <label>
                    Select Concert*
                    <select name="concertId" value={formData.concertId} onChange={handleChange} required>
                        <option value="">-- Choose --</option>
                        {concerts.map(c => (
                            <option key={c.id} value={c.id}>
                                {c.concertName} ({new Date(c.date).toLocaleDateString()})
                            </option>
                        ))}
                    </select>
                </label>
                <label>
                    Number of Tickets*
                    <input type="number" name="numberOfTickets" min="1" value={formData.numberOfTickets} onChange={handleChange} required />
                </label>
                <label>
                    Description
                    <textarea name="description" value={formData.description} onChange={handleChange} />
                </label>
                <label>
                    Price (USD)
                    <input type="number" name="price" min="0" step="0.01" value={formData.price} onChange={handleChange} />
                </label>
                <label>
                    Start Date
                    <input type="datetime-local" name="startDate" value={formData.startDate} onChange={handleChange} />
                </label>
                <label>
                    End Date
                    <input type="datetime-local" name="endDate" value={formData.endDate} onChange={handleChange} />
                </label>
                <label>
                    Area
                    <input type="text" name="area" value={formData.area} onChange={handleChange} />
                </label>
                <label>
                    Seat
                    <input type="text" name="seat" value={formData.seat} onChange={handleChange} />
                </label>
                <div className="form-buttons">
                    <button type="submit">Generate</button>
                    <button type="button" onClick={goBack}>Back</button>
                </div>
                {message && <p className="message">{message}</p>}
            </form>
        </div>
    );
}
