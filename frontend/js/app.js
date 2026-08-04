const navToggle = document.querySelector('.nav-toggle');
const navLinks = document.querySelector('.nav-links');

if (navToggle && navLinks) {
    navToggle.addEventListener('click', () => {
        navLinks.classList.toggle('open');
    });

    document.querySelectorAll('.nav-links a').forEach((link) => {
        link.addEventListener('click', () => navLinks.classList.remove('open'));
    });
}

document.querySelectorAll('[data-year]').forEach((el) => {
    el.textContent = new Date().getFullYear();
});
