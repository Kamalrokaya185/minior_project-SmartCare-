// ======================================================
// SmartCare - Patient Appointment History
// Temporary mock data
// ======================================================


// ======================================================
// MOCK DATA
// Later replace with API fetch()
// ======================================================

const appointments = [

    {
        id: "apt-001",

        date: "2026-08-01",

        doctor: "Dr. Rai",

        specialization: "Cardiology",

        clinic: "City Health Clinic",

        department: "Cardiology",

        time: "10:30 AM",

        status: "Completed",

        fee: 800
    },

    {
        id: "apt-002",

        date: "2026-07-20",

        doctor: "Dr. Shah",

        specialization: "Dermatology",

        clinic: "ABC Medical Center",

        department: "Dermatology",

        time: "02:00 PM",

        status: "Cancelled",

        fee: 700
    },

    {
        id: "apt-003",

        date: "2026-07-10",

        doctor: "Dr. Sharma",

        specialization: "General Medicine",

        clinic: "City Health Clinic",

        department: "General Medicine",

        time: "11:00 AM",

        status: "Completed",

        fee: 500
    },

    {
        id: "apt-004",

        date: "2026-06-25",

        doctor: "Dr. Rai",

        specialization: "Cardiology",

        clinic: "City Health Clinic",

        department: "Cardiology",

        time: "09:30 AM",

        status: "Expired",

        fee: 800
    }

];


// ======================================================
// DOM
// ======================================================

const tableBody =
    document.getElementById("appointmentHistoryBody");

const appointmentCount =
    document.getElementById("appointmentCount");

const searchInput =
    document.getElementById("searchInput");

const statusFilter =
    document.getElementById("statusFilter");

const detailModal =
    document.getElementById("detailModal");

const modalBody =
    document.getElementById("modalBody");

const closeModalButton =
    document.getElementById("closeModal");


// ======================================================
// RENDER APPOINTMENTS
// ======================================================

function renderAppointments(list) {

    appointmentCount.textContent =
        `${list.length} appointment${list.length !== 1 ? "s" : ""}`;


    if (!list.length) {

        tableBody.innerHTML = `

            <tr>

                <td
                    colspan="8"
                    style="
                        text-align:center;
                        color:#94a3b8;
                        padding:30px;
                    "
                >

                    No appointment history found.

                </td>

            </tr>

        `;

        return;
    }


    tableBody.innerHTML = list.map(appointment => `

        <tr>

            <td>
                ${formatDate(appointment.date)}
            </td>

            <td>
                <strong>
                    ${escapeHtml(appointment.doctor)}
                </strong>
            </td>

            <td>
                ${escapeHtml(appointment.specialization)}
            </td>

            <td>
                ${escapeHtml(appointment.clinic)}
            </td>

            <td>
                ${escapeHtml(appointment.department)}
            </td>

            <td>
                ${escapeHtml(appointment.time)}
            </td>

            <td>

                <span class="
                    status
                    ${appointment.status.toLowerCase()}
                ">

                    ${escapeHtml(appointment.status)}

                </span>

            </td>

            <td>

                <button
                    class="btn-view"
                    onclick="viewAppointment('${appointment.id}')"
                >

                    View

                </button>

            </td>

        </tr>

    `).join("");
}


// ======================================================
// SEARCH + FILTER
// ======================================================

function filterAppointments() {

    const search =
        searchInput.value.toLowerCase().trim();

    const status =
        statusFilter.value;


    const filtered =
        appointments.filter(appointment => {

            const matchesSearch =

                appointment.doctor
                    .toLowerCase()
                    .includes(search)

                ||

                appointment.clinic
                    .toLowerCase()
                    .includes(search)

                ||

                appointment.specialization
                    .toLowerCase()
                    .includes(search);


            const matchesStatus =

                status === "all" ||
                appointment.status === status;


            return matchesSearch && matchesStatus;
        });


    renderAppointments(filtered);
}


searchInput.addEventListener(
    "input",
    filterAppointments
);

statusFilter.addEventListener(
    "change",
    filterAppointments
);


// ======================================================
// VIEW APPOINTMENT DETAILS
// ======================================================

function viewAppointment(id) {

    const appointment =
        appointments.find(
            item => item.id === id
        );


    if (!appointment) {
        return;
    }


    modalBody.innerHTML = `

        <div class="appointment-details">

            <p>
                <strong>Doctor:</strong>
                ${escapeHtml(appointment.doctor)}
            </p>

            <p>
                <strong>Specialization:</strong>
                ${escapeHtml(appointment.specialization)}
            </p>

            <p>
                <strong>Clinic:</strong>
                ${escapeHtml(appointment.clinic)}
            </p>

            <p>
                <strong>Department:</strong>
                ${escapeHtml(appointment.department)}
            </p>

            <p>
                <strong>Date:</strong>
                ${formatDate(appointment.date)}
            </p>

            <p>
                <strong>Time:</strong>
                ${escapeHtml(appointment.time)}
            </p>

            <p>
                <strong>Consultation Fee:</strong>
                Rs. ${appointment.fee}
            </p>

            <p>
                <strong>Status:</strong>

                <span class="
                    status
                    ${appointment.status.toLowerCase()}
                ">

                    ${escapeHtml(appointment.status)}

                </span>

            </p>

        </div>

    `;


    detailModal.classList.remove("hidden");

    detailModal.setAttribute(
        "aria-hidden",
        "false"
    );
}


// ======================================================
// CLOSE MODAL
// ======================================================

function closeModal() {

    detailModal.classList.add("hidden");

    detailModal.setAttribute(
        "aria-hidden",
        "true"
    );
}


closeModalButton.addEventListener(
    "click",
    closeModal
);


detailModal.addEventListener(
    "click",
    function (event) {

        if (event.target === detailModal) {
            closeModal();
        }

    }
);


// ======================================================
// FORMAT DATE
// ======================================================

function formatDate(dateString) {

    const date =
        new Date(dateString + "T00:00:00");

    return date.toLocaleDateString(
        "en-US",
        {
            month: "short",
            day: "2-digit",
            year: "numeric"
        }
    );
}


// ======================================================
// SECURITY
// ======================================================

function escapeHtml(value) {

    if (
        value === null ||
        value === undefined
    ) {
        return "";
    }


    return String(value).replace(
        /[&<>"']/g,
        char => ({
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        }[char])
    );
}


// ======================================================
// INITIALIZE
// ======================================================

document.addEventListener(
    "DOMContentLoaded",
    () => {

        renderAppointments(appointments);

    }
);