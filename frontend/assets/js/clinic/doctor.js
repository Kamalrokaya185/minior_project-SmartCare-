requireRole("Clinic"); // or "SuperAdmin" for admin pages, "Patient" for patient pages
// CLINIC_ID now comes from the logged-in Clinic user's session, not a manual constant.
const CLINIC_ID = localStorage.getItem("smartcare_profile_id");

if (!CLINIC_ID) {
    alert("No clinic linked to this account. Please log in again.");
}

// ===== DOM references =====
const departmentBody = document.getElementById("departmentBody"); // doctor rows render here (name kept from HTML)
const addDoctorBtn = document.getElementById("addDoctorBtn");

const addModal = document.getElementById("addModal");
const addForm = document.getElementById("addForm");
const formTitle = document.getElementById("formTitle");

const fullNameInput = document.getElementById("fullName");
const specializationInput = document.getElementById("specialization");
const departmentInput = document.getElementById("department"); // becomes a <select> once populated
const licenseNumberInput = document.getElementById("licenseNumber");
const genderInput = document.getElementById("gender");

const detailModal = document.getElementById("detailModal");
const modalBody = document.getElementById("modalBody");

let editingMembershipId = null;
let editingDoctorProfileId = null;

// ===== Load department options into the form =====
async function loadDepartmentOptions() {
    
    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/departments`, { headers: authHeaders() });
        if (!response.ok) return;

        const departments = await response.json();

        // Works whether #department is an <input> (replaced below) or already a <select>
        const select = document.createElement("select");
        select.id = "department";
        select.required = true;
        select.innerHTML = `<option value="" disabled selected>-- Select department --</option>` +
            departments.map(d => `<option value="${d.id}">${escapeHtml(d.name)}</option>`).join("");

        departmentInput.replaceWith(select);
    } catch (err) {
        console.error("Error loading departments:", err);
    }

}


// 1. Fetch & Render Departments
async function fetchDepartments() {
    // const tableBody = document.getElementById('deparmentTableBody');
    // tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; color: #94a3b8;">Loading departments...</td></tr>`;

    try {
        const token = getToken();
        const CURRENT_CLINIC_ID = localStorage.getItem("smartcare_profile_id");
        const response = await fetch(`${API_BASE_URL}/clinics/${CURRENT_CLINIC_ID}/departments`, {
            method: 'GET',
            headers: authHeaders()
        });

        console.log(response);
        if (!response.ok) {
            throw new Error(`Error ${response.status}: ${response.statusText}`);
        }

        const departments = await response.json();
        console.log(departments);
        renderDepartmentTable(departments);
    } catch (error) {
        console.error('Error fetching departments:', error);
        // tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; color: #ef4444;">Failed to load departments.</td></tr>`;
    }
}
function renderDepartmentTable(department){
    
}

function getDepartmentSelect() {
    return document.getElementById("department");
}

// ===== Load & render doctor list (across ALL departments at this clinic) =====
async function loadDoctors() {
    if (!CLINIC_ID) return;

    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/doctors`, { headers: authHeaders() });
        if (!response.ok) {
            console.error("Failed to load doctors:", response.status);
            return;
        }

        const doctors = await response.json();
        renderDoctorTable(doctors);
    } catch (err) {
        console.error("Error loading doctors:", err);
    }
}

function renderDoctorTable(doctors) {
    departmentBody.innerHTML = "";

    if (!doctors.length) {
        departmentBody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:#94a3b8;">No doctors added yet.</td></tr>`;
        return;
    }

    doctors.forEach((doc) => {
        const row = document.createElement("tr");
        // Table headers say Name/Email/Phone/Address/Status/Action — DoctorProfile no longer has
        // Email/Phone/Address, so Specialization + License are shown in their place instead.
        row.innerHTML = `
            <td>${escapeHtml(doc.fullName)}</td>
            <td>${escapeHtml(doc.specialization ?? "—")}</td>
            <td>${escapeHtml(doc.licenseNumber ?? "—")}</td>
            <td>—</td>
            <td>${doc.isActive === false ? "Inactive" : "Active"}</td>
            <td class="action-cell">
                <button class="btn btn-view" data-action="view" data-id="${doc.clinicMembershipId}">View</button>
                <button class="btn btn-edit" data-action="edit" data-id="${doc.clinicMembershipId}">Edit</button>
                <button class="btn btn-danger" data-action="toggle" data-id="${doc.clinicMembershipId}">
                    ${doc.isActive === false ? "Reactivate" : "Deactivate"}
                </button>
            </td>
        `;
        departmentBody.appendChild(row);
    });
}

function escapeHtml(str) {
    if (!str) return "";
    return String(str).replace(/[&<>"']/g, (c) => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[c]));
}

// ===== Row action clicks =====
departmentBody.addEventListener("click", async (e) => {
    const btn = e.target.closest("button[data-action]");
    if (!btn) return;

    const membershipId = btn.getAttribute("data-id");
    const action = btn.getAttribute("data-action");

    if (action === "view") await openDetailModal(membershipId);
    if (action === "edit") await openEditModal(membershipId);
    if (action === "toggle") await toggleDoctorActive(membershipId, btn);
});

// ===== View detail modal =====
async function openDetailModal(membershipId) {
    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/doctors/${membershipId}`, { headers: authHeaders() });
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
    } catch (err) {
        console.error("Error loading doctor details:", err);
    }
}

// ===== Add / Edit modal =====
addDoctorBtn.addEventListener("click", async () => {
    editingMembershipId = null;
    editingDoctorProfileId = null;
    formTitle.textContent = "Add Doctor";
    addForm.reset();
    setLicenseSpecializationEditable(true);
    await loadDepartmentOptions();
    showModal(addModal);
});

async function openEditModal(membershipId) {
    try {
        const response = await fetch(`${API_BASE_URL}/clinics/${CLINIC_ID}/doctors/${membershipId}`, { headers: authHeaders() });
        if (!response.ok) { alert("Could not load doctor for editing."); return; }

        const doc = await response.json();
        editingMembershipId = membershipId;
        editingDoctorProfileId = doc.doctorProfileId;

        formTitle.textContent = "Edit Doctor";
        fullNameInput.value = doc.fullName;
        specializationInput.value = doc.specialization;
        licenseNumberInput.value = doc.licenseNumber;
        genderInput.value = doc.gender ?? "";

        await loadDepartmentOptions();
        const select = getDepartmentSelect();
        if (select && doc.departmentId) select.value = doc.departmentId;

        setLicenseSpecializationEditable(false);
        showModal(addModal);
    } catch (err) {
        console.error("Error opening edit modal:", err);
    }
}

function setLicenseSpecializationEditable(editable) {
    licenseNumberInput.disabled = !editable;
    specializationInput.disabled = !editable;
}

// ===== Form submit =====
addForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    if (editingMembershipId) await updateDoctorProfile();
    else await createDoctor();
});

async function createDoctor() {
    const select = getDepartmentSelect();
    const body = {
        fullName: fullNameInput.value.trim(),
        licenseNumber: licenseNumberInput.value.trim(),
        specialization: specializationInput.value.trim(),
        gender: genderInput.value || null,
        departmentId: select ? select.value : null,
        consultationFee: null
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

        closeModal(addModal);
        await loadDoctors();
    } catch (err) {
        console.error("Error creating doctor:", err);
        alert("Something went wrong while adding the doctor.");
    }
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

        closeModal(addModal);
        await loadDoctors();
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
        await loadDoctors();
    } catch (err) {
        console.error("Error toggling doctor status:", err);
    }
}

// ===== Modal helpers =====
function showModal(modalEl) { modalEl.classList.remove("hidden"); modalEl.setAttribute("aria-hidden", "false"); }
function closeModal(modalEl) { modalEl.classList.add("hidden"); modalEl.setAttribute("aria-hidden", "true"); }

document.querySelectorAll(".modal-close, [data-close]").forEach((el) => {
    el.addEventListener("click", () => { closeModal(addModal); closeModal(detailModal); });
});

[addModal, detailModal].forEach((modalEl) => {
    modalEl.addEventListener("click", (e) => { if (e.target === modalEl) closeModal(modalEl); });
});

// ===== Initial load =====
document.addEventListener("DOMContentLoaded", () => {
    requireRole("Clinic");
    loadDoctors();
    fetchDepartments();
});
