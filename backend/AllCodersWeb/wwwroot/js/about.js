document.addEventListener('DOMContentLoaded', () => {
    // Animation des compteurs
    const counters = document.querySelectorAll('.stat-number');
    
    const animateCounter = (counter) => {
        const target = parseInt(counter.getAttribute('data-target'));
        let current = 0;
        const increment = target / 100;
        
        const updateCounter = () => {
            if (current < target) {
                current += increment;
                counter.textContent = Math.ceil(current);
                requestAnimationFrame(updateCounter);
            } else {
                counter.textContent = target;
            }
        };
        
        updateCounter();
    };

    // Observer pour déclencher l'animation des compteurs
    const statsObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                statsObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    counters.forEach(counter => {
        statsObserver.observe(counter);
    });

    // Animation des icônes technologiques
    const techItems = document.querySelectorAll('.tech-item');
    
    techItems.forEach(item => {
        item.addEventListener('mouseenter', () => {
            const icon = item.querySelector('i');
            icon.style.transform = 'scale(1.2) rotate(360deg)';
            icon.style.transition = 'transform 0.5s ease';
        });

        item.addEventListener('mouseleave', () => {
            const icon = item.querySelector('i');
            icon.style.transform = 'scale(1) rotate(0deg)';
        });
    });

    // Effet de particules pour l'arrière-plan
    const createParticle = () => {
        const particle = document.createElement('div');
        particle.className = 'tech-particle';
        
        const size = Math.random() * 5 + 2;
        particle.style.width = `${size}px`;
        particle.style.height = `${size}px`;
        
        particle.style.left = Math.random() * 100 + '%';
        particle.style.top = Math.random() * 100 + '%';
        
        particle.style.animation = `float ${Math.random() * 3 + 2}s linear infinite`;
        
        return particle;
    };

    const aboutSection = document.querySelector('.about');
    if (aboutSection) {
        for (let i = 0; i < 30; i++) {
            aboutSection.appendChild(createParticle());
        }
    }

    // Animation des textes au scroll
    const textElements = document.querySelectorAll('.about-text p, .gradient-text');
    
    const textObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, { threshold: 0.1 });

    textElements.forEach(element => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(20px)';
        element.style.transition = 'all 0.5s ease';
        textObserver.observe(element);
    });
}); 