// === Global Navigation & Footer Logic (Runs on all pages) ===
const navToggle = document.querySelector('.nav-toggle');
const navLinks = document.querySelector('.nav-links');

if (navToggle && navLinks) {
    navToggle.addEventListener('click', () => {
        navLinks.classList.toggle('open');
    });

    document.querySelectorAll('.nav-links a').forEach((link) => {
        link.addEventListener('click', () => navLinks.classList.remove('open'));
    });
}

// // Update copyright year
// document.querySelectorAll('[data-year]').forEach((el) => {
//     el.textContent = new Date().getFullYear();
// });

// // === Login Page Logic ===
// const form = document.querySelector('form');

// if (form) {
//     form.addEventListener('submit', (event) => {
//         event.preventDefault();
//         window.location.href = 'patient-dashboard.html';
//     });
// }

// Javascript for register page
const patientBtn = document.querySelector('[data-type="patient"]');
const clinicBtn = document.querySelector('[data-type="clinic"]');
const patientForm = document.getElementById("patient-form");
const clinicForm = document.getElementById("clinic-form");

patientBtn.onclick = () => {
    patientBtn.classList.add("active");
    clinicBtn.classList.remove("active");
    
    patientForm.hidden = false;
    clinicForm.hidden = true;
};

clinicBtn.onclick = () => {
    clinicBtn.classList.add("active");
    patientBtn.classList.remove("active");
    
    clinicForm.hidden = false;
    patientForm.hidden = true;
};