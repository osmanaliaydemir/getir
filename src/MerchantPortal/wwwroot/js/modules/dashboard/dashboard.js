/**
 * Dashboard Page Module
 * Handles SignalR real-time updates and dashboard interactions
 */

(function() {
    'use strict';

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        initDashboard();
    });

    /**
     * Initialize dashboard functionality
     */
    function initDashboard() {
        updateLastUpdateTime();
        setupSignalRHandlers();
    }

    /**
     * Update last update time every second
     */
    function updateLastUpdateTime() {
        const lastUpdateElement = document.getElementById('lastUpdate');
        if (!lastUpdateElement) return;

        function update() {
            const now = new Date();
            lastUpdateElement.textContent = now.toLocaleTimeString('tr-TR', { 
                hour: '2-digit', 
                minute: '2-digit' 
            });
        }

        // Initial update
        update();
        
        // Update every second
        setInterval(update, 1000);
    }

    /**
     * Setup SignalR event handlers
     */
    function setupSignalRHandlers() {
        if (!window.signalRConnection) {
            console.warn('SignalR connection not available');
            return;
        }

        // Listen for new orders
        window.signalRConnection.on("NewOrderReceived", function (data) {
            console.log('New order received:', data);
            
            // Show notification
            if (typeof showToast === 'function') {
                showToast(
                    `#${data.orderNumber} - ${data.customerName} - ₺${data.totalAmount.toFixed(2)}`,
                    'success',
                    5000
                );
            }
            
            // Play sound
            if (typeof playNotificationSound === 'function') {
                playNotificationSound();
            }
            
            // Flash browser tab
            if (typeof flashBrowserTab === 'function') {
                flashBrowserTab();
            }
            
            // Refresh dashboard after 2 seconds
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        });
        
        // Listen for order status changes
        window.signalRConnection.on("OrderStatusChanged", function (data) {
            console.log('Order status changed:', data);
            
            if (typeof showToast === 'function') {
                showToast(
                    `#${data.orderNumber} - ${data.status}`,
                    'info',
                    5000
                );
            }
        });
    }

})();

