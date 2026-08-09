const loginForm = document.getElementById("loginForm");
const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");
const errorMessage = document.getElementById("errorMessage");

function showError(message) {
    errorMessage.textContent = message;
    errorMessage.hidden = false;
}

function clearError() {
    errorMessage.textContent = "";
    errorMessage.hidden = true;
}

loginForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    clearError();

    const body = {
        email: emailInput.value.trim(),
        password: passwordInput.value
    };

    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        const result = await response.json();

        if (!response.ok) {
            const message = typeof result === "string" ? result : (result.title || result.error || "Login failed. Check your email and password.");
            showError(message);
            return;
        }

        saveSession(result);
        redirectByRole();

    } catch (err) {
        console.error("Login error:", err);
        showError("Something went wrong. Please check your connection and try again.");
    }
});