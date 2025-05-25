document.addEventListener('DOMContentLoaded', () => {
    // Gestion du chargement progressif des images
    const lazyLoadImages = () => {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.src; // Déclenche le chargement
                    img.classList.add('loading');
                    
                    img.onload = () => {
                        img.classList.remove('loading');
                        img.classList.add('loaded');
                        observer.unobserve(img);
                    };
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '50px'
        });

        document.querySelectorAll('.lazy-load').forEach(img => {
            imageObserver.observe(img);
        });
    };

    // Animation des projets au scroll
    const portfolioItems = document.querySelectorAll('.portfolio-item');
    
    const portfolioObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                
                // Animer les éléments de l'overlay
                const overlay = entry.target.querySelector('.portfolio-overlay');
                if (overlay) {
                    const elements = overlay.children;
                    Array.from(elements).forEach((el, index) => {
                        el.style.transitionDelay = `${index * 0.1}s`;
                        el.style.opacity = '1';
                        el.style.transform = 'translateY(0)';
                    });
                }
            }
        });
    }, { threshold: 0.1 });

    portfolioItems.forEach(item => {
        item.style.opacity = '0';
        item.style.transform = 'translateY(50px)';
        item.style.transition = 'all 0.5s ease';
        
        // Initialiser les éléments de l'overlay
        const overlay = item.querySelector('.portfolio-overlay');
        if (overlay) {
            const elements = overlay.children;
            Array.from(elements).forEach(el => {
                el.style.opacity = '0';
                el.style.transform = 'translateY(20px)';
                el.style.transition = 'all 0.5s ease';
            });
        }
        
        portfolioObserver.observe(item);
    });

    // Effet de hover 3D amélioré
    portfolioItems.forEach(item => {
        let rect = item.getBoundingClientRect();
        let mouseX = 0;
        let mouseY = 0;
        let isHovered = false;

        item.addEventListener('mouseenter', () => {
            isHovered = true;
            // Mettre à jour le rectangle au cas où la position a changé
            rect = item.getBoundingClientRect();
        });

        item.addEventListener('mouseleave', () => {
            isHovered = false;
            item.style.transform = `
                perspective(1000px)
                rotateX(0)
                rotateY(0)
                scale(1)
            `;
        });

        item.addEventListener('mousemove', (e) => {
            if (!isHovered) return;
            
            mouseX = e.clientX - rect.left;
            mouseY = e.clientY - rect.top;
            
            const xRotation = ((mouseY - rect.height / 2) / rect.height) * 20;
            const yRotation = ((mouseX - rect.width / 2) / rect.width) * 20;
            
            item.style.transform = `
                perspective(1000px)
                rotateX(${-xRotation}deg)
                rotateY(${yRotation}deg)
                scale(1.05)
            `;
        });
    });

    // Effet de parallaxe sur les images
    let ticking = false;
    
    document.addEventListener('mousemove', (e) => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                const moveX = (e.clientX * 0.05) / 8;
                const moveY = (e.clientY * 0.05) / 8;

                portfolioItems.forEach(item => {
                    const img = item.querySelector('img');
                    if (img) {
                        img.style.transform = `translate(${moveX}px, ${moveY}px)`;
                    }
                });
                
                ticking = false;
            });
            
            ticking = true;
        }
    });

    // Initialiser le chargement progressif des images
    lazyLoadImages();
}); 