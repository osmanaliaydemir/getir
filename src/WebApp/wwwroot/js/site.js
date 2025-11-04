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
        progressBar.style.transform = 'scaleX(0)';
    }, 10);

    const removeToast = (el) => {
        el.style.transform = 'translateX(100%)';
        setTimeout(() => el.remove(), 300);
    };

    // Auto dismiss
    const timer = setTimeout(() => removeToast(toast), duration);

    // Pause on hover
    toast.addEventListener('mouseenter', () => clearTimeout(timer));
    toast.addEventListener('mouseleave', () => setTimeout(() => removeToast(toast), duration / 2));

    // Click to dismiss
    toast.addEventListener('click', () => removeToast(toast));
};

// Store CSRF token from meta to localStorage for ApiClient
(function() {
    const setCsrfFromMeta = () => {
        try {
            const meta = document.querySelector('meta[name="csrf-token"]');
            if (meta && meta.content) {
                localStorage.setItem('csrf_token', meta.content);
            }
        } catch (e) {
            // no-op
        }
    };
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', setCsrfFromMeta);
    } else {
        setCsrfFromMeta();
    }
})();

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

// Share product functionality
window.shareProduct = async (title, text, url) => {
    if (navigator.share) {
        try {
            await navigator.share({
                title: title,
                text: text,
                url: url
            });
            return true;
        } catch (error) {
            // User cancelled or error occurred
            throw error;
        }
    } else {
        // Web Share API not supported
        throw new Error('Web Share API not supported');
    }
};

// Copy to clipboard functionality
window.copyToClipboard = async (text) => {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (error) {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            textArea.style.top = '-999999px';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            try {
                document.execCommand('copy');
                textArea.remove();
                return true;
            } catch (err) {
                textArea.remove();
                throw err;
            }
        }
    } else {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        textArea.style.top = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        try {
            document.execCommand('copy');
            textArea.remove();
            return true;
        } catch (err) {
            textArea.remove();
            throw err;
        }
    }
};

// ===== Leaflet Map Helpers =====
window._orderMaps = window._orderMaps || {};

window.initOrderMap = function(orderId, elementId, lat, lng) {
    try {
        if (!window.L) return false;
        const el = document.getElementById(elementId);
        if (!el) return false;

        const map = L.map(el).setView([lat || 41.0082, lng || 28.9784], 14);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        const markers = {};
        if (lat && lng) {
            markers.courier = L.marker([lat, lng], { title: 'Kurye' }).addTo(map);
        }
        window._orderMaps[orderId] = { map, markers, follow: true };
        return true;
    } catch {
        return false;
    }
};

window.updateCourierMarker = function(orderId, lat, lng) {
    try {
        const ctx = window._orderMaps[orderId];
        if (!ctx || !lat || !lng) return false;
        if (!ctx.markers.courier) {
            ctx.markers.courier = L.marker([lat, lng], { title: 'Kurye' }).addTo(ctx.map);
        } else {
            ctx.markers.courier.setLatLng([lat, lng]);
        }
        if (ctx.follow) {
            ctx.map.panTo([lat, lng]);
        }
        // Append to trail polyline
        if (!ctx.trail) {
            ctx.trail = L.polyline([[lat, lng]], { color: '#0d6efd', weight: 4, opacity: 0.7 }).addTo(ctx.map);
        } else {
            const latlngs = ctx.trail.getLatLngs();
            latlngs.push([lat, lng]);
            ctx.trail.setLatLngs(latlngs);
        }
        return true;
    } catch {
        return false;
    }
};

window.setDestinationMarker = function(orderId, lat, lng) {
    try {
        const ctx = window._orderMaps[orderId];
        if (!ctx || !lat || !lng) return false;
        if (!ctx.markers.destination) {
            ctx.markers.destination = L.marker([lat, lng], { title: 'Teslimat', icon: L.icon({
                iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
                iconSize: [25,41], iconAnchor: [12,41] })
            }).addTo(ctx.map);
        } else {
            ctx.markers.destination.setLatLng([lat, lng]);
        }
        return true;
    } catch { return false; }
};

window.setMerchantMarker = function(orderId, lat, lng) {
    try {
        const ctx = window._orderMaps[orderId];
        if (!ctx || !lat || !lng) return false;
        if (!ctx.markers.merchant) {
            ctx.markers.merchant = L.marker([lat, lng], { title: 'Mağaza' }).addTo(ctx.map);
        } else {
            ctx.markers.merchant.setLatLng([lat, lng]);
        }
        return true;
    } catch { return false; }
};

window.drawRoute = function(orderId, fromLat, fromLng, toLat, toLng) {
    try {
        const ctx = window._orderMaps[orderId];
        if (!ctx || !fromLat || !fromLng || !toLat || !toLng) return false;
        const latlngs = [[fromLat, fromLng], [toLat, toLng]];
        if (!ctx.route) {
            ctx.route = L.polyline(latlngs, { color: '#198754', weight: 3, dashArray: '6 6' }).addTo(ctx.map);
        } else {
            ctx.route.setLatLngs(latlngs);
        }
        ctx.map.fitBounds(L.latLngBounds(latlngs), { padding: [20,20] });
        return true;
    } catch { return false; }
};

// Haversine + ETA
function haversineKm(lat1, lon1, lat2, lon2) {
    const R = 6371; // km
    const dLat = (lat2-lat1) * Math.PI/180;
    const dLon = (lon2-lon1) * Math.PI/180;
    const a = Math.sin(dLat/2)*Math.sin(dLat/2) + Math.cos(lat1*Math.PI/180)*Math.cos(lat2*Math.PI/180)*Math.sin(dLon/2)*Math.sin(dLon/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    return R * c;
}

window.updateRouteAndEta = function(orderId, courierLat, courierLng, destLat, destLng, avgSpeedKmh = 25) {
    try {
        const dist = haversineKm(courierLat, courierLng, destLat, destLng);
        const minutes = Math.max(1, Math.round((dist / avgSpeedKmh) * 60));
        window.drawRoute(orderId, courierLat, courierLng, destLat, destLng);
        window.updateEtaBadge(`eta-${orderId}`, `${minutes} dk`);
        return true;
    } catch { return false; }
};

// ===== OSRM Routing (production-grade route and ETA) =====
window.updateRouteAndEtaOsrm = async function(orderId, courierLat, courierLng, destLat, destLng) {
    try {
        const ctx = window._orderMaps[orderId];
        if (!ctx) return false;
        const url = `https://router.project-osrm.org/route/v1/driving/${courierLng},${courierLat};${destLng},${destLat}?overview=full&geometries=geojson`;
        const resp = await fetch(url, { method: 'GET' });
        if (!resp.ok) return false;
        const data = await resp.json();
        if (!data || !data.routes || !data.routes[0]) return false;
        const route = data.routes[0];
        const seconds = Math.max(60, Math.round(route.duration));
        const minutes = Math.max(1, Math.round(seconds / 60));
        const coords = route.geometry.coordinates.map(([lng, lat]) => [lat, lng]);

        if (!ctx.osrmRoute) {
            ctx.osrmRoute = L.polyline(coords, { color: '#198754', weight: 4 }).addTo(ctx.map);
        } else {
            ctx.osrmRoute.setLatLngs(coords);
        }

        // Fit bounds softly
        const bounds = L.latLngBounds(coords);
        ctx.map.fitBounds(bounds, { padding: [20, 20] });

        // Update ETA badge
        window.updateEtaBadge(`eta-${orderId}`, `${minutes} dk`);
        return true;
    } catch (e) {
        // fallback is ok, do nothing
        return false;
    }
};
// UX helpers
window.toggleFollowCourier = function(orderId, enabled) {
    try {
        const ctx = window._orderMaps[orderId];
        if (!ctx) return false;
        ctx.follow = !!enabled;
        return true;
    } catch { return false; }
};

window.toggleTrail = function(orderId, enabled) {
    try {
        const ctx = window._orderMaps[orderId];
        if (!ctx) return false;
        if (enabled === false && ctx.trail) {
            ctx.map.removeLayer(ctx.trail);
            ctx.trail = null;
        }
        return true;
    } catch { return false; }
};
window.updateEtaBadge = function(elementId, etaText) {
    try {
        const el = document.getElementById(elementId);
        if (!el) return false;
        el.textContent = etaText || '';
        return true;
    } catch {
        return false;
    }
};

// ===== Localization UI helpers =====
// Safe no-op function used by Blazor component LanguageSelector via JS interop
// Prevents JSException if not needed in current layout
window.updateLanguageDisplay = function(culture, elementId) {
    try {
        const targetId = elementId || 'language-display';
        const el = document.getElementById(targetId);
        if (el && typeof culture === 'string') {
            el.textContent = culture.toUpperCase();
        }
        return true;
    } catch {
        return false;
    }
};

// Update html lang and dir attributes safely
window.updateLanguageAttributes = function(culture) {
    try {
        const html = document.documentElement;
        const isRtl = typeof culture === 'string' && culture.toLowerCase().startsWith('ar');
        html.setAttribute('lang', (culture || 'tr').toLowerCase());
        html.setAttribute('dir', isRtl ? 'rtl' : 'ltr');
        return true;
    } catch {
        return false;
    }
};

// ===== Geolocation Helpers =====
window.getCurrentPositionAsync = function() {
    return new Promise(function(resolve) {
        if (!('geolocation' in navigator)) {
            resolve(null);
            return;
        }
        navigator.geolocation.getCurrentPosition(function(pos) {
            resolve({ latitude: pos.coords.latitude, longitude: pos.coords.longitude });
        }, function() {
            resolve(null);
        }, { enableHighAccuracy: true, timeout: 5000, maximumAge: 10000 });
    });
};