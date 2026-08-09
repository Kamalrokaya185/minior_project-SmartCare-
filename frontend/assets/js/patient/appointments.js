// ======================================================
// SmartCare - Book Appointment
// Temporary frontend data
// ======================================================


// ======================================================
// MOCK DATA
// Later replace this with API calls
// ======================================================

const clinics = [
    {
        id: "clinic-001",
        name: "City Health Clinic",
        city: "Kathmandu",
        address: "New Baneshwor, Kathmandu",
        phone: "9800000000"
    },
    {
        id: "clinic-002",
        name: "ABC Medical Center",
        city: "Pokhara",
        address: "Lakeside, Pokhara",
        phone: "9811111111"
    }
];


const departments = [
    {
        id: "dept-001",
        clinicId: "clinic-001",
        name: "Cardiology"
    },
    {
        id: "dept-002",
        clinicId: "clinic-001",
        name: "Dermatology"
    },
    {
        id: "dept-003",
        clinicId: "clinic-002",
        name: "General Medicine"
    }
];


const doctors = [
    {
        id: "doctor-001",
        clinicId: "clinic-001",
        departmentId: "dept-001",
        name: "Dr. Sharma",
        specialization: "Cardiology",
        consultationFee: 800
    },
    {
        id: "doctor-002",
        clinicId: "clinic-001",
        departmentId: "dept-002",
        name: "Dr. Rai",
        specialization: "Dermatology",
        consultationFee: 700
    },
    {
        id: "doctor-003",
        clinicId: "clinic-002",
        departmentId: "dept-003",
        name: "Dr. Shah",
        specialization: "General Medicine",
        consultationFee: 500
    }
];


const availableDates = [
    {
        date: "2026-08-15",
        display: "Aug 15",
        day: "Saturday"
    },
    {
        date: "2026-08-17",
        display: "Aug 17",
        day: "Monday"
    },
    {
        date: "2026-08-18",
        display: "Aug 18",
        day: "Tuesday"
    }
];


const slots = [
    {
        id: "slot-001",
        time: "10:00 AM",
        available: true
    },
    {
        id: "slot-002",
        time: "10:30 AM",
        available: true
    },
    {
        id: "slot-003",
        time: "11:00 AM",
        available: false
    },
    {
        id: "slot-004",
        time: "11:30 AM",
        available: true
    },
    {
        id: "slot-005",
        time: "12:00 PM",
        available: true
    }
];


// ======================================================
// BOOKING STATE
// ======================================================

const booking = {

    step: 1,

    clinicId: null,

    departmentId: null,

    doctorId: null,

    date: null,

    slotId: null

};


// ======================================================
// DOM
// ======================================================

const bookingContent =
    document.getElementById("bookingContent");

const bookingSteps =
    document.getElementById("bookingSteps");

const bookingSummary =
    document.getElementById("bookingSummary");


// ======================================================
// STEPS
// ======================================================

function renderSteps() {

    const steps = [
        "Clinic",
        "Department",
        "Doctor",
        "Date",
        "Time",
        "Confirm"
    ];

    bookingSteps.innerHTML = steps.map((step, index) => {

        const number = index + 1;

        return `
            <div class="booking-step 
                ${booking.step >= number ? "active" : ""}">

                <span class="step-number">
                    ${number}
                </span>

                <span>${step}</span>

            </div>
        `;

    }).join("");
}


// ======================================================
// STEP 1 - CLINIC
// ======================================================

function renderClinics() {

    bookingContent.innerHTML = `

        <h2>Select a Clinic</h2>

        <p>
            Choose the clinic where you want to book
            your appointment.
        </p>

        <input
            type="text"
            id="clinicSearch"
            class="search-box"
            placeholder="Search clinic..."
        >

        <div
            id="clinicList"
            class="selection-list">
        </div>

    `;

    renderClinicList(clinics);

    document
        .getElementById("clinicSearch")
        .addEventListener("input", function () {

            const search = this.value.toLowerCase();

            const filtered = clinics.filter(clinic =>
                clinic.name.toLowerCase().includes(search) ||
                clinic.city.toLowerCase().includes(search)
            );

            renderClinicList(filtered);
        });
}


