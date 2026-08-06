

const doctors =[];

let currentDoctorId = null;

const tableBody = document.getElementById('doctorTableBody');
const detailModal = document.getElementById('detailModal');
const editModal = document.getElementById('editModal');
const modalBody = document.getElementById('modalBody');
const editForm = document.getElementById('editForm');
const formTitle = document.getElementById('formTitle');
const addDoctorBtn = document.getElementById('addDoctorBtn');
const tableSummary = document.querySelector('.table-summary');

function renderDoctors() {
    tableBody.innerHTML = '';
    doctors.forEach((doctor) => {
        const row = document.createElement('tr');
        row.innerHTML = `
      <td>${doctor.doctorName}</td>
      <td>${doctor.timeSlot}</td>
      <td>
        <div class="action-group">
          <button class="btn btn-view" data-action="view" data-id="${doctor.id}">View</button>
          <button class="btn btn-edit" data-action="edit" data-id="${doctor.id}">Edit</button>
          <button class="btn btn-delete" data-action="delete" data-id="${doctor.id}">Delete</button>
        </div>
      </td>
    `;
        tableBody.appendChild(row);
    });
}

function openModal(modal) {
    modal.classList.remove('hidden');
    modal.setAttribute('aria-hidden', 'false');
}

function closeModal(modal) {
    modal.classList.add('hidden');
    modal.setAttribute('aria-hidden', 'true');
}

function showDoctorDetails(id) {
    const doctor = doctors.find((item) => item.id === Number(id));
    if (!doctor) return;

    modalBody.innerHTML = `
    <div class="detail-list">
      <div class="detail-item"><strong>Doctor Name</strong>${doctor.doctorName}</div>
      <div class="detail-item"><strong>Doctor Specialization</strong>${doctor.specialization}</div>
      <div class="detail-item"><strong>Medical License Number</strong>${doctor.licenseNumber}</div>
      <div class="detail-item"><strong>Address</strong>${doctor.address}</div>
      <div class="detail-item"><strong>Email Address</strong>${doctor.email}</div>
      <div class="detail-item"><strong>Phone Number</strong>${doctor.phone}</div>
    </div>
  `;
    openModal(detailModal);
}

function openEditForm(id) {
    const doctor = doctors.find((item) => item.id === Number(id));
    if (!doctor) return;

    currentDoctorId = doctor.id;
    formTitle.textContent = `Edit ${doctor.doctorName}`;
    document.getElementById('doctorName').value = doctor.doctorName;
    document.getElementById('timeSlot').value = doctor.timeSlot;
    document.getElementById('specialization').value = doctor.specialization;
    document.getElementById('licenseNumber').value = doctor.licenseNumber;
    document.getElementById('address').value = doctor.address;
    document.getElementById('email').value = doctor.email;
    document.getElementById('phone').value = doctor.phone;
    openModal(editModal);
}

function openAddForm() {
    currentDoctorId = null;
    formTitle.textContent = 'Add New Doctor';
    document.getElementById('doctorName').value = '';
    const timeSlotField = document.getElementById('timeSlotField');
    if (timeSlotField) timeSlotField.classList.add('hidden');
    const timeSlotInput = document.getElementById('timeSlot');
    timeSlotInput.value = '';
    timeSlotInput.required = false;
    document.getElementById('specialization').value = '';
    document.getElementById('licenseNumber').value = '';
    document.getElementById('address').value = '';
    document.getElementById('email').value = '';
    document.getElementById('phone').value = '';
    openModal(editModal);
}

function openEditForm(id) {
    const doctor = doctors.find((item) => item.id === Number(id));
    if (!doctor) return;

    currentDoctorId = doctor.id;
    formTitle.textContent = `Edit ${doctor.doctorName}`;
    const patientField = document.getElementById('patientNameField');
    if (patientField) patientField.classList.remove('hidden');
    const timeSlotField = document.getElementById('timeSlotField');
    if (timeSlotField) timeSlotField.classList.remove('hidden');
    const timeSlotInput = document.getElementById('timeSlot');
    document.getElementById('doctorName').value = doctor.doctorName;
    timeSlotInput.value = doctor.timeSlot;
    timeSlotInput.required = true;
    document.getElementById('specialization').value = doctor.specialization;
    document.getElementById('licenseNumber').value = doctor.licenseNumber;
    document.getElementById('address').value = doctor.address;
    document.getElementById('email').value = doctor.email;
    document.getElementById('phone').value = doctor.phone;
    openModal(editModal);
}

function deleteDoctor(id) {
    const doctor = doctors.find((item) => item.id === Number(id));
    if (!doctor) return;

    const confirmed = window.confirm(`Delete ${doctor.doctorName} from the schedule?`);
    if (confirmed) {
        const index = doctors.findIndex((item) => item.id === Number(id));
        if (index !== -1) {
            doctors.splice(index, 1);
            renderDoctors();
        }
    }
}

tableBody.addEventListener('click', (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;

    const { action, id } = button.dataset;
    if (action === 'view') showDoctorDetails(id);
    if (action === 'edit') openEditForm(id);
    if (action === 'delete') deleteDoctor(id);
});

document.querySelectorAll('.modal-close').forEach((button) => {
    button.addEventListener('click', () => {
        const modal = button.closest('.modal');
        if (modal) closeModal(modal);
    });
});

document.querySelectorAll('.modal').forEach((modal) => {
    modal.addEventListener('click', (event) => {
        if (event.target === modal) closeModal(modal);
    });
});

editForm.addEventListener('submit', (event) => {
    event.preventDefault();

    const doctorData = {
        doctorName: document.getElementById('doctorName').value.trim(),
        timeSlot: document.getElementById('timeSlot').value.trim(),
        specialization: document.getElementById('specialization').value.trim(),
        licenseNumber: document.getElementById('licenseNumber').value.trim(),
        address: document.getElementById('address').value.trim(),
        email: document.getElementById('email').value.trim(),
        phone: document.getElementById('phone').value.trim()
    };

    if (currentDoctorId === null) {
        const nextId = doctors.length ? Math.max(...doctors.map((d) => d.id)) + 1 : 1;
        doctors.push({ id: nextId, ...doctorData });
    } else {
        const doctor = doctors.find((item) => item.id === currentDoctorId);
        if (!doctor) return;
        Object.assign(doctor, doctorData);
    }

    renderDoctors();
    closeModal(editModal);
    currentDoctorId = null;
});

addDoctorBtn.addEventListener('click', openAddForm);

renderDoctors();