import { useEffect, useState } from 'react';
import QRCode from 'react-qr-code';
import { DOMAIN } from './Utils';
import '../css/Profile.css';

function Profile() {
    const [tickets, setTickets] = useState([]);

    useEffect(() => {
        const stored = localStorage.getItem('user');
        if (!stored) return;

        const user = JSON.parse(stored);
        fetch(`${DOMAIN}/api/orders/mytickets/${user.id}`)
            .then(res => res.json())
            .then(setTickets)
            .catch(err => console.error("Error fetching tickets:", err));
    }, []);

    return (
        <div className="profile-container">
            <h2>My Purchased Tickets</h2>
            {tickets.length === 0 ? (
                <p>No tickets purchased yet.</p>
            ) : (
                <div className="table-container">
                    <table className="ticket-table">
                        <thead>
                            <tr>
                                <th>Concert</th>
                                <th>Date</th>
                                <th>Venue</th>
                                <th>Location</th>
                                <th>Price</th>
                                <th>Purchased</th>
                                <th>Serial Number</th>
                                <th>QR Code</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tickets.map((t, idx) => (
                                <tr key={idx}>
                                    <td>{t.concertName}</td>
                                    <td>{new Date(t.date).toLocaleDateString()}</td>
                                    <td>{t.venue}</td>
                                    <td>{t.location}</td>
                                    <td>${t.price.toFixed(2)}</td>
                                    <td>{new Date(t.purchaseDate).toLocaleString()}</td>
                                    <td>{t.serialNumber}</td>
                                    <td>
                                        <QRCode
                                            value={JSON.stringify({ serial: t.serialNumber, concert: t.concertName })}
                                            size={128}
                                            style={{ height: "5em", maxWidth: "100%", width: "100%" }}
                                        />
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
}

export default Profile;
