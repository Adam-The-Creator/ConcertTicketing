import { useEffect, useState } from "react";
import { DOMAIN } from './Utils';
import BuyTickets from "./BuyTickets";
import '../css/BuyTickets.css'


function EventDetails({ event, onBack, isAdmin, onEdit }) {
    const [details, setDetails] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [availableTickets, setAvailableTickets] = useState(null);
    const [buying, setBuying] = useState(false);

    useEffect(() => {
        const fetchDetails = async () => {
            try {
                const response = await fetch(`${DOMAIN}/api/events/eventdetails/${event.id}`);
                if (!response.ok) throw new Error("Failed to fetch event details");
                const data = await response.json();
                setDetails(data);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        const fetchAvailability = async () => {
            try {
                const response = await fetch(`${DOMAIN}/api/tickets/available/${event.id}`);
                if (!response.ok) throw new Error("Failed to fetch ticket availability");
                const count = await response.json();
                setAvailableTickets(count);
            } catch {
                setAvailableTickets(0);
            }
        };

        fetchDetails();
        fetchAvailability();
    }, [event.id]);

    const handleBuyClick = () => {
        if (availableTickets === 0) {
            alert("All tickets are sold out!");
        } else {
            setBuying(true);
        }
    };

    const handleEditClick = () => {
        onEdit(event);
    };

    if (loading) return <p>Loading...</p>;
    if (error) return <p>Error: {error}</p>;
    if (!details) return null;

    if (buying) {
        return <BuyTickets concert={details} onBack={() => setBuying(false)} />;
    }

    const soldOut = availableTickets === 0;


    return (
        <div className="event-details">
            <div className="image-container">
                <img src={details.imageUrl || '/images/default.jpg'} alt={details.concertName} />
            </div>
            <div className="text-container">
                {soldOut && !isAdmin && (
                    <div className="sold-out">
                        All tickets are sold out!
                    </div>
                )}
                <div className="buttons">
                    <button className="back-btn" onClick={onBack}>&larr; Back to Events</button>
                    {isAdmin ? (
                        <button className="edit-btn" onClick={handleEditClick}>
                            Edit
                        </button>
                    ) : (
                        <button
                            className="buy-btn"
                            onClick={handleBuyClick}
                            /*disabled={soldOut}*/
                            style={{
                                backgroundColor: soldOut ? 'crimson' : '#007bff',
                                cursor: soldOut ? 'not-allowed' : 'pointer'
                            }}
                        >
                            Buy Tickets
                        </button>
                    )}
                </div>
                <h2>{details.concertName}</h2>
                <p>{details.description}</p>
                <p><strong>Date:</strong> {new Date(details.date).toLocaleString()}</p>
                <p><strong>Venue:</strong> {details.venueName} - {details.venueLocation}</p>
                <p><strong>Performers:</strong> <strong>{details.mainArtistName}</strong>
                    {details.artists && details.artists.length > 0 ? ", " + details.artists.join(", ") : ""}
                </p>
                <p><strong>Genres:</strong> {details.genres.join(", ")}</p>
            </div>
        </div>
    );
}

export default EventDetails;