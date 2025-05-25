document.addEventListener('DOMContentLoaded', () => {
    const shareButtons = document.querySelectorAll('.share-button');
    
    shareButtons.forEach(button => {
        button.addEventListener('click', async (e) => {
            e.preventDefault();
            
            const platform = button.dataset.platform;
            const url = encodeURIComponent(window.location.href);
            const title = encodeURIComponent(document.title);
            
            let shareUrl;
            
            switch (platform) {
                case 'facebook':
                    shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${url}`;
                    break;
                case 'twitter':
                    shareUrl = `https://twitter.com/intent/tweet?url=${url}&text=${title}`;
                    break;
                case 'linkedin':
                    shareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${url}`;
                    break;
                case 'whatsapp':
                    shareUrl = `https://wa.me/?text=${title}%20${url}`;
                    break;
                default:
                    return;
            }
            
            // Ouvrir la fenêtre de partage
            window.open(shareUrl, '_blank', 'width=600,height=400');
            
            try {
                // Enregistrer la statistique de partage
                await fetch('/api/share', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        platform,
                        url: window.location.href,
                        shareCount: 1
                    })
                });
                
                // Mettre à jour le compteur si présent
                const counter = button.querySelector('.share-count');
                if (counter) {
                    const currentCount = parseInt(counter.textContent) || 0;
                    counter.textContent = currentCount + 1;
                }
                
                // Animation de succès
                button.classList.add('shared');
                setTimeout(() => button.classList.remove('shared'), 1000);
                
            } catch (error) {
                console.error('Erreur lors de l\'enregistrement du partage:', error);
            }
        });
    });
    
    // Styles pour les boutons de partage
    const style = document.createElement('style');
    style.textContent = `
        .share-button {
            display: inline-flex;
            align-items: center;
            padding: 8px 16px;
            border-radius: 20px;
            border: none;
            color: white;
            cursor: pointer;
            transition: all 0.3s ease;
            margin: 5px;
            font-size: 14px;
        }
        
        .share-button i {
            margin-right: 8px;
        }
        
        .share-button.facebook {
            background: #1877f2;
        }
        
        .share-button.twitter {
            background: #1da1f2;
        }
        
        .share-button.linkedin {
            background: #0077b5;
        }
        
        .share-button.whatsapp {
            background: #25d366;
        }
        
        .share-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(0,0,0,0.2);
        }
        
        .share-button.shared {
            animation: pulse 1s;
        }
        
        .share-count {
            background: rgba(255,255,255,0.2);
            padding: 2px 6px;
            border-radius: 10px;
            margin-left: 8px;
            font-size: 12px;
        }
        
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.1); }
            100% { transform: scale(1); }
        }
    `;
    document.head.appendChild(style);
}); 