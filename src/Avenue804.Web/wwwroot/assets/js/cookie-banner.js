(function () {
  'use strict';

  // Bumped from the original "804-cookie-consent" so previously-stored values
  // (which were always the literal "true") get re-prompted under the new
  // accepted/declined scheme. Old key is also cleaned up below.
  var KEY = '804-cookie-consent-v2';
  var OLD_KEY = '804-cookie-consent';

  function getEl()        { return document.getElementById('cookieBanner'); }
  function getAcceptBtn() { return document.getElementById('cookieBannerAccept'); }
  function getDeclineBtn(){ return document.getElementById('cookieBannerDecline'); }

  function show() {
    var b = getEl();
    if (b) b.style.display = 'flex';
  }
  function hide() {
    var b = getEl();
    if (b) b.style.display = 'none';
  }

  function setConsent(value) {
    try { localStorage.setItem(KEY, value); } catch (_) { /* private mode — best effort */ }
    try { localStorage.removeItem(OLD_KEY); } catch (_) { /* no-op */ }
  }

  function readConsent() {
    try {
      // Honour either the new key OR a stale "true" value from the old key, so
      // existing users aren't pestered after upgrading.
      return localStorage.getItem(KEY) || localStorage.getItem(OLD_KEY);
    } catch (_) {
      return null; // private mode — re-show every load is fine
    }
  }

  function init() {
    var banner = getEl();
    if (!banner) return;

    var acceptBtn  = getAcceptBtn();
    var declineBtn = getDeclineBtn();

    // ── Always wire handlers BEFORE deciding visibility. ──
    // The previous build returned early when consent was already stored, which
    // meant: (a) the banner — visible by default via the inline display:flex —
    // never got hidden, and (b) the buttons had no click listeners attached,
    // so nothing happened on click.
    if (acceptBtn && !acceptBtn.dataset.cookieWired) {
      acceptBtn.dataset.cookieWired = '1';
      acceptBtn.addEventListener('click', function (e) {
        e.preventDefault();
        setConsent('accepted');
        hide();
      });
    }
    if (declineBtn && !declineBtn.dataset.cookieWired) {
      declineBtn.dataset.cookieWired = '1';
      declineBtn.addEventListener('click', function (e) {
        e.preventDefault();
        setConsent('declined');
        hide();
      });
    }

    // ── Now decide whether to show or hide on this page load. ──
    if (readConsent()) {
      hide();
    } else {
      show();
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
