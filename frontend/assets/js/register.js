document.addEventListener("DOMContentLoaded", () => {
    // API Configuration
    const API_BASE_URL = "https://localhost:7135/api/v1/auth"; // Update port if your backend runs on a different port

    // DOM Elements
    const typeButtons = document.querySelectorAll(".type-btn");
    const patientForm = document.getElementById("patient-form");
    const clinicForm = document.getElementById("clinic-form");

    // -------------------------------------------------------------------------
    // 1. Toggle Switch Handling (Patient vs. Clinic)
    // -------------------------------------------------------------------------
    typeButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
            // Update active state on buttons
            typeButtons.forEach((b) => b.classList.remove("active"));
            btn.classList.add("active");

            const type = btn.getAttribute("data-type");

            if (type === "patient") {
                patientForm.removeAttribute("hidden");
                clinicForm.setAttribute("hidden", "true");
            } else if (type === "clinic") {
                clinicForm.removeAttribute("hidden");
                patientForm.setAttribute("hidden", "true");
            }
        });
    });

    // -------------------------------------------------------------------------
    // 2. Patient Form Submission
    // -------------------------------------------------------------------------
    patientForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const submitBtn = patientForm.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        submitBtn.textContent = "Registering...";

        // Collect inputs matching CreatePatientProfileCommand
        const payload = {
            fullName: document.getElementById("patient-full-name").value.trim(),
            gender: document.getElementById("gender").value,
            DateOfBirth: document.getElementById("patient-dob").value,
            nid: document.getElementById("patient-nid").value.trim(),
            emergencyContactPhone: document.getElementById("patient-emergency-phone").value.trim(),
            emergencyContactName: document.getElementById("patient-emergency-name").value.trim(),
            emergencyContactRelationship: document.getElementById("patient-emergency-relationship").value.trim(),
            email: document.getElementById("patient-email").value.trim(),
            password: document.getElementById("patient-password").value
        };

        try {
            const response = await fetch(`${API_BASE_URL}/patients-register`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(payload),
            });

            const data = await response.json();

            if (response.ok) {
                alert("Patient registration successful! Redirecting to login...");
                window.location.href = "login.html";
            } else {
                alert(`Registration failed: ${data.error || data.message || "Something went wrong"}`);
            }
        } catch (error) {
            console.error("Error submitting patient form:", error);
            alert("Network error. Please check if your backend server is running.");
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = "Register as Patient";
        }
    });

    // -------------------------------------------------------------------------
    // 3. Clinic Form Submission
    // -------------------------------------------------------------------------
    clinicForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const submitBtn = clinicForm.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        submitBtn.textContent = "Registering...";

        // Collect inputs matching RegisterClinicCommand
        const payload = {
            name: document.getElementById("clinic-name").value.trim(),
            slug: document.getElementById("clinic-slug-name").value.trim(),
            phone: document.getElementById("clinic-phone").value.trim(),
            address: document.getElementById("clinic-addresh").value.trim(),
            city: document.getElementById("clinic-city").value.trim(),
            state: document.getElementById("state").value,
            email: document.getElementById("clinic-email").value.trim(),
            password: document.getElementById("clinic-password").value,
            // Standardizing owner defaults from clinic details
            ownerEmail: document.getElementById("clinic-email").value.trim(),
            ownerPassword: document.getElementById("clinic-password").value,
            ownerFullName: document.getElementById("clinic-name").value.trim()
        };

        try {
            const response = await fetch(`${API_BASE_URL}/clinics-register`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(payload),
            });

            const data = await response.json();

            if (response.ok) {
                alert("Clinic registration successful! Pending Super Admin approval. Redirecting to login...");
                window.location.href = "login.html";
            } else {
                alert(`Registration failed: ${data.error || data.message || "Something went wrong"}`);
            }
        } catch (error) {
            console.error("Error submitting clinic form:", error);
            alert("Network error. Please check if your backend server is running.");
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = "Register Clinic";
        }
    });
});