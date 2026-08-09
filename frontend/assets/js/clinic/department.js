
requireRole("Clinic"); // or "SuperAdmin" for admin pages, "Patient" for patient pages
const CLINIC_ID = localStorage.getItem("smartcare_profile_id");

if (!CLINIC_ID) {
    alert("No clinic linked to this account. Please log in again.");
}

// ===== DOM references =====
const departmentsContainer = document.getElementById("departmentsContainer");
const addDepartmentBtn = document.getElementById("addDepartmentBtn");
const addDepartmentModal = document.getElementById("addDepartmentModal");
const addDepartmentForm = document.getElementById("addDepartmentForm");
const deptNameInput = document.getElementById("deptName");
const deptDescriptionInput = document.getElementById("deptDescription");

const detailModal = document.getElementById("detailModal");
const modalBody = document.getElementById("modalBody");

const addDoctorModal = document.getElementById("addDoctorModal");
const addDoctorForm = document.getElementById("addDoctorForm");
const doctorFormTitle = document.getElementById("doctorFormTitle");
const fullNameInput = document.getElementById("fullName");
const specializationInput = document.getElementById("specialization");
const departmentSelect = document.getElementById("departmentSelect");
const licenseNumberInput = document.getElementById("licenseNumber");
const genderInput = document.getElementById("gender");
const consultationfeeInput = document.getElementById("consultationfee");

const scheduleBody = document.getElementById("scheduleBody");
const scheduleTypeSelect = document.getElementById("scheduleType");
const dayOfWeekWrapper = document.getElementById("dayOfWeekWrapper");
const specificDateWrapper = document.getElementById("specificDateWrapper");
const effectiveFromWrapper = document.getElementById("effectiveFromWrapper");
const effectiveToWrapper = document.getElementById("effectiveToWrapper");
const startTimeWrapper = document.getElementById("startTimeWrapper");
const endTimeWrapper = document.getElementById("endTimeWrapper");
const slotDurationWrapper = document.getElementById("slotDurationWrapper");

const dayOfWeekInput = document.getElementById("dayOfWeek");
const specificDateInput = document.getElementById("specificDate");
const effectiveFromInput = document.getElementById("effectiveFrom");
const effectiveToInput = document.getElementById("effectiveTo");
const startTimeInput = document.getElementById("startTime");
const endTimeInput = document.getElementById("endTime");
const slotDurationInput = document.getElementById("slotDurationMinutes");

let currentViewMembershipId = null; // tracks which doctor's schedules are shown in the View modal

let allDepartments = [];
let editingMembershipId = null;
let editingDoctorProfileId = null;
let preselectedDepartmentId = null; // set when "Add Doctor" is clicked from a specific department card

scheduleTypeSelect.addEventListener("change", () => {
    const type = scheduleTypeSelect.value;

    const showAll = type === "recurring" || type === "onetime";
    startTimeWrapper.hidden = !showAll;
    endTimeWrapper.hidden = !showAll;
    slotDurationWrapper.hidden = !showAll;

    dayOfWeekWrapper.hidden = type !== "recurring";
    effectiveFromWrapper.hidden = type !== "recurring";
    effectiveToWrapper.hidden = type !== "recurring";
    specificDateWrapper.hidden = type !== "onetime";
});



// ===== Load everything: departments, then each department's doctor list =====
async function loadDepartmentsAndDoctors() {
    if (!CLINIC_ID) return;

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/departments`, { headers: authHeaders() });
        if (!response.ok) {
            departmentsContainer.innerHTML = `<p style="text-align:center;color:#94a3b8;">Failed to load departments.</p>`;
            return;
        }

        allDepartments = await response.json();
        console.log(allDepartments);

        if (!allDepartments.length) {
            departmentsContainer.innerHTML = `<p style="text-align:center;color:#94a3b8;">No departments yet. Click "Add Department" to create one.</p>`;
            return;
        }

        departmentsContainer.innerHTML = "";
        for (const dept of allDepartments) {
            await renderDepartmentCard(dept);
        }
    } catch (err) {
        console.error("Error loading departments:", err);
    }
}

async function renderDepartmentCard(dept) {
    const card = document.createElement("section");
    card.className = "card table-card";
    card.dataset.departmentId = dept.id;

    card.innerHTML = `
        <div class="table-header">
            <h2>${escapeHtml(dept.name)}</h2>
            <div class="table-header-actions">
                <span class="table-summary">${escapeHtml(dept.description || "")}</span>
                <button class="btn btn-primary" data-action="add-doctor" data-department-id="${dept.id}" type="button">Add Doctor</button>
            </div>
        </div>
        <div class="table-wrapper-approve">
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Specialization</th>
                        <th>License</th>
                        <th>ConsultationFee</th>
                        <th>Gender</th>
                        <th>Status</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody id="doctorBody-${dept.id}">
                    <tr><td colspan="5" style="text-align:center;color:#94a3b8;">Loading...</td></tr>
                </tbody>
            </table>
        </div>
    `;

    departmentsContainer.appendChild(card);
    await loadDoctorsForDepartment(dept.id);
}

async function loadDoctorsForDepartment(departmentId) {
    const tbody = document.getElementById(`doctorBody-${departmentId}`);
    if (!tbody) return;

    try {
        const response = await fetch(
            `${API_BASE_URL}/clinics/${CLINIC_ID}/doctors?departmentId=${departmentId}&activeOnly=false`,
            { headers: authHeaders() });

        if (!response.ok) {
            tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;color:#94a3b8;">Failed to load doctors.</td></tr>`;
            return;
        }

        const doctors = await response.json();
console.log(doctors);

        if (!doctors.length) {
            tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;color:#94a3b8;">No doctors in this department yet.</td></tr>`;
            return;
        }
        tbody.innerHTML = doctors.map((doc) => `
            <tr>
                <td>${escapeHtml(doc.fullName)}</td>
                <td>${escapeHtml(doc.specialization ?? "—")}</td>
                <td>${escapeHtml(doc.licenseNumber ?? "-")}</td>
                <td>${escapeHtml(doc.consultationFee ?? "-")}</td>
                <td>${escapeHtml(doc.gender ?? "-")}</td>
                <td>
                    <span class="status-pill ${doc.isActive ? "active" : "inactive"}">
                        ${doc.isActive ? "Active" : "Inactive"}
                    </span>
                </td>
                <td class="action-cell">
                    <button class="btn btn-view" data-action="view" data-id="${doc.clinicMembershipId}">View</button>
                    <button class="btn btn-edit" data-action="edit" data-id="${doc.clinicMembershipId}">Edit</button>
                    <button class="btn btn-danger" data-action="toggle" data-id="${doc.clinicMembershipId}">
                        ${doc.isActive ? "Deactivate" : "Reactivate"}
                    </button>
                </td>
            </tr>
        `).join("");
    } catch (err) {
        console.error(`Error loading doctors for department ${departmentId}:`, err);
    }
}

function escapeHtml(str) {
    if (!str) return "";
    return String(str).replace(/[&<>"']/g, (c) => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[c]));
}

// ===== Add Department =====
addDepartmentBtn.addEventListener("click", () => {
    addDepartmentForm.reset();
    showModal(addDepartmentModal);
});

addDepartmentForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    const body = {
        name: deptNameInput.value.trim(),
        description: deptDescriptionInput.value.trim() || null
    };

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/departments`, {
            method: "POST", headers: authHeaders(), body: JSON.stringify(body)
        });

        const result = await response.json();

        if (!response.ok) {
            alert(result.title || result || "Failed to add department.");
            return;
        }

        closeModal(addDepartmentModal);
        await loadDepartmentsAndDoctors();
    } catch (err) {
        console.error("Error adding department:", err);
        alert("Something went wrong while adding the department.");
    }
});

// ===== Delegated clicks: "Add Doctor" per department card, plus row actions =====
departmentsContainer.addEventListener("click", async (e) => {
    const addDoctorBtn = e.target.closest("[data-action='add-doctor']");
    if (addDoctorBtn) {
        preselectedDepartmentId = addDoctorBtn.getAttribute("data-department-id");
        openAddDoctorModal();
        return;
    }

    const rowBtn = e.target.closest("button[data-action]");
    if (!rowBtn) return;

    const membershipId = rowBtn.getAttribute("data-id");
    const action = rowBtn.getAttribute("data-action");

    if (action === "view") await openDetailModal(membershipId);
    if (action === "edit") await openEditDoctorModal(membershipId);
    if (action === "toggle") await toggleDoctorActive(membershipId, rowBtn);
});


scheduleBody.addEventListener("click", async (e) => {
    const btn = e.target.closest("button[data-action='toggle-schedule']");
    if (!btn) return;

    const scheduleId = btn.getAttribute("data-schedule-id");
    const isCurrentlyActive = btn.textContent.trim() === "Deactivate";

    if (!confirm(isCurrentlyActive ? "Deactivate this schedule?" : "Reactivate this schedule?")) return;

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/doctors/schedules/${scheduleId}/status`, {
            method: "PATCH", headers: authHeaders(), body: JSON.stringify({ isActive: !isCurrentlyActive })
        });

        if (!response.ok) { alert("Failed to update schedule status."); return; }

        await loadSchedules(currentViewMembershipId);
    } catch (err) {
        console.error("Error toggling schedule:", err);
    }
});

// ===== Populate department dropdown inside the doctor form =====
function populateDepartmentSelect(selectedId) {
    departmentSelect.innerHTML = `<option value="" disabled ${!selectedId ? "selected" : ""} >-- Select department --</option>` +
        allDepartments.map(d =>
            `<option value="${d.id}" ${d.id === selectedId ? "selected" : ""} >${escapeHtml(d.name)}</option>`
        ).join("");
}

// ===== Add Doctor =====
function openAddDoctorModal() {
    editingMembershipId = null;
    editingDoctorProfileId = null;
    doctorFormTitle.textContent = "Add Doctor";
    addDoctorForm.reset();
    setLicenseSpecializationEditable(true);
    populateDepartmentSelect(preselectedDepartmentId);
    resetScheduleFields();
    document.querySelector(".schedule-fieldset").hidden = false;
    showModal(addDoctorModal);
}

function resetScheduleFields() {
    scheduleTypeSelect.value = "";
    dayOfWeekWrapper.hidden = true;
    specificDateWrapper.hidden = true;
    effectiveFromWrapper.hidden = true;
    effectiveToWrapper.hidden = true;
    startTimeWrapper.hidden = true;
    endTimeWrapper.hidden = true;
    slotDurationWrapper.hidden = true;
}

// ===== View Doctor =====
async function openDetailModal(membershipId) {
    currentViewMembershipId = membershipId;

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/doctors/${membershipId}/schedules`, { headers: authHeaders() });
        if (!response.ok) { alert("Could not load doctor details."); return; }

        const doc = await response.json();
        modalBody.innerHTML = `
            <p><strong>Full Name:</strong> ${escapeHtml(doc.fullName)}</p>
            <p><strong>Specialization:</strong> ${escapeHtml(doc.specialization)}</p>
            <p><strong>License Number:</strong> ${escapeHtml(doc.licenseNumber)}</p>
            <p><strong>Gender:</strong> ${escapeHtml(doc.gender ?? "—")}</p>
            <p><strong>Consultation Fee:</strong> ${doc.consultationFee ?? "—"}</p>
            <p><strong>Status:</strong> ${doc.isActive ? "Active" : "Inactive"}</p>
        `;

        showModal(detailModal);
        await loadSchedules(membershipId);

    } catch (err) {
        console.error("Error loading doctor details:", err);
    }
}

const DAY_NAMES = { 1: "Monday", 2: "Tuesday", 3: "Wednesday", 4: "Thursday", 5: "Friday", 6: "Saturday", 7: "Sunday" };

async function loadSchedules(membershipId) {
    scheduleBody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:#94a3b8;">Loading...</td></tr>`;

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/doctors/${membershipId}/schedules`, { headers: authHeaders() });
        if (!response.ok) {
            scheduleBody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:#94a3b8;">Failed to load schedules.</td></tr>`;
            return;
        }

        const schedules = await response.json();

        if (!schedules.length) {
            scheduleBody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:#94a3b8;">No schedules set up yet.</td></tr>`;
            return;
        }

        scheduleBody.innerHTML = schedules.map((s) => `
            <tr>
                <td>${s.isRecurring ? "Recurring" : "One-time"}</td>
                <td>${s.isRecurring ? DAY_NAMES[s.dayOfWeek] : s.specificDate}</td>
                <td>${s.startTime} - ${s.endTime}</td>
                <td>${s.slotDurationMinutes} min</td>
                <td><span class="status-pill ${s.isActive ? "active" : "inactive"}">${s.isActive ? "Active" : "Inactive"}</span></td>
                <td>
                    <button class="btn btn-danger" data-action="toggle-schedule" data-schedule-id="${s.id}">
                        ${s.isActive ? "Deactivate" : "Reactivate"}
                    </button>
                </td>
            </tr>
        `).join("");
    } catch (err) {
        console.error("Error loading schedules:", err);
    }
}

// ===== Edit Doctor =====
async function openEditDoctorModal(membershipId) {
    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/doctors/${membershipId}`, { headers: authHeaders() });
        if (!response.ok) { alert("Could not load doctor for editing."); return; }

        const doc = await response.json();
        editingMembershipId = membershipId;
        editingDoctorProfileId = doc.doctorProfileId;

        doctorFormTitle.textContent = "Edit Doctor";
        fullNameInput.value = doc.fullName;
        specializationInput.value = doc.specialization;
        licenseNumberInput.value = doc.licenseNumber;
        genderInput.value = doc.gender ?? "";

        populateDepartmentSelect(doc.departmentId);
        setLicenseSpecializationEditable(false);
        resetScheduleFields();
        document.querySelector(".schedule-fieldset").hidden = true; // editing never touches schedules

        showModal(addDoctorModal);
    } catch (err) {
        console.error("Error opening edit modal:", err);
    }
}

function setLicenseSpecializationEditable(editable) {
    licenseNumberInput.disabled = !editable;
    specializationInput.disabled = !editable;
}

// ===== Doctor form submit (Add or Edit) =====
addDoctorForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    if (editingMembershipId) await updateDoctorProfile();
    else await createDoctor();
});

async function createDoctor() {
    const type = scheduleTypeSelect.value;
    const hasSchedule = type === "recurring" || type === "onetime";

    const body = {
        fullName: fullNameInput.value.trim(),
        licenseNumber: licenseNumberInput.value.trim(),
        specialization: specializationInput.value.trim(),
        gender: genderInput.value || null,
        departmentId: departmentSelect.value || null,
        consultationFee: null,

        isRecurring: hasSchedule ? (type === "recurring") : null,
        dayOfWeek: type === "recurring" ? Number(dayOfWeekInput.value) : null,
        specificDate: type === "onetime" ? specificDateInput.value : null,
        startTime: hasSchedule ? startTimeInput.value : null,
        endTime: hasSchedule ? endTimeInput.value : null,
        slotDurationMinutes: hasSchedule ? Number(slotDurationInput.value) : null,
        effectiveFrom: type === "recurring" ? (effectiveFromInput.value || todayIsoDate()) : null,
        effectiveTo: type === "recurring" ? (effectiveToInput.value || null) : null
    };

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/doctors`, {
            method: "POST", headers: authHeaders(), body: JSON.stringify(body)
        });
        const result = await response.json();

        if (!response.ok) {
            alert(result.title || result || "Failed to add doctor.");
            return;
        }

        closeModal(addDoctorModal);
        await loadDoctorsForDepartment(body.departmentId);
    } catch (err) {
        console.error("Error creating doctor:", err);
        alert("Something went wrong while adding the doctor.");
    }
}

function todayIsoDate() {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;
}

async function updateDoctorProfile() {
    const body = { fullName: fullNameInput.value.trim(), gender: genderInput.value || null };

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/doctors/${editingDoctorProfileId}/profile`, {
            method: "PUT", headers: authHeaders(), body: JSON.stringify(body)
        });

        if (!response.ok) {
            const result = await response.json();
            alert(result.title || result || "Failed to update doctor.");
            return;
        }

        closeModal(addDoctorModal);
        await loadDoctorsForDepartment(departmentSelect.value);
    } catch (err) {
        console.error("Error updating doctor:", err);
        alert("Something went wrong while updating the doctor.");
    }
}

// ===== Deactivate / Reactivate toggle =====
async function toggleDoctorActive(membershipId, btnEl) {
    const isCurrentlyActive = btnEl.textContent.trim() === "Deactivate";
    const newActiveState = !isCurrentlyActive;

    if (!confirm(isCurrentlyActive ? "Deactivate this doctor at this clinic?" : "Reactivate this doctor at this clinic?")) return;

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/doctors/${membershipId}/status`, {
            method: "PATCH", headers: authHeaders(), body: JSON.stringify({ isActive: newActiveState })
        });

        if (!response.ok) { alert("Failed to update doctor status."); return; }

        // Refresh just the department card this row belongs to
        const card = btnEl.closest("section[data-department-id]");
        if (card) await loadDoctorsForDepartment(card.dataset.departmentId);
    } catch (err) {
        console.error("Error toggling doctor status:", err);
    }
}

// ===== Modal helpers =====
function showModal(modalEl) { modalEl.classList.remove("hidden"); modalEl.setAttribute("aria-hidden", "false"); }
function closeModal(modalEl) { modalEl.classList.add("hidden"); modalEl.setAttribute("aria-hidden", "true"); }

document.querySelectorAll(".modal-close, [data-close]").forEach((el) => {
    el.addEventListener("click", () => {
        closeModal(addDepartmentModal);
        closeModal(addDoctorModal);
        closeModal(detailModal);
    });
});

[addDepartmentModal, addDoctorModal, detailModal].forEach((modalEl) => {
    modalEl.addEventListener("click", (e) => { if (e.target === modalEl) closeModal(modalEl); });
});

// ===== Initial load =====
document.addEventListener("DOMContentLoaded", () => {
    requireRole("Clinic");
    loadDepartmentsAndDoctors();
});
































// document.addEventListener('DOMContentLoaded', () => {
//     initModalEvents();
//     fetchDepartments();
// });


// // 1. Fetch & Render Departments
// async function fetchDepartments() {
//     const tableBody = document.getElementById('deparmentTableBody');
//     tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; color: #94a3b8;">Loading departments...</td></tr>`;

//     try {
//         const token = getToken();
//         const CURRENT_CLINIC_ID = localStorage.getItem("smartcare_profile_id");
//         const response = await fetch(`${API_BASE_URL}/clinics/${CURRENT_CLINIC_ID}/departments`, {
//             method: 'GET',
//             headers: authHeaders()
//         });

//         console.log(response);
//         if (!response.ok) {
//             throw new Error(`Error ${response.status}: ${response.statusText}`);
//         }

//         const departments = await response.json();
//         console.log(departments);
//         renderDepartmentTable(departments);
//     } catch (error) {
//         console.error('Error fetching departments:', error);
//         tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; color: #ef4444;">Failed to load departments.</td></tr>`;
//     }
// }

// // 2. Render Table Rows
// function renderDepartmentTable(departments) {
//     const tableBody = document.getElementById('deparmentTableBody');

//     if (!departments || departments.length === 0) {
//         tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; color: #94a3b8;">No departments found.</td></tr>`;
//         return;
//     }

//     tableBody.innerHTML = departments.map(dep => {
//         const name = dep.name || dep.Name || '';
//         const description = dep.description || dep.Description || 'N/A';
//         const status = dep.isActive ?? dep.IsActive ?? true;
//         const id = dep.id || dep.Id;

//         return `
//             <tr>
//                 <td><strong>${escapeHtml(name)}</strong></td>
//                 <td>${escapeHtml(description)}</td>
//                 <td>
//                     <span class="status-badge ${status ? 'status-approved' : 'status-suspend'}">
//                         ${status ? 'Active' : 'Inactive'}
//                     </span>
//                 </td>
//                 <td>
//                     <button type="button" class="btn btn-secondary" onclick="viewDetails('${id}', '${escapeHtml(name)}', '${escapeHtml(description)}')">
//                         <i class="fa-solid fa-eye"></i> View
//                     </button>
//                 </td>
//             </tr>
//         `;
//     }).join('');
// }

// // 3. Handle Add Department Form Submission
// const addForm = document.getElementById('addForm');
// addForm.addEventListener('submit', async (e) => {
//     e.preventDefault();

//     const nameInput = document.getElementById('departmentName');
//     // Targets the input inside the #description container
//     const descriptionInput = document.querySelector('#description input') || document.getElementById('description');

//     const payload = {
//         name: nameInput.value.trim(),
//         description: descriptionInput.value.trim()
//     };

//     try {
//         const token = getToken();
//         const CURRENT_CLINIC_ID = localStorage.getItem("smartcare_profile_id");
//         const response = await fetch(`${API_BASE_URL}/clinics/${CURRENT_CLINIC_ID}/departments`, {
//             method: 'POST',
//             headers: authHeaders(),
//             body: JSON.stringify(payload)
//         });

//         if (response.ok) {
//             alert('Department added successfully!');
//             addForm.reset();
//             closeModal('addModal');
//             fetchDepartments(); // Refresh list
//         } else {
//             const errorText = await response.text();
//             alert(`Failed to create department: ${errorText}`);
//         }
//     } catch (error) {
//         console.error('Error creating department:', error);
//         alert('Server error occurred while adding department.');
//     }
// });

// // 4. Modal Event Controllers
// function initModalEvents() {
//     const addBtn = document.getElementById('addDepartmentBtn');
//     if (addBtn) {
//         addBtn.addEventListener('click', () => openModal('addModal'));
//     }

//     // Close buttons handler
//     document.querySelectorAll('.modal-close, [data-close="add"]').forEach(btn => {
//         btn.addEventListener('click', (e) => {
//             const modal = e.target.closest('.modal');
//             if (modal) closeModal(modal.id);
//         });
//     });
// }

// function openModal(modalId) {
//     const modal = document.getElementById(modalId);
//     if (modal) {
//         modal.classList.remove('hidden');
//         modal.setAttribute('aria-hidden', 'false');
//     }
// }

// function closeModal(modalId) {
//     const modal = document.getElementById(modalId);
//     if (modal) {
//         modal.classList.add('hidden');
//         modal.setAttribute('aria-hidden', 'true');
//     }
// }

// function viewDetails(id, name, description) {
//     const modalBody = document.getElementById('modalBody');
//     modalBody.innerHTML = `
//         <p><strong>ID:</strong> ${id}</p>
//         <p><strong>Name:</strong> ${name}</p>
//         <p><strong>Description:</strong> ${description}</p>
//     `;
//     openModal('detailModal');
// }

// // Utility: XSS Protection
// function escapeHtml(str) {
//     if (!str) return '';
//     return String(str)
//         .replace(/&/g, '&amp;')
//         .replace(/</g, '&lt;')
//         .replace(/>/g, '&gt;')
//         .replace(/"/g, '&quot;');
// }