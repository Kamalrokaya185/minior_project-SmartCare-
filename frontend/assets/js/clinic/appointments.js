requireRole("Clinic"); // or "SuperAdmin" for admin pages, "Patient" for patient pages
const CLINIC_ID = localStorage.getItem("smartcare_profile_id");

if (!CLINIC_ID) {
    alert("No clinic linked to this account. Please log in again.");
}

const appointmentsBody = document.getElementById("appointmentsBody");
const dateFilter = document.getElementById("dateFilter");
const todayBtn = document.getElementById("todayBtn");
const listTitle = document.getElementById("listTitle");

function todayIso() {
    const now = new Date();
    const yyyy = now.getFullYear();
    const mm = String(now.getMonth() + 1).padStart(2, "0");
    const dd = String(now.getDate()).padStart(2, "0");
    return `${yyyy}-${mm}-${dd}`;
}

async function loadAppointments(date) {
    if (!CLINIC_ID) return;

    appointmentsBody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:#94a3b8;">Loading...</td></tr>`;

    const url = date
        ? `${API_BASE_URL}/appointments/clinic/${CLINIC_ID}?date=${date}`
        : `${API_BASE_URL}/appointments/clinic/${CLINIC_ID}`; // omit date -> backend defaults to today

    try {
        const response = await fetch(url, { headers: authHeaders() });
        if (!response.ok) {
            appointmentsBody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:#94a3b8;">Failed to load appointments.</td></tr>`;
            return;
        }

        const appointments = await response.json();
        renderAppointments(appointments);

        listTitle.textContent = date && date !== todayIso()
            ? `Appointments — ${date}`
            : "Today's Appointments";

    } catch (err) {
        console.error("Error loading appointments:", err);
        appointmentsBody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:#94a3b8;">Something went wrong.</td></tr>`;
    }
}

function renderAppointments(appointments) {
    if (!appointments.length) {
        appointmentsBody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:#94a3b8;">No appointments found for this date.</td></tr>`;
        return;
    }

    appointmentsBody.innerHTML = appointments.map((a) => `
        <tr>
            <td>${escapeHtml(a.patientName)}</td>
            <td>${escapeHtml(a.doctorName)}</td>
            <td>${escapeHtml(a.specialization ?? "—")}</td>
            <td>${escapeHtml(a.appointmentTime)}</td>
            <td><span class="status-pill ${statusClass(a.status)}">${escapeHtml(a.status)}</span></td>
            <td><span class="status-pill ${paymentClass(a.paymentStatus)}">${escapeHtml(a.paymentStatus)}</span></td>
            <td>${a.feeAtBooking}</td>
        </tr>
    `).join("");
}

function statusClass(status) {
    if (status === "Confirmed" || status === "Completed") return "active";
    if (status === "Cancelled" || status === "Rejected" || status === "NoShow" || status === "Expired") return "inactive";
    return "pending";
}

function paymentClass(status) {
    if (status === "Verified") return "active";
    if (status === "Rejected") return "inactive";
    return "pending";
}

function escapeHtml(str) {
    if (!str) return "";
    return String(str).replace(/[&<>"']/g, (c) => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[c]));
}

// ===== Date filter events =====
dateFilter.addEventListener("change", () => {
    if (dateFilter.value) loadAppointments(dateFilter.value);
});

todayBtn.addEventListener("click", () => {
    dateFilter.value = todayIso();
    loadAppointments(); // no date param -> backend defaults to today, matches your "default is today" requirement
});

// ===== Initial load =====
document.addEventListener("DOMContentLoaded", () => {
    requireRole("Clinic");
    dateFilter.value = todayIso();
    loadAppointments(); // today, by default, via backend default
});