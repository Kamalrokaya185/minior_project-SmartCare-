async function loadDashboardCounts() {
    const token = getToken();
    console.log("1. JWT Token Check:", token ? "Token present" : "No token found!");

    if (!token) {
        console.error("No JWT token found in localStorage! Please log in again.");
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/dashboard/admin/counts`, {
            method: "GET",
            headers: authHeaders()
        });

        console.log("2. HTTP Response Status:", response.status);

        if (!response.ok) {
            console.error("Failed to load dashboard counts. Status:", response.status);
            return;
        }

        let rawData = await response.json();
        console.log("3. API Raw JSON Response:", rawData);

        // Extract inner object if wrapped in Result/ApiResponse wrapper
        let counts = rawData;
        if (rawData && typeof rawData === 'object') {
            if (rawData.value) counts = rawData.value;
            else if (rawData.data) counts = rawData.data;
            else if (rawData.result) counts = rawData.result;
        }

        // Create a case-insensitive lookup dictionary
        const normalizedCounts = {};
        if (counts && typeof counts === 'object') {
            Object.keys(counts).forEach(k => {
                normalizedCounts[k.toLowerCase()] = counts[k];
            });
        }

        // Target and update elements
        const statElements = document.querySelectorAll("[data-stat]");
        console.log(`4. Target HTML elements found: ${statElements.length}`);

        statElements.forEach((el) => {
            const key = el.getAttribute("data-stat");
            if (key) {
                const lowerKey = key.toLowerCase();
                if (lowerKey in normalizedCounts) {
                    el.textContent = normalizedCounts[lowerKey];
                    console.log(`Updated [data-stat="${key}"] ->`, normalizedCounts[lowerKey]);
                } else {
                    console.warn(`No match found in API response for data-stat="${key}"`);
                }
            }
        });

    } catch (err) {
        console.error("Error loading dashboard counts:", err);
    }
}

// Automatically load counts when the DOM is loaded
document.addEventListener("DOMContentLoaded", loadDashboardCounts);