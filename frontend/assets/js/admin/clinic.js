
document.addEventListener('DOMContentLoaded', () => {
    fetchClinics();
});

// 1. Fetch all clinics and populate tables
async function fetchClinics() {
    try {
        const token = localStorage.getItem('token'); // Get stored JWT Token if needed
        const response = await fetch(`${API_BASE_URL}/superadmin/clinics`, {
            method: 'GET',
            headers: authHeaders()
        });

        if (!response.ok) {
            throw new Error(`Failed to fetch clinics: ${response.statusText}`);
        }

        const clinics = await response.json();
        renderTables(clinics);
    } catch (error) {
        console.error('Error fetching clinics:', error);
        showErrorInTables('Failed to load clinic data.');
    }
}

// 2. Separate clinics into Approved, Pending, and Suspended tables
function renderTables(clinics) {
    const approvedBody = document.getElementById('approveClinicTableBody');
    const pendingBody = document.getElementById('pendingClinicTableBody');
    const suspendBody = document.getElementById('suspendClinicTableBody');

    // Filter clinics by status (assuming backend returns string status: "Approved", "Pending", "Suspended")
    const activeClinics = clinics.filter(c => c.status?.toLowerCase() === 'active');
    const pendingClinics = clinics.filter(c => c.status?.toLowerCase() === 'pending');
    const suspendedClinics = clinics.filter(c => c.status?.toLowerCase() === 'suspended');

    approvedBody.innerHTML = generateRows(activeClinics, 'Approved');
    pendingBody.innerHTML = generateRows(pendingClinics, 'Pending');
    suspendBody.innerHTML = generateRows(suspendedClinics, 'Suspended');
}

// 3. Generate HTML rows for each table section
function generateRows(clinics, currentSection) {
    if (clinics.length === 0) {
        return `<tr><td colspan="6" style="text-align: center; color: #94a3b8;">No ${currentSection.toLowerCase()} clinics found.</td></tr>`;
    }

    return clinics.map(clinic => {
        const fullAddress = `${clinic.address || ''}, ${clinic.city || ''}`.trim() || 'N/A';
        
        return `
            <tr>
                <td><strong>${escapeHtml(clinic.name)}</strong></td>
                <td>${escapeHtml(clinic.email)}</td>
                <td>${escapeHtml(clinic.phone || 'N/A')}</td>
                <td>${escapeHtml(fullAddress)}</td>
                <td>
                    <span class="status-badge status-${currentSection.toLowerCase()}">
                        ${clinic.status}
                    </span>
                </td>
                <td>
                    ${renderActionButtons(clinic.id, currentSection)}
                </td>
            </tr>
        `;
    }).join('');
}

// 4. Render Action Buttons based on section type
function renderActionButtons(clinicId, currentSection) {
    if (currentSection === 'Pending') {
        return `
            <button onclick="changeClinicStatus('${clinicId}', 'Approve')" style="background-color: #10b981; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-right: 4px;">Approve</button>
            <button onclick="changeClinicStatus('${clinicId}', 'Suspend')" style="background-color: #ef4444; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer;">Reject</button>
        `;
    } else if (currentSection === 'Approved') {
        return `
            <button onclick="changeClinicStatus('${clinicId}', 'Suspend')" style="background-color: #bc1111; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer;">Suspend</button>
        `;
    } else if (currentSection === 'Suspended') {
        return `
            <button onclick="changeClinicStatus('${clinicId}', 'Approve')" style="background-color: #10b981; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer;">Reactivate</button>
        `;
    }
    return '';
}

// 5. Trigger HTTP PATCH request to update status
async function changeClinicStatus(clinicId, actionName) {
    if (!confirm(`Are you sure you want to ${actionName.toLowerCase()} this clinic?`)) return;

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_BASE_URL}/clinics/${clinicId}/status`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': token ? `Bearer ${token}` : ''
            },
            body: JSON.stringify({ action: actionName }) // Matches UpdateClinicStatusBody record
        });

        if (response.ok) {
            alert(`Clinic status successfully updated to ${actionName}!`);
            fetchClinics(); // Refresh tables after update
        } else {
            const errorMessage = await response.text();
            alert(`Failed to update status: ${errorMessage}`);
        }
    } catch (error) {
        console.error('Error updating clinic status:', error);
        alert('Server error occurred while updating status.');
    }
}

// Utility: Show error in tables when fetch fails
function showErrorInTables(msg) {
    const errorRow = `<tr><td colspan="6" style="text-align: center; color: #ef4444;">${msg}</td></tr>`;
    document.getElementById('approveClinicTableBody').innerHTML = errorRow;
    document.getElementById('pendingClinicTableBody').innerHTML = errorRow;
    document.getElementById('suspendClinicTableBody').innerHTML = errorRow;
}

// Utility: Avoid XSS
function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
