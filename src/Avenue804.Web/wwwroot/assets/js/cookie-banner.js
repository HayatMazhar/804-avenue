(function () {
  'use strict';
  var KEY = '804-cookie-consent';

  function show() {
    var b = document.getElementById('cookieBanner');
    if (b) b.style.display = 'flex';
  }

  function hide() {
    var b = document.getElementById('cookieBanner');
    if (b) b.style.display = 'none';
  }

  function accept() {
    try { localStorage.setItem(KEY, 'true'); } catch (_) { /* private mode */ }
    hide();
  }

  function decline() {
    hide();
  }

  function init() {
    try {
      if (localStorage.getItem(KEY)) return;
    } catch (_) { /* private mode — show every time */ }
    show();

    var acceptBtn = document.getElementById('cookieBannerAccept');
    var declineBtn = document.getElementById('cookieBannerDecline');
    if (acceptBtn) acceptBtn.addEventListener('click', accept);
    if (declineBtn) declineBtn.addEventListener('click', decline);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
