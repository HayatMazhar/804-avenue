(function () {
  'use strict';
  var el = document.currentScript || document.querySelector('script[data-tawk-property]');
  if (!el) return;

  var property = el.getAttribute('data-tawk-property');
  var widget = el.getAttribute('data-tawk-widget');
  if (!property || !widget) return;

  window.Tawk_API = window.Tawk_API || {};
  window.Tawk_LoadStart = new Date();

  var s1 = document.createElement('script');
  var s0 = document.getElementsByTagName('script')[0];
  s1.async = true;
  s1.src = 'https://embed.tawk.to/' + encodeURIComponent(property) + '/' + encodeURIComponent(widget);
  s1.charset = 'UTF-8';
  s1.setAttribute('crossorigin', '*');
  if (s0 && s0.parentNode) {
    s0.parentNode.insertBefore(s1, s0);
  } else {
    document.head.appendChild(s1);
  }
})();
