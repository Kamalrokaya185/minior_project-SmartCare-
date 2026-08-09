const API_BASE_URL = "https://localhost:7135/api/v1";

function saveSession(loginResponse) {
    localStorage.setItem("smartcare_token", loginResponse.token);
    localStorage.setItem("smartcare_roles", JSON.stringify(loginResponse.roles));
    localStorage.setItem("smartcare_user_id", loginResponse.userId);
    localStorage.setItem("smartcare_full_name", loginResponse.fullName);
    localStorage.setItem("smartcare_profile_id",loginResponse.profileid);
}

function getToken() {
    return localStorage.getItem("smartcare_token");
}

function getRoles() {
    try {
        return JSON.parse(localStorage.getItem("smartcare_roles") || "[]");
    } catch {
        return [];
    }
}

function hasRole(role) {
    return getRoles().includes(role);
}

function isLoggedIn() {
    return !!getToken();
}

function logout() {
    localStorage.removeItem("smartcare_token");
    localStorage.removeItem("smartcare_roles");
    localStorage.removeItem("smartcare_user_id");
    localStorage.removeItem("smartcare_full_name");
    window.location.href = "/login.html";
}

function authHeaders() {
    const token = getToken();
    return {
        "Content-Type": "application/json",
        "Accept": "application/json",
        ...(token ? { "Authorization": `Bearer ${token}` } : {})
    };
}

// Redirects to the right dashboard based on role — call this right after a successful login
function redirectByRole() {
    if (hasRole("SuperAdmin")) window.location.href = "/pages/admin/dashboard.html";
    else if (hasRole("Clinic")) window.location.href = "/pages/clinic/dashboard.html";
    else if (hasRole("Patient")) window.location.href = "/pages/patient/dashboard.html";
    else window.location.href = "/login.html";
}

// Call at the top of any protected page. Redirects away if not logged in / wrong role.
function requireRole(requiredRole) {
    if (!isLoggedIn()) {
        window.location.href = "/login.html";
        return false;
    }
    if (requiredRole && !hasRole(requiredRole)) {
        alert("You don't have access to this page.");
        window.location.href = "/login.html";
        return false;
    }
    return true;
}