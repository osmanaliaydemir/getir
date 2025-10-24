// Service Worker for Getir WebApp
const CACHE_NAME = 'getir-v1.0.0';
const STATIC_CACHE_NAME = 'getir-static-v1.0.0';
const DYNAMIC_CACHE_NAME = 'getir-dynamic-v1.0.0';

// Static assets to cache
const STATIC_ASSETS = [
  '/',
  '/manifest.json',
  '/css/site.css',
  '/js/site.js',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png',
  '/images/getir-logo.png',
  '/images/offline-image.jpg'
];

// API endpoints to cache
const API_CACHE_PATTERNS = [
  /^https:\/\/ajilgo\.runasp\.net\/api\/v1\/merchant/,
  /^https:\/\/ajilgo\.runasp\.net\/api\/v1\/product/,
  /^https:\/\/ajilgo\.runasp\.net\/api\/v1\/category/
];

// Install event - cache static assets
self.addEventListener('install', event => {
  console.log('Service Worker: Installing...');
  
  event.waitUntil(
    caches.open(STATIC_CACHE_NAME)
      .then(cache => {
        console.log('Service Worker: Caching static assets');
        return cache.addAll(STATIC_ASSETS);
      })
      .then(() => {
        console.log('Service Worker: Installation complete');
        return self.skipWaiting();
      })
      .catch(error => {
        console.error('Service Worker: Installation failed', error);
      })
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
  console.log('Service Worker: Activating...');
  
  event.waitUntil(
    caches.keys()
      .then(cacheNames => {
        return Promise.all(
          cacheNames.map(cacheName => {
            if (cacheName !== STATIC_CACHE_NAME && 
                cacheName !== DYNAMIC_CACHE_NAME) {
              console.log('Service Worker: Deleting old cache', cacheName);
              return caches.delete(cacheName);
            }
          })
        );
      })
      .then(() => {
        console.log('Service Worker: Activation complete');
        return self.clients.claim();
      })
  );
});

// Fetch event - serve from cache or network
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip non-GET requests
  if (request.method !== 'GET') {
    return;
  }

  // Skip chrome-extension and other non-http requests
  if (!request.url.startsWith('http')) {
    return;
  }

  // Handle different types of requests
  if (isStaticAsset(request.url)) {
    event.respondWith(handleStaticAsset(request));
  } else if (isApiRequest(request.url)) {
    event.respondWith(handleApiRequest(request));
  } else if (isPageRequest(request)) {
    event.respondWith(handlePageRequest(request));
  } else {
    event.respondWith(handleOtherRequest(request));
  }
});

// Handle static assets (CSS, JS, images, etc.)
async function handleStaticAsset(request) {
  try {
    const cache = await caches.open(STATIC_CACHE_NAME);
    const cachedResponse = await cache.match(request);
    
    if (cachedResponse) {
      console.log('Service Worker: Serving static asset from cache', request.url);
      return cachedResponse;
    }

    const networkResponse = await fetch(request);
    
    if (networkResponse.ok) {
      cache.put(request, networkResponse.clone());
    }
    
    return networkResponse;
  } catch (error) {
    console.error('Service Worker: Error handling static asset', error);
    return new Response('Static asset not available offline', { status: 503 });
  }
}

// Handle API requests
async function handleApiRequest(request) {
  try {
    const cache = await caches.open(DYNAMIC_CACHE_NAME);
    const cachedResponse = await cache.match(request);
    
    // Try network first for API requests
    try {
      const networkResponse = await fetch(request);
      
      if (networkResponse.ok) {
        // Cache successful API responses
        cache.put(request, networkResponse.clone());
        console.log('Service Worker: API response cached', request.url);
      }
      
      return networkResponse;
    } catch (networkError) {
      // If network fails, try cache
      if (cachedResponse) {
        console.log('Service Worker: Serving API response from cache', request.url);
        return cachedResponse;
      }
      
      // Return offline response for API
      return new Response(JSON.stringify({
        error: 'API not available offline',
        message: 'Please check your internet connection'
      }), {
        status: 503,
        headers: { 'Content-Type': 'application/json' }
      });
    }
  } catch (error) {
    console.error('Service Worker: Error handling API request', error);
    return new Response('API not available', { status: 503 });
  }
}

// Handle page requests (HTML pages)
async function handlePageRequest(request) {
  try {
    const cache = await caches.open(DYNAMIC_CACHE_NAME);
    const cachedResponse = await cache.match(request);
    
    // Try network first
    try {
      const networkResponse = await fetch(request);
      
      if (networkResponse.ok) {
        cache.put(request, networkResponse.clone());
      }
      
      return networkResponse;
    } catch (networkError) {
      // If network fails, try cache
      if (cachedResponse) {
        console.log('Service Worker: Serving page from cache', request.url);
        return cachedResponse;
      }
      
      // Return offline page
      return new Response(`
        <!DOCTYPE html>
        <html lang="tr">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Çevrimdışı - Getir</title>
          <style>
            body { 
              font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
              margin: 0; padding: 20px; text-align: center; 
              background: linear-gradient(135deg, #5d3ebc, #784ba0);
              color: white; min-height: 100vh; display: flex; 
              flex-direction: column; justify-content: center;
            }
            .offline-container { max-width: 400px; margin: 0 auto; }
            .offline-icon { font-size: 4rem; margin-bottom: 1rem; }
            h1 { margin-bottom: 1rem; }
            p { margin-bottom: 2rem; opacity: 0.9; }
            .retry-btn { 
              background: white; color: #5d3ebc; border: none; 
              padding: 12px 24px; border-radius: 8px; 
              font-weight: bold; cursor: pointer; 
              transition: transform 0.2s;
            }
            .retry-btn:hover { transform: scale(1.05); }
          </style>
        </head>
        <body>
          <div class="offline-container">
            <div class="offline-icon">📱</div>
            <h1>Çevrimdışısınız</h1>
            <p>İnternet bağlantınızı kontrol edin ve tekrar deneyin.</p>
            <button class="retry-btn" onclick="window.location.reload()">
              Tekrar Dene
            </button>
          </div>
        </body>
        </html>
      `, {
        status: 200,
        headers: { 'Content-Type': 'text/html' }
      });
    }
  } catch (error) {
    console.error('Service Worker: Error handling page request', error);
    return new Response('Page not available', { status: 503 });
  }
}

// Handle other requests
async function handleOtherRequest(request) {
  try {
    return await fetch(request);
  } catch (error) {
    console.error('Service Worker: Error handling other request', error);
    return new Response('Request failed', { status: 503 });
  }
}

// Helper functions
function isStaticAsset(url) {
  return url.includes('/css/') || 
         url.includes('/js/') || 
         url.includes('/images/') || 
         url.includes('/icons/') ||
         url.includes('/fonts/') ||
         url.endsWith('.css') ||
         url.endsWith('.js') ||
         url.endsWith('.png') ||
         url.endsWith('.jpg') ||
         url.endsWith('.jpeg') ||
         url.endsWith('.gif') ||
         url.endsWith('.svg') ||
         url.endsWith('.woff') ||
         url.endsWith('.woff2');
}

function isApiRequest(url) {
  return API_CACHE_PATTERNS.some(pattern => pattern.test(url));
}

function isPageRequest(request) {
  return request.headers.get('accept')?.includes('text/html') || false;
}

// Background sync for offline actions
self.addEventListener('sync', event => {
  console.log('Service Worker: Background sync triggered', event.tag);
  
  if (event.tag === 'cart-sync') {
    event.waitUntil(syncCartData());
  } else if (event.tag === 'order-sync') {
    event.waitUntil(syncOrderData());
  }
});

// Push notifications
self.addEventListener('push', event => {
  console.log('Service Worker: Push notification received');
  
  const options = {
    body: event.data ? event.data.text() : 'Yeni bildirim',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    vibrate: [200, 100, 200],
    data: {
      dateOfArrival: Date.now(),
      primaryKey: 1
    },
    actions: [
      {
        action: 'explore',
        title: 'Keşfet',
        icon: '/icons/explore-icon.png'
      },
      {
        action: 'close',
        title: 'Kapat',
        icon: '/icons/close-icon.png'
      }
    ]
  };

  event.waitUntil(
    self.registration.showNotification('Getir', options)
  );
});

// Notification click handler
self.addEventListener('notificationclick', event => {
  console.log('Service Worker: Notification clicked');
  
  event.notification.close();
  
  if (event.action === 'explore') {
    event.waitUntil(
      clients.openWindow('/merchants')
    );
  } else if (event.action === 'close') {
    // Just close the notification
  } else {
    event.waitUntil(
      clients.openWindow('/')
    );
  }
});

// Sync functions
async function syncCartData() {
  try {
    console.log('Service Worker: Syncing cart data');
    // Implement cart sync logic here
  } catch (error) {
    console.error('Service Worker: Cart sync failed', error);
  }
}

async function syncOrderData() {
  try {
    console.log('Service Worker: Syncing order data');
    // Implement order sync logic here
  } catch (error) {
    console.error('Service Worker: Order sync failed', error);
  }
}

console.log('Service Worker: Loaded successfully');
