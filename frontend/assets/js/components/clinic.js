function toggleSidebar() {
const container = document.getElementById("sidebar-container");    
container.classList.toggle("active");    
const sidebar = document.getElementById("sidebar");
if (sidebar) { sidebar.classList.toggle("active");}
}
fetch("../../assets/components/sidepanel/clinic.html")
    .then(res => res.text())
    .then(data => {
        document.getElementById("sidebar-container").innerHTML = data;
    });
