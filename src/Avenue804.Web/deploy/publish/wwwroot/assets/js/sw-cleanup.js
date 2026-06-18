// One-shot cleanup: unregisters any leftover service worker from prior
// experiments and clears their caches. Loaded only when the app sets
// data-sw-cleanup="true" on the script tag (Site:UnregisterServiceWorker).
// Once we ship a real service worker we'll delete this file.
(function () {
  'use strict';
  if (!('serviceWorker' in navigator)) return;
  navigator.serviceWorker.getRegistrations().then(function (regs) {
    regs.forEach(function (r) { r.unregister(); });
  }).catch(function () { /* ignore */ });
  if (window.caches && caches.keys) {
    caches.keys().then(function (keys) {
      keys.forEach(function (k) { caches.delete(k); });
    }).catch(function () { /* ignore */ });
  }
})();
