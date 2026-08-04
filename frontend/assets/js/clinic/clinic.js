async function loadDashboard() {
    try {
        const response = await fetch("/api/dashboard/stats");
        const data = await response.json();

        document.getElementById("total-doctors").innerText = data.totalDoctors;
        document.getElementById("total-patients").innerText = data.totalPatients;
        const totalClinics = document.getElementById("total-clinics");
        if (totalClinics) totalClinics.innerText = data.totalClinics;
        document.getElementById("today-appointments").innerText = data.todayAppointments;
        document.getElementById("available-doctors").innerText = data.availableDoctors;
        document.getElementById("busy-doctors").innerText = data.busyDoctors;
        document.getElementById("new-patients").innerText = data.newPatients;

    } catch (error) {
        console.error("Error loading dashboard:", error);
    }
}

window.onload = loadDashboard;
