// Change this port to match your running .NET local web server port
requireRole("Clinic"); // or "SuperAdmin" for admin pages, "Patient" for patient pages

async function loadDashboard() {
    try {
        const response = await fetch("API_BASE_URL");
        const data = await response.json();

        console.log(data)
        document.getElementById("total-doctors").innerText = data.totalDoctors;
        document.getElementById("today-appointments").innerText = data.todayAppointments;
        document.getElementById("available-doctors").innerText = data.availableDoctors;
        document.getElementById("busy-doctors").innerText = data.busyDoctors;

    } catch (error) {
        console.error("Error loading dashboard:", error);
    }
}


window.onload = loadDashboard;
