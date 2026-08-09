// ===============================
// Temporary Mock Data
// Later replace this with fetch()
// ===============================

const upcomingAppointment = {
    doctorName: "Dr. Sharma",
    specialization: "Cardiology",
    clinicName: "City Health Clinic",
    date: "Aug 15, 2026",
    time: "10:30 AM",
    status: "Confirmed",
    appointmentId: "apt-001"
};

const recentAppointments = [
    {
        id: "apt-002",
        date: "Aug 01, 2026",
        doctor: "Dr. Rai",
        clinic: "City Health Clinic",
        status: "Completed"
    },
    {
        id: "apt-003",
        date: "Jul 20, 2026",
        doctor: "Dr. Shah",
        clinic: "ABC Clinic",
        status: "Cancelled"
    }
];


// ===============================
// Render Upcoming Appointment
// ===============================

function renderUpcomingAppointment() {

    const container = document.getElementById("upcomingAppointment");

    if (!upcomingAppointment) {
        container.innerHTML = `
            <div class="empty-state">
                No upcoming appointments.
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
                    ${escapeHtml(appointment.specialization)}
                </p>

                <p class="clinic">
                    ${escapeHtml(appointment.clinicName)}
                </p>

                <div class="appointment-time">
                    <span>📅 ${escapeHtml(appointment.date)}</span>
                    <span>🕐 ${escapeHtml(appointment.time)}</span>
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

        <a href="find-clinic.html" class="quick-action">
            <span class="quick-icon">🏥</span>
            <span>Find Clinic</span>
        </a>

        <a href="appointments.html" class="quick-action">
            <span class="quick-icon">📅</span>
            <span>My Appointments</span>
        </a>

        <a href="profile.html" class="quick-action">
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

                            <td>${escapeHtml(appointment.date)}</td>

                            <td>${escapeHtml(appointment.doctor)}</td>

                            <td>${escapeHtml(appointment.clinic)}</td>

                            <td>
                                <span class="status ${appointment.status.toLowerCase()}">
                                    ${escapeHtml(appointment.status)}
                                </span>
                            </td>

                            <td>
                                <button
                                    class="btn btn-small"
                                    onclick="viewAppointment('${appointment.id}')">
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

    console.log("View appointment:", id);

    // Later:
    // window.location.href =
    //     `appointment-details.html?id=${id}`;
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

    renderUpcomingAppointment();
    renderQuickActions();
    renderRecentAppointments();

});