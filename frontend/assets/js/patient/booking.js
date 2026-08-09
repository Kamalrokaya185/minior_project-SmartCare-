requireRole("Patient");

// ===== Booking state, carried across steps =====
const state = {
    clinicId: null, clinicName: null,
    departmentId: null, departmentName: null,
    clinicMembershipId: null, doctorName: null, specialization: null, consultationFee: null,
    date: null,
    scheduleSlotId: null, startTime: null, endTime: null,
    appointmentId: null
};

// ===== Step navigation =====
function goToStep(step) {
    document.querySelectorAll(".booking-panel").forEach(p => p.classList.add("hidden"));
    document.getElementById(`panel-${step}`).classList.remove("hidden");

    document.querySelectorAll(".step-indicator").forEach(s => s.classList.remove("active"));
    document.querySelector(`.step-indicator[data-step="${step}"]`)?.classList.add("active");
}

document.querySelectorAll("[data-back]").forEach(btn => {
    btn.addEventListener("click", () => goToStep(Number(btn.getAttribute("data-back"))));
});

function escapeHtml(str) {
    if (!str) return "";
    return String(str).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}

// ===== Step 1: Clinic =====
async function loadClinics(search) {
    const url = search
        ? `${API_BASE_URL}/booking/clinics?search=${encodeURIComponent(search)}`
        : `${API_BASE_URL}/booking/clinics`;

    const response = await fetch(url, { headers: authHeaders() });
    const clinics = await response.json();

    const list = document.getElementById("clinicList");
    list.innerHTML = clinics.length
        ? clinics.map(c => `
            <div class="select-card" data-id="${c.id}" data-name="${escapeHtml(c.name)}">
                <strong>${escapeHtml(c.name)}</strong>
                <p>${escapeHtml(c.city ?? "")}, ${escapeHtml(c.state ?? "")}</p>
            </div>`).join("")
        : `<p>No clinics found.</p>`;

    list.querySelectorAll(".select-card").forEach(card => {
        card.addEventListener("click", () => {
            state.clinicId = card.dataset.id;
            state.clinicName = card.dataset.name;
            goToStep(2);
            loadDepartments();
        });
    });
}

document.getElementById("clinicSearch").addEventListener("input", (e) => loadClinics(e.target.value));

// ===== Step 2: Department =====
async function loadDepartments() {
    const response = await fetch(`${API_BASE_URL}/booking/clinics/${state.clinicId}/departments`, { headers: authHeaders() });
    const departments = await response.json();

    const list = document.getElementById("departmentList");
    list.innerHTML = departments.length
        ? departments.map(d => `
            <div class="select-card" data-id="${d.id}" data-name="${escapeHtml(d.name)}">
                <strong>${escapeHtml(d.name)}</strong>
                <p>${escapeHtml(d.description ?? "")}</p>
            </div>`).join("")
        : `<p>No departments available at this clinic yet.</p>`;

    list.querySelectorAll(".select-card").forEach(card => {
        card.addEventListener("click", () => {
            state.departmentId = card.dataset.id;
            state.departmentName = card.dataset.name;
            goToStep(3);
            loadDoctors();
        });
    });
}

// ===== Step 3: Doctor =====
async function loadDoctors() {
    const response = await fetch(
        `${API_BASE_URL}/booking/clinics/${state.clinicId}/doctors?departmentId=${state.departmentId}`,
        { headers: authHeaders() });
    const doctors = await response.json();

    const list = document.getElementById("doctorList");
    list.innerHTML = doctors.length
        ? doctors.map(d => `
            <div class="select-card" data-id="${d.clinicMembershipId}" data-name="${escapeHtml(d.fullName)}"
                 data-spec="${escapeHtml(d.specialization ?? "")}" data-fee="${d.consultationFee ?? 0}">
                <strong>${escapeHtml(d.fullName)}</strong>
                <p>${escapeHtml(d.specialization ?? "")}</p>
                <p>Fee: ${d.consultationFee ?? "—"}</p>
            </div>`).join("")
        : `<p>No active doctors in this department right now.</p>`;

    list.querySelectorAll(".select-card").forEach(card => {
        card.addEventListener("click", () => {
            state.clinicMembershipId = card.dataset.id;
            state.doctorName = card.dataset.name;
            state.specialization = card.dataset.spec;
            state.consultationFee = card.dataset.fee;
            goToStep(4);
            loadDates();
        });
    });
}

// ===== Step 4: Date =====
async function loadDates() {
    const response = await fetch(
        `${API_BASE_URL}/booking/doctors/${state.clinicMembershipId}/available-dates?daysAhead=30`,
        { headers: authHeaders() });
    const dates = await response.json();

    const list = document.getElementById("dateList");
    list.innerHTML = dates.length
        ? dates.map(d => `<div class="select-card" data-date="${d}"><strong>${d}</strong></div>`).join("")
        : `<p>No upcoming availability for this doctor.</p>`;

    list.querySelectorAll(".select-card").forEach(card => {
        card.addEventListener("click", () => {
            state.date = card.dataset.date;
            goToStep(5);
            loadSlots();
        });
    });
}

// ===== Step 5: Time Slot =====
async function loadSlots() {
    const response = await fetch(
        `${API_BASE_URL}/booking/doctors/${state.clinicMembershipId}/availability?date=${state.date}`,
        { headers: authHeaders() });
    const slots = await response.json();

    const list = document.getElementById("slotList");
    const availableSlots = slots.filter(s => s.isAvailable);

    list.innerHTML = availableSlots.length
        ? availableSlots.map(s => `
            <div class="select-card" data-start="${s.startTime}" data-end="${s.endTime}">
                <strong>${s.startTime} - ${s.endTime}</strong>
            </div>`).join("")
        : `<p>No open slots on this date.</p>`;

    list.querySelectorAll(".select-card").forEach(card => {
        card.addEventListener("click", async () => {
            await reserveSlot(card.dataset.start, card.dataset.end);
        });
    });
}

async function reserveSlot(startTime, endTime) {
    const body = {
        clinicMembershipId: state.clinicMembershipId,
        slotDate: state.date,
        startTime, endTime
    };

    const response = await fetch(`${API_BASE_URL}/booking/slots/reserve`, {
        method: "POST", headers: authHeaders(), body: JSON.stringify(body)
    });

    if (response.status === 409) {
        alert("Sorry, this slot was just taken. Please pick another.");
        await loadSlots();
        return;
    }
    if (!response.ok) {
        const result = await response.json();
        alert(result.title || result || "Could not reserve this slot.");
        return;
    }

    const result = await response.json();
    state.scheduleSlotId = result.scheduleSlotId;
    state.startTime = startTime;
    state.endTime = endTime;

    showReview();
    goToStep(6);
}

// ===== Step 6: Review =====
function showReview() {
    document.getElementById("reviewSummary").innerHTML = `
        <p><strong>Clinic:</strong> ${escapeHtml(state.clinicName)}</p>
        <p><strong>Department:</strong> ${escapeHtml(state.departmentName)}</p>
        <p><strong>Doctor:</strong> ${escapeHtml(state.doctorName)} (${escapeHtml(state.specialization)})</p>
        <p><strong>Date:</strong> ${state.date}</p>
        <p><strong>Time:</strong> ${state.startTime} - ${state.endTime}</p>
        <p><strong>Consultation Fee:</strong> ${state.consultationFee}</p>
    `;
}

document.getElementById("confirmBookingBtn").addEventListener("click", async () => {
    const body = {
        clinicId: state.clinicId,
        clinicMembershipId: state.clinicMembershipId,
        departmentId: state.departmentId,
        scheduleSlotId: state.scheduleSlotId,
        appointmentDate: state.date,
        appointmentTime: state.startTime,
        notes: document.getElementById("notesInput").value.trim() || null
    };

    try {
        const response = await fetch(`${API_BASE_URL}/appointments`, {
            method: "POST", headers: authHeaders(), body: JSON.stringify(body)
        });
        const result = await response.json();

        if (!response.ok) {
            alert(result.title || result || "Failed to create appointment.");
            return;
        }

        state.appointmentId = result.appointmentId;
        showPaymentStep();
        goToStep(7);
    } catch (err) {
        console.error("Error confirming booking:", err);
        alert("Something went wrong while confirming the booking.");
    }
});

// ===== Step 7: Payment =====
function showPaymentStep() {
    document.getElementById("paymentInstructions").innerHTML = `
        <p>Scan the clinic's QR code using your banking/wallet app and pay <strong>${escapeHtml(state.consultationFee)}</strong>.</p>
        <p>Then upload a screenshot of the completed payment below.</p>
    `;
}

document.getElementById("submitProofBtn").addEventListener("click", async () => {
    const fileInput = document.getElementById("proofFile");
    if (!fileInput.files.length) { alert("Please select a screenshot to upload."); return; }

    const formData = new FormData();
    formData.append("file", fileInput.files[0]);

    try {
        const uploadResponse = await fetch(`${API_BASE_URL}/uploads/payment-proof`, {
            method: "POST",
            headers: { "Authorization": authHeaders().Authorization }, // no Content-Type — browser sets multipart boundary
            body: formData
        });
        const uploadResult = await uploadResponse.json();

        if (!uploadResponse.ok) {
            alert(uploadResult.title || uploadResult || "Upload failed.");
            return;
        }

        const submitBody = {
            paymentProofUrl: uploadResult.url,
            paymentMethod: document.getElementById("paymentMethod").value
        };

        const submitResponse = await fetch(`${API_BASE_URL}/appointments/${state.appointmentId}/payment-proof`, {
            method: "POST", headers: authHeaders(), body: JSON.stringify(submitBody)
        });

        if (!submitResponse.ok) {
            const result = await submitResponse.json();
            alert(result.title || result || "Failed to submit payment proof.");
            return;
        }

        document.getElementById("submitProofBtn").classList.add("hidden");
        document.getElementById("waitingMessage").classList.remove("hidden");

    } catch (err) {
        console.error("Error uploading payment proof:", err);
        alert("Something went wrong while uploading payment proof.");
    }
});

// ===== Initial load =====
document.addEventListener("DOMContentLoaded", () => {
    loadClinics();
    goToStep(1);
});