function renderClinicList(list) {

    const container =
        document.getElementById("clinicList");

    if (!list.length) {

        container.innerHTML =
            `<p>No clinics found.</p>`;

        return;
    }

    container.innerHTML = list.map(clinic => `

        <div class="selection-card">

            <h3>${escapeHtml(clinic.name)}</h3>

            <p>📍 ${escapeHtml(clinic.address)}</p>

            <p>🏙 ${escapeHtml(clinic.city)}</p>

            <button
                class="btn btn-primary select-button"
                onclick="selectClinic('${clinic.id}')">

                Select Clinic

            </button>

        </div>

    `).join("");
}


// ======================================================
// STEP 2 - DEPARTMENT
// ======================================================

function renderDepartments() {

    const clinicDepartments =
        departments.filter(
            d => d.clinicId === booking.clinicId
        );

    bookingContent.innerHTML = `

        <h2>Select Department</h2>

        <div class="selection-list">

            ${clinicDepartments.map(department => `

                <div
                    class="selection-card"
                    onclick="selectDepartment('${department.id}')">

                    <h3>
                        ${escapeHtml(department.name)}
                    </h3>

                    <p>
                        Choose this department
                    </p>

                </div>

            `).join("")}

        </div>

        <div class="booking-actions">

            <button
                class="btn btn-secondary"
                onclick="goBack()">

                Back

            </button>

        </div>
    `;
}


// ======================================================
// STEP 3 - DOCTOR
// ======================================================

function renderDoctors() {

    const clinicDoctors =
        doctors.filter(doctor =>
            doctor.clinicId === booking.clinicId &&
            doctor.departmentId === booking.departmentId
        );

    bookingContent.innerHTML = `

        <h2>Select Doctor</h2>

        <div class="selection-list">

            ${clinicDoctors.map(doctor => `

                <div class="selection-card">

                    <h3>
                        ${escapeHtml(doctor.name)}
                    </h3>

                    <p>
                        ${escapeHtml(doctor.specialization)}
                    </p>

                    <p>
                        Consultation Fee:
                        Rs. ${doctor.consultationFee}
                    </p>

                    <button
                        class="btn btn-primary"
                        onclick="selectDoctor('${doctor.id}')">

                        Select Doctor

                    </button>

                </div>

            `).join("")}

        </div>

        <div class="booking-actions">

            <button
                class="btn btn-secondary"
                onclick="goBack()">

                Back

            </button>

        </div>
    `;
}


// ======================================================
// STEP 4 - DATE
// ======================================================

function renderDates() {

    bookingContent.innerHTML = `

        <h2>Select Date</h2>

        <p>
            Choose an available appointment date.
        </p>

        <div class="date-list">

            ${availableDates.map(date => `

                <div
                    class="date-card"
                    onclick="selectDate('${date.date}')">

                    <strong>
                        ${escapeHtml(date.display)}
                    </strong>

                    <span>
                        ${escapeHtml(date.day)}
                    </span>

                </div>

            `).join("")}

        </div>

        <div class="booking-actions">

            <button
                class="btn btn-secondary"
                onclick="goBack()">

                Back

            </button>

        </div>
    `;
}


// ======================================================
// STEP 5 - TIME SLOT
// ======================================================

function renderSlots() {

    bookingContent.innerHTML = `

        <h2>Select Appointment Time</h2>

        <p>
            Choose an available time slot.
        </p>

        <div class="slot-grid">

            ${slots.map(slot => `

                <div
                    class="slot
                    ${!slot.available ? "unavailable" : ""}"
                    ${slot.available
                        ? `onclick="selectSlot('${slot.id}')"`
                        : ""}>

                    ${escapeHtml(slot.time)}

                    ${!slot.available
                        ? "<br><small>Booked</small>"
                        : ""}

                </div>

            `).join("")}

        </div>

        <div class="booking-actions">

            <button
                class="btn btn-secondary"
                onclick="goBack()">

                Back

            </button>

        </div>
    `;
}


// ======================================================
// STEP 6 - CONFIRM
// ======================================================

function renderConfirmation() {

    const clinic =
        clinics.find(c => c.id === booking.clinicId);

    const department =
        departments.find(d =>
            d.id === booking.departmentId
        );

    const doctor =
        doctors.find(d =>
            d.id === booking.doctorId
        );

    const date =
        availableDates.find(d =>
            d.date === booking.date
        );

    const slot =
        slots.find(s =>
            s.id === booking.slotId
        );


    bookingContent.innerHTML = `

        <h2>Confirm Appointment</h2>

        <div class="confirmation">

            <p>
                <strong>Clinic:</strong>
                ${escapeHtml(clinic.name)}
            </p>

            <p>
                <strong>Department:</strong>
                ${escapeHtml(department.name)}
            </p>

            <p>
                <strong>Doctor:</strong>
                ${escapeHtml(doctor.name)}
            </p>

            <p>
                <strong>Date:</strong>
                ${escapeHtml(date.display)}
            </p>

            <p>
                <strong>Time:</strong>
                ${escapeHtml(slot.time)}
            </p>

            <p>
                <strong>Consultation Fee:</strong>
                Rs. ${doctor.consultationFee}
            </p>

        </div>

        <div class="booking-actions">

            <button
                class="btn btn-secondary"
                onclick="goBack()">

                Back

            </button>

            <button
                class="btn btn-primary"
                onclick="confirmBooking()">

                Confirm & Continue to Payment

            </button>

        </div>
    `;
}


// ======================================================
// SELECTION FUNCTIONS
// ======================================================

function selectClinic(id) {

    booking.clinicId = id;

    booking.step = 2;

    updatePage();
}


function selectDepartment(id) {

    booking.departmentId = id;

    booking.step = 3;

    updatePage();
}


function selectDoctor(id) {

    booking.doctorId = id;

    booking.step = 4;

    updatePage();
}


function selectDate(date) {

    booking.date = date;

    booking.step = 5;

    updatePage();
}


function selectSlot(id) {

    booking.slotId = id;

    booking.step = 6;

    updatePage();
}


// ======================================================
// BACK
// ======================================================

function goBack() {

    if (booking.step > 1) {

        booking.step--;

        updatePage();
    }
}


// ======================================================
// CONFIRM BOOKING
// ======================================================

function confirmBooking() {

    console.log("Booking data:", booking);

    /*
        LATER:

        fetch(`${API_BASE_URL}/appointments`, {
            method: "POST",
            headers: authHeaders(),
            body: JSON.stringify({
                clinicId: booking.clinicId,
                doctorId: booking.doctorId,
                scheduleSlotId: booking.slotId
            })
        });

    */

    alert(
        "Appointment created! Next step: payment."
    );
}


// ======================================================
// UPDATE PAGE
// ======================================================

function updatePage() {

    renderSteps();

    switch (booking.step) {

        case 1:
            renderClinics();
            break;

        case 2:
            renderDepartments();
            break;

        case 3:
            renderDoctors();
            break;

        case 4:
            renderDates();
            break;

        case 5:
            renderSlots();
            break;

        case 6:
            renderConfirmation();
            break;
    }

    renderSummary();
}


// ======================================================
// BOOKING SUMMARY
// ======================================================

function renderSummary() {

    if (!booking.clinicId) {

        bookingSummary.style.display = "none";
        return;
    }

    const clinic =
        clinics.find(c => c.id === booking.clinicId);

    const doctor =
        doctors.find(d => d.id === booking.doctorId);

    bookingSummary.style.display = "block";

    bookingSummary.innerHTML = `

        <h3>Booking Summary</h3>

        <div class="summary-row">
            <span>Clinic</span>
            <strong>
                ${escapeHtml(clinic?.name || "—")}
            </strong>
        </div>

        <div class="summary-row">
            <span>Doctor</span>
            <strong>
                ${escapeHtml(doctor?.name || "—")}
            </strong>
        </div>

        <div class="summary-row">
            <span>Date</span>
            <strong>
                ${escapeHtml(booking.date || "—")}
            </strong>
        </div>

        <div class="summary-row">
            <span>Time</span>
            <strong>
                ${escapeHtml(
                    slots.find(s => s.id === booking.slotId)?.time || "—"
                )}
            </strong>
        </div>
    `;
}


// ======================================================
// SECURITY HELPER
// ======================================================

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


// ======================================================
// INITIALIZE
// ======================================================

document.addEventListener("DOMContentLoaded", () => {

    updatePage();

});