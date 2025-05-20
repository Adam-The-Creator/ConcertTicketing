import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { DOMAIN } from './Utils';
import CreateConcert from './CreateConcert';
import AddArtist from './AddArtist';
import AddTickets from './AddTickets';
import EditArtist from './EditArtist';
import AddGenre from './AddGenre';

function Widget({ title, value }) {
    return (
        <div className="widget">
            <h4>{title}</h4>
            <p>{value}</p>
        </div>
    );
}

function ActionCard({ title, description, onClick }) {
    return (
        <div
            className="action-card"
            onClick={onClick}
        >
            <h4>{title}</h4>
            <p>{description}</p>
        </div>
    );
}

export default function AdminContent() {
    const [stats, setStats] = useState({
        events: 0,
        ticketsSold: 0,
        salesToday: 0,
        incomeMonth: 0
    });
    const [showCreateConcert, setShowCreateConcert] = useState(false);
    const [showAddArtist, setShowAddArtist] = useState(false);
    const [showAddTickets, setShowAddTickets] = useState(false);
    const [showEditArtist, setShowEditArtist] = useState(false);
    const [showAddGenre, setShowAddGenre] = useState(false);

    useEffect(() => {
        async function fetchStats() {
            try {
                const [eventsRes, ticketsRes, todayRes, incomeRes] = await Promise.all([
                    axios.get(`${DOMAIN}/api/stats/events/count`),
                    axios.get(`${DOMAIN}/api/stats/tickets/sold`),
                    axios.get(`${DOMAIN}/api/stats/tickets/sold/today`),
                    axios.get(`${DOMAIN}/api/stats/income/monthly`),
                ]);
                setStats({
                    events: eventsRes.data.count,
                    ticketsSold: ticketsRes.data.total,
                    salesToday: todayRes.data.today,
                    incomeMonth: incomeRes.data.amount,
                });
            } catch (err) {
                console.error('Failed to fetch stats:', err);
            }
        }
        fetchStats();
    }, []);

    if (showCreateConcert) {
        return <CreateConcert goBack={() => setShowCreateConcert(false)} />;
    }

    if (showAddArtist) {
        return <AddArtist goBack={() => setShowAddArtist(false)} />;
    }

    if (showAddTickets) {
        return <AddTickets goBack={() => setShowAddTickets(false)} />;
    }

    if (showEditArtist) {
        return <EditArtist goBack={() => setShowEditArtist(false)} />;
    }

    if (showAddGenre) {
        return <AddGenre goBack={() => setShowAddGenre(false)} />;
    }

    return (
        <div className="admin-dashboard">
            <h2>Admin Dashboard</h2>

            <section className="dashboard-section statistics">
                <h3>Statistics</h3>
                <hr />
                <div className="dashboard-widgets">
                    <Widget title="Events" value={stats.events} />
                    <Widget title="Tickets sold" value={stats.ticketsSold} />
                    <Widget title="Sales today" value={stats.salesToday} />
                    <Widget title="Income this month" value={`$ ${stats.incomeMonth.toLocaleString()}`} />
                </div>
            </section>

            <section className="dashboard-section event-management">
                <h3>Events handling</h3>
                <hr />
                <div className="action-grid">
                    <ActionCard
                        title="Add new concerts"
                        description="Create a new concert event with basic details."
                        onClick={() => setShowCreateConcert(true)}
                    />
                    <ActionCard
                        title="Add new artist"
                        description="Add a new artist and assign genres."
                        onClick={() => setShowAddArtist(true)}
                    />
                    <ActionCard
                        title="Edit artist"
                        description="Update genre or edit artist information."
                        onClick={() => setShowEditArtist(true)}
                    />
                    <ActionCard
                        title="Add new genre"
                        description="Add new genre."
                        onClick={() => setShowAddGenre(true)}
                    />
                    <ActionCard
                        title="Add new tickets"
                        description="Generate single or multiple tickets for an event."
                        onClick={() => setShowAddTickets(true)}
                    />
                </div>
            </section>

            <section className="dashboard-section user-management">
                <h3>Users</h3>
                <hr />
                {/* List of users + admin operations */}
            </section>
        </div>
    );
}
