requireRole("Patient");

// ===============================
// State (populated from the API)
// ===============================

let upcomingAppointment = null;
let recentAppointments = [];

const CANCELLABLE_STATUSES = ["Pending", "Confirmed", "CheckedIn"];
const CLOSED_STATUSES = ["Completed", "Cancelled", "Rejected", "NoShow", "Expired"];

function toDateTime(dateStr, timeStr) {
    return new Date(`${dateStr}T${timeStr}`);
}

// ===============================
// Load appointments from the API
// ===============================

async function loadDashboardData() {
    try {
        const response = await fetch(`${API_BASE_URL}/appointments/mine`, { headers: authHeaders() });

        if (!response.ok) {
            console.error("Failed to load appointments:", response.status);
            upcomingAppointment = null;
            recentAppointments = [];
            renderUpcomingAppointment();
            renderRecentAppointments();
            return;
        }

        const appointments = await response.json();

        // Upcoming = soonest, non-past, still-relevant appointment
        const now = new Date();
        upcomingAppointment = appointments
            .filter(a => CANCELLABLE_STATUSES.includes(a.status))
            .filter(a => toDateTime(a.appointmentDate, a.appointmentTime) >= now)
            .sort((a, b) => toDateTime(a.appointmentDate, a.appointmentTime) - toDateTime(b.appointmentDate, b.appointmentTime))[0]
            ?? null;

        // Recent = everything else, most recent first, excluding the one already shown as Upcoming
        recentAppointments = appointments
            .filter(a => !upcomingAppointment || a.appointmentId !== upcomingAppointment.appointmentId)
            .sort((a, b) => toDateTime(b.appointmentDate, b.appointmentTime) - toDateTime(a.appointmentDate, a.appointmentTime));

    } catch (err) {
        console.error("Error loading dashboard data:", err);
        upcomingAppointment = null;
        recentAppointments = [];
    }

    renderUpcomingAppointment();
    renderRecentAppointments();
}

// ===============================
// Render Upcoming Appointment
// ===============================

function renderUpcomingAppointment() {

    const container = document.getElementById("upcomingAppointment");

    if (!upcomingAppointment) {
        container.innerHTML = `
            <div class="empty-state">
                No upcoming appointments. <a href="find-clinic.html">Book one now</a>.
            </div>
        `;
        return;
    }

    const appointment = upcomingAppointment;

    container.innerHTML = `
        <div class="appointment-card">

            <div class="appointment-info">

                <h3>${escapeHtml(appointment.doctorName)}</h3>

                <p class="specialization">
                    ${escapeHtml(appointment.specialization ?? "")}
                </p>

                <p class="clinic">
                    ${escapeHtml(appointment.clinicName)}
                </p>

                <div class="appointment-time">
                    <span>📅 ${escapeHtml(appointment.appointmentDate)}</span>
                    <span>🕐 ${escapeHtml(appointment.appointmentTime)}</span>
                </div>

                <div class="appointment-footer">

                    <span class="status ${appointment.status.toLowerCase()}">
                        Status: ${escapeHtml(appointment.status)}
                    </span>

                    <button
                        class="btn btn-primary"
                        onclick="viewAppointment('${appointment.appointmentId}')">
                        View Details
                    </button>

                </div>

            </div>

        </div>
    `;
}


// ===============================
// Render Quick Actions
// ===============================

function renderQuickActions() {

    const container = document.getElementById("quickActions");

    container.innerHTML = `

        <a href="booking.html" class="quick-action">
            <span class="quick-icon">🏥</span>
            <span>Find Clinic</span>
        </a>

        <a href="my-appointments.html" class="quick-action">
            <span class="quick-icon">📅</span>
            <span>My Appointments</span>
        </a>

        <a href="setting.html" class="quick-action">
            <span class="quick-icon">👤</span>
            <span>My Profile</span>
        </a>

    `;
}


// ===============================
// Render Recent Appointments
// ===============================

function renderRecentAppointments() {

    const container = document.getElementById("recentAppointments");

    if (!recentAppointments.length) {

        container.innerHTML = `
            <div class="empty-state">
                No recent appointments.
            </div>
        `;

        return;
    }

    container.innerHTML = `

        <div class="table-wrapper">

            <table class="appointments-table">

                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Doctor</th>
                        <th>Clinic</th>
                        <th>Status</th>
                        <th>Action</th>
                    </tr>
                </thead>

                <tbody>

                    ${recentAppointments.map(appointment => `

                        <tr>

                            <td>${escapeHtml(appointment.appointmentDate)}</td>

                            <td>${escapeHtml(appointment.doctorName)}</td>

                            <td>${escapeHtml(appointment.clinicName)}</td>

                            <td>
                                <span class="status ${appointment.status.toLowerCase()}">
                                    ${escapeHtml(appointment.status)}
                                </span>
                            </td>

                            <td>
                                <button
                                    class="btn btn-small"
                                    onclick="viewAppointment('${appointment.appointmentId}')">
                                    View
                                </button>
                            </td>

                        </tr>

                    `).join("")}

                </tbody>

            </table>

        </div>
    `;
}


// ===============================
// Appointment Details
// ===============================

function viewAppointment(id) {
    window.location.href = `my-appointments.html?id=${id}`;
}


// ===============================
// Security helper
// ===============================

function escapeHtml(value) {

    if (value === null || value === undefined) {
        return "";
    }

    return String(value).replace(/[&<>"']/g, char => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    }[char]));
}


// ===============================
// Initialize Dashboard
// ===============================

document.addEventListener("DOMContentLoaded", () => {

    renderQuickActions();
    loadDashboardData(); // fetches real data, then renders Upcoming + Recent

});