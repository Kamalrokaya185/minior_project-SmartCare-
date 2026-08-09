
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
        console.log(clinics);
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
        const clinicDataJson = encodeURIComponent(JSON.stringify(clinic));
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
                    <button onclick="detailModal('${clinicDataJson}')" style="background-color: #10b981; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-right: 4px;">View</button>
                    ${renderActionButtons(clinic.id, currentSection)}
                </td>
            </tr>
        `;
    }).join('');
}


function detailModal(clinicDataEncoded) {
    // 1. Decode and parse the clinic object safely
    let clinic = {};
    try {
        clinic = JSON.parse(decodeURIComponent(clinicDataEncoded));
    } catch (error) {
        console.error("Error parsing clinic data:", error);
        return;
    }

    // 2. Remove existing modal if open
    const existingModal = document.getElementById('clinicDetailModal');
    if (existingModal) {
        existingModal.remove();
    }

    // Format dates cleanly
    const formatDate = (isoString) => {
        if (!isoString) return 'N/A';
        const date = new Date(isoString);
        return isNaN(date.getTime()) ? 'N/A' : date.toLocaleString();
    };

    const createdAt = formatDate(clinic.createdAtUtc);
    const approvedAt = formatDate(clinic.approvedAtUtc);
    const location = [clinic.city, clinic.state].filter(Boolean).join(', ') || 'N/A';

// <div class="detail-row">
//     <strong>Clinic ID:</strong> 
//     <span class="mono-id">${escapeHtml(clinic.id || 'N/A')}</span>
// </div>

    // 3. Create dynamic modal overlay
    const modalHTML = `
        <div id="clinicDetailModal" class="modal-overlay">
            <div class="modal-card">
                <div class="modal-header">
                    <h3>${escapeHtml(clinic.name || 'Clinic Details')}</h3>
                    <button class="modal-close-btn" onclick="closeClinicModal()">&times;</button>
                </div>
                
                <div class="modal-body">
                    
                    <div class="detail-row">
                        <strong>Email:</strong> 
                        <span>${escapeHtml(clinic.email || 'N/A')}</span>
                    </div>
                    <div class="detail-row">
                        <strong>Phone:</strong> 
                        <span>${escapeHtml(clinic.phone || 'N/A')}</span>
                    </div>
                    <div class="detail-row">
                        <strong>Location:</strong> 
                        <span>${escapeHtml(location)}</span>
                    </div>
                    <div class="detail-row">
                        <strong>Slug:</strong> 
                        <span>${escapeHtml(clinic.slug || 'N/A')}</span>
                    </div>
                    <div class="detail-row">
                        <strong>Status:</strong> 
                        <span class="status-badge status-${(clinic.status || '').toLowerCase()}">${clinic.status || 'N/A'}</span>
                    </div>
                    <div class="detail-row">
                        <strong>Created At:</strong> 
                        <span>${createdAt}</span>
                    </div>
                    <div class="detail-row">
                        <strong>Approved At:</strong> 
                        <span>${approvedAt}</span>
                    </div>
                </div>

                <div class="modal-footer">
                    <button onclick="handleApproveClinic('${clinic.id}')" style="background-color: #10b981; color: white; border: none; padding: 8px 14px; border-radius: 4px; cursor: pointer; font-weight: 500;">
                        Approve
                    </button>
                    <button onclick="closeClinicModal()" style="background-color: #64748b; color: white; border: none; padding: 8px 14px; border-radius: 4px; cursor: pointer; font-weight: 500;">
                        Close
                    </button>
                </div>
            </div>
        </div>
    `;

    // 4. Inject into document body
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    injectModalStyles();
}

// Function triggered by clicking Approve inside the modal
function handleApproveClinic(clinicId) {
    console.log("Approved Clinic ID:", clinicId);
    alert(`Clinic ID ${clinicId} has been approved.`);
    closeClinicModal();
}

// Helper to remove modal from DOM
function closeClinicModal() {
    const modal = document.getElementById('clinicDetailModal');
    if (modal) {
        modal.remove();
    }
}

// Dynamic CSS Injection
function injectModalStyles() {
    if (document.getElementById('clinicModalDynamicStyles')) return;

    const style = document.createElement('style');
    style.id = 'clinicModalDynamicStyles';
    style.textContent = `
        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            background: rgba(15, 23, 42, 0.55);
            backdrop-filter: blur(3px);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 1000;
        }
        .modal-card {
            background: #ffffff;
            border-radius: 10px;
            width: 90%;
            max-width: 520px;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
            overflow: hidden;
            animation: modalFadeIn 0.2s ease-out;
        }
        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1rem 1.25rem;
            border-bottom: 1px solid #e2e8f0;
            background: #f8fafc;
        }
        .modal-header h3 {
            margin: 0;
            font-size: 1.15rem;
            color: #0f172a;
            font-weight: 600;
        }
        .modal-close-btn {
            background: none;
            border: none;
            font-size: 1.5rem;
            line-height: 1;
            color: #64748b;
            cursor: pointer;
        }
        .modal-close-btn:hover { color: #0f172a; }
        .modal-body {
            padding: 1.25rem;
            display: flex;
            flex-direction: column;
            gap: 0.85rem;
        }
        .detail-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 1px dashed #f1f5f9;
            padding-bottom: 0.5rem;
            font-size: 0.9rem;
        }
        .detail-row strong { color: #475569; font-weight: 600; }
        .detail-row span { color: #0f172a; text-align: right; }
        .mono-id {
            font-family: monospace;
            font-size: 0.8rem;
            background: #f1f5f9;
            padding: 2px 6px;
            border-radius: 4px;
            color: #334155 !important;
        }
        .modal-footer {
            padding: 1rem 1.25rem;
            border-top: 1px solid #e2e8f0;
            display: flex;
            justify-content: flex-end;
            gap: 10px;
            background: #f8fafc;
        }
        @keyframes modalFadeIn {
            from { opacity: 0; transform: translateY(-10px); }
            to { opacity: 1; transform: translateY(0); }
        }
    `;
    document.head.appendChild(style);
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
        const response = await fetch(`${API_BASE_URL}/superadmin/clinics/${clinicId}/status`, {
            method: 'PATCH',
            headers: authHeaders(),
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
