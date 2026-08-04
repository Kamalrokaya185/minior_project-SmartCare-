const apiUrl = "/api/patients";
const body = document.getElementById("patient-table-body");
const dialog = document.getElementById("patient-dialog");
const form = document.getElementById("patient-form");
const message = document.getElementById("message");

function showMessage(text, type) { message.textContent = text; message.className = `message ${type}`; message.hidden = false; }
function escapeHtml(value) { const element = document.createElement("span"); element.textContent = value ?? ""; return element.innerHTML; }
function patientMarkup(patient) { return `<tr><td>${escapeHtml(patient.name)}</td><td>${patient.userId}</td><td>${patient.age}</td><td>${escapeHtml(patient.gender)}</td><td>${escapeHtml(patient.contactno)}</td><td>${escapeHtml(patient.address)}</td><td><button class="edit-button" data-id="${patient.patientId}">Edit</button></td></tr>`; }

async function loadPatients() {
    body.innerHTML = '<tr><td colspan="7" class="loading">Loading patients…</td></tr>';
    try {
        const response = await fetch(apiUrl);
        if (!response.ok) throw new Error("Unable to load patients.");
        const patients = await response.json();
        body.innerHTML = patients.length ? patients.map(patientMarkup).join("") : '<tr><td colspan="7" class="loading">No patients have been added yet.</td></tr>';
        body.querySelectorAll(".edit-button").forEach(button => button.addEventListener("click", () => openEdit(button.dataset.id, patients)));
    } catch (error) { body.innerHTML = `<tr><td colspan="7" class="loading">${escapeHtml(error.message)}</td></tr>`; showMessage("The patient API is unavailable. Start SmartCare and refresh this page.", "error"); }
}
function openEdit(id, patients) {
    const patient = patients.find(item => item.patientId === Number(id)); if (!patient) return;
    document.getElementById("form-title").textContent = "Edit patient"; document.getElementById("patient-id").value = patient.patientId; document.getElementById("name").value = patient.name; document.getElementById("user-id").value = patient.userId; document.getElementById("age").value = patient.age; document.getElementById("gender").value = patient.gender; document.getElementById("contactno").value = patient.contactno; document.getElementById("address").value = patient.address; dialog.showModal();
}
document.getElementById("add-patient").addEventListener("click", () => { form.reset(); document.getElementById("patient-id").value = ""; document.getElementById("form-title").textContent = "Add patient"; dialog.showModal(); });
document.getElementById("close-dialog").addEventListener("click", () => dialog.close()); document.getElementById("cancel-dialog").addEventListener("click", () => dialog.close());
form.addEventListener("submit", async event => {
    event.preventDefault(); const id = Number(document.getElementById("patient-id").value); const patient = { patientId: id || 0, name: document.getElementById("name").value.trim(), userId: Number(document.getElementById("user-id").value), age: Number(document.getElementById("age").value), gender: document.getElementById("gender").value.trim(), contactno: document.getElementById("contactno").value.trim(), address: document.getElementById("address").value.trim() };
    try { const response = await fetch(id ? `${apiUrl}/${id}` : apiUrl, { method: id ? "PUT" : "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(patient) }); if (!response.ok) throw new Error(await response.text() || "Could not save the patient."); dialog.close(); showMessage(`Patient ${id ? "updated" : "added"} successfully.`, "success"); loadPatients(); } catch (error) { showMessage(error.message, "error"); }
});
loadPatients();
