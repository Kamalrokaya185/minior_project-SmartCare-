
document.addEventListener('DOMContentLoaded', () => {
    fetchPatients();
});

// 1. Fetch all patient records from backend
async function fetchPatients() {
    const tableBody = document.getElementById('patient-table-body');
    const messageContainer = document.getElementById('message');

    // Show initial loading state
    tableBody.innerHTML = `
        <tr>
            <td colspan="4" style="text-align: center; color: #64748b; padding: 1.5rem;">
                Loading patients…
            </td>
        </tr>
    `;

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_BASE_URL}/superadmin/patients`, {
            method: 'GET',
            headers: authHeaders()
        });

        if (!response.ok) {
            throw new Error(`Failed to load data (Status ${response.status})`);
        }

        const patients = await response.json();
        renderPatientTable(patients);
    } catch (error) {
        console.error('Error fetching patients:', error);
        
        // Show error message block in UI
        if (messageContainer) {
            messageContainer.textContent = 'Failed to load patient records. Please try again.';
            messageContainer.className = 'message error';
            messageContainer.hidden = false;
        }

        tableBody.innerHTML = `
            <tr>
                <td colspan="4" style="text-align: center; color: #ef4444; padding: 1.5rem;">
                    Error loading patient records.
                </td>
            </tr>
        `;
    }
}

// 2. Populate the HTML table with backend data
function renderPatientTable(patients) {
    const tableBody = document.getElementById('patient-table-body');

    if (!patients || patients.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="4" style="text-align: center; color: #94a3b8; padding: 1.5rem;">
                    No patient records found.
                </td>
            </tr>
        `;
        return;
    }

    tableBody.innerHTML = patients.map(patient => {
        // Handle property names flexibly (camelCase vs PascalCase)
        const id = patient.id || patient.Id;
        const gender = patient.gender || patient.Gender || 'N/A';
        const dobRaw = patient.dateOfBirth || patient.DateOfBirth || patient.dob;
        const dob = dobRaw ? formatDate(dobRaw) : 'N/A';
        const nid = patient.nid || patient.NID || patient.nationalId || 'N/A';

        // Additional fields for View Details modal
        const emergencyContact = patient.emergencyContactName || patient.EmergencyContactName || 'N/A';
        const emergencyPhone = patient.emergencyContactPhone || patient.EmergencyContactPhone || 'N/A';
        const emergencyRelation = patient.emergencyContactRelationship || patient.EmergencyContactRelationship || 'N/A';

        return `
            <tr>
                <td><strong>${escapeHtml(gender)}</strong></td>
                <td>${escapeHtml(dob)}</td>
                <td>${escapeHtml(nid)}</td>
                <td>
                    <button type="button" 
                            class="btn btn-secondary" 
                            style="padding: 6px 12px; cursor: pointer; border-radius: 4px; border: 1px solid #cbd5e1; background: #ffffff;"
                            onclick="viewPatientDetails('${id}', '${escapeHtml(gender)}', '${escapeHtml(dob)}', '${escapeHtml(nid)}', '${escapeHtml(emergencyContact)}', '${escapeHtml(emergencyRelation)}', '${escapeHtml(emergencyPhone)}')">
                        <i class="fa-solid fa-eye"></i> View Details
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

// 3. Action: Display Modal with Full Details
function viewPatientDetails(id, gender, dob, nid, contactName, relation, contactPhone) {
    // Check if modal container already exists, or dynamically inject one
    let modal = document.getElementById('patientDetailModal');
    
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'patientDetailModal';
        modal.style.cssText = `
            position: fixed; top: 0; left: 0; width: 100%; height: 100%;
            background: rgba(0,0,0,0.5); display: flex; align-items: center;
            justify-content: center; z-index: 1000;
        `;
        document.body.appendChild(modal);
    }

    modal.innerHTML = `
        <div style="background: white; padding: 2rem; border-radius: 8px; width: 420px; max-width: 90%; position: relative; box-shadow: 0 4px 12px rgba(0,0,0,0.15);">
            <h3 style="margin-top: 0; margin-bottom: 1rem; color: #0f172a; border-bottom: 1px solid #e2e8f0; padding-bottom: 0.5rem;">
                Patient Details
            </h3>
            
            <div style="line-height: 1.8; color: #334155; font-size: 0.95rem;">
                <p style="margin: 0.4rem 0;"><strong>Patient ID:</strong> <span style="font-size: 0.85rem; color: #64748b;">${id}</span></p>
                <p style="margin: 0.4rem 0;"><strong>Gender:</strong> ${gender}</p>
                <p style="margin: 0.4rem 0;"><strong>Date of Birth:</strong> ${dob}</p>
                <p style="margin: 0.4rem 0;"><strong>National ID:</strong> ${nid}</p>
                <hr style="border: none; border-top: 1px dashed #e2e8f0; margin: 0.8rem 0;" />
                <p style="margin: 0.4rem 0;"><strong>Emergency Contact:</strong> ${contactName} (${relation})</p>
                <p style="margin: 0.4rem 0;"><strong>Emergency Phone:</strong> ${contactPhone}</p>
            </div>

            <div style="margin-top: 1.5rem; text-align: right;">
                <button type="button" 
                        onclick="closePatientModal()" 
                        style="background: #3b82f6; color: white; border: none; padding: 8px 16px; border-radius: 6px; cursor: pointer;">
                    Close
                </button>
            </div>
        </div>
    `;

    modal.style.display = 'flex';
}

// 4. Close Modal
function closePatientModal() {
    const modal = document.getElementById('patientDetailModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Helper: Format Date string nicely (e.g. 2001-05-10 -> May 10, 2001)
function formatDate(dateStr) {
    try {
        const date = new Date(dateStr);
        if (isNaN(date.getTime())) return dateStr;
        return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    } catch {
        return dateStr;
    }
}

// Helper: Security HTML escaping against XSS
function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}