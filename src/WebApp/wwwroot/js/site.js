// Modern Toast Notification System
window.showToast = (message, type = 'info', duration = 4000) => {
    // Remove existing toasts
    const existingToasts = document.querySelectorAll('.toast-notification');
    existingToasts.forEach(toast => toast.remove());

    // Create toast container if it doesn't exist
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            display: flex;
            flex-direction: column;
            gap: 10px;
        `;
        document.body.appendChild(container);
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.className = 'toast-notification';
    
    // Set toast type and styling
    const typeConfig = {
        success: { icon: '✓', bgColor: '#10b981', textColor: '#ffffff' },
        error: { icon: '✕', bgColor: '#ef4444', textColor: '#ffffff' },
        warning: { icon: '⚠', bgColor: '#f59e0b', textColor: '#ffffff' },
        info: { icon: 'ℹ', bgColor: '#3b82f6', textColor: '#ffffff' }
    };

    const config = typeConfig[type] || typeConfig.info;
    
    toast.style.cssText = `
        background: ${config.bgColor};
        color: ${config.textColor};
        padding: 16px 20px;
        border-radius: 12px;
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
        display: flex;
        align-items: center;
        gap: 12px;
        min-width: 300px;
        max-width: 400px;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        font-size: 14px;
        font-weight: 500;
        line-height: 1.4;
        transform: translateX(100%);
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        cursor: pointer;
        position: relative;
        overflow: hidden;
    `;

    // Add icon
    const icon = document.createElement('span');
    icon.style.cssText = `
        font-size: 18px;
        font-weight: bold;
        flex-shrink: 0;
    `;
    icon.textContent = config.icon;
    toast.appendChild(icon);

    // Add message
    const messageEl = document.createElement('span');
    messageEl.textContent = message;
    messageEl.style.cssText = `
        flex: 1;
        word-wrap: break-word;
    `;
    toast.appendChild(messageEl);

    // Add close button
    const closeBtn = document.createElement('button');
    closeBtn.innerHTML = '×';
    closeBtn.style.cssText = `
        background: none;
        border: none;
        color: inherit;
        font-size: 20px;
        font-weight: bold;
        cursor: pointer;
        padding: 0;
        margin-left: 8px;
        opacity: 0.8;
        transition: opacity 0.2s;
        flex-shrink: 0;
    `;
    closeBtn.onmouseover = () => closeBtn.style.opacity = '1';
    closeBtn.onmouseout = () => closeBtn.style.opacity = '0.8';
    closeBtn.onclick = () => removeToast(toast);
    toast.appendChild(closeBtn);

    // Add progress bar
    const progressBar = document.createElement('div');
    progressBar.style.cssText = `
        position: absolute;
        bottom: 0;
        left: 0;
        height: 3px;
        background: rgba(255, 255, 255, 0.3);
        width: 100%;
        transform: scaleX(1);
        transform-origin: left;
        transition: transform ${duration}ms linear;
    `;
    toast.appendChild(progressBar);

    // Add to container
    container.appendChild(toast);

    // Animate in
    setTimeout(() => {
        toast.style.transform = 'translateX(0)';
    }, 10);

    // Auto remove
    const autoRemoveTimer = setTimeout(() => {
        removeToast(toast);
    }, duration);

    // Progress bar animation
    setTimeout(() => {
        progressBar.style.transform = 'scaleX(0)';
    }, 10);

    // Click to dismiss
    toast.onclick = () => {
        clearTimeout(autoRemoveTimer);
        removeToast(toast);
    };
};

function removeToast(toast) {
    toast.style.transform = 'translateX(100%)';
    toast.style.opacity = '0';
    setTimeout(() => {
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 300);
}

// Success toast
window.showSuccess = (message, duration = 4000) => {
    window.showToast(message, 'success', duration);
};

// Error toast
window.showError = (message, duration = 5000) => {
    window.showToast(message, 'error', duration);
};

// Warning toast
window.showWarning = (message, duration = 4000) => {
    window.showToast(message, 'warning', duration);
};

// Info toast
window.showInfo = (message, duration = 4000) => {
    window.showToast(message, 'info', duration);
};
