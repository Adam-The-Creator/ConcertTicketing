import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';

export default function AddTickets({ goBack }) {
    const [concerts, setConcerts] = useState([]);
    const [selectedConcertId, setSelectedConcertId] = useState('');
    const [ticketCount, setTicketCount] = useState('');
    const [message, setMessage] = useState('');

    useEffect(() => {
        async function fetchConcerts() {
            try {
                const res = await axios.get(`${DOMAIN}/api/events/upcoming`);
                setConcerts(res.data);
            } catch (err) {
                console.error("Failed to fetch upcoming concerts:", err);
            }
        }

        fetchConcerts();
    }, []);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!selectedConcertId || !ticketCount) {
            return setMessage("Please fill in all fields.");
        }

        try {
            await axios.post(`${DOMAIN}/api/tickets/generate`, {
                concertId: parseInt(selectedConcertId),
                numberOfTickets: parseInt(ticketCount)
            });
            setMessage("Tickets successfully generated.");
        } catch (err) {
            console.error("Failed to generate tickets:", err);
            setMessage("An error occurred.");
        }
    };

    return (
        <div className="add-tickets-form">
            <h3>Generate Tickets</h3>
            <form onSubmit={handleSubmit}>
                <label>
                    Select Concert:
                    <select value={selectedConcertId} onChange={e => setSelectedConcertId(e.target.value)}>
                        <option value="">-- Choose --</option>
                        {concerts.map(c => (
                            <option key={c.id} value={c.id}>
                                {c.concertName} ({new Date(c.date).toLocaleDateString()})
                            </option>
                        ))}
                    </select>
                </label>
                <label>
                    Number of Tickets:
                    <input
                        type="number"
                        min="1"
                        value={ticketCount}
                        onChange={e => setTicketCount(e.target.value)}
                    />
                </label>
                <button type="submit">Generate</button>
                <button type="button" onClick={goBack}>Back</button>
            </form>
            {message && <p>{message}</p>}
        </div>
    );
}
