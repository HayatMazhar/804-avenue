(function () {
  'use strict';
  var el = document.currentScript || document.querySelector('script[data-ga4-id]');
  if (!el) return;

  var id = el.getAttribute('data-ga4-id');
  if (!id) return;

  var loader = document.createElement('script');
  loader.async = true;
  loader.src = 'https://www.googletagmanager.com/gtag/js?id=' + encodeURIComponent(id);
  document.head.appendChild(loader);

  window.dataLayer = window.dataLayer || [];
  function gtag() { window.dataLayer.push(arguments); }
  window.gtag = gtag;
  gtag('js', new Date());
  gtag('config', id, { anonymize_ip: true });
})();
