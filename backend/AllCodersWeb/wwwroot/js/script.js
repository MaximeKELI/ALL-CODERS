// Navigation
document.addEventListener('DOMContentLoaded', () => {
    const navToggle = document.querySelector('.nav-toggle');
    const navMenu = document.querySelector('.nav-menu');
    
    navToggle?.addEventListener('click', () => {
        navMenu?.classList.toggle('active');
    });

    // Changement de couleur de la navbar au scroll
    window.addEventListener('scroll', () => {
        const navbar = document.querySelector('.navbar');
        if (window.scrollY > 50) {
            navbar?.classList.add('scrolled');
        } else {
            navbar?.classList.remove('scrolled');
        }
    });
});

// Création des particules
function createParticles() {
    const particlesContainer = document.querySelector('.particles');
    const numberOfParticles = 50;

    for (let i = 0; i < numberOfParticles; i++) {
        const particle = document.createElement('div');
        particle.className = 'particle';
        
        // Position aléatoire
        particle.style.left = Math.random() * 100 + '%';
        particle.style.top = Math.random() * 100 + '%';
        
        // Taille aléatoire
        const size = Math.random() * 5 + 2;
        particle.style.width = size + 'px';
        particle.style.height = size + 'px';
        
        // Animation aléatoire
        particle.style.animation = `float ${Math.random() * 3 + 2}s linear infinite`;
        
        particlesContainer?.appendChild(particle);
    }
}

// Création des éléments flottants
function createFloatingElements() {
    const container = document.querySelector('.floating-elements');
    const colors = ['#2196f3', '#ff1b6b', '#00ff9d'];
    const numberOfElements = 8;

    for (let i = 0; i < numberOfElements; i++) {
        const element = document.createElement('div');
        element.className = 'floating-element';
        
        // Style aléatoire
        element.style.background = colors[Math.floor(Math.random() * colors.length)];
        element.style.left = Math.random() * 100 + '%';
        element.style.top = Math.random() * 100 + '%';
        
        // Animation aléatoire
        const duration = Math.random() * 10 + 5;
        element.style.animation = `float ${duration}s ease-in-out infinite`;
        
        container?.appendChild(element);
    }
}

// Animation de typing
function typeWriter(element, text, speed = 100) {
    let i = 0;
    function type() {
        if (i < text.length) {
            if (element) {
                element.innerHTML += text.charAt(i);
                i++;
                setTimeout(type, speed);
            }
        }
    }
    type();
}

// Initialisation
document.addEventListener('DOMContentLoaded', () => {
    createParticles();
    createFloatingElements();

    // Animation des cartes de service au scroll
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate');
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.service-card').forEach(card => {
        observer.observe(card);
    });

    // Smooth scroll pour les liens d'ancrage
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}); 