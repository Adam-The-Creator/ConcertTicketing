import React, { useState, useEffect } from "react";
import { DOMAIN } from "./Utils";

export default function BuyTickets({ concert, onBack }) {
    const [quantity, setQuantity] = useState(1);
    const [ticketPrice, setTicketPrice] = useState(null);
    const [discountCode, setDiscountCode] = useState("");

    const user = JSON.parse(localStorage.getItem("user"));
    const userId = user?.id;

    console.log(user);

    useEffect(() => {
        const fetchPrice = async () => {
            try {
                const response = await fetch(`${DOMAIN}/api/tickets/price/${concert.id}`);
                if (!response.ok) throw new Error("Failed to fetch ticket price");
                const price = await response.json();
                setTicketPrice(price);
            } catch (err) {
                console.error(err);
                setTicketPrice(0);
            }
        };

        fetchPrice();
    }, [concert.id]);

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!userId) {
            alert("Please sign in to purchase tickets.");
            return;
        }

        try {
            const response = await fetch(`${DOMAIN}/api/orders/purchase`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    concertId: concert.id,
                    userId: userId,
                    quantity: quantity,
                    totalPrice: ticketPrice * quantity
                }),
            });

            if (!response.ok) throw new Error("Ticket purchase failed.");
            const data = await response.json();
            alert(data.message);
            onBack();
        } catch (err) {
            console.error(err);
            alert("Something went wrong during the purchase.");
        }
    };



    return (
        <div className="buy-tickets-form">
            <h2>Buy Tickets</h2>
            <p><strong>Concert:</strong> {concert.concertName}</p>
            <p><strong>Date:</strong> {new Date(concert.date).toLocaleString()}</p>
            <p><strong>Main Artist:</strong> {concert.mainArtistName}</p>

            {ticketPrice !== null ? (
                <p><strong>Price per Ticket:</strong> ${ticketPrice.toFixed(2)}</p>
            ) : (
                <p>Loading ticket price...</p>
            )}

            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="quantity">Number of Tickets:</label>
                    <input
                        id="quantity"
                        type="number"
                        min="1"
                        value={quantity}
                        onChange={(e) => setQuantity(parseInt(e.target.value))}
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="discount">Discount Code:</label>
                    <input
                        id="discount"
                        type="text"
                        value={discountCode}
                        onChange={(e) => setDiscountCode(e.target.value)}
                        placeholder="Enter discount code"
                    />
                </div>

                <p className="total-price">
                    <strong>Total Price:</strong> ${(ticketPrice * quantity).toFixed(2)}
                </p>

                <div className="form-buttons">
                    <button type="submit" className="btn primary">Buy</button>
                    <button type="button" onClick={onBack} className="btn secondary">Cancel</button>
                </div>
            </form>

        </div>
    );
}