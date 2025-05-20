import React, { useEffect, useState } from 'react';
import { DOMAIN } from './Utils';

function UpcomingEvents({ onSelectEvent }) {
    const [events, setEvents] = useState([]);

    useEffect(() => {
        const fetchEvents = async () => {
            try {
                const response = await fetch(`${DOMAIN}/api/events/upcoming`);
                if (!response.ok) throw new Error('Failed to fetch events');
                const data = await response.json();
                const sorted = data.sort((a, b) => new Date(a.date) - new Date(b.date));

                setEvents(sorted);
            } catch (error) {
                console.error('Error fetching upcoming events:', error);
            }
        };

        fetchEvents();
    }, []);

    return (
        <div className="upcoming-events-container">
            <h2>Upcoming Events</h2>
            <div className="events-grid">
                {events.map((event) => (
                    <div key={event.id} className="event-card" onClick={() => onSelectEvent(event)} style={{ cursor: 'pointer' }}>
                        <img src={event.imageUrl || './src/assets/default.jpg'} alt={event.concertName} className="event-image" />
                        <h3 className="event-name">{event.concertName}</h3>
                        <p className="event-date">{new Date(event.date).toLocaleDateString()}</p>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default UpcomingEvents;
