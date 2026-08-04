const doctors = [
    {
        id: 1,
        doctorName: 'Dr. Aisha Khan',
        patientName: 'John Doe',
        timeSlot: '09:00 AM - 09:30 AM',
        specialization: 'General Medicine',
        licenseNumber: 'ML-10145',
        address: 'Block A, Suite 12, SmartCare Hospital',
        email: 'aisha.khan@smartcare.com',
        phone: '+1 555 123 4567'
    },
    {
        id: 2,
        doctorName: 'Dr. Samuel Lee',
        patientName: 'Mary Smith',
        timeSlot: '10:00 AM - 10:30 AM',
        specialization: 'Pediatrics',
        licenseNumber: 'ML-10146',
        address: 'Block B, Room 18, SmartCare Hospital',
        email: 'samuel.lee@smartcare.com',
        phone: '+1 555 234 5678'
    },
    {
        id: 3,
        doctorName: 'Dr. Daniel Patel',
        patientName: 'Liam Brown',
        timeSlot: '11:00 AM - 11:30 AM',
        specialization: 'Cardiology',
        licenseNumber: 'ML-10147',
        address: 'Block C, Floor 2, SmartCare Hospital',
        email: 'daniel.patel@smartcare.com',
        phone: '+1 555 345 6789'
    },
    {
        id: 4,
        doctorName: 'Dr. Nabila Rahman',
        patientName: 'Sara Khan',
        timeSlot: '12:00 PM - 12:30 PM',
        specialization: 'Dermatology',
        licenseNumber: 'ML-10148',
        address: 'Block D, Suite 4, SmartCare Hospital',
        email: 'nabila.rahman@smartcare.com',
        phone: '+1 555 456 7890'
    },
    {
        id: 5,
        doctorName: 'Dr. Malik Hassan',
        patientName: 'Ali Rehman',
        timeSlot: '01:00 PM - 01:30 PM',
        specialization: 'Neurology',
        licenseNumber: 'ML-10149',
        address: 'Block E, Room 22, SmartCare Hospital',
        email: 'malik.hassan@smartcare.com',
        phone: '+1 555 567 8901'
    },
    {
        id: 6,
        doctorName: 'Dr. Rida Shah',
        patientName: 'Haris Iqbal',
        timeSlot: '02:00 PM - 02:30 PM',
        specialization: 'Orthopedics',
        licenseNumber: 'ML-10150',
        address: 'Block F, Floor 3, SmartCare Hospital',
        email: 'rida.shah@smartcare.com',
        phone: '+1 555 678 9012'
    },
    {
        id: 7,
        doctorName: 'Dr. Jamil Malik',
        patientName: 'Noor Fatima',
        timeSlot: '03:00 PM - 03:30 PM',
        specialization: 'ENT',
        licenseNumber: 'ML-10151',
        address: 'Block G, Suite 8, SmartCare Hospital',
        email: 'jamil.malik@smartcare.com',
        phone: '+1 555 789 0123'
    },
    {
        id: 8,
        doctorName: 'Dr. Farah Ahmed',
        patientName: 'Ayesha Noor',
        timeSlot: '04:00 PM - 04:30 PM',
        specialization: 'Gynecology',
        licenseNumber: 'ML-10152',
        address: 'Block H, Room 10, SmartCare Hospital',
        email: 'farah.ahmed@smartcare.com',
        phone: '+1 555 890 1234'
    },
    {
        id: 9,
        doctorName: 'Dr. Zain Abbas',
        patientName: 'Omar Hassan',
        timeSlot: '05:00 PM - 05:30 PM',
        specialization: 'Psychiatry',
        licenseNumber: 'ML-10153',
        address: 'Block I, Floor 1, SmartCare Hospital',
        email: 'zain.abbas@smartcare.com',
        phone: '+1 555 901 2345'
    },
    {
        id: 10,
        doctorName: 'Dr. Sana Bukhari',
        patientName: 'Taha Ali',
        timeSlot: '06:00 PM - 06:30 PM',
        specialization: 'Family Medicine',
        licenseNumber: 'ML-10154',
        address: 'Block J, Suite 6, SmartCare Hospital',
        email: 'sana.bukhari@smartcare.com',
        phone: '+1 555 012 3456'
    }
];

let currentDoctorId = null;

const tableBody = document.getElementById('doctorTableBody');
const detailModal = document.getElementById('detailModal');
const editModal = document.getElementById('editModal');
const modalBody = document.getElementById('modalBody');
const editForm = document.getElementById('editForm');
const formTitle = document.getElementById('formTitle');

function renderDoctors() {
    tableBody.innerHTML = '';
    doctors.forEach((doctor) => {
        const row = document.createElement('tr');
        row.innerHTML = `
      <td>${doctor.doctorName}</td>
      <td>${doctor.patientName}</td>
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
    document.getElementById('patientName').value = doctor.patientName;
    document.getElementById('timeSlot').value = doctor.timeSlot;
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

    console.log(button.dataset)
    console.log(button)

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
    if (!currentDoctorId) return;

    const doctor = doctors.find((item) => item.id === currentDoctorId);
    if (!doctor) return;

    doctor.doctorName = document.getElementById('doctorName').value.trim();
    doctor.patientName = document.getElementById('patientName').value.trim();
    doctor.timeSlot = document.getElementById('timeSlot').value.trim();
    doctor.specialization = document.getElementById('specialization').value.trim();
    doctor.licenseNumber = document.getElementById('licenseNumber').value.trim();
    doctor.address = document.getElementById('address').value.trim();
    doctor.email = document.getElementById('email').value.trim();
    doctor.phone = document.getElementById('phone').value.trim();

    renderDoctors();
    closeModal(editModal);
    currentDoctorId = null;
});

renderDoctors();