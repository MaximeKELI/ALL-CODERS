document.addEventListener('DOMContentLoaded', () => {
    const contactForm = document.getElementById('contactForm');
    const submitBtn = contactForm?.querySelector('.submit-btn');
    
    // Animation des éléments d'information au scroll
    const infoItems = document.querySelectorAll('.info-item');
    
    const infoObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateX(0)';
            }
        });
    }, { threshold: 0.1 });

    infoItems.forEach(item => {
        infoObserver.observe(item);
    });

    // Gestion des inputs du formulaire
    const formInputs = document.querySelectorAll('.form-group input, .form-group textarea');
    
    formInputs.forEach(input => {
        // Animation du label
        input.addEventListener('focus', () => {
            const label = input.nextElementSibling;
            if (label) {
                label.style.transform = 'translateY(-2rem)';
                label.style.fontSize = '0.8rem';
                label.style.color = 'var(--primary-color)';
            }
        });

        input.addEventListener('blur', () => {
            const label = input.nextElementSibling;
            if (label && !input.value) {
                label.style.transform = 'translateY(0)';
                label.style.fontSize = '1rem';
                label.style.color = 'rgba(255, 255, 255, 0.6)';
            }
        });

        // Validation en temps réel
        input.addEventListener('input', () => {
            validateInput(input);
        });
    });

    // Fonction de validation des inputs selon les règles C#
    const validateInput = (input) => {
        const value = input.value.trim();
        let isValid = true;
        let errorMessage = '';

        switch (input.id) {
            case 'name':
                if (value.length === 0) {
                    isValid = false;
                    errorMessage = 'Le nom est requis';
                }
                break;
            case 'email':
                if (value.length === 0) {
                    isValid = false;
                    errorMessage = 'L\'email est requis';
                } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
                    isValid = false;
                    errorMessage = 'Format d\'email invalide';
                }
                break;
            case 'message':
                if (value.length === 0) {
                    isValid = false;
                    errorMessage = 'Le message est requis';
                } else if (value.length < 10) {
                    isValid = false;
                    errorMessage = 'Le message doit contenir au moins 10 caractères';
                }
                break;
        }

        // Mise à jour du style et affichage du message d'erreur
        const errorElement = input.parentElement.querySelector('.error-message');
        if (!isValid) {
            input.style.borderColor = '#ff1b6b';
            input.style.background = 'rgba(255, 27, 107, 0.1)';
            
            if (!errorElement) {
                const error = document.createElement('span');
                error.className = 'error-message';
                error.style.color = '#ff1b6b';
                error.style.fontSize = '0.8rem';
                error.style.marginTop = '0.5rem';
                error.style.display = 'block';
                input.parentElement.appendChild(error);
            }
            if (errorElement) {
                errorElement.textContent = errorMessage;
            }
        } else {
            input.style.borderColor = '#00ff9d';
            input.style.background = 'rgba(0, 255, 157, 0.1)';
            if (errorElement) {
                errorElement.remove();
            }
        }

        return isValid;
    };

    // Gestion de la soumission du formulaire
    contactForm?.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const name = document.getElementById('name');
        const email = document.getElementById('email');
        const message = document.getElementById('message');
        
        if (!name || !email || !message) return;

        // Validation de tous les champs
        const isNameValid = validateInput(name);
        const isEmailValid = validateInput(email);
        const isMessageValid = validateInput(message);

        if (!isNameValid || !isEmailValid || !isMessageValid) {
            showNotification('Veuillez corriger les erreurs dans le formulaire', 'error');
            return;
        }

        // Animation du bouton pendant l'envoi
        if (submitBtn) {
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            submitBtn.disabled = true;
        }

        // Envoi à l'API
        try {
            const response = await fetch('/api/contact', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    name: name.value.trim(),
                    email: email.value.trim(),
                    message: message.value.trim()
                })
            });

            const data = await response.json();
            
            if (response.ok) {
                showNotification(data.message, 'success');
                contactForm.reset();
                
                // Réinitialisation des styles des inputs
                formInputs.forEach(input => {
                    input.style.borderColor = '';
                    input.style.background = '';
                    const label = input.nextElementSibling;
                    if (label) {
                        label.style.transform = 'translateY(0)';
                        label.style.fontSize = '1rem';
                        label.style.color = 'rgba(255, 255, 255, 0.6)';
                    }
                    const errorElement = input.parentElement.querySelector('.error-message');
                    if (errorElement) {
                        errorElement.remove();
                    }
                });
            } else {
                showNotification(data.message || 'Une erreur est survenue', 'error');
            }
        } catch (error) {
            showNotification('Erreur de connexion au serveur', 'error');
            console.error('Erreur:', error);
        } finally {
            if (submitBtn) {
                submitBtn.innerHTML = '<span>Envoyer</span><i class="fas fa-paper-plane"></i>';
                submitBtn.disabled = false;
            }
        }
    });

    // Système de notification amélioré
    const showNotification = (message, type) => {
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.textContent = message;

        // Ajout d'un bouton de fermeture
        const closeButton = document.createElement('button');
        closeButton.innerHTML = '&times;';
        closeButton.className = 'notification-close';
        closeButton.onclick = () => {
            notification.style.transform = 'translateX(100%)';
            notification.style.opacity = '0';
            setTimeout(() => notification.remove(), 300);
        };
        notification.appendChild(closeButton);

        document.body.appendChild(notification);

        // Animation d'entrée
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
            notification.style.opacity = '1';
        }, 100);

        // Animation de sortie automatique
        setTimeout(() => {
            if (document.body.contains(notification)) {
                notification.style.transform = 'translateX(100%)';
                notification.style.opacity = '0';
                setTimeout(() => {
                    if (document.body.contains(notification)) {
                        notification.remove();
                    }
                }, 300);
            }
        }, 5000);
    };

    // Styles pour les notifications
    const style = document.createElement('style');
    style.textContent = `
        .notification {
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 40px 15px 25px;
            border-radius: 10px;
            color: white;
            transform: translateX(100%);
            opacity: 0;
            transition: all 0.3s ease;
            z-index: 1000;
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
        }

        .notification.success {
            background: linear-gradient(135deg, #00ff9d, #00bf8f);
        }

        .notification.error {
            background: linear-gradient(135deg, #ff1b6b, #ff0844);
        }

        .notification-close {
            position: absolute;
            right: 10px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            color: white;
            font-size: 20px;
            cursor: pointer;
            padding: 0 5px;
            opacity: 0.8;
            transition: opacity 0.3s ease;
        }

        .notification-close:hover {
            opacity: 1;
        }

        .error-message {
            margin-top: 0.5rem;
            color: #ff1b6b;
            font-size: 0.8rem;
            display: block;
        }
    `;
    document.head.appendChild(style);
}); 