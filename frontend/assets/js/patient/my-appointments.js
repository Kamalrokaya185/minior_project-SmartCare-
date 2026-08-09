requireRole("Patient");

const upcomingCard = document.getElementById("upcomingCard");
const historyBody = document.getElementById("historyBody");
const detailModal = document.getElementById("detailModal");
const modalBody = document.getElementById("modalBody");
const cancelAppointmentBtn = document.getElementById("cancelAppointmentBtn");

let allAppointments = [];
let currentDetailAppointmentId = null;

const CANCELLABLE_STATUSES = ["Pending", "Confirmed"];
const PAST_OR_CLOSED_STATUSES = ["Completed", "Cancelled", "Rejected", "NoShow", "Expired"];

function escapeHtml(str) {
    if (!str) return "";
    return String(str).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}

function statusClass(status) {
    if (status === "Confirmed" || status === "Completed") return "active";
    if (PAST_OR_CLOSED_STATUSES.includes(status) && status !== "Completed") return "inactive";
    return "pending";
}

function paymentClass(status) {
    if (status === "Verified") return "active";
    if (status === "Rejected") return "inactive";
    return "pending";
}

function toDateTime(dateStr, timeStr) {
    return new Date(`${dateStr}T${timeStr}`);
}

// ===== Load appointments =====
async function loadAppointments() {
    try {
        const response = await fetch(`${API_BASE_URL}/appointments/mine`, { headers: authHeaders() });
        if (!response.ok) {
            upcomingCard.innerHTML = `<p class="empty-state">Failed to load appointments.</p>`;
            historyBody.innerHTML = `<tr><td colspan="7" class="empty-state">Failed to load appointments.</td></tr>`;
            return;
        }

        allAppointments = await response.json();
        renderUpcoming();
        renderHistory();

    } catch (err) {
        console.error("Error loading appointments:", err);
    }
}

// ===== Upcoming: soonest future, non-cancelled/rejected/expired appointment =====
function renderUpcoming() {
    const now = new Date();

    const upcoming = allAppointments
        .filter(a => CANCELLABLE_STATUSES.includes(a.status) || a.status === "CheckedIn")
        .filter(a => toDateTime(a.appointmentDate, a.appointmentTime) >= now)
        .sort((a, b) => toDateTime(a.appointmentDate, a.appointmentTime) - toDateTime(b.appointmentDate, b.appointmentTime))[0];

    if (!upcoming) {
        upcomingCard.innerHTML = `<p class="empty-state">No upcoming appointments. <a href="booking.html">Book one now</a>.</p>`;
        return;
    }

    upcomingCard.innerHTML = `
        <div class="upcoming-card">
            <h3>${escapeHtml(upcoming.doctorName)} — ${escapeHtml(upcoming.specialization ?? "")}</h3>
            <p><strong>Clinic:</strong> ${escapeHtml(upcoming.clinicName)}</p>
            <p><strong>Department:</strong> ${escapeHtml(upcoming.departmentName ?? "—")}</p>
            <p><strong>Date:</strong> ${upcoming.appointmentDate} at ${upcoming.appointmentTime}</p>
            <p><strong>Status:</strong> <span class="status-pill ${statusClass(upcoming.status)}">${upcoming.status}</span></p>
        </div>
    `;
}

// ===== History table: everything =====
function renderHistory() {
    if (!allAppointments.length) {
        historyBody.innerHTML = `<tr><td colspan="7" class="empty-state">No appointments yet.</td></tr>`;
        return;
    }

    const sorted = [...allAppointments].sort((a, b) =>
        toDateTime(b.appointmentDate, b.appointmentTime) - toDateTime(a.appointmentDate, a.appointmentTime));

    historyBody.innerHTML = sorted.map(a => `
        <tr>
            <td>${a.appointmentDate}</td>
            <td>${a.appointmentTime}</td>
            <td>${escapeHtml(a.doctorName)}</td>
            <td>${escapeHtml(a.clinicName)}</td>
            <td><span class="status-pill ${statusClass(a.status)}">${a.status}</span></td>
            <td><span class="status-pill ${paymentClass(a.paymentStatus)}">${a.paymentStatus}</span></td>
            <td><button class="btn btn-view" data-action="view" data-id="${a.appointmentId}">View Details</button></td>
        </tr>
    `).join("");
}

// ===== Row click -> detail modal =====
historyBody.addEventListener("click", (e) => {
    const btn = e.target.closest("button[data-action='view']");
    if (!btn) return;
    openDetailModal(btn.getAttribute("data-id"));
});

function openDetailModal(appointmentId) {
    const appt = allAppointments.find(a => a.appointmentId === appointmentId);
    if (!appt) return;

    currentDetailAppointmentId = appointmentId;

    modalBody.innerHTML = `
        <p><strong>Doctor:</strong> ${escapeHtml(appt.doctorName)}</p>
        <p><strong>Specialization:</strong> ${escapeHtml(appt.specialization ?? "—")}</p>
        <p><strong>Clinic:</strong> ${escapeHtml(appt.clinicName)}</p>
        <p><strong>Department:</strong> ${escapeHtml(appt.departmentName ?? "—")}</p>
        <p><strong>Date:</strong> ${appt.appointmentDate}</p>
        <p><strong>Time:</strong> ${appt.appointmentTime}</p>
        <p><strong>Status:</strong> <span class="status-pill ${statusClass(appt.status)}">${appt.status}</span></p>
        <p><strong>Payment Status:</strong> <span class="status-pill ${paymentClass(appt.paymentStatus)}">${appt.paymentStatus}</span></p>
        <p><strong>Fee:</strong> ${appt.feeAtBooking}</p>
    `;

    // Cancel is only offered for statuses where cancellation is actually meaningful
    cancelAppointmentBtn.classList.toggle("hidden", !CANCELLABLE_STATUSES.includes(appt.status));

    showModal(detailModal);
}

// ===== Cancel =====
cancelAppointmentBtn.addEventListener("click", async () => {
    if (!currentDetailAppointmentId) return;
    if (!confirm("Are you sure you want to cancel this appointment? Refund eligibility depends on the clinic's cancellation policy.")) return;

    const reason = prompt("Reason for cancellation (optional):") || null;

    try {
        const response = await fetch(`${API_BASE_URL}/appointments/${currentDetailAppointmentId}/cancel`, {
            method: "POST", headers: authHeaders(), body: JSON.stringify({ reason })
        });

        if (!response.ok) {
            const result = await response.json();
            alert(result.title || result || "Failed to cancel appointment.");
            return;
        }

        alert("Appointment cancelled. If eligible, a refund request has been created automatically per the clinic's policy.");
        closeModal(detailModal);
        await loadAppointments();

    } catch (err) {
        console.error("Error cancelling appointment:", err);
        alert("Something went wrong while cancelling.");
    }
});

// ===== Modal helpers =====
function showModal(modalEl) { modalEl.classList.remove("hidden"); modalEl.setAttribute("aria-hidden", "false"); }
function closeModal(modalEl) { modalEl.classList.add("hidden"); modalEl.setAttribute("aria-hidden", "true"); }

document.querySelectorAll(".modal-close").forEach(el => el.addEventListener("click", () => closeModal(detailModal)));
detailModal.addEventListener("click", (e) => { if (e.target === detailModal) closeModal(detailModal); });

// ===== Initial load =====
document.addEventListener("DOMContentLoaded", loadAppointments